using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class AcabarPaisajeNivelCuatro
{
 [InitializeOnLoadMethod]static void Preparar()=>EditorApplication.delayCall+=Aplicar;
 public static void Aplicar()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode||AssetDatabase.LoadAssetAtPath<SceneAsset>(ConstruirPuenteAbismo.Ruta)==null)return;
  var e=SceneManager.GetSceneByPath(ConstruirPuenteAbismo.Ruta);bool cerrar=!e.isLoaded;if(cerrar)e=EditorSceneManager.OpenScene(ConstruirPuenteAbismo.Ruta,OpenSceneMode.Additive);
  if(e.GetRootGameObjects().Any(o=>o.name=="Nivel 4 · paredes de roca y pertenencias")){if(cerrar)EditorSceneManager.CloseScene(e,true);return;}
  var mundo=e.GetRootGameObjects().First(o=>o.name.StartsWith("Nivel 04 ·")).transform;var t=mundo.GetComponentInChildren<Terrain>();var m=e.GetRootGameObjects().Select(o=>o.GetComponent<NivelCuatroChaski>()).First(o=>o!=null);var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Personajes/Chasqui/MaterialPiedra.mat");
  var azar=new System.Random(401);foreach(var f in mundo.GetComponentsInChildren<MeshFilter>().Where(f=>f.name=="Roca asentada · ladera"))
  {var p=f.transform;p.rotation=Quaternion.Euler(0,(float)azar.NextDouble()*360,0);float min=float.PositiveInfinity;foreach(var v in f.sharedMesh.vertices){var q=p.TransformPoint(v);min=Mathf.Min(min,t.SampleHeight(q)+t.transform.position.y);}var b=f.GetComponent<Renderer>().bounds;p.position+=Vector3.up*(min-b.min.y-.16f);}
  for(int tramo=0;tramo<2;tramo++)
  {
   float inicio=tramo==0?74:227,fin=tramo==0?140:378;var v=new List<Vector3>();var uv=new List<Vector2>();var idx=new List<int>();
   for(int i=0;i<=90;i++)for(int j=0;j<13;j++){float z=Mathf.Lerp(inicio,fin,i/90f),u=j/12f,x=NivelCuatroChaski.Centro(z)+(tramo==0?1:-1)*(3+u*20);var p=new Vector3(x,0,z);p.y=t.SampleHeight(p)+t.transform.position.y+.035f;v.Add(p);uv.Add(new Vector2(x/3,z/3));if(i==0||j==12)continue;int a=(i-1)*13+j,b=i*13+j;idx.AddRange(new[]{a,b,a+1,a+1,b,b+1});}
   if(tramo==1)for(int k=0;k<idx.Count;k+=3){int swap=idx[k];idx[k]=idx[k+2];idx[k+2]=swap;}
   var mesh=new Mesh{name="Pared rocosa continua"};mesh.SetVertices(v);mesh.SetTriangles(idx,0);mesh.SetUVs(0,uv);mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,"Assets/Niveles/PuenteAbismo/Pared"+tramo+".asset");var o=new GameObject("Ladera de roca · pared continua");o.transform.SetParent(mundo,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=mat;
  }
  var cuero=AssetDatabase.LoadAssetAtPath<Material>("Assets/Personajes/Chasqui/Materiales/Material_10.mat");var bag=m.hallazgo.position;
  Linea("Correa de chuspa abandonada",mundo,new[]{bag+new Vector3(-.20f,.14f,0),bag+new Vector3(-.23f,.03f,-.30f),bag+new Vector3(.20f,.025f,-.35f),bag+new Vector3(.20f,.14f,0)},.022f,cuero);
  Linea("Tira de sandalia",mundo,new[]{bag+new Vector3(.33f,.07f,.13f),bag+new Vector3(.40f,.15f,.18f),bag+new Vector3(.47f,.07f,.24f)},.022f,cuero);
  var rio=mundo.GetComponentsInChildren<MeshFilter>().First(o=>o.name=="Río · fondo de la quebrada");var vertices=rio.sharedMesh.vertices;var col=t.GetComponent<TerrainCollider>();Physics.SyncTransforms();for(int i=0;i<vertices.Length;i++)if(i%9==0||i%9==8){var q=vertices[i];if(col.Raycast(new Ray(q+Vector3.up*80,Vector3.down),out var hit,160))q.y=Mathf.Min(q.y,hit.point.y+.012f);vertices[i]=q;}rio.sharedMesh.vertices=vertices;rio.sharedMesh.RecalculateNormals();EditorUtility.SetDirty(rio.sharedMesh);
  var marca=new GameObject("Nivel 4 · paredes de roca y pertenencias");SceneManager.MoveGameObjectToScene(marca,e);EditorSceneManager.SaveScene(e);AssetDatabase.SaveAssets();if(cerrar)EditorSceneManager.CloseScene(e,true);
 }
 static void Linea(string n,Transform padre,Vector3[] p,float ancho,Material mat){var o=new GameObject(n);o.transform.SetParent(padre,false);var l=o.AddComponent<LineRenderer>();l.useWorldSpace=true;l.sharedMaterial=mat;l.widthMultiplier=ancho;l.positionCount=p.Length;l.SetPositions(p);}
}
