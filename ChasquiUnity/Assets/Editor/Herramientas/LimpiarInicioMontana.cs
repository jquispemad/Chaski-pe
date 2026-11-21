using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class LimpiarInicioMontana
{
 [InitializeOnLoadMethod]static void Preparar()=>EditorApplication.delayCall+=Aplicar;
 static void Aplicar()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode||AssetDatabase.LoadAssetAtPath<SceneAsset>(ConstruirMontanaSagrada.Ruta)==null)return;
  var e=SceneManager.GetSceneByPath(ConstruirMontanaSagrada.Ruta);bool cerrar=!e.isLoaded;if(cerrar)e=EditorSceneManager.OpenScene(ConstruirMontanaSagrada.Ruta,OpenSceneMode.Additive);
  var m=e.GetRootGameObjects().Select(o=>o.GetComponent<NivelCincoChaski>()).FirstOrDefault(o=>o!=null);if(m==null)return;
  if(m.responsableSalida!=null){Object.DestroyImmediate(m.responsableSalida.gameObject);m.responsableSalida=null;}
  foreach(var root in e.GetRootGameObjects().Where(o=>o.name.StartsWith("Continuidad")&&o.name.Contains("nivel 2")).ToArray())Object.DestroyImmediate(root);
  var mundo=e.GetRootGameObjects().FirstOrDefault(o=>o.name.StartsWith("Nivel 03 · valle"));
  if(mundo!=null)foreach(Transform hijo in mundo.transform.Cast<Transform>().ToArray())
  {
   if(hijo.position.z<40&&hijo.position.z>-35&&(hijo.name.Contains("Casa")||hijo.name.Contains("Vivienda")||hijo.name.Contains("Posta")||hijo.name.Contains("posta")))Object.DestroyImmediate(hijo.gameObject);
  }
  EditorSceneManager.SaveScene(e);if(cerrar)EditorSceneManager.CloseScene(e,true);
 }
}
