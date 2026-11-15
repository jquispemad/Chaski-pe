using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
[InitializeOnLoad]
public static class CubrirCoronillaChasqui
{
 static CubrirCoronillaChasqui(){EditorApplication.delayCall+=Aplicar;}
 static void Aplicar()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode||SessionState.GetBool("CoronillaV5",false))return;
  var p=Object.FindFirstObjectByType<ChasquiMovimiento>();if(p==null||p.GetComponent<CabelloDinamicoChasqui>()==null)return;
  var head=p.GetComponent<CabelloDinamicoChasqui>().cabeza;if(head.Find("Cabello de coronilla")!=null)return;var r=p.GetComponentsInChildren<SkinnedMeshRenderer>().First(t=>t.name=="Chasqui_Malla_Animada");var bake=new Mesh();r.BakeMesh(bake);
  var pose=bake.vertices.Select(v=>p.visual.InverseTransformPoint(r.transform.TransformPoint(v))).ToArray();var v=new List<Vector3>();var uv=new List<Vector2>();var tri=new List<int>();
  for(int s=0;s<r.sharedMesh.subMeshCount;s++)
  {
   if(s==12||s==14)continue;var indices=r.sharedMesh.GetTriangles(s);
   for(int i=0;i<indices.Length;i+=3)
   {
    int a=indices[i],b=indices[i+1],c=indices[i+2];Vector3 centro=(pose[a]+pose[b]+pose[c])/3;
    if(centro.y<1.805f||centro.y>1.915f||Mathf.Abs(centro.x)>.145f||centro.z<-.08f||centro.z>.20f)continue;
    foreach(int k in new[]{a,b,c})
    {
     Vector3 radial=pose[k]-new Vector3(0,1.755f,.09f);Vector3 punto=pose[k]+radial.normalized*.008f;
     tri.Add(v.Count);v.Add(head.InverseTransformPoint(p.visual.TransformPoint(punto)));uv.Add(new Vector2(Mathf.Atan2(radial.x,radial.z)*2,punto.y*8));
    }
   }
  }
  var mesh=new Mesh{name="Cabello sobre coronilla exacta"};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(tri,0);mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();
  string path="Assets/Personajes/Chasqui/CoronillaCabello.asset";AssetDatabase.CreateAsset(mesh,path);
  var o=new GameObject("Cabello de coronilla");o.transform.SetParent(head,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;
  o.AddComponent<MeshRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Personajes/Chasqui/Materiales/CabelloNatural.mat");Object.DestroyImmediate(bake);
  EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);AssetDatabase.SaveAssets();SessionState.SetBool("CoronillaV5",true);Debug.Log("CORONILLA_CUBIERTA: "+tri.Count/3+" triangulos");
 }
}
