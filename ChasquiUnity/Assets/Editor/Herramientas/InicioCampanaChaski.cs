using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class InicioCampanaChaski
{
    const string Raiz = "Assets/Scenes";
    const string Primera = Raiz + "/Campana/Nivel01_PrimeraMision.unity";

    static InicioCampanaChaski()
    {
        EditorApplication.delayCall += Configurar;
    }

    static void Configurar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        bool migrar = AssetDatabase.LoadAssetAtPath<SceneAsset>(Raiz + "/Nivel01_PrimeraMision.unity") != null;
        if (migrar && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Mover("Nivel01_PrimeraMision", "Campana");
        Mover("Nivel01_Valle", "Borradores");
        Mover("Chasqui", "Personaje");
        var primera = AssetDatabase.LoadAssetAtPath<SceneAsset>(Primera);
        if (primera == null) return;
        var activa = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var portada=AssetDatabase.LoadAssetAtPath<SceneAsset>(CrearInicioJuegoChaski.Ruta);
        EditorSceneManager.playModeStartScene = portada!=null?portada:primera;
        if (migrar)
        {
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Primera, true) };
            EditorSceneManager.OpenScene(Primera);
            Selection.activeObject = primera;
            Debug.Log("CAMPANA_CHASKI_LISTA: Play inicia en La primera misión.");
        }
    }

    static void Mover(string nombre, string grupo)
    {
        string carpeta = Raiz + "/" + grupo;
        if (!AssetDatabase.IsValidFolder(carpeta)) AssetDatabase.CreateFolder(Raiz, grupo);
        string origen = Raiz + "/" + nombre + ".unity";
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(origen) == null) return;
        string destino = carpeta + "/" + nombre + ".unity";
        string error = AssetDatabase.MoveAsset(origen, destino);
        if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
    }
}
