using Unity.Collections;
using UnityEngine;

public sealed class RamasVientoChaski : MonoBehaviour
{
    Mesh original, copia;
    Vector3[] vertices;
    NativeArray<Vector3> salida;
    Renderer rend;
    float siguiente;
    Bounds limites;

    void Start()
    {
        var f = GetComponent<MeshFilter>();
        if (f == null || f.sharedMesh == null) { enabled = false; return; }
        original = f.sharedMesh;
        copia = Instantiate(original);
        copia.MarkDynamic();
        f.sharedMesh = copia;
        vertices = original.vertices;
        salida = new NativeArray<Vector3>(vertices.Length, Allocator.Persistent);
        limites = original.bounds;
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        if (copia == null || Time.timeScale <= 0f) return;
        float ahora = Time.time;
        if (ahora < siguiente) return;
        siguiente = ahora + .05f;

        if (rend != null && !rend.isVisible) return;
        var cam = Camera.main;
        if (cam != null && (transform.position - cam.transform.position).sqrMagnitude > 3025f) return;

        float rafaga = Mathf.SmoothStep(0f, 1f, (Mathf.Sin(ahora * .55f) + .35f) / 1.35f);
        float invH = 1f / Mathf.Max(.1f, limites.size.y);
        float minY = limites.min.y;
        float phaseBase = ahora * 1.7f + transform.position.x * .2f;
        float ampX = .025f + rafaga * .10f;
        float t2 = ahora * 2f;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i];
            float peso = Mathf.Clamp01((v.y - minY) * invH);
            float onda = Mathf.Sin(phaseBase + v.y * 1.8f);
            v.x += onda * ampX * peso;
            v.z += Mathf.Sin(t2 + v.x) * .025f * peso;
            salida[i] = v;
        }
        copia.SetVertices(salida);
    }

    void OnDestroy()
    {
        if (salida.IsCreated) salida.Dispose();
        if (copia != null) Destroy(copia);
    }
}
