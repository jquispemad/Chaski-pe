using System.Collections;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(100)]
public sealed class TocarPututuChaski:MonoBehaviour
{
    ChasquiMovimiento movimiento;
    Animator animador;
    Transform brazo,codo,mano,cabeza;
    SkinnedMeshRenderer origen;
    Mesh original,oculta,horneada,instrumento;
    GameObject objeto;
    Vector3 cintura;
    Quaternion orientacion;
    float mezcla;
    bool activo;
    public void Iniciar(AudioPrimeraMision audio)
    {
        if(activo||audio.pututu==null)return;
        movimiento=GetComponent<ChasquiMovimiento>();animador=movimiento.animador;
        var huesos=GetComponentsInChildren<Transform>(true);
        brazo=huesos.FirstOrDefault(t=>t.name=="upper_arm.R");codo=huesos.FirstOrDefault(t=>t.name=="forearm.R");mano=huesos.FirstOrDefault(t=>t.name=="hand.R");cabeza=huesos.FirstOrDefault(t=>t.name=="head");
        origen=GetComponentsInChildren<SkinnedMeshRenderer>().FirstOrDefault(r=>r.enabled&&r.sharedMesh!=null&&r.sharedMesh.subMeshCount>14&&r.sharedMesh.GetTriangles(14).Length>0);
        if(origen==null||cabeza==null||brazo==null||codo==null||mano==null){audio.TocarPututu();return;}
        original=origen.sharedMesh;horneada=new Mesh();origen.BakeMesh(horneada);
        var indices=original.GetTriangles(14);var vertices=horneada.vertices;
        var centro=Vector3.zero;foreach(int i in indices)centro+=vertices[i];centro/=indices.Length;
        instrumento=new Mesh();instrumento.vertices=vertices.Select(v=>v-centro).ToArray();instrumento.normals=horneada.normals;instrumento.uv=horneada.uv;instrumento.triangles=indices;instrumento.RecalculateBounds();
        cintura=transform.InverseTransformPoint(origen.transform.TransformPoint(centro));orientacion=origen.transform.rotation;
        objeto=new GameObject("Pututu · instrumento en las manos");objeto.AddComponent<MeshFilter>().sharedMesh=instrumento;objeto.AddComponent<MeshRenderer>().sharedMaterial=origen.sharedMaterials[14];objeto.transform.localScale=origen.transform.lossyScale;
        oculta=Instantiate(original);oculta.SetTriangles(new int[0],14);origen.sharedMesh=oculta;
        activo=true;StartCoroutine(Secuencia(audio));
    }
    IEnumerator Secuencia(AudioPrimeraMision audio)
    {
        movimiento.enabled=false;animador.SetFloat("Movimiento",0);
        for(float t=0;t<1.05f;t+=Time.deltaTime){mezcla=Mathf.SmoothStep(0,1,t/1.05f);yield return null;}
        mezcla=1;audio.TocarPututu();yield return new WaitForSeconds(audio.pututu.length);
        for(float t=0;t<1.05f;t+=Time.deltaTime){mezcla=1-Mathf.SmoothStep(0,1,t/1.05f);yield return null;}
        Limpiar();movimiento.enabled=true;
    }
    void LateUpdate()
    {
        if(!activo||objeto==null)return;
        var boca=cabeza.position+movimiento.visual.forward*.22f-Vector3.up*.065f;
        var p=Vector3.Lerp(transform.TransformPoint(cintura),boca,mezcla);
        objeto.transform.position=p;
        objeto.transform.rotation=Quaternion.Slerp(orientacion,Quaternion.LookRotation(movimiento.visual.forward,Vector3.up)*Quaternion.Euler(0,0,-25),mezcla);
        EntregaInicialChaski.Apuntar(brazo,codo,mano,p,mezcla);
    }
    void Limpiar()
    {
        activo=false;if(origen!=null&&original!=null)origen.sharedMesh=original;
        if(objeto!=null)Destroy(objeto);if(oculta!=null)Destroy(oculta);if(horneada!=null)Destroy(horneada);if(instrumento!=null)Destroy(instrumento);
    }
    void OnDestroy(){Limpiar();}
}
