using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class ConfigurarControlesChasqui
{
    [MenuItem("Chasqui/Activar movimiento y salto")]
    public static void Aplicar()
    {
        var escena = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (escena.path != "Assets/Scenes/Chasqui.unity")
            throw new System.Exception("Abre la escena Chasqui antes de configurar los controles.");
        var movimiento = Object.FindFirstObjectByType<ChasquiMovimiento>();
        if (movimiento == null)
        {
            var modelo = GameObject.Find("Chasqui");
            if (modelo == null) throw new System.Exception("No se encuentra el personaje Chasqui.");
            var renderers = modelo.GetComponentsInChildren<Renderer>();
            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            var jugador = new GameObject("Chasqui Jugador");
            jugador.transform.position = modelo.transform.position;
            var visual = new GameObject("Visual"); visual.transform.SetParent(jugador.transform, false);
            modelo.transform.SetParent(visual.transform, true);
            var cuerpo = jugador.AddComponent<CharacterController>();
            cuerpo.height = bounds.size.y;
            cuerpo.radius = .23f;
            cuerpo.center = new Vector3(0, bounds.center.y - jugador.transform.position.y, 0);
            cuerpo.stepOffset = .28f; cuerpo.slopeLimit = 45f; cuerpo.skinWidth = .025f; cuerpo.minMoveDistance = 0f;
            movimiento = jugador.AddComponent<ChasquiMovimiento>(); movimiento.visual = visual.transform;
        }
        var cam = Camera.main;
        if (cam != null)
        {
            var seguimiento = cam.GetComponent<CamaraChasqui>();
            if (seguimiento == null) seguimiento = cam.gameObject.AddComponent<CamaraChasqui>();
            seguimiento.objetivo = movimiento.transform;
            cam.transform.position = movimiento.transform.position + new Vector3(0f,2.3f,-4.5f);
            cam.transform.LookAt(movimiento.transform.position + Vector3.up * 1.1f);
            cam.fieldOfView = 55f; cam.farClipPlane = 300f;
            movimiento.camara = cam.transform;
        }
        var suelo = GameObject.Find("Suelo");
        if (suelo != null) suelo.transform.localScale = new Vector3(10f,1f,10f);
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Selection.activeGameObject = movimiento.gameObject;
        Debug.Log("CONTROLES_CHASQUI_LISTOS: WASD, flechas y Espacio.");
    }
}
