using UnityEngine;
public sealed class RamasVientoChaski:MonoBehaviour
{
 Mesh original,copia;Vector3[] vertices,salida;float siguiente;
 void Start(){var f=GetComponent<MeshFilter>();if(f==null||f.sharedMesh==null){enabled=false;return;}original=f.sharedMesh;copia=Instantiate(original);copia.MarkDynamic();f.sharedMesh=copia;vertices=original.vertices;salida=new Vector3[vertices.Length];}
 void Update(){if(copia==null||Time.timeScale<=0||Time.time<siguiente)return;siguiente=Time.time+.05f;float rafaga=Mathf.SmoothStep(0,1,(Mathf.Sin(Time.time*.55f)+.35f)/1.35f);var b=original.bounds;for(int i=0;i<vertices.Length;i++){var v=vertices[i];float peso=Mathf.Clamp01((v.y-b.min.y)/Mathf.Max(.1f,b.size.y));float onda=Mathf.Sin(Time.time*1.7f+transform.position.x*.2f+v.y*1.8f);salida[i]=v+Vector3.right*(onda*(.025f+rafaga*.10f)*peso)+Vector3.forward*(Mathf.Sin(Time.time*2+v.x)*.025f*peso);}copia.vertices=salida;}
 void OnDestroy(){if(copia!=null)Destroy(copia);}
}
