using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

static class AcortarRecorridoPueblo
{
    const string Ruta = "Assets/Scenes/Campana/Nivel01_PrimeraMision.unity";

    [InitializeOnLoadMethod]
    static void Preparar() => EditorApplication.delayCall += Aplicar;

    static void Aplicar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var escena = SceneManager.GetSceneByPath(Ruta);
        bool cerrar = !escena.isLoaded;
        if (cerrar) escena = EditorSceneManager.OpenScene(Ruta, OpenSceneMode.Additive);
        if (escena.GetRootGameObjects().Any(o => o.name == "Recorrido corto · saltos"))
        {
            if (cerrar) EditorSceneManager.CloseScene(escena,true);
            return;
        }
        var mision = escena.GetRootGameObjects().Select(o => o.GetComponent<PrimeraMisionChaski>()).First(m => m != null);
        var grupo = new GameObject("Recorrido corto · saltos");
        SceneManager.MoveGameObjectToScene(grupo,escena);
        mision.llegada = 185;
        mision.circuloEntrega.position = new Vector3(0,.035f,189);
        var relevista = escena.GetRootGameObjects().First(o => o.name == "Relevista de la posta");
        relevista.transform.position = new Vector3(.88f,.1f,190);
        var pueblo = escena.GetRootGameObjects().First(o => o.name.StartsWith("Pueblo"));
        var casa = pueblo.GetComponentsInChildren<Transform>().First(t => t.name == "Chaskiwasi");
        casa.position += new Vector3(0,0,-59);
        foreach (var t in pueblo.GetComponentsInChildren<Transform>())
        {
            if (t.name.StartsWith("Colisión de piedra") || t.name == "Piedra para saltar" ||
                t.name == "Piedra del trayecto" || t.name == "Piedra de práctica" || t.name == "Escalón de piedra")
                t.gameObject.SetActive(false);
            if (t.name == "Arco diagonal de práctica" && t.position.z > 185 || t.name == "Paso bajo" && t.position.z > 185)
                t.gameObject.SetActive(false);
            if (t.name == "Límite posta") t.position = new Vector3(0,1,196);
        }
        var letreros = escena.GetRootGameObjects().First(o => o.name == "Letreros de madera");
        var cartel = letreros.transform.Find("CHASKIWASI");
        cartel.position = new Vector3(2.1f,0,185);
        var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Personajes/Chasqui/MaterialPiedra.mat");
        float[] tramos = { 26, 38, 46, 54, 64, 89, 97, 105, 130, 138, 146, 168, 175 };
        for (int i=0;i<tramos.Length;i++)
        {
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>($"Assets/Niveles/Valle/Roca{i%4}.asset");
            float alto = .46f + (i%3)*.09f;
            var roca = new GameObject("Piedra de salto apoyada");
            roca.transform.SetParent(grupo.transform,false);
            var escala = new Vector3(1.35f/mesh.bounds.size.x,alto/mesh.bounds.size.y,1.2f/mesh.bounds.size.z);
            roca.transform.localScale = escala;
            roca.transform.position = new Vector3(0,-.025f- mesh.bounds.min.y*escala.y,tramos[i]);
            roca.AddComponent<MeshFilter>().sharedMesh = mesh;
            roca.AddComponent<MeshRenderer>().sharedMaterial = material;
            var col = roca.AddComponent<BoxCollider>();
            col.center = mesh.bounds.center;
            col.size = new Vector3(mesh.bounds.size.x*.90f,mesh.bounds.size.y*.95f,mesh.bounds.size.z*.86f);
        }
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Debug.Log("RECORRIDO_CORTO_LISTO: meta a 189 metros, 13 piedras apoyadas y grupos de saltos.");
        if (cerrar) EditorSceneManager.CloseScene(escena,true);
    }
}
