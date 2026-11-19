using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AsentarRecodoNivelCuatro
{
    [InitializeOnLoadMethod] static void Preparar() => EditorApplication.delayCall += Aplicar;
    static void Aplicar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var escena = SceneManager.GetSceneByPath(ConstruirPuenteAbismo.Ruta);
        bool cerrar = !escena.isLoaded;
        if (cerrar) escena = EditorSceneManager.OpenScene(ConstruirPuenteAbismo.Ruta, OpenSceneMode.Additive);
        var objetos = escena.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<Transform>()).ToArray();
        var terreno = objetos.Select(o => o.GetComponent<Terrain>()).FirstOrDefault(o => o != null);
        if (terreno == null) return;
        int indice = 0;
        foreach (var roca in objetos.Where(o => o.name == "Montaña · recodo del siguiente nivel"))
        {
            // La malla está desplazada respecto al origen: centrar el volumen visible antes de asentarlo.
            var render = roca.GetComponent<MeshRenderer>();
            int lado = indice++ == 0 ? -1 : 1;
            float centro = NivelCuatroChaski.Centro(390) + 31 * .30f + 8 * Mathf.Sin(31 * .055f);
            var destino = new Vector3(centro + lado * 6, 0, 423);
            roca.position += new Vector3(destino.x - render.bounds.center.x, 0, destino.z - render.bounds.center.z);
            float apoyo = terreno.SampleHeight(destino) + terreno.transform.position.y;
            roca.position += Vector3.up * (apoyo - render.bounds.min.y - 1.5f);
        }
        EditorSceneManager.SaveScene(escena);
        System.IO.File.WriteAllText("Temp/nivel04_recodo.txt", "Camino hasta 423 m; rocas del recodo centradas y asentadas en el terreno.\n");
        if (cerrar) EditorSceneManager.CloseScene(escena, true);
    }
}
