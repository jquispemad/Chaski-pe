using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
[InitializeOnLoad]
public static class AcortarCapaChasqui
{
 static AcortarCapaChasqui(){EditorApplication.delayCall+=Aplicar;}
 static void Aplicar()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode)return;var p=Object.FindFirstObjectByType<ChasquiMovimiento>();if(p==null)return;
  var c=p.GetComponent<CapaFisicaChasqui>();if(c==null)return;var r=c.tela.GetComponent<SkinnedMeshRenderer>();
  const string path="Assets/Personajes/Chasqui/CapaCortaFlexible.asset";if(AssetDatabase.GetAssetPath(r.sharedMesh)==path)return;
  var m=Object.Instantiate(r.sharedMesh);var v=m.vertices;for(int i=0;i<v.Length;i++)v[i].y=1.5f+(v[i].y-1.5f)*.85f;m.vertices=v;m.RecalculateNormals();m.RecalculateTangents();m.RecalculateBounds();AssetDatabase.CreateAsset(m,path);
  var coef=c.tela.coefficients;c.tela.enabled=false;r.sharedMesh=m;c.tela.enabled=true;c.tela.coefficients=coef;
  EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);AssetDatabase.SaveAssets();
  var ropa=p.GetComponent<RopaDinamicaChasqui>();var cam=p.camara;var entorno=ropa.entorno;p.camara=null;ropa.entorno=new BoxCollider[0];try{PrefabUtility.SaveAsPrefabAsset(p.gameObject,"Assets/Personajes/Chasqui/ChasquiJugadorRealista.prefab");}finally{p.camara=cam;ropa.entorno=entorno;}
  Debug.Log("CAPA_CORTA: longitud reducida y limite de estiramiento instalado.");
 }
}
