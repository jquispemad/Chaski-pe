using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

static class OrdenarZonaPosta
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
        var pueblo=escena.GetRootGameObjects().First(o=>o.name.StartsWith("Pueblo"));
        if(pueblo.transform.Find("Distribución de viviendas revisada") == null)
        {
            var marca=new GameObject("Distribución de viviendas revisada");
            marca.transform.SetParent(pueblo.transform,false);
            foreach(var t in pueblo.GetComponentsInChildren<Transform>())
            {
                if((t.position.z > 150 && (t.name=="Andén" || t.name=="Cultivo")) ||
                    (t.position.z >= 144 && t.name.StartsWith("Maíz")))
                    t.gameObject.SetActive(false);
            }
            var casas=pueblo.GetComponentsInChildren<Transform>().Where(t=>t.name=="Vivienda" || t.name=="Chaskiwasi").ToArray();
            int vivienda=0;
            foreach(var casa in casas)
            {
                float objetivo;
                if(casa.name=="Chaskiwasi")objetivo=189;
                else { float[] posiciones={8,20,181}; if(vivienda>=posiciones.Length){casa.gameObject.SetActive(false);continue;} objetivo=posiciones[vivienda++]; }
                // House roots originally sit at the origin. Translate their children as a group.
                float centro=casa.GetComponentsInChildren<MeshRenderer>().First(r=>r.name=="Muros").transform.position.z;
                casa.position += Vector3.forward*(objetivo-centro);
            }
            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            Debug.Log("POSTA_ORDENADA: viviendas fuera de montones y final sin andenes.");
        }
        if(cerrar)EditorSceneManager.CloseScene(escena,true);
    }
}
