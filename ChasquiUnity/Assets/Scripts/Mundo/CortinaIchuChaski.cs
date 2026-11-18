using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public sealed class CortinaIchuChaski : MonoBehaviour
{
    Mesh malla;
    Vector3[] reposo, deformados;
    BoxCollider contacto;
    float apertura;
    public Vector3 PuntoDeAgarre => transform.TransformPoint(new Vector3(.5f*(1-apertura),1.05f,0));
    void Awake()
    {
        malla=Instantiate(GetComponent<MeshFilter>().sharedMesh);malla.MarkDynamic();
        GetComponent<MeshFilter>().sharedMesh=malla;reposo=malla.vertices;deformados=new Vector3[reposo.Length];
        contacto=GetComponent<BoxCollider>();
    }
    public void Abrir(float cantidad)
    {
        apertura=Mathf.Clamp01(cantidad);
        if(malla==null)return;
        for(int i=0;i<reposo.Length;i++)
        {
            var v= reposo[i];float bajo=1-Mathf.Clamp01(v.y/2.12f);
            // Upper knots remain attached; the hanging mat gathers to one side and bends inward.
            float recoger=apertura*Mathf.SmoothStep(0,1,Mathf.Clamp01(bajo/.10f));
            v.x=-.82f+(v.x+.82f)*Mathf.Lerp(1,.13f,recoger);
            v.z+=Mathf.Sin(bajo*Mathf.PI)*apertura*.23f;
            v.y+=apertura*bajo*.16f;
            deformados[i]=v;
        }
        malla.vertices=deformados;malla.RecalculateNormals();malla.RecalculateBounds();
        if(contacto!=null)contacto.enabled=apertura<.2f;
    }
    void OnDestroy(){if(malla!=null)Destroy(malla);}
}
