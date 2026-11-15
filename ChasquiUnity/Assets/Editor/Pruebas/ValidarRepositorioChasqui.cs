using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ValidarRepositorioChasqui
{
    [MenuItem("Chasqui/Validar escena para repositorio")]
    public static void Ejecutar()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Detenga Play antes de validar.");
        var escena=EditorSceneManager.OpenScene("Assets/Scenes/Nivel01_Valle.unity");
        int faltantes=0;
        foreach(var root in escena.GetRootGameObjects())
            foreach(var t in root.GetComponentsInChildren<Transform>(true))
                faltantes+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
        if(faltantes>0)throw new InvalidOperationException("Scripts faltantes en el Valle: "+faltantes);
        var jugador=UnityEngine.Object.FindFirstObjectByType<ChasquiMovimiento>();
        if(jugador==null||jugador.GetComponent<TelaCapaSinCruces>()==null)
            throw new InvalidOperationException("Falta el jugador o la simulacion de capa.");
        Debug.Log("REPOSITORIO_CHASQUI_VALIDADO: escena principal abierta, jugador y capa presentes, cero scripts faltantes.");
    }
}
