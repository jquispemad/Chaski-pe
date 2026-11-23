using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[DefaultExecutionOrder(109)]
public sealed class CabelloMallaChasqui : MonoBehaviour
{
    public CabelloDinamicoChasqui movimiento;
    public Transform[] mechones;

    Mesh mesh;
    int cantidadVertices;
    NativeArray<Vector3> baseVertices, salidaVertices;
    NativeArray<BoneWeight> pesos;
    NativeArray<float3> pivotes;
    NativeArray<quaternion> rotaciones;

    void Awake()
    {
        var filtro = GetComponent<MeshFilter>();
        if (filtro == null || filtro.sharedMesh == null)
        {
            enabled = false;
            return;
        }

        mesh = Instantiate(filtro.sharedMesh);
        mesh.MarkDynamic();
        filtro.sharedMesh = mesh;

        var srcVertices = mesh.vertices;
        var srcPesos = mesh.boneWeights;
        cantidadVertices = srcVertices.Length;

        baseVertices = new NativeArray<Vector3>(srcVertices, Allocator.Persistent);
        salidaVertices = new NativeArray<Vector3>(cantidadVertices, Allocator.Persistent);
        pesos = new NativeArray<BoneWeight>(srcPesos, Allocator.Persistent);

        int count = mechones != null ? mechones.Length : 0;
        pivotes = new NativeArray<float3>(math.max(1, count), Allocator.Persistent);
        rotaciones = new NativeArray<quaternion>(math.max(1, count), Allocator.Persistent);
    }

    void LateUpdate()
    {
        if (mesh == null || cantidadVertices == 0) return;

        int count = mechones != null ? mechones.Length : 0;
        if (count > pivotes.Length)
        {
            if (pivotes.IsCreated) pivotes.Dispose();
            if (rotaciones.IsCreated) rotaciones.Dispose();
            pivotes = new NativeArray<float3>(count, Allocator.Persistent);
            rotaciones = new NativeArray<quaternion>(count, Allocator.Persistent);
        }

        for (int j = 0; j < count; j++)
        {
            var m = mechones[j];
            if (m != null)
            {
                pivotes[j] = m.localPosition;
                rotaciones[j] = m.localRotation;
            }
            else
            {
                pivotes[j] = float3.zero;
                rotaciones[j] = quaternion.identity;
            }
        }

        var job = new CabelloDeformarJob
        {
            baseVertices = baseVertices,
            pesos = pesos,
            pivotes = pivotes,
            rotaciones = rotaciones,
            salida = salidaVertices,
            cantidadMechones = count
        };

        job.Schedule(cantidadVertices, 256).Complete();

        mesh.SetVertices(salidaVertices);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    [BurstCompile(FloatMode = FloatMode.Fast)]
    struct CabelloDeformarJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> baseVertices;
        [ReadOnly] public NativeArray<BoneWeight> pesos;
        [ReadOnly] public NativeArray<float3> pivotes;
        [ReadOnly] public NativeArray<quaternion> rotaciones;
        [WriteOnly] public NativeArray<Vector3> salida;
        public int cantidadMechones;

        public void Execute(int i)
        {
            float3 v = (float3)baseVertices[i];
            if (i < pesos.Length)
            {
                BoneWeight w = pesos[i];
                float f = w.weight1;
                int j = w.boneIndex1 - 1;
                if (f > 0f && j >= 0 && j < cantidadMechones)
                {
                    float3 pivote = pivotes[j];
                    float3 rotado = pivote + math.mul(rotaciones[j], v - pivote);
                    v = math.lerp(v, rotado, f);
                }
            }
            salida[i] = v;
        }
    }

    void OnDestroy()
    {
        if (baseVertices.IsCreated) baseVertices.Dispose();
        if (salidaVertices.IsCreated) salidaVertices.Dispose();
        if (pesos.IsCreated) pesos.Dispose();
        if (pivotes.IsCreated) pivotes.Dispose();
        if (rotaciones.IsCreated) rotaciones.Dispose();
        if (mesh != null) Destroy(mesh);
    }
}
