using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
[InitializeOnLoad]
public static class InspeccionarPersonajeRealismo
{
 static InspeccionarPersonajeRealismo(){EditorApplication.delayCall+=Leer;}
 static void Leer(){if(EditorApplication.isPlayingOrWillChangePlaymode)return;var p=Object.FindFirstObjectByType<ChasquiMovimiento>();if(p==null)return;var s=new StringBuilder();
 foreach(var r in p.GetComponentsInChildren<SkinnedMeshRenderer>(true)){s.AppendLine(r.name+" mesh="+r.sharedMesh.name+" vertices="+r.sharedMesh.vertexCount+" escala="+r.transform.lossyScale);for(int i=0;i<r.sharedMaterials.Length;i++)s.AppendLine(i+" "+r.sharedMaterials[i].name+" tris="+r.sharedMesh.GetTriangles(i).Length/3);}
 foreach(var b in p.GetComponentsInChildren<Transform>().Where(t=>new[]{"chest","neck","head","pelvis"}.Contains(t.name)))s.AppendLine(b.name+" pos="+p.visual.InverseTransformPoint(b.position)+" scale="+b.lossyScale);
 var ropa=p.GetComponent<RopaDinamicaChasqui>();var bake=new Mesh();ropa.piel.BakeMesh(bake);var vertices=bake.vertices;var bounds=new Bounds();bool primero=true;for(int i=0;i<vertices.Length;i++)if(ropa.esCapa[i]){var v=p.visual.InverseTransformPoint(ropa.piel.transform.TransformPoint(vertices[i]));if(primero){bounds=new Bounds(v,Vector3.zero);primero=false;}else bounds.Encapsulate(v);}s.AppendLine("Capa bounds "+bounds);Object.DestroyImmediate(bake);File.WriteAllText("Temp/chasqui_inspeccion_realismo.txt",s.ToString());}
}
