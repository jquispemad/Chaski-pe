using Unity.Collections;
using UnityEngine;

public sealed class PuenteVientoChaski : MonoBehaviour
{
    public NivelCuatroChaski nivel;
    Mesh mesh;
    Vector3[] originales;
    NativeArray<Vector3> deformados;
    Renderer rend;
    float siguiente;

    void Start()
    {
        var f = GetComponent<MeshFilter>();
        if (f == null || f.sharedMesh == null) { enabled = false; return; }
        mesh = Instantiate(f.sharedMesh);
        mesh.MarkDynamic();
        f.sharedMesh = mesh;
        originales = mesh.vertices;
        deformados = new NativeArray<Vector3>(originales.Length, Allocator.Persistent);
        rend = GetComponent<Renderer>();
    }

    void LateUpdate()
    {
        if (mesh == null || nivel == null || Time.deltaTime <= 0f || Time.time < siguiente) return;
        if (Mathf.Abs(nivel.jugador.transform.position.z - 168f) > 75f) return;
        if (rend != null && !rend.isVisible) return;
        siguiente = Time.time + .033f;

        float fuerza = nivel.FuerzaViento;
        float tBase = Time.time * 1.7f;
        for (int i = 0; i < originales.Length; i++)
        {
            Vector3 p = originales[i];
            float t = Mathf.Clamp01((p.z - 150f) / 36f);
            float altura = p.y - NivelCuatroChaski.PisoPuente(p.z);
            if (altura > .18f) p.x += Mathf.Sin(t * Mathf.PI) * Mathf.Sin(tBase + p.z * .14f) * .025f * fuerza;
            deformados[i] = p;
        }
        mesh.SetVertices(deformados);
    }

    void OnDestroy()
    {
        if (deformados.IsCreated) deformados.Dispose();
        if (mesh != null) Destroy(mesh);
    }
}
