using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

[DefaultExecutionOrder(120)]
public sealed class RopaDinamicaChasqui : MonoBehaviour
{
    [Serializable] public class Volumen
    {
        public Transform inicio, fin;
        public Vector3 desplazamientoInicio, desplazamientoFin;
        public float radioInicio = .1f, radioFin = .1f;
    }

    struct Capsula
    {
        public float3 a, b, eje, min, max;
        public float ra, rb, inverso;
    }

    struct Caja
    {
        public float3 center, extents;
    }

    public SkinnedMeshRenderer piel;
    public MeshFilter salida;
    public Transform marco;
    [HideInInspector] public float[] libertad;
    [HideInInspector] public bool[] esCapa;
    public Volumen[] cuerpo;
    public BoxCollider[] entorno;
    public float separacion = .012f;

    Mesh horneada, deformada;
    int cantidadVertices;
    Volumen[] cuerpoValido;
    Matrix4x4 marcoAnterior;
    Vector3 posicionAnterior, velocidadAnterior;
    float reloj;
    bool iniciado;

    NativeArray<Vector3> lectura, normalesEntrada, escritura;
    NativeArray<float3> posiciones, posicionesFinal, velocidades, objetivos, normales, baseMundos;
    NativeArray<float> libertadNative, deformaciones, resultadoMax;
    NativeArray<byte> esCapaNative;
    NativeArray<int> vecinoOffsets, vecinoIndices;
    NativeArray<Capsula> capsulas;
    NativeArray<Caja> cajas;
    int cantidadCapsulas;

    public float UltimaDeformacionMaxima { get; private set; }
    public int CantidadVertices => cantidadVertices;

    void Awake()
    {
        if (piel == null || salida == null || marco == null || piel.sharedMesh == null)
        {
            enabled = false;
            return;
        }

        horneada = new Mesh();
        horneada.MarkDynamic();
        deformada = Instantiate(piel.sharedMesh);
        deformada.name = "Ropa simulada en juego";
        deformada.MarkDynamic();
        salida.sharedMesh = deformada;

        int n = deformada.vertexCount;
        cantidadVertices = n;

        lectura = new NativeArray<Vector3>(n, Allocator.Persistent);
        normalesEntrada = new NativeArray<Vector3>(n, Allocator.Persistent);
        escritura = new NativeArray<Vector3>(n, Allocator.Persistent);
        posiciones = new NativeArray<float3>(n, Allocator.Persistent);
        posicionesFinal = new NativeArray<float3>(n, Allocator.Persistent);
        velocidades = new NativeArray<float3>(n, Allocator.Persistent);
        objetivos = new NativeArray<float3>(n, Allocator.Persistent);
        normales = new NativeArray<float3>(n, Allocator.Persistent);
        baseMundos = new NativeArray<float3>(n, Allocator.Persistent);
        libertadNative = new NativeArray<float>(n, Allocator.Persistent);
        deformaciones = new NativeArray<float>(n, Allocator.Persistent);
        resultadoMax = new NativeArray<float>(1, Allocator.Persistent);
        esCapaNative = new NativeArray<byte>(n, Allocator.Persistent);

        int libLen = libertad != null ? math.min(n, libertad.Length) : 0;
        for (int i = 0; i < libLen; i++) libertadNative[i] = libertad[i];
        int capaLen = esCapa != null ? math.min(n, esCapa.Length) : 0;
        for (int i = 0; i < capaLen; i++) esCapaNative[i] = esCapa[i] ? (byte)1 : (byte)0;

        // Build flattened CSR adjacency table once for Burst-compatible smoothing.
        var enlaces = new System.Collections.Generic.HashSet<int>[n];
        for (int i = 0; i < n; i++) enlaces[i] = new System.Collections.Generic.HashSet<int>();
        var triangles = deformada.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int a = triangles[i], b = triangles[i + 1], c = triangles[i + 2];
            enlaces[a].Add(b); enlaces[a].Add(c);
            enlaces[b].Add(a); enlaces[b].Add(c);
            enlaces[c].Add(a); enlaces[c].Add(b);
        }

        vecinoOffsets = new NativeArray<int>(n + 1, Allocator.Persistent);
        int totalVecinos = 0;
        for (int i = 0; i < n; i++)
        {
            vecinoOffsets[i] = totalVecinos;
            totalVecinos += enlaces[i].Count;
        }
        vecinoOffsets[n] = totalVecinos;
        vecinoIndices = new NativeArray<int>(math.max(1, totalVecinos), Allocator.Persistent);
        int cursor = 0;
        for (int i = 0; i < n; i++)
        {
            foreach (int v in enlaces[i]) vecinoIndices[cursor++] = v;
        }

        entorno = Array.FindAll(entorno ?? Array.Empty<BoxCollider>(), c => c != null);
        cuerpoValido = Array.FindAll(cuerpo ?? Array.Empty<Volumen>(), v => v != null && v.inicio != null && v.fin != null);
        capsulas = new NativeArray<Capsula>(math.max(1, cuerpoValido.Length), Allocator.Persistent);
        cajas = new NativeArray<Caja>(math.max(1, entorno.Length), Allocator.Persistent);

        marcoAnterior = marco.localToWorldMatrix;
        posicionAnterior = transform.position;
    }

    void LateUpdate()
    {
        if (horneada == null || cantidadVertices == 0) return;

        float dt = Mathf.Clamp(Time.deltaTime, .001f, .04f);
        reloj += dt;

        Matrix4x4 actual = marco.localToWorldMatrix;
        Matrix4x4 delta = actual * marcoAnterior.inverse;
        Vector3 posActual = transform.position;
        Vector3 velocidad = (posActual - posicionAnterior) / dt;
        if (velocidad.magnitude > 20f) iniciado = false;
        Vector3 aceleracion = Vector3.ClampMagnitude((velocidad - velocidadAnterior) / dt, 8f);
        float fuerza = Mathf.Clamp01(Vector3.ProjectOnPlane(velocidad, Vector3.up).magnitude / 4.8f);

        cantidadCapsulas = 0;
        for (int i = 0; i < cuerpoValido.Length; i++)
        {
            var v = cuerpoValido[i];
            if (v.inicio == null || v.fin == null) continue;
            float3 a = (float3)(v.inicio.position + marco.TransformVector(v.desplazamientoInicio));
            float3 b = (float3)(v.fin.position + marco.TransformVector(v.desplazamientoFin));
            float3 eje = b - a;
            float3 radio = new float3(math.max(v.radioInicio, v.radioFin) + separacion);
            float sq = math.lengthsq(eje);
            capsulas[cantidadCapsulas++] = new Capsula
            {
                a = a,
                b = b,
                eje = eje,
                inverso = sq > .000001f ? 1f / sq : 0f,
                min = math.min(a, b) - radio,
                max = math.max(a, b) + radio,
                ra = v.radioInicio,
                rb = v.radioFin
            };
        }

        int cantidadCajas = 0;
        Vector3 cerca = marco.position + Vector3.up;
        for (int i = 0; i < entorno.Length; i++)
        {
            if (entorno[i] == null) continue;
            var b = entorno[i].bounds;
            if (b.SqrDistance(cerca) < 5f)
            {
                cajas[cantidadCajas++] = new Caja { center = b.center, extents = b.extents };
            }
        }

        piel.BakeMesh(horneada);
        if (horneada.vertexCount != cantidadVertices) return;
        using (var meshData = Mesh.AcquireReadOnlyMeshData(horneada))
        {
            var md = meshData[0];
            md.GetVertices(lectura);
            if (md.HasVertexAttribute(VertexAttribute.Normal))
            {
                md.GetNormals(normalesEntrada);
            }
            else
            {
                for (int i = 0; i < cantidadVertices; i++) normalesEntrada[i] = Vector3.up;
            }
        }

        var job1 = new Paso1Job
        {
            lectura = lectura,
            normalesEntrada = normalesEntrada,
            libertad = libertadNative,
            esCapa = esCapaNative,
            capsulas = capsulas,
            cajas = cajas,
            posiciones = posiciones,
            velocidades = velocidades,
            objetivos = objetivos,
            normales = normales,
            baseMundos = baseMundos,
            aMundo = piel.transform.localToWorldMatrix,
            pielRot = piel.transform.rotation,
            marcoInv = marco.worldToLocalMatrix,
            delta = delta,
            marcoPos = marco.position,
            marcoForward = marco.forward,
            marcoUp = marco.up,
            marcoRight = marco.right,
            aceleracion = aceleracion,
            dt = dt,
            reloj = reloj,
            fuerza = fuerza,
            separacion = separacion,
            iniciado = iniciado,
            cantidadCapsulas = cantidadCapsulas,
            cantidadCajas = cantidadCajas
        };

        var job2 = new Paso2Job
        {
            posicionesIn = posiciones,
            objetivos = objetivos,
            normales = normales,
            baseMundos = baseMundos,
            libertad = libertadNative,
            vecinoOffsets = vecinoOffsets,
            vecinoIndices = vecinoIndices,
            capsulas = capsulas,
            cajas = cajas,
            posicionesOut = posicionesFinal,
            escritura = escritura,
            deformaciones = deformaciones,
            aLocal = salida.transform.worldToLocalMatrix,
            separacion = separacion,
            cantidadCapsulas = cantidadCapsulas,
            cantidadCajas = cantidadCajas
        };

        var maxJob = new MaxDeformacionJob
        {
            deformaciones = deformaciones,
            resultado = resultadoMax,
            cantidad = cantidadVertices
        };

        JobHandle h1 = job1.Schedule(cantidadVertices, 256);
        JobHandle h2 = job2.Schedule(cantidadVertices, 256, h1);
        JobHandle h3 = maxJob.Schedule(h2);
        h3.Complete();

        (posiciones, posicionesFinal) = (posicionesFinal, posiciones);
        UltimaDeformacionMaxima = resultadoMax[0];

        deformada.SetVertices(escritura);
        deformada.RecalculateNormals();
        deformada.RecalculateBounds();

        marcoAnterior = actual;
        posicionAnterior = posActual;
        velocidadAnterior = velocidad;
        iniciado = true;
    }

    [BurstCompile(FloatMode = FloatMode.Fast)]
    struct Paso1Job : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> lectura, normalesEntrada;
        [ReadOnly] public NativeArray<float> libertad;
        [ReadOnly] public NativeArray<byte> esCapa;
        [ReadOnly] public NativeArray<Capsula> capsulas;
        [ReadOnly] public NativeArray<Caja> cajas;

        public NativeArray<float3> posiciones, velocidades;
        [WriteOnly] public NativeArray<float3> objetivos, normales, baseMundos;

        public float4x4 aMundo, marcoInv, delta;
        public quaternion pielRot;
        public float3 marcoPos, marcoForward, marcoUp, marcoRight, aceleracion;
        public float dt, reloj, fuerza, separacion;
        public bool iniciado;
        public int cantidadCapsulas, cantidadCajas;

        public void Execute(int i)
        {
            float3 basePose = math.mul(aMundo, new float4((float3)lectura[i], 1f)).xyz;
            baseMundos[i] = basePose;

            float3 n = math.normalizesafe(math.mul(pielRot, (float3)normalesEntrada[i]), new float3(0f, 1f, 0f));
            normales[i] = n;

            float f = libertad[i];
            float3 pLocal = math.mul(marcoInv, new float4(basePose, 1f)).xyz;
            float3 deseada = basePose;
            bool capa = esCapa[i] != 0;

            if (capa)
            {
                float fase = f * 8f - reloj * (7f + fuerza * 7f);
                deseada -= marcoForward * (math.sin(pLocal.x * 25f + f * .4f) * (.008f + .014f * f) * f);
                deseada += marcoUp * (math.sin(fase + pLocal.x * 2f) * (.004f + .026f * fuerza) * f * f);
                deseada += marcoRight * (math.sin(fase * .75f) * (.003f + .012f * fuerza) * f * f);
                deseada -= aceleracion * (.0025f * f);
            }
            else
            {
                float3 radialRaw = basePose - (marcoPos + marcoUp * pLocal.y);
                radialRaw -= marcoUp * math.dot(radialRaw, marcoUp);
                float3 radial = math.normalizesafe(radialRaw, float3.zero);
                float angulo = math.atan2(pLocal.x, pLocal.z);
                deseada += radial * (math.sin(angulo * 9f + pLocal.y * 3f) * .007f * f);
                deseada += radial * (math.sin(angulo * 7f - reloj * (4f + fuerza * 4f)) * (.003f + .012f * fuerza) * f * f);
                deseada -= aceleracion * (.0015f * f);
            }

            objetivos[i] = deseada;

            float3 pos;
            float3 vel;
            if (!iniciado)
            {
                pos = deseada;
                vel = float3.zero;
            }
            else
            {
                pos = math.mul(delta, new float4(posiciones[i], 1f)).xyz;
                vel = math.mul((float3x3)delta, velocidades[i]);
                if (f < .005f)
                {
                    pos = deseada;
                    vel = float3.zero;
                }
                else
                {
                    float smoothTime = math.max(.0001f, math.lerp(.018f, .065f, f));
                    float omega = 2f / smoothTime;
                    float x = omega * dt;
                    float exp = 1f / (1f + x + .48f * x * x + .235f * x * x * x);
                    float3 change = pos - deseada;
                    float3 temp = (vel + omega * change) * dt;
                    vel = (vel - omega * temp) * exp;
                    float3 nueva = deseada + (change + temp) * exp;
                    if (math.dot(deseada - pos, nueva - deseada) > 0f)
                    {
                        pos = deseada;
                        vel = float3.zero;
                    }
                    else
                    {
                        pos = nueva;
                    }
                }
            }

            float3 desplazamiento = pos - deseada;
            float limite = capa ? .13f : .22f;
            float dispSq = math.lengthsq(desplazamiento);
            if (dispSq > limite * limite)
            {
                pos = deseada + desplazamiento * (limite * math.rsqrt(dispSq));
            }

            pos = Resolver(pos, n, true, capsulas, cantidadCapsulas, cajas, cantidadCajas, separacion);
            posiciones[i] = pos;
            velocidades[i] = vel;
        }
    }

    [BurstCompile(FloatMode = FloatMode.Fast)]
    struct Paso2Job : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> posicionesIn, objetivos, normales, baseMundos;
        [ReadOnly] public NativeArray<float> libertad;
        [ReadOnly] public NativeArray<int> vecinoOffsets, vecinoIndices;
        [ReadOnly] public NativeArray<Capsula> capsulas;
        [ReadOnly] public NativeArray<Caja> cajas;

        [WriteOnly] public NativeArray<float3> posicionesOut;
        [WriteOnly] public NativeArray<Vector3> escritura;
        [WriteOnly] public NativeArray<float> deformaciones;

        public float4x4 aLocal;
        public float separacion;
        public int cantidadCapsulas, cantidadCajas;

        public void Execute(int i)
        {
            int inicio = vecinoOffsets[i];
            int fin = vecinoOffsets[i + 1];
            int cuenta = fin - inicio;
            float3 promedio = float3.zero;
            if (cuenta > 0)
            {
                float3 suma = float3.zero;
                for (int k = inicio; k < fin; k++)
                {
                    int vecino = vecinoIndices[k];
                    suma += posicionesIn[vecino] - objetivos[vecino];
                }
                promedio = suma / cuenta;
            }

            float3 suavizada = math.lerp(posicionesIn[i], objetivos[i] + promedio, .20f * libertad[i]);
            float3 pos = Resolver(suavizada, normales[i], true, capsulas, cantidadCapsulas, cajas, cantidadCajas, separacion);

            posicionesOut[i] = pos;
            deformaciones[i] = math.distance(pos, baseMundos[i]);
            escritura[i] = math.mul(aLocal, new float4(pos, 1f)).xyz;
        }
    }

    [BurstCompile(FloatMode = FloatMode.Fast)]
    struct MaxDeformacionJob : IJob
    {
        [ReadOnly] public NativeArray<float> deformaciones;
        [WriteOnly] public NativeArray<float> resultado;
        public int cantidad;

        public void Execute()
        {
            float maxD = 0f;
            for (int i = 0; i < cantidad; i++)
            {
                float d = deformaciones[i];
                if (d > maxD) maxD = d;
            }
            resultado[0] = maxD;
        }
    }

    static float3 Resolver(
        float3 p,
        float3 normal,
        bool contraCajas,
        NativeArray<Capsula> capsulas,
        int cantidadCapsulas,
        NativeArray<Caja> cajas,
        int cantidadCajas,
        float separacion)
    {
        if (contraCajas)
        {
            for (int caja = 0; caja < cantidadCajas; caja++)
            {
                Caja box = cajas[caja];
                float3 d = p - box.center;
                float3 h = box.extents;
                if (math.abs(d.x) > h.x || math.abs(d.y) > h.y || math.abs(d.z) > h.z) continue;
                float x = h.x - math.abs(d.x);
                float y = h.y - math.abs(d.y);
                float z = h.z - math.abs(d.z);
                if (x < y && x < z) p.x = box.center.x + (d.x >= 0f ? 1f : -1f) * (h.x + separacion);
                else if (y < z) p.y = box.center.y + (d.y >= 0f ? 1f : -1f) * (h.y + separacion);
                else p.z = box.center.z + (d.z >= 0f ? 1f : -1f) * (h.z + separacion);
            }
        }

        for (int paso = 0; paso < 20; paso++)
        {
            bool corregido = false;
            for (int ci = 0; ci < cantidadCapsulas; ci++)
            {
                Capsula c = capsulas[ci];
                if (p.x < c.min.x || p.x > c.max.x || p.y < c.min.y || p.y > c.max.y || p.z < c.min.z || p.z > c.max.z) continue;
                float3 eje = c.eje;
                float t = math.clamp(math.dot(p - c.a, eje) * c.inverso, 0f, 1f);
                float3 centro = c.a + eje * t;
                float3 d = p - centro;
                float r = math.lerp(c.ra, c.rb, t) + separacion;
                float dSq = math.lengthsq(d);
                if (dSq < r * r)
                {
                    float3 dir = dSq > .0000001f ? d * math.rsqrt(dSq) : normal;
                    p = centro + dir * r;
                    corregido = true;
                }
            }
            if (!corregido) break;
        }
        return p;
    }

    public int ContarPenetraciones()
    {
        if (!iniciado || !posiciones.IsCreated) return 0;
        int count = 0;
        for (int i = 0; i < cantidadVertices; i++)
        {
            float3 p = posiciones[i];
            for (int ci = 0; ci < cantidadCapsulas; ci++)
            {
                Capsula c = capsulas[ci];
                if (p.x < c.min.x || p.x > c.max.x || p.y < c.min.y || p.y > c.max.y || p.z < c.min.z || p.z > c.max.z) continue;
                float3 eje = c.eje;
                float t = math.clamp(math.dot(p - c.a, eje) * c.inverso, 0f, 1f);
                float r = math.lerp(c.ra, c.rb, t) - .002f;
                if (math.lengthsq(p - c.a - eje * t) < r * r)
                {
                    count++;
                    break;
                }
            }
        }
        return count;
    }

    void OnDestroy()
    {
        if (lectura.IsCreated) lectura.Dispose();
        if (normalesEntrada.IsCreated) normalesEntrada.Dispose();
        if (escritura.IsCreated) escritura.Dispose();
        if (posiciones.IsCreated) posiciones.Dispose();
        if (posicionesFinal.IsCreated) posicionesFinal.Dispose();
        if (velocidades.IsCreated) velocidades.Dispose();
        if (objetivos.IsCreated) objetivos.Dispose();
        if (normales.IsCreated) normales.Dispose();
        if (baseMundos.IsCreated) baseMundos.Dispose();
        if (libertadNative.IsCreated) libertadNative.Dispose();
        if (deformaciones.IsCreated) deformaciones.Dispose();
        if (resultadoMax.IsCreated) resultadoMax.Dispose();
        if (esCapaNative.IsCreated) esCapaNative.Dispose();
        if (vecinoOffsets.IsCreated) vecinoOffsets.Dispose();
        if (vecinoIndices.IsCreated) vecinoIndices.Dispose();
        if (capsulas.IsCreated) capsulas.Dispose();
        if (cajas.IsCreated) cajas.Dispose();
        if (horneada != null) Destroy(horneada);
        if (deformada != null) Destroy(deformada);
    }
}
