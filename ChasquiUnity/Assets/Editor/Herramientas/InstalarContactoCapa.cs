using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
[InitializeOnLoad]
public static class InstalarContactoCapa
{
 static InstalarContactoCapa(){EditorApplication.delayCall+=Aplicar;}
 static void Aplicar()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode)return;var p=Object.FindFirstObjectByType<ChasquiMovimiento>();if(p==null||p.GetComponent<TelaCapaSinCruces>()!=null)return;
  var t=p.gameObject.AddComponent<TelaCapaSinCruces>();t.capa=p.GetComponent<CapaFisicaChasqui>();t.ropa=p.GetComponent<RopaDinamicaChasqui>();
  EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);AssetDatabase.SaveAssets();
  var ropa=t.ropa;var cam=p.camara;var entorno=ropa.entorno;p.camara=null;ropa.entorno=new BoxCollider[0];
  try{PrefabUtility.SaveAsPrefabAsset(p.gameObject,"Assets/Personajes/Chasqui/ChasquiJugadorRealista.prefab");}finally{p.camara=cam;ropa.entorno=entorno;}
  Debug.Log("CONTACTO_CAPA_INSTALADO: simulacion sobre superficie actual de tunica.");
 }
}
