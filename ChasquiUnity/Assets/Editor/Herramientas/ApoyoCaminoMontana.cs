using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class ApoyoCaminoMontana
{
 [InitializeOnLoadMethod]static void Preparar()=>EditorApplication.delayCall+=Aplicar;
 static void Aplicar()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode||AssetDatabase.LoadAssetAtPath<SceneAsset>(ConstruirMontanaSagrada.Ruta)==null)return;
  var e=SceneManager.GetSceneByPath(ConstruirMontanaSagrada.Ruta);bool cerrar=!e.isLoaded;if(cerrar)e=EditorSceneManager.OpenScene(ConstruirMontanaSagrada.Ruta,OpenSceneMode.Additive);
  var t=e.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Terrain>()).First();int total=0;
  foreach(var f in e.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<MeshFilter>()).Where(o=>o.name.StartsWith("Sendero")))
  {
   // El personaje pisa exactamente la misma malla que se dibuja, incluso después de nivelar casas.
   var mesh=f.sharedMesh;var vs=mesh.vertices;
   // Muestrear también entre vértices evita que el relieve interpolado atraviese el sendero.
   for(int i=0;i<vs.Length;i++){var p=f.transform.TransformPoint(vs[i]);float alto=t.SampleHeight(p)+t.transform.position.y;for(int dx=-1;dx<=1;dx++)for(int dz=-1;dz<=1;dz++){var q=p+new Vector3(dx*.25f,0,dz*.25f);alto=Mathf.Max(alto,t.SampleHeight(q)+t.transform.position.y);}p.y=alto+.045f;vs[i]=f.transform.InverseTransformPoint(p);}mesh.vertices=vs;
   if(vs.Length%7==0){var indices=new System.Collections.Generic.List<int>();for(int fila=1;fila<vs.Length/7;fila++)for(int k=0;k<6;k++){int a=(fila-1)*7+k,b=fila*7+k;indices.AddRange(new[]{a,b,a+1,a+1,b,b+1});}mesh.SetTriangles(indices,0);}
   mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
   var c=f.GetComponent<MeshCollider>();if(c==null)c=f.gameObject.AddComponent<MeshCollider>();c.sharedMesh=null;c.sharedMesh=mesh;c.convex=false;total++;
  }
  Physics.SyncTransforms();EditorSceneManager.SaveScene(e);AssetDatabase.SaveAssets();System.IO.File.WriteAllText("Temp/nivel05_camino_apoyo.txt",$"Mallas de camino con colisión coincidente: {total}\n");if(cerrar)EditorSceneManager.CloseScene(e,true);
 }
}
