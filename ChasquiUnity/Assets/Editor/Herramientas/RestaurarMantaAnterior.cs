using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
[InitializeOnLoad]
public static class RestaurarMantaAnterior
{
 static RestaurarMantaAnterior(){EditorApplication.delayCall+=Aplicar;}
 static void Aplicar()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode)return;
  var p=Object.FindFirstObjectByType<ChasquiMovimiento>();if(p==null)return;
  var capa=p.GetComponent<CapaFisicaChasqui>();if(capa==null)return;
  var r=capa.tela.GetComponent<SkinnedMeshRenderer>();var ya=p.GetComponentsInChildren<MeshFilter>().Any(f=>f.sharedMesh!=null&&AssetDatabase.GetAssetPath(f.sharedMesh).EndsWith("/AmarrePlanoCuello.asset"));if(ya&&!AssetDatabase.GetAssetPath(r.sharedMesh).EndsWith("/MantaUnaPieza.asset"))return;
  var anterior=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Personajes/Chasqui/LlicllaFisicaExterior.asset");
  var coef=capa.tela.coefficients;capa.tela.enabled=false;r.sharedMesh=anterior;capa.tela.enabled=true;
  var uv=anterior.uv;for(int i=0;i<coef.Length;i++){float t=uv[i].y;coef[i].maxDistance=t<.075f?0:Mathf.Lerp(.025f,1.15f,Mathf.InverseLerp(.075f,1,t));}capa.tela.coefficients=coef;
  var nudo=p.GetComponentsInChildren<Transform>().FirstOrDefault(t=>t.name=="Nudo de tela");
  if(nudo!=null)
  {
   var root=nudo.parent;var f=root.GetComponent<MeshFilter>();if(f==null)f=root.gameObject.AddComponent<MeshFilter>();f.sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Personajes/Chasqui/AmarrePlanoCuello.asset");
   var mr=root.GetComponent<MeshRenderer>();if(mr==null)mr=root.gameObject.AddComponent<MeshRenderer>();mr.sharedMaterial=r.sharedMaterials[0];nudo.position=p.visual.TransformPoint(new Vector3(0,1.446f,.194f));nudo.localScale=new Vector3(.047f,.035f,.028f);
  }
  EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);AssetDatabase.SaveAssets();
  var ropa=p.GetComponent<RopaDinamicaChasqui>();var cam=p.camara;var entorno=ropa.entorno;p.camara=null;ropa.entorno=new BoxCollider[0];
  try{PrefabUtility.SaveAsPrefabAsset(p.gameObject,"Assets/Personajes/Chasqui/ChasquiJugadorRealista.prefab");}finally{p.camara=cam;ropa.entorno=entorno;}
  Debug.Log("MANTA_RESTAURADA: deshecho exclusivamente el ultimo cambio de tela continua.");
 }
}

