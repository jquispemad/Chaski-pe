using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

static class LimpiarRayasPueblo
{
    const string Ruta = "Assets/Scenes/Campana/Nivel01_PrimeraMision.unity";

    [InitializeOnLoadMethod]
    static void Programar() => EditorApplication.delayCall += Aplicar;

    static void Aplicar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            AssetDatabase.LoadAssetAtPath<SceneAsset>(Ruta) == null) return;
        var escena = SceneManager.GetSceneByPath(Ruta);
        bool cerrar = !escena.isLoaded;
        if (cerrar) escena = EditorSceneManager.OpenScene(Ruta, OpenSceneMode.Additive);
        var paisaje = escena.GetRootGameObjects().FirstOrDefault(o => o.name == "Paisaje andino");
        if (paisaje != null)
        {
            var ichu = paisaje.GetComponentsInChildren<Transform>()
                .Where(t => t.name == "Ichu del sendero").ToArray();
            var paja = AssetDatabase.LoadAssetAtPath<Material>("Assets/Niveles/Valle/TechoIchu.mat");
            var shaderIchu = Shader.Find("Chasqui/Ichu trenzado estable");
            if(paja != null && shaderIchu != null && paja.shader != shaderIchu)
            {
                paja.shader = shaderIchu;
                paja.color = new Color(.68f,.49f,.25f);
                EditorUtility.SetDirty(paja);
                AssetDatabase.SaveAssets();
                Debug.Log("TECHO_ICHU_ESTABLE: trenzado de fibras con filtrado aplicado.");
            }
            bool cambiado = ichu.Length > 0;
            foreach (var planta in ichu) Object.DestroyImmediate(planta.gameObject);
            foreach (var terreno in paisaje.GetComponentsInChildren<Terrain>())
            {
                if (terreno.detailObjectDistance == 0) continue;
                terreno.detailObjectDistance = 0;
                cambiado = true;
            }
            foreach (var techo in escena.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<MeshRenderer>()))
            {
                if (techo.name != "Techo de paja" || paja == null || techo.sharedMaterial == paja) continue;
                techo.sharedMaterial = paja;
                cambiado = true;
            }
            if (cambiado)
            {
                EditorSceneManager.MarkSceneDirty(escena);
                EditorSceneManager.SaveScene(escena);
                Debug.Log($"PUEBLO_SIN_HEBRAS: retirados {ichu.Length} grupos de ichu y aplicada textura de paja.");
            }
        }
        if (cerrar) EditorSceneManager.CloseScene(escena, true);
    }
}
