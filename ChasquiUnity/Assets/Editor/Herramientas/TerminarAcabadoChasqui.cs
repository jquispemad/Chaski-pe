using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
[InitializeOnLoad]
public static class TerminarAcabadoChasqui
{
 static TerminarAcabadoChasqui(){EditorApplication.delayCall+=Aplicar;}
 static void Aplicar()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode)return;var p=Object.FindFirstObjectByType<ChasquiMovimiento>();if(p==null||p.GetComponent<VersionPersonajeChasqui>()!=null)return;
  var pelo=p.GetComponent<CabelloDinamicoChasqui>();if(pelo==null||pelo.cabeza.Find("Cabello de coronilla")==null)return;
  var cuerpo=p.GetComponentsInChildren<SkinnedMeshRenderer>().First(r=>r.name=="Chasqui_Malla_Animada");var bake=new Mesh();cuerpo.BakeMesh(bake);var pose=bake.vertices;
  foreach(int sub in new[]{4,9})
  {
   var quedan=new List<int>();var tri=cuerpo.sharedMesh.GetTriangles(sub);
   for(int i=0;i<tri.Length;i+=3){Vector3 c=p.visual.InverseTransformPoint(cuerpo.transform.TransformPoint((pose[tri[i]]+pose[tri[i+1]]+pose[tri[i+2]])/3));if(c.z>.185f&&c.y>1.70f&&c.y<1.79f&&Mathf.Abs(c.x)<.09f)quedan.AddRange(new[]{tri[i],tri[i+1],tri[i+2]});}
   cuerpo.sharedMesh.SetTriangles(quedan,sub);
  }
  Object.DestroyImmediate(bake);EditorUtility.SetDirty(cuerpo.sharedMesh);
  p.gameObject.AddComponent<VersionPersonajeChasqui>();EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);AssetDatabase.SaveAssets();
  var cam=p.camara;var ropa=p.GetComponent<RopaDinamicaChasqui>();var entorno=ropa.entorno;p.camara=null;ropa.entorno=new BoxCollider[0];try{PrefabUtility.SaveAsPrefabAsset(p.gameObject,"Assets/Personajes/Chasqui/ChasquiJugadorRealista.prefab");}finally{p.camara=cam;ropa.entorno=entorno;}
 }
}
