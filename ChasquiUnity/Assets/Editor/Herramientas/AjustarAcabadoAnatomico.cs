using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
[InitializeOnLoad]
public static class AjustarAcabadoAnatomico
{
 static AjustarAcabadoAnatomico(){EditorApplication.delayCall+=Aplicar;}
 static void Aplicar()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode||SessionState.GetBool("AcabadoAnatomicoV3",false))return;
  var p=Object.FindFirstObjectByType<ChasquiMovimiento>();if(p==null)return;var cabello=p.visual.Find("Cabello · mechones naturales");if(cabello==null)return;
  if(p.GetComponent<CabelloDinamicoChasqui>().cabeza.Find("Cabello de coronilla")!=null)return;
  var hair=cabello.GetComponent<SkinnedMeshRenderer>().sharedMesh;var v=hair.vertices;
  int cuero=65*19;
  for(int i=0;i<v.Length;i++)
  {
   if(i<cuero){v[i].x*=.87f;v[i].y=1.745f+(v[i].y-1.755f)*.72f;v[i].z=.055f+(v[i].z-.055f)*.87f;}
   else {int j=((i-cuero)%30)/2;v[i].y-=.05f*(1-j/14f);}
  }
  hair.vertices=v;hair.RecalculateNormals();hair.RecalculateBounds();EditorUtility.SetDirty(hair);
  var render=p.GetComponentsInChildren<SkinnedMeshRenderer>().First(r=>r.name=="Chasqui_Malla_Animada");
  var mesh=render.sharedMesh;v=mesh.vertices;var normals=mesh.normals;var ids=mesh.GetTriangles(0).Distinct().ToArray();
  // Weld shading across coincident skin vertices without changing animation weights or seams.
  var sum=new Dictionary<Vector3Int,Vector3>();
  foreach(int i in ids){var k=Vector3Int.RoundToInt(v[i]*10000);sum[k]=sum.TryGetValue(k,out var n)?n+normals[i]:normals[i];}
  foreach(int i in ids)normals[i]=sum[Vector3Int.RoundToInt(v[i]*10000)].normalized;
  mesh.normals=normals;EditorUtility.SetDirty(mesh);
  EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);AssetDatabase.SaveAssets();SessionState.SetBool("AcabadoAnatomicoV3",true);
  var camara=p.camara;var ropa=p.GetComponent<RopaDinamicaChasqui>();var entorno=ropa.entorno;p.camara=null;ropa.entorno=new BoxCollider[0];
  try{PrefabUtility.SaveAsPrefabAsset(p.gameObject,"Assets/Personajes/Chasqui/ChasquiJugadorRealista.prefab");}finally{p.camara=camara;ropa.entorno=entorno;}
 }
}
