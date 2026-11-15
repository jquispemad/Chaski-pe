using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
[InitializeOnLoad]
public static class AjustarSombrasLana
{
 static AjustarSombrasLana(){EditorApplication.delayCall+=Aplicar;}
 static void Aplicar()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode)return;var p=Object.FindFirstObjectByType<ChasquiMovimiento>();if(p==null||p.GetComponent<CapaFisicaChasqui>()==null)return;
  var capa=p.GetComponent<CapaFisicaChasqui>();var r=capa.tela.GetComponent<SkinnedMeshRenderer>();if(AssetDatabase.GetAssetPath(r.sharedMesh).EndsWith("/LlicllaFisicaExterior.asset")||AssetDatabase.GetAssetPath(r.sharedMesh).EndsWith("/MantaUnaPieza.asset")||AssetDatabase.GetAssetPath(r.sharedMesh).EndsWith("/CapaCortaFlexible.asset"))return;
  var m=Object.Instantiate(r.sharedMesh);m.name="Lliclla · normales exteriores";
  for(int s=0;s<m.subMeshCount;s++){var t=m.GetTriangles(s);for(int i=0;i<t.Length;i+=3){int a=t[i+1];t[i+1]=t[i+2];t[i+2]=a;}m.SetTriangles(t,s);}m.RecalculateNormals();m.RecalculateTangents();
  AssetDatabase.CreateAsset(m,"Assets/Personajes/Chasqui/LlicllaFisicaExterior.asset");var coef=capa.tela.coefficients;capa.tela.enabled=false;r.sharedMesh=m;capa.tela.enabled=true;capa.tela.coefficients=coef;
  r.shadowCastingMode=ShadowCastingMode.Off;
  EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);AssetDatabase.SaveAssets();
  var cam=p.camara;var ropa=p.GetComponent<RopaDinamicaChasqui>();var entorno=ropa.entorno;p.camara=null;ropa.entorno=new BoxCollider[0];try{PrefabUtility.SaveAsPrefabAsset(p.gameObject,"Assets/Personajes/Chasqui/ChasquiJugadorRealista.prefab");}finally{p.camara=cam;ropa.entorno=entorno;}
 }
}

