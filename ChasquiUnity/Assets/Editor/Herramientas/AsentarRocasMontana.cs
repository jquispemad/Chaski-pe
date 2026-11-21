using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class AsentarRocasMontana
{
 [InitializeOnLoadMethod]static void Preparar()=>EditorApplication.delayCall+=Aplicar;
 static void Aplicar()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode||AssetDatabase.LoadAssetAtPath<SceneAsset>(ConstruirMontanaSagrada.Ruta)==null)return;
  var e=SceneManager.GetSceneByPath(ConstruirMontanaSagrada.Ruta);bool cerrar=!e.isLoaded;if(cerrar)e=EditorSceneManager.OpenScene(ConstruirMontanaSagrada.Ruta,OpenSceneMode.Additive);
  var t=e.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Terrain>()).First();int cantidad=0;
  foreach(var f in e.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<MeshFilter>()).Where(o=>o.name.ToLowerInvariant().Contains("roca")&&o.sharedMesh!=null))
  {
   var r=f.GetComponent<Renderer>();if(r==null)continue;var b=r.bounds;if(b.size.x>30||b.size.z>30)continue;
   float apoyo=float.PositiveInfinity;
   for(int x=-1;x<=1;x++)for(int z=-1;z<=1;z++){var q=b.center+new Vector3(x*b.extents.x*.65f,0,z*b.extents.z*.65f);apoyo=Mathf.Min(apoyo,t.SampleHeight(q)+t.transform.position.y);}
   f.transform.position+=Vector3.up*(apoyo-r.bounds.min.y-.18f);cantidad++;
  }
  EditorSceneManager.SaveScene(e);System.IO.File.WriteAllText("Temp/nivel05_rocas_asentadas.txt",$"Rocas asentadas: {cantidad}\n");if(cerrar)EditorSceneManager.CloseScene(e,true);
 }
}
