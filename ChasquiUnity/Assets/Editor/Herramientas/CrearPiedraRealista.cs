using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class CrearPiedraRealista
{
    const string Dir = "Assets/Personajes/Chasqui/";

    [MenuItem("Chasqui/Crear piedra realista")]
    public static void Crear()
    {
        var jugador = Object.FindFirstObjectByType<ChasquiMovimiento>();
        Vector3 posicion = jugador != null
            ? new Vector3(-2.7f, 0, 1.8f)
            : Vector3.zero;

        Mesh malla = GenerarMallaRoca(42, 4, .9f);
        string rutaMalla = Dir + "PiedraRealista.asset";
        var anterior = AssetDatabase.LoadAssetAtPath<Mesh>(rutaMalla);
        if (anterior != null) { EditorUtility.CopySerialized(malla, anterior); Object.DestroyImmediate(malla); malla = anterior; }
        else AssetDatabase.CreateAsset(malla, rutaMalla);

        Shader shader = Shader.Find("Chasqui/Piedra realista");
        if (shader == null) { Debug.LogError("No se encontró el shader 'Chasqui/Piedra realista'. Importa PiedraRealista.shader primero."); return; }

        string rutaMat = Dir + "MaterialPiedra.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(rutaMat);
        if (mat == null) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, rutaMat); }
        else mat.shader = shader;

        mat.SetColor("_ColorBase", new Color(.48f, .43f, .38f));
        mat.SetColor("_ColorOscuro", new Color(.22f, .18f, .15f));
        mat.SetColor("_ColorMusgo", new Color(.30f, .38f, .22f));
        mat.SetFloat("_Escala", 7f);
        mat.SetFloat("_Detalle", .65f);
        mat.SetFloat("_Rugosidad", .85f);
        mat.SetFloat("_Metalico", .03f);
        mat.SetFloat("_Musgo", .4f);
        mat.SetFloat("_Relieve", .9f);
        EditorUtility.SetDirty(mat);

        var existente = GameObject.Find("Piedra Realista");
        if (existente != null) Object.DestroyImmediate(existente);

        var piedra = new GameObject("Piedra Realista");
        piedra.transform.position = posicion;
        piedra.transform.localScale = Vector3.one * 1.2f;
        piedra.transform.rotation = Quaternion.Euler(0, 137, 0);

        posicion.y=-malla.bounds.min.y*1.2f-.035f;piedra.transform.position=posicion;
        var filtro = piedra.AddComponent<MeshFilter>();
        filtro.sharedMesh = malla;
        var render = piedra.AddComponent<MeshRenderer>();
        render.sharedMaterial = mat;
        render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        render.receiveShadows = true;

        var collider = piedra.AddComponent<MeshCollider>();
        collider.sharedMesh = malla;
        collider.convex = false;

        Undo.RegisterCreatedObjectUndo(piedra, "Crear piedra realista");
        Selection.activeGameObject = piedra;
        EditorSceneManager.MarkSceneDirty(piedra.scene);
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        Renderizar(piedra);

        Debug.Log("PIEDRA_LISTA: piedra realista procedural colocada al lado de los cubos.");
    }

    static void Renderizar(GameObject piedra)
    {
        var go=new GameObject("Vista temporal piedra");var cam=go.AddComponent<Camera>();var centro=piedra.GetComponent<Renderer>().bounds.center;cam.transform.position=centro+new Vector3(2.8f,1.5f,-3.3f);cam.transform.LookAt(centro);cam.fieldOfView=38;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.13f,.15f,.18f);cam.allowHDR=true;
        var rt=RenderTexture.GetTemporary(1100,850,24);var activa=RenderTexture.active;var tex=new Texture2D(1100,850,TextureFormat.RGB24,false);try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1100,850),0,0);tex.Apply();System.IO.File.WriteAllBytes(System.IO.Path.Combine(System.IO.Path.GetTempPath(),"Chasqui_PiedraMuestra.png"),tex.EncodeToPNG());}finally{RenderTexture.active=activa;RenderTexture.ReleaseTemporary(rt);Object.DestroyImmediate(tex);Object.DestroyImmediate(go);}
    }
    static Mesh GenerarMallaRoca(int semilla, int subdivisiones, float radio)
    {
        var rand = new System.Random(semilla);
        var vertices = new List<Vector3>();
        var triangulos = new List<int>();

        float t = (1f + Mathf.Sqrt(5f)) / 2f;
        var base12 = new Vector3[] {
            new(-1, t,0), new(1, t,0), new(-1,-t,0), new(1,-t,0),
            new(0,-1, t), new(0, 1, t), new(0,-1,-t), new(0, 1,-t),
            new( t,0,-1), new( t,0, 1), new(-t,0,-1), new(-t,0, 1)
        };
        for (int i = 0; i < 12; i++) vertices.Add(base12[i].normalized);

        int[][] caras = {
            new[]{0,11,5}, new[]{0,5,1}, new[]{0,1,7}, new[]{0,7,10}, new[]{0,10,11},
            new[]{1,5,9}, new[]{5,11,4}, new[]{11,10,2}, new[]{10,7,6}, new[]{7,1,8},
            new[]{3,9,4}, new[]{3,4,2}, new[]{3,2,6}, new[]{3,6,8}, new[]{3,8,9},
            new[]{4,9,5}, new[]{2,4,11}, new[]{6,2,10}, new[]{8,6,7}, new[]{9,8,1}
        };
        for (int i = 0; i < 20; i++)
            foreach (int idx in caras[i]) triangulos.Add(idx);

        var cache = new Dictionary<long, int>();
        for (int s = 0; s < subdivisiones; s++)
        {
            var nuevosTri = new List<int>();
            for (int i = 0; i < triangulos.Count; i += 3)
            {
                int a = triangulos[i], b = triangulos[i + 1], c = triangulos[i + 2];
                int ab = PuntoMedio(a, b, vertices, cache);
                int bc = PuntoMedio(b, c, vertices, cache);
                int ca = PuntoMedio(c, a, vertices, cache);
                nuevosTri.AddRange(new[] { a, ab, ca, b, bc, ab, c, ca, bc, ab, bc, ca });
            }
            triangulos = nuevosTri;
        }

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 v = vertices[i];
            float nx = PerlinNoise3D(v.x * 2.3f + 10, v.y * 2.3f, v.z * 2.3f, semilla);
            float ny = PerlinNoise3D(v.x * 4.7f + 20, v.y * 4.7f, v.z * 4.7f, semilla + 1);
            float nz = PerlinNoise3D(v.x * 9.1f + 30, v.y * 9.1f, v.z * 9.1f, semilla + 2);
            float deformacion = nx * .35f + ny * .18f + nz * .08f;

            float aplanar = 1f - Mathf.Abs(v.y) * .25f;
            float escalaY = .7f + Mathf.Abs(v.y) * .1f;
            Vector3 deformado = new Vector3(
                v.x * (1f + deformacion * aplanar) * 1.15f,
                v.y * escalaY * (1f + deformacion * .5f),
                v.z * (1f + deformacion * aplanar)
            );

            float angulo = Mathf.Atan2(v.z, v.x);
            float variacion = (float)(rand.NextDouble() * .06 - .03);
            float grieta = Mathf.Abs(Mathf.Sin(angulo * 5f + v.y * 8f)) < .12f ? -.04f : 0f;
            deformado += v.normalized * (variacion + grieta);

            if (v.y < -.6f)
                deformado.y = Mathf.Max(deformado.y, -.65f);

            vertices[i] = deformado * radio;
        }

        var malla = new Mesh { name = "Piedra procedural" };
        malla.SetVertices(vertices);
        malla.SetTriangles(triangulos, 0);
        malla.RecalculateNormals();
        malla.RecalculateBounds();

        return malla;
    }

    static int PuntoMedio(int a, int b, List<Vector3> verts, Dictionary<long, int> cache)
    {
        long menor = Mathf.Min(a, b), mayor = Mathf.Max(a, b);
        long clave = (menor << 20) + mayor;
        if (cache.TryGetValue(clave, out int idx)) return idx;
        idx = verts.Count;
        verts.Add(((verts[a] + verts[b]) * .5f).normalized);
        cache[clave] = idx;
        return idx;
    }

    static float PerlinNoise3D(float x, float y, float z, int semilla)
    {
        float xy = Mathf.PerlinNoise(x + semilla * .17f, y + semilla * .31f);
        float xz = Mathf.PerlinNoise(x + semilla * .47f, z + semilla * .59f);
        float yz = Mathf.PerlinNoise(y + semilla * .73f, z + semilla * .89f);
        return (xy + xz + yz) / 3f - .5f;
    }
}

