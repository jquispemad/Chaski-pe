using UnityEngine;
[DefaultExecutionOrder(109)]
public sealed class CabelloMallaChasqui : MonoBehaviour
{
 public CabelloDinamicoChasqui movimiento;
 public Transform[] mechones;
 Mesh mesh;Vector3[] baseVertices,vertices;BoneWeight[] pesos;
 void Awake(){var filtro=GetComponent<MeshFilter>();mesh=Instantiate(filtro.sharedMesh);filtro.sharedMesh=mesh;baseVertices=mesh.vertices;vertices=new Vector3[baseVertices.Length];pesos=mesh.boneWeights;}
 void LateUpdate()
 {
  for(int i=0;i<vertices.Length;i++)
  {
   var w=pesos[i];float f=w.weight1;int j=w.boneIndex1-1;Vector3 v=baseVertices[i];
   if(f>0&&j>=0&&j<mechones.Length){var m=mechones[j];Vector3 pivote=m.localPosition;v=Vector3.Lerp(v,pivote+m.localRotation*(v-pivote),f);}
   vertices[i]=v;
  }
  mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();
 }
 void OnDestroy(){if(mesh!=null)Destroy(mesh);}
}
