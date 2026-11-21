using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class EnlazarMontanaSagrada
{
 [InitializeOnLoadMethod]static void Preparar()=>EditorApplication.delayCall+=Aplicar;
 static void Aplicar()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode||AssetDatabase.LoadAssetAtPath<SceneAsset>(ConstruirMontanaSagrada.Ruta)==null)return;
  var e=SceneManager.GetSceneByPath(ConstruirMontanaSagrada.Ruta);bool cerrar=!e.isLoaded;if(cerrar)e=EditorSceneManager.OpenScene(ConstruirMontanaSagrada.Ruta,OpenSceneMode.Additive);
  if(e.GetRootGameObjects().Any(o=>o.name=="Enlace 4 a 5 · pertenencias")){if(cerrar)EditorSceneManager.CloseScene(e,true);return;}
  var nivel=e.GetRootGameObjects().Select(o=>o.GetComponent<NivelCincoChaski>()).First(o=>o!=null);
  var fuente=EditorSceneManager.OpenScene(ConstruirPuenteAbismo.Ruta,OpenSceneMode.Additive);var anterior=fuente.GetRootGameObjects().Select(o=>o.GetComponent<NivelCuatroChaski>()).First(o=>o!=null);
  var origen=anterior.jugador.transform.position;var inicio=nivel.jugador.transform.position;var offset=inicio-(anterior.hallazgo.position+Vector3.back*1.3f+Vector3.up*.12f);nivel.offsetDesdeCuatro=offset;
  var marca=new GameObject("Enlace 4 a 5 · pertenencias");SceneManager.MoveGameObjectToScene(marca,e);
  var mundo=fuente.GetRootGameObjects().First(o=>o.name.StartsWith("Nivel 04 ·")).transform;
  foreach(Transform o in mundo)if(o.GetComponent<Terrain>()==null&&o.position.z>=350&&o.position.z<=390&&!o.name.Contains("Límite")&&!o.name.Contains("Barrera")){var copia=Object.Instantiate(o.gameObject);SceneManager.MoveGameObjectToScene(copia,e);copia.transform.SetParent(marca.transform);copia.transform.position+=offset;}
  var t=e.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Terrain>()).First();var t4=mundo.GetComponentInChildren<Terrain>();var d=t.terrainData;int n=d.heightmapResolution;var h=d.GetHeights(0,0,n,n);
  for(int z=0;z<n;z++)for(int x=0;x<n;x++){var p=t.transform.position+new Vector3(x*d.size.x/(n-1),0,z*d.size.z/(n-1));if(p.z>35)continue;var q=p-offset;float y=t4.SampleHeight(q)+t4.transform.position.y+offset.y;float w=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(15,35,p.z));h[z,x]=Mathf.Lerp(h[z,x],(y-t.transform.position.y)/d.size.y,w);}d.SetHeights(0,0,h);
  foreach(var filtro in e.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<MeshFilter>()).Where(o=>o.name.StartsWith("Sendero"))){var mesh=filtro.sharedMesh;var vs=mesh.vertices;for(int i=0;i<vs.Length;i++){var p=filtro.transform.TransformPoint(vs[i]);p.y=t.SampleHeight(p)+t.transform.position.y+.09f;vs[i]=filtro.transform.InverseTransformPoint(p);}mesh.vertices=vs;mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);}
  EditorUtility.SetDirty(d);EditorSceneManager.CloseScene(fuente,true);EditorSceneManager.SaveScene(e);AssetDatabase.SaveAssets();if(cerrar)EditorSceneManager.CloseScene(e,true);
 }
}
