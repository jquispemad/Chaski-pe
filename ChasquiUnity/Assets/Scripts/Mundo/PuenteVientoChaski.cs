using UnityEngine;

public sealed class PuenteVientoChaski:MonoBehaviour
{
    public NivelCuatroChaski nivel;
    Mesh mesh;Vector3[] originales,deformados;float siguiente;
    void Start(){var f=GetComponent<MeshFilter>();mesh=Instantiate(f.sharedMesh);f.sharedMesh=mesh;originales=mesh.vertices;deformados=new Vector3[originales.Length];}
    void LateUpdate()
    {
        if(mesh==null||nivel==null||Time.deltaTime<=0||Time.time<siguiente||Mathf.Abs(nivel.jugador.transform.position.z-168)>75)return;
        siguiente=Time.time+.033f;
        for(int i=0;i<originales.Length;i++)
        {
            var p=originales[i];float t=Mathf.Clamp01((p.z-150)/36),altura=p.y-NivelCuatroChaski.PisoPuente(p.z);
            if(altura>.18f)p.x+=Mathf.Sin(t*Mathf.PI)*Mathf.Sin(Time.time*1.7f+p.z*.14f)*.025f*nivel.FuerzaViento;
            deformados[i]=p;
        }
        mesh.vertices=deformados;
    }
    void OnDestroy(){if(mesh!=null)Destroy(mesh);}
}
