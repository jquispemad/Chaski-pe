using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
[InitializeOnLoad]
public static class CorregirCabelleraCompleta
{
 static CorregirCabelleraCompleta(){EditorApplication.delayCall+=Aplicar;}
 static void Aplicar()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode||SessionState.GetBool("CabelleraCompletaV4",false))return;
  var p=UnityEngine.Object.FindFirstObjectByType<ChasquiMovimiento>();if(p==null)return;var obj=p.visual.Find("Cabello · mechones naturales");if(obj==null)return;
  if(p.GetComponent<CabelloDinamicoChasqui>().cabeza.Find("Cabello de coronilla")!=null)return;
  var cuerpo=p.GetComponentsInChildren<SkinnedMeshRenderer>().First(r=>r.name=="Chasqui_Malla_Animada");var bake=new Mesh();cuerpo.BakeMesh(bake);
  var pose=bake.vertices.Select(v=>p.visual.InverseTransformPoint(cuerpo.transform.TransformPoint(v))).ToArray();
  var v=new List<Vector3>();var uv=new List<Vector2>();var pesos=new List<BoneWeight>();var tri=new List<int>();var mapa=new Dictionary<int,int>();
  var originales=cuerpo.sharedMesh.GetTriangles(0);
  // Cover the actual upper skull rather than estimating a dome that can expose skin.
  for(int i=0;i<originales.Length;i+=3)
  {
   int a=originales[i],b=originales[i+1],c=originales[i+2];
   if(Mathf.Min(pose[a].y,Mathf.Min(pose[b].y,pose[c].y))<1.795f)continue;
   foreach(int k in new[]{a,b,c})
   {
    if(!mapa.TryGetValue(k,out int indice))
    {
     indice=v.Count;mapa[k]=indice;Vector3 radial=pose[k]-new Vector3(0,1.755f,.095f);Vector3 punto=pose[k]+radial.normalized*.0035f;
     v.Add(punto);uv.Add(new Vector2(Mathf.Atan2(radial.x,radial.z)/(Mathf.PI*2),punto.y*5));pesos.Add(new BoneWeight{boneIndex0=0,weight0=1});
    }
    tri.Add(indice);
   }
  }
  // Dense continuous underlayer follows the rear and sides down to the nape.
  const int A=100,B=20;int offset=v.Count;
  for(int j=0;j<=B;j++)for(int i=0;i<=A;i++)
  {
   float t=j/(float)B,phi=Mathf.Lerp(.85f,Mathf.PI*2-.85f,i/(float)A);
   float radio=.127f+.012f*Mathf.Sin(t*Mathf.PI);
   Vector3 punto=new Vector3(Mathf.Sin(phi)*radio,1.80f-t*.285f,.085f+Mathf.Cos(phi)*radio-.012f*t);
   punto.z+=Mathf.Sin(phi*9+t*2)*.0025f;
   v.Add(punto);uv.Add(new Vector2(i/(float)A,t));int grupo=Mathf.Clamp(Mathf.FloorToInt(Mathf.InverseLerp(-.18f,.18f,punto.x)*8),0,7);float f=Mathf.SmoothStep(0,1,t)*.9f;pesos.Add(new BoneWeight{boneIndex0=0,weight0=1-f,boneIndex1=grupo+1,weight1=f});
  }
  for(int j=0;j<B;j++)for(int i=0;i<A;i++){int a=offset+j*(A+1)+i;tri.AddRange(new[]{a,a+1,a+A+1,a+1,a+A+2,a+A+1});}
  var azar=new System.Random(918);
  for(int mechon=0;mechon<180;mechon++)
  {
   float phi=Mathf.Lerp(.75f,Mathf.PI*2-.75f,mechon/179f);int inicio=v.Count;float longitud=.37f+(float)azar.NextDouble()*.035f;
   for(int j=0;j<=18;j++)
   {
    float t=j/18f;float radio=.022f+.125f*Mathf.Sin(t*Mathf.PI*.5f);Vector3 centro=new Vector3(Mathf.Sin(phi)*radio,1.896f-t*longitud,.09f+Mathf.Cos(phi)*radio-.012f*t);
    centro.x+=Mathf.Sin(t*8+phi*3)*.002f*t;centro.z+=Mathf.Sin(t*9+phi)*.003f*t;
    Vector3 lado=new Vector3(Mathf.Cos(phi),0,-Mathf.Sin(phi));float ancho=.010f*Mathf.Lerp(1,.08f,Mathf.Pow(t,7));
    int grupo=Mathf.Clamp(Mathf.FloorToInt(Mathf.InverseLerp(-.18f,.18f,centro.x)*8),0,7);float f=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.36f,1,t))*.9f;
    for(int k=0;k<2;k++){v.Add(centro+lado*ancho*(k==0?-.5f:.5f));uv.Add(new Vector2(k,t));pesos.Add(new BoneWeight{boneIndex0=0,weight0=1-f,boneIndex1=grupo+1,weight1=f});}
    if(j<18){int a=inicio+j*2;tri.AddRange(new[]{a,a+2,a+1,a+1,a+2,a+3});}
   }
  }
  var r=obj.GetComponent<SkinnedMeshRenderer>();var mesh=new Mesh{name="Cabellera completa sobre el craneo · 180 mechones"};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(tri,0);mesh.boneWeights=pesos.ToArray();mesh.bindposes=r.bones.Select(b=>b.worldToLocalMatrix*obj.localToWorldMatrix).ToArray();mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();
  EditorUtility.CopySerialized(mesh,r.sharedMesh);UnityEngine.Object.DestroyImmediate(mesh);UnityEngine.Object.DestroyImmediate(bake);EditorUtility.SetDirty(r.sharedMesh);
  EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);AssetDatabase.SaveAssets();SessionState.SetBool("CabelleraCompletaV4",true);
  var cam=p.camara;var ropa=p.GetComponent<RopaDinamicaChasqui>();var entorno=ropa.entorno;p.camara=null;ropa.entorno=new BoxCollider[0];try{PrefabUtility.SaveAsPrefabAsset(p.gameObject,"Assets/Personajes/Chasqui/ChasquiJugadorRealista.prefab");}finally{p.camara=cam;ropa.entorno=entorno;}
  Debug.Log("CABELLERA_COMPLETA_CORREGIDA");
 }
}
