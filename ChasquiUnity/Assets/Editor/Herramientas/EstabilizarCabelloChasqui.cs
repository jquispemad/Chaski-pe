using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
[InitializeOnLoad]
public static class EstabilizarCabelloChasqui
{
 static EstabilizarCabelloChasqui(){EditorApplication.delayCall+=Aplicar;}
 static void Aplicar()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode)return;var p=Object.FindFirstObjectByType<ChasquiMovimiento>();if(p==null||p.GetComponentInChildren<CabelloMallaChasqui>()!=null)return;
  var pelo=p.GetComponent<CabelloDinamicoChasqui>();if(pelo==null||pelo.cabeza.Find("Cabello de coronilla")==null)return;
  var anterior=p.visual.Find("Cabello · mechones naturales").GetComponent<SkinnedMeshRenderer>();
  var puntos=new List<Vector3>();var uvs=new List<Vector2>();var pesos=new List<BoneWeight>();var quedan=new List<int>();
  const int A=100,B=20;
  for(int j=0;j<=B;j++)for(int i=0;i<=A;i++){
   float t=j/(float)B,phi=Mathf.Lerp(1.3f,Mathf.PI*2-1.3f,i/(float)A),radio=.13f+.012f*Mathf.Sin(t*Mathf.PI);
   var punto=new Vector3(Mathf.Sin(phi)*radio,1.795f-t*.27f,.085f+Mathf.Cos(phi)*radio-.012f*t);
   puntos.Add(punto);uvs.Add(new Vector2(i/(float)A,t));int grupo=Mathf.Clamp(Mathf.FloorToInt(Mathf.InverseLerp(-.18f,.18f,punto.x)*8),0,7);float f=Mathf.SmoothStep(0,1,t)*.9f;pesos.Add(new BoneWeight{boneIndex0=0,weight0=1-f,boneIndex1=grupo+1,weight1=f});
  }
  for(int j=0;j<B;j++)for(int i=0;i<A;i++){int a=j*(A+1)+i;quedan.AddRange(new[]{a,a+1,a+A+1,a+1,a+A+2,a+A+1});}
  for(int m=0;m<140;m++){
   float phi=Mathf.Lerp(1.25f,Mathf.PI*2-1.25f,m/139f),longitud=.37f+.018f*Mathf.Sin(m*2.3f);int inicio=puntos.Count;
   for(int j=0;j<=18;j++){
    float t=j/18f,radio=.022f+.123f*Mathf.Sin(t*Mathf.PI*.5f);Vector3 centro=new Vector3(Mathf.Sin(phi)*radio,1.895f-t*longitud,.09f+Mathf.Cos(phi)*radio-.012f*t);
    Vector3 lado=new Vector3(Mathf.Cos(phi),0,-Mathf.Sin(phi));float ancho=.011f*Mathf.Lerp(1,.1f,Mathf.Pow(t,7));int grupo=Mathf.Clamp(Mathf.FloorToInt(Mathf.InverseLerp(-.18f,.18f,centro.x)*8),0,7);float f=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.4f,1,t))*.9f;
    for(int k=0;k<2;k++){puntos.Add(centro+lado*ancho*(k==0?-.5f:.5f));uvs.Add(new Vector2(k,t));pesos.Add(new BoneWeight{boneIndex0=0,weight0=1-f,boneIndex1=grupo+1,weight1=f});}
    if(j<18){int a=inicio+j*2;quedan.AddRange(new[]{a,a+2,a+1,a+1,a+2,a+3});}
   }
  }
  var mesh=new Mesh{name="Cabellera con raices estables"};mesh.SetVertices(puntos.Select(v=>pelo.cabeza.InverseTransformPoint(p.visual.TransformPoint(v))).ToList());mesh.SetUVs(0,uvs);mesh.boneWeights=pesos.ToArray();mesh.SetTriangles(quedan,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
  AssetDatabase.CreateAsset(mesh,"Assets/Personajes/Chasqui/CabelloRaicesEstables.asset");
  var o=new GameObject("Cabellera flexible definitiva");o.transform.SetParent(pelo.cabeza,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=anterior.sharedMaterial;
  var mover=o.AddComponent<CabelloMallaChasqui>();mover.movimiento=pelo;mover.mechones=pelo.mechones;anterior.enabled=false;
  EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);AssetDatabase.SaveAssets();
  var cam=p.camara;var ropa=p.GetComponent<RopaDinamicaChasqui>();var entorno=ropa.entorno;p.camara=null;ropa.entorno=new BoxCollider[0];try{PrefabUtility.SaveAsPrefabAsset(p.gameObject,"Assets/Personajes/Chasqui/ChasquiJugadorRealista.prefab");}finally{p.camara=cam;ropa.entorno=entorno;}
 }
}

