using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

static class MontonesPiedraChaski
{
    [InitializeOnLoadMethod]
    static void Preparar() => EditorApplication.delayCall += Aplicar;

    static void Aplicar()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        const string ruta="Assets/Scenes/Campana/Nivel01_PrimeraMision.unity";
        var escena=SceneManager.GetSceneByPath(ruta);
        bool cerrar=!escena.isLoaded;
        if(cerrar)escena=EditorSceneManager.OpenScene(ruta,OpenSceneMode.Additive);
        if(escena.GetRootGameObjects().Any(o=>o.name=="Montones escalonados"))
        {
            if(cerrar)EditorSceneManager.CloseScene(escena,true);
            return;
        }
        var anterior=escena.GetRootGameObjects().FirstOrDefault(o=>o.name=="Recorrido corto · saltos");
        if(anterior!=null)anterior.SetActive(false);
        var grupo=new GameObject("Montones escalonados").transform;
        SceneManager.MoveGameObjectToScene(grupo.gameObject,escena);
        var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Personajes/Chasqui/MaterialPiedra.mat");
        float[] alturas={.32f,.70f,1.08f,.70f,.32f};
        float[] centros={37,61,95,137,172};
        for(int m=0;m<centros.Length;m++)
            for(int p=0;p<alturas.Length;p++)
            {
                float z=centros[m]+(p-2)*1.12f;
                var mesh=AssetDatabase.LoadAssetAtPath<Mesh>($"Assets/Niveles/Valle/Roca{(m+p)%4}.asset");
                var o=new GameObject("Piedra escalonada");
                o.transform.SetParent(grupo,false);
                float alto=alturas[p];
                var escala=new Vector3(1.65f/mesh.bounds.size.x,alto/mesh.bounds.size.y,1.45f/mesh.bounds.size.z);
                o.transform.localScale=escala;
                o.transform.position=new Vector3(0,-.035f-mesh.bounds.min.y*escala.y,z);
                o.AddComponent<MeshFilter>().sharedMesh=mesh;
                o.AddComponent<MeshRenderer>().sharedMaterial=material;
                var colision=new GameObject("Apoyo de salto");
                colision.transform.SetParent(grupo,false);
                colision.transform.position=new Vector3(0,(alto-.035f)*.5f,z);
                colision.AddComponent<BoxCollider>().size=new Vector3(1.4f,alto-.035f,1.18f);
            }
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Debug.Log("MONTONES_CHASKI_LISTOS: cinco montones escalonados, 25 piedras.");
        if(cerrar)EditorSceneManager.CloseScene(escena,true);
    }
}
