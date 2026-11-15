using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
[InitializeOnLoad]
public static class MedirCabezaChasqui
{
 static MedirCabezaChasqui(){EditorApplication.delayCall+=Medir;}
 static void Medir(){if(EditorApplication.isPlayingOrWillChangePlaymode)return;var p=Object.FindFirstObjectByType<ChasquiMovimiento>();if(p==null)return;var r=p.GetComponentsInChildren<SkinnedMeshRenderer>().First(t=>t.name=="Chasqui_Malla_Animada");var b=new Mesh();r.BakeMesh(b);var v=b.vertices;string s="";foreach(int sub in new[]{0,11}){var pts=r.sharedMesh.GetTriangles(sub).Distinct().Select(i=>p.visual.InverseTransformPoint(r.transform.TransformPoint(v[i]))).Where(a=>a.y>1.7f).ToArray();s+=sub+" min="+new Vector3(pts.Min(a=>a.x),pts.Min(a=>a.y),pts.Min(a=>a.z))+" max="+new Vector3(pts.Max(a=>a.x),pts.Max(a=>a.y),pts.Max(a=>a.z))+"\n";}File.WriteAllText("Temp/cabeza_medidas.txt",s);Object.DestroyImmediate(b);}
}
