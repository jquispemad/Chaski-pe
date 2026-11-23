using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

// Simulate after the tunic has finished deforming, so contact follows its
// current surface rather than only the character's rest pose.
[DefaultExecutionOrder(135)]
public sealed class TelaCapaSinCruces : MonoBehaviour
{
    public CapaFisicaChasqui capa;
    public RopaDinamicaChasqui ropa;
    public float separacion = .028f;
    public int Penetraciones { get; private set; }
    public Vector3[] Vertices => locales;

    struct Enlace
    {
        public int a, b;
        public float largo, rigidez;
    }

    SkinnedMeshRenderer origen;
    Mesh horneada, malla;
    Transform marco, salida;
    Vector3[] locales;
    Vector3 ultima;
    Matrix4x4 marcoAnterior;
    bool listo;
    float apertura;
    int cantidadVertices;

    NativeArray<Vector3> verticesCapaNative, verticesTunicaNative, localesNative;
    NativeArray<float3> puntos, antes, objetivos;
    NativeArray<float2> coordenadas;
    NativeArray<float> libre, largoAnclaje, espaldaGrid, contactoGrid;
    NativeArray<int> superficie, penetracionesOut;
    NativeArray<Enlace> enlaces;

    const float Celda = .065f;
    const int GridW = 64;
    const int GridH = 32;
    const int GridOffsetX = 32;

    void Start()
    {
        if (capa == null || ropa == null || capa.tela == null || ropa.salida == null || ropa.salida.sharedMesh == null)
        {
            enabled = false;
            return;
        }

        marco = capa.marco;
        origen = capa.tela.GetComponent<SkinnedMeshRenderer>();
        if (marco == null || origen == null || origen.sharedMesh == null)
        {
            enabled = false;
            return;
        }

        capa.tela.enabled = false;
        origen.enabled = false;
        horneada = new Mesh();
        horneada.MarkDynamic();
        origen.BakeMesh(horneada);

        malla = Instantiate(origen.sharedMesh);
        malla.name = "Capa flexible en contacto con tunica";
        malla.MarkDynamic();

        var o = new GameObject("Capa · tela simulada sin cruces");
        o.transform.SetParent(origen.transform, false);
        salida = o.transform;
        o.AddComponent<MeshFilter>().sharedMesh = malla;
        var r = o.AddComponent<MeshRenderer>();
        r.sharedMaterials = origen.sharedMaterials;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        var uv = malla.uv;
        var v = horneada.vertices;
        int n = v.Length;
        cantidadVertices = n;

        locales = new Vector3[n];
        verticesCapaNative = new NativeArray<Vector3>(n, Allocator.Persistent);
        localesNative = new NativeArray<Vector3>(n, Allocator.Persistent);
        puntos = new NativeArray<float3>(n, Allocator.Persistent);
        antes = new NativeArray<float3>(n, Allocator.Persistent);
        objetivos = new NativeArray<float3>(n, Allocator.Persistent);
        coordenadas = new NativeArray<float2>(n, Allocator.Persistent);
        libre = new NativeArray<float>(n, Allocator.Persistent);
        largoAnclaje = new NativeArray<float>(n, Allocator.Persistent);

        for (int i = 0; i < n; i++)
        {
            v[i].x *= 1.16f;
            float3 p = origen.transform.TransformPoint(v[i]);
            puntos[i] = p;
            antes[i] = p;
            Vector2 coord = i < uv.Length ? uv[i] : Vector2.zero;
            coordenadas[i] = coord;
            libre[i] = coord.y < .075f ? 0f : 1f;
        }

        const int ancho = 33, alto = 43;
        var listaEnlaces = new List<Enlace>(ancho * alto * 5);
        for (int y = 0; y < alto; y++)
        {
            for (int x = 0; x < ancho; x++)
            {
                int a = y * ancho + x;
                if (a >= n) continue;
                if (x + 1 < ancho && a + 1 < n) Agregar(listaEnlaces, a, a + 1, 1f);
                if (y + 1 < alto && a + ancho < n) Agregar(listaEnlaces, a, a + ancho, 1f);
                if (x + 1 < ancho && y + 1 < alto && a + ancho + 1 < n)
                {
                    Agregar(listaEnlaces, a, a + ancho + 1, .65f);
                    Agregar(listaEnlaces, a + 1, a + ancho, .65f);
                }
                if (y + 2 < alto && a + ancho * 2 < n) Agregar(listaEnlaces, a, a + ancho * 2, .35f);
            }
        }

        enlaces = new NativeArray<Enlace>(listaEnlaces.Count, Allocator.Persistent);
        for (int i = 0; i < listaEnlaces.Count; i++) enlaces[i] = listaEnlaces[i];

        var usados = new HashSet<int>(ropa.salida.sharedMesh.triangles);
        superficie = new NativeArray<int>(usados.Count, Allocator.Persistent);
        int idx = 0;
        foreach (int s in usados) superficie[idx++] = s;

        verticesTunicaNative = new NativeArray<Vector3>(ropa.salida.sharedMesh.vertexCount, Allocator.Persistent);

        for (int i = 0; i < n; i++)
        {
            int anclaIdx = i % 33 + 66;
            largoAnclaje[i] = anclaIdx < n ? math.distance(puntos[i], puntos[anclaIdx]) : 0f;
        }

        espaldaGrid = new NativeArray<float>(GridW * GridH, Allocator.Persistent);
        contactoGrid = new NativeArray<float>(GridW * GridH, Allocator.Persistent);
        penetracionesOut = new NativeArray<int>(1, Allocator.Persistent);

        ultima = transform.position;
        marcoAnterior = marco.localToWorldMatrix;
        listo = true;
    }

    void Agregar(List<Enlace> lista, int a, int b, float rigidez)
    {
        lista.Add(new Enlace { a = a, b = b, largo = math.distance(puntos[a], puntos[b]), rigidez = rigidez });
    }

    void LateUpdate()
    {
        if (!listo || ropa.salida == null || ropa.salida.sharedMesh == null) return;

        float dt = Mathf.Clamp(Time.deltaTime, .001f, .0333f);
        origen.BakeMesh(horneada);

        using (var capaData = Mesh.AcquireReadOnlyMeshData(horneada))
        {
            capaData[0].GetVertices(verticesCapaNative);
        }

        Mesh tunicaMesh = ropa.salida.sharedMesh;
        if (!verticesTunicaNative.IsCreated || verticesTunicaNative.Length != tunicaMesh.vertexCount)
        {
            if (verticesTunicaNative.IsCreated) verticesTunicaNative.Dispose();
            verticesTunicaNative = new NativeArray<Vector3>(tunicaMesh.vertexCount, Allocator.Persistent);
        }
        using (var tunicaData = Mesh.AcquireReadOnlyMeshData(tunicaMesh))
        {
            tunicaData[0].GetVertices(verticesTunicaNative);
        }

        Vector3 traslado = transform.position - ultima;
        ultima = transform.position;
        bool teleport = traslado.magnitude > 2f;

        apertura = Mathf.Lerp(apertura, Mathf.Clamp01(capa.Rapidez / 4.8f), 1f - Mathf.Exp(-dt * 3.5f));
        Matrix4x4 actual = marco.localToWorldMatrix;
        Matrix4x4 transporte = actual * marcoAnterior.inverse;
        marcoAnterior = actual;

        var job = new SimularCapaJob
        {
            verticesCapa = verticesCapaNative,
            verticesTunica = verticesTunicaNative,
            superficie = superficie,
            coordenadas = coordenadas,
            libre = libre,
            largoAnclaje = largoAnclaje,
            enlaces = enlaces,
            puntos = puntos,
            antes = antes,
            objetivos = objetivos,
            espaldaGrid = espaldaGrid,
            contactoGrid = contactoGrid,
            localesOut = localesNative,
            penetracionesOut = penetracionesOut,
            aMarcoTunica = marco.worldToLocalMatrix * ropa.salida.transform.localToWorldMatrix,
            origenLocalToWorld = origen.transform.localToWorldMatrix,
            marcoLocalToWorld = actual,
            marcoWorldToLocal = marco.worldToLocalMatrix,
            salidaWorldToLocal = salida.worldToLocalMatrix,
            transporte = transporte,
            externalAccel = capa.tela.externalAcceleration,
            gravity = Physics.gravity,
            marcoUp = marco.up,
            marcoForward = marco.forward,
            marcoRight = marco.right,
            dt = dt,
            tiempo = Time.time,
            apertura = apertura,
            rapidez = capa.Rapidez,
            separacion = separacion,
            teleport = teleport,
            cantidadVertices = cantidadVertices
        };

        job.Run();

        Penetraciones = penetracionesOut[0];
        NativeArray<Vector3>.Copy(localesNative, locales);
        malla.SetVertices(localesNative);
        malla.RecalculateNormals();
        malla.RecalculateBounds();
    }

    [BurstCompile(FloatMode = FloatMode.Fast)]
    struct SimularCapaJob : IJob
    {
        [ReadOnly] public NativeArray<Vector3> verticesCapa, verticesTunica;
        [ReadOnly] public NativeArray<int> superficie;
        [ReadOnly] public NativeArray<float2> coordenadas;
        [ReadOnly] public NativeArray<float> libre, largoAnclaje;
        [ReadOnly] public NativeArray<Enlace> enlaces;

        public NativeArray<float3> puntos, antes, objetivos;
        public NativeArray<float> espaldaGrid, contactoGrid;
        [WriteOnly] public NativeArray<Vector3> localesOut;
        [WriteOnly] public NativeArray<int> penetracionesOut;

        public float4x4 aMarcoTunica, origenLocalToWorld, marcoLocalToWorld, marcoWorldToLocal, salidaWorldToLocal, transporte;
        public float3 externalAccel, gravity, marcoUp, marcoForward, marcoRight;
        public float dt, tiempo, apertura, rapidez, separacion;
        public bool teleport;
        public int cantidadVertices;

        public void Execute()
        {
            // Build flat spatial heightfield of the back of the tunic in marco space (L1-cache friendly).
            int totalCeldas = GridW * GridH;
            for (int c = 0; c < totalCeldas; c++)
            {
                espaldaGrid[c] = float.PositiveInfinity;
                contactoGrid[c] = float.PositiveInfinity;
            }

            int supLen = superficie.Length;
            for (int s = 0; s < supLen; s++)
            {
                int vi = superficie[s];
                float3 q = math.mul(aMarcoTunica, new float4((float3)verticesTunica[vi], 1f)).xyz;
                if (q.y < .40f || q.y > 1.58f) continue;
                int gx = (int)math.round(q.x / Celda) + GridOffsetX;
                int gy = (int)math.round(q.y / Celda);
                if (gx < 0 || gx >= GridW || gy < 0 || gy >= GridH) continue;
                int idx = gy * GridW + gx;
                if (q.z < espaldaGrid[idx]) espaldaGrid[idx] = q.z;
            }

            for (int gy = 0; gy < GridH; gy++)
            {
                int row = gy * GridW;
                for (int gx = 0; gx < GridW; gx++)
                {
                    float z = espaldaGrid[row + gx];
                    if (float.IsPositiveInfinity(z)) continue;
                    float limite = z - separacion;
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int ny = gy + dy;
                        if (ny < 0 || ny >= GridH) continue;
                        int nRow = ny * GridW;
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int nx = gx + dx;
                            if (nx < 0 || nx >= GridW) continue;
                            int nIdx = nRow + nx;
                            if (limite < contactoGrid[nIdx]) contactoGrid[nIdx] = limite;
                        }
                    }
                }
            }

            float carrera = math.saturate(rapidez / 4.8f);
            float expInercia = math.exp(-dt * 2.0f);
            float dt2 = dt * dt;

            for (int i = 0; i < cantidadVertices; i++)
            {
                float3 basePose = (float3)verticesCapa[i];
                basePose.x *= 1.16f;
                float3 obj = math.mul(origenLocalToWorld, new float4(basePose, 1f)).xyz;
                if (libre[i] == 0f)
                {
                    float3 q = math.mul(marcoWorldToLocal, new float4(obj, 1f)).xyz;
                    if (Limite(q, out float zLim))
                    {
                        q.z = math.min(q.z, zLim);
                        obj = math.mul(marcoLocalToWorld, new float4(q, 1f)).xyz;
                    }
                }
                objetivos[i] = obj;

                float3 p = math.mul(transporte, new float4(puntos[i], 1f)).xyz;
                float3 a = math.mul(transporte, new float4(antes[i], 1f)).xyz;
                if (teleport || libre[i] == 0f)
                {
                    puntos[i] = obj;
                    antes[i] = obj;
                    continue;
                }

                float3 diff = p - a;
                float diffSq = math.lengthsq(diff);
                float3 inercia = (diffSq > .09f * .09f ? diff * (.09f * math.rsqrt(diffSq)) : diff) * expInercia;
                antes[i] = p;

                float2 coord = coordenadas[i];
                float caida = coord.y;
                float ancho = coord.x;
                float onda = math.sin(tiempo * 8.3f - caida * 7f + ancho * 2.4f);
                float3 viento = externalAccel
                    + marcoUp * (carrera * (24f + onda * 12f) * caida)
                    - marcoForward * (carrera * (14f + onda * 9f) * caida)
                    + marcoRight * (math.sin(tiempo * 6f - caida * 5f) * carrera * 3f * caida);
                puntos[i] = p + inercia + (gravity + viento) * dt2;
            }

            float reposo = 1f - apertura;
            float recoger = math.lerp(.66f, 1f, apertura);
            float factorX = math.lerp(.075f, .20f, apertura);
            int numEnlaces = enlaces.Length;

            for (int paso = 0; paso < 16; paso++)
            {
                for (int ei = 0; ei < numEnlaces; ei++)
                {
                    Enlace e = enlaces[ei];
                    float3 d = puntos[e.b] - puntos[e.a];
                    float l = math.length(d);
                    float la = libre[e.a];
                    float lb = libre[e.b];
                    float w = la + lb;
                    if (l < .000001f || w == 0f) continue;
                    float rig = l < e.largo ? e.rigidez * .06f : e.rigidez;
                    float3 ajuste = d * ((l - e.largo) / l * rig / w);
                    puntos[e.a] += ajuste * la;
                    puntos[e.b] -= ajuste * lb;
                }

                for (int i = 0; i < cantidadVertices; i++)
                {
                    if (libre[i] == 0f)
                    {
                        puntos[i] = objetivos[i];
                        continue;
                    }

                    float3 p = puntos[i];
                    float3 q = math.mul(marcoWorldToLocal, new float4(p, 1f)).xyz;
                    float3 refLocal = math.mul(marcoWorldToLocal, new float4(objetivos[i], 1f)).xyz;
                    float2 coord = coordenadas[i];
                    float t = coord.y;
                    float desplazar = math.sin(tiempo * 5.2f - t * 4f) * .025f * t * carrera;
                    q.x = math.lerp(q.x, refLocal.x * recoger + desplazar, factorX);
                    float absB = math.abs(coord.x - .5f) * 2f;
                    float borde = absB * absB * absB;
                    if (t < .5f) q.y = math.lerp(q.y, math.min(q.y, refLocal.y - .065f * borde * reposo), .04f);
                    if (i >= 33)
                    {
                        float3 filaAnterior = math.mul(marcoWorldToLocal, new float4(puntos[i - 33], 1f)).xyz;
                        q.y = math.min(q.y, filaAnterior.y - .004f);
                    }
                    p = math.mul(marcoLocalToWorld, new float4(q, 1f)).xyz;

                    float3 ancla = objetivos[i % 33 + 66];
                    float3 d = p - ancla;
                    float maximo = largoAnclaje[i];
                    float dSq = math.lengthsq(d);
                    if (dSq > maximo * maximo)
                    {
                        p = ancla + d * (maximo * math.rsqrt(dSq));
                    }

                    float3 qc = math.mul(marcoWorldToLocal, new float4(p, 1f)).xyz;
                    if (Limite(qc, out float zLim) && qc.z > zLim)
                    {
                        qc.z = zLim;
                        p = math.mul(marcoLocalToWorld, new float4(qc, 1f)).xyz;
                        float3 anteriorLocal = math.mul(marcoWorldToLocal, new float4(antes[i], 1f)).xyz;
                        anteriorLocal.z = math.min(anteriorLocal.z, zLim);
                        antes[i] = math.mul(marcoLocalToWorld, new float4(anteriorLocal, 1f)).xyz;
                    }
                    puntos[i] = p;
                }
            }

            int pen = 0;
            for (int i = 0; i < cantidadVertices; i++)
            {
                float3 p = puntos[i];
                float3 obj = objetivos[i];
                float desviacion = math.lengthsq(p - obj);
                if (float.IsNaN(desviacion) || desviacion > 2.25f)
                {
                    p = obj;
                    puntos[i] = obj;
                    antes[i] = obj;
                }
                if (libre[i] > 0f)
                {
                    float3 q = math.mul(marcoWorldToLocal, new float4(p, 1f)).xyz;
                    if (Limite(q, out float zLim) && q.z > zLim + .002f) pen++;
                }
                localesOut[i] = math.mul(salidaWorldToLocal, new float4(p, 1f)).xyz;
            }
            penetracionesOut[0] = pen;
        }

        bool Limite(float3 q, out float limite)
        {
            int gx = (int)math.round(q.x / Celda) + GridOffsetX;
            int gy = (int)math.round(q.y / Celda);
            if (gx < 0 || gx >= GridW || gy < 0 || gy >= GridH)
            {
                limite = float.PositiveInfinity;
                return false;
            }
            limite = contactoGrid[gy * GridW + gx];
            return !float.IsPositiveInfinity(limite);
        }
    }

    void OnDestroy()
    {
        if (verticesCapaNative.IsCreated) verticesCapaNative.Dispose();
        if (verticesTunicaNative.IsCreated) verticesTunicaNative.Dispose();
        if (localesNative.IsCreated) localesNative.Dispose();
        if (puntos.IsCreated) puntos.Dispose();
        if (antes.IsCreated) antes.Dispose();
        if (objetivos.IsCreated) objetivos.Dispose();
        if (coordenadas.IsCreated) coordenadas.Dispose();
        if (libre.IsCreated) libre.Dispose();
        if (largoAnclaje.IsCreated) largoAnclaje.Dispose();
        if (espaldaGrid.IsCreated) espaldaGrid.Dispose();
        if (contactoGrid.IsCreated) contactoGrid.Dispose();
        if (superficie.IsCreated) superficie.Dispose();
        if (penetracionesOut.IsCreated) penetracionesOut.Dispose();
        if (enlaces.IsCreated) enlaces.Dispose();
        if (horneada != null) Destroy(horneada);
        if (malla != null) Destroy(malla);
    }
}
