using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(102)]
public sealed class ConsumoChaski:MonoBehaviour
{
    Transform brazo,codo,mano,cabeza,pecho;ChasquiMovimiento movimiento;
    GameObject objeto;float progreso;int tipo;Transform camara;Vector3 camaraAnterior;Quaternion giroAnterior;
    Transform marcoBolso;Vector3 aperturaLocal;Mesh captura;float ladoBrazo;bool cursorVisible;CursorLockMode cursorModo;
    public bool Mostrando=>objeto!=null;
    public void Mostrar(int alimento)
    {
        Ocultar();tipo=alimento;var ts=GetComponentsInChildren<Transform>(true);movimiento=GetComponent<ChasquiMovimiento>();
        cabeza=ts.FirstOrDefault(t=>t.name=="head");pecho=ts.FirstOrDefault(t=>t.name=="chest");marcoBolso=null;
        foreach(var piel in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if(piel.sharedMesh==null||piel.sharedMesh.subMeshCount<=13)continue;var indices=piel.sharedMesh.GetTriangles(13);if(indices.Length==0)continue;
            captura=new Mesh();piel.BakeMesh(captura);var vertices=captura.vertices;var candidatos=indices.Where(i=>piel.transform.TransformPoint(vertices[i]).y<transform.position.y+1.05f).ToArray();if(candidatos.Length==0)candidatos=indices;var limites=new Bounds(vertices[candidatos[0]],Vector3.zero);foreach(var i in candidatos)limites.Encapsulate(vertices[i]);
            marcoBolso=piel.transform;aperturaLocal=new Vector3(limites.center.x,limites.max.y-.025f,limites.center.z);Destroy(captura);captura=null;break;
        }
        Vector3 bolso=PosicionBolso;var derecho=ts.FirstOrDefault(t=>t.name=="upper_arm.R");var izquierdo=ts.FirstOrDefault(t=>t.name=="upper_arm.L");
        string lado=derecho!=null&&izquierdo!=null&&Vector3.Distance(bolso,derecho.position)<Vector3.Distance(bolso,izquierdo.position)?"R":"L";
        brazo=ts.FirstOrDefault(t=>t.name=="upper_arm."+lado);codo=ts.FirstOrDefault(t=>t.name=="forearm."+lado);mano=ts.FirstOrDefault(t=>t.name=="hand."+lado);
        ladoBrazo=brazo!=null&&Vector3.Dot(brazo.position-transform.position,movimiento.visual.right)>=0?1:-1;
        if(mano==null||cabeza==null)return;objeto=AspectoRacionChaski.Crear(tipo,true);objeto.SetActive(false);progreso=0;
        cursorVisible=Cursor.visible;cursorModo=Cursor.lockState;Cursor.visible=false;
        camara=movimiento.camara;if(camara!=null){camaraAnterior=camara.position;giroAnterior=camara.rotation;}
    }
    public void Actualizar(float valor){progreso=Mathf.Clamp01(valor);}
    public Vector3 PosicionBolso=>marcoBolso!=null?marcoBolso.TransformPoint(aperturaLocal):transform.position+Vector3.up*.86f+(movimiento!=null?movimiento.visual.right:transform.right)*.2f;
    void LateUpdate()
    {
        if(objeto==null||movimiento==null||Time.timeScale<=0)return;
        Vector3 frente=movimiento.visual.forward,derecha=movimiento.visual.right;
        Vector3 boca=cabeza.position+frente*.14f-Vector3.up*.055f;
        Vector3 chuspa=PosicionBolso;
        float levantar=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.23f,.48f,progreso));
        float guardar=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.79f,.98f,progreso));
        float peso=Mathf.SmoothStep(0,1,Mathf.InverseLerp(0,.1f,progreso))*(1-guardar*.95f);
        float viaje=levantar*(1-guardar);
        Vector3 destinoBoca=boca-Vector3.up*(tipo==3?.095f:.018f);
        Vector3 exterior=Vector3.Lerp(chuspa,destinoBoca,.5f)+frente*.30f+derecha*ladoBrazo*.16f;
        Vector3 objetivo=(1-viaje)*(1-viaje)*chuspa+2*(1-viaje)*viaje*exterior+viaje*viaje*destinoBoca;
        objetivo+=frente*(Mathf.Sin(progreso*Mathf.PI*10)*.007f*levantar*(1-guardar));
        if(pecho!=null)pecho.rotation=Quaternion.AngleAxis(2.5f*levantar*(1-guardar),derecha)*pecho.rotation;
        Alcanzar(objetivo,peso,frente,derecha*ladoBrazo);
        Cursor.visible=false;
        objeto.SetActive(progreso>.23f&&progreso<.95f);
        objeto.transform.position=mano.position+Vector3.up*.012f+frente*.015f;
        objeto.transform.rotation=Quaternion.LookRotation(frente,Vector3.up)*Quaternion.Euler(tipo==3?-30*levantar*(1-guardar):12,0,0);
        if(tipo!=3&&progreso>.55f)objeto.transform.localScale=Vector3.one*Mathf.Lerp(1,.18f,Mathf.InverseLerp(.55f,.78f,progreso));
        if(cabeza!=null)cabeza.rotation=Quaternion.AngleAxis((tipo==3?-7:2+Mathf.Sin(progreso*30)*1.1f)*levantar*(1-guardar),derecha)*cabeza.rotation;
        if(camara!=null)
        {
            Vector3 centro=Vector3.Lerp((chuspa+cabeza.position)*.5f,cabeza.position-Vector3.up*.18f,levantar*.35f);Vector3 destino=centro+frente*2.05f+derecha*.75f+Vector3.up*.22f;var direccion=destino-centro;
            foreach(var hit in Physics.SphereCastAll(centro,.13f,direccion.normalized,direccion.magnitude,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance))if(!hit.transform.IsChildOf(transform)){destino=centro+direccion.normalized*Mathf.Max(.4f,hit.distance-.16f);break;}
            float suavidad=1-Mathf.Exp(-Time.deltaTime*7);camara.position=Vector3.Lerp(camara.position,destino,suavidad);camara.rotation=Quaternion.Slerp(camara.rotation,Quaternion.LookRotation(centro-camara.position),suavidad);
        }
    }
    void Alcanzar(Vector3 destino,float peso,Vector3 frente,Vector3 exterior)
    {
        if(brazo==null||codo==null||mano==null)return;
        var a=brazo.localRotation;var b=codo.localRotation;Vector3 hombro=brazo.position;
        float l1=Vector3.Distance(hombro,codo.position),l2=Vector3.Distance(codo.position,mano.position);
        Vector3 delta=destino-hombro;if(delta.sqrMagnitude<.00001f||l1<.001f||l2<.001f)return;
        float d=Mathf.Clamp(delta.magnitude,Mathf.Abs(l1-l2)+.015f,(l1+l2)*.96f);Vector3 dir=delta.normalized;
        // El codo se dobla por fuera del torso, usando el marco del personaje, también para el brazo izquierdo.
        Vector3 polo=Vector3.ProjectOnPlane(exterior*.85f+frente*.45f-Vector3.up*.40f,dir).normalized;
        if(polo.sqrMagnitude<.01f)polo=Vector3.Cross(dir,Vector3.up).normalized;
        float cos=Mathf.Clamp((l1*l1+d*d-l2*l2)/(2*l1*d),-1,1);
        Vector3 codoDestino=hombro+dir*l1*cos+polo*l1*Mathf.Sqrt(1-cos*cos);
        brazo.rotation=Quaternion.FromToRotation(codo.position-hombro,codoDestino-hombro)*brazo.rotation;
        codo.rotation=Quaternion.FromToRotation(mano.position-codo.position,hombro+dir*d-codo.position)*codo.rotation;
        var finalA=brazo.localRotation;var finalB=codo.localRotation;
        brazo.localRotation=Quaternion.Slerp(a,finalA,peso);codo.localRotation=Quaternion.Slerp(b,finalB,peso);
    }
    public void Ocultar(){bool activo=objeto!=null;if(activo)Destroy(objeto);objeto=null;if(camara!=null){camara.SetPositionAndRotation(camaraAnterior,giroAnterior);camara=null;}if(activo&&Time.timeScale>0){Cursor.lockState=cursorModo;Cursor.visible=cursorVisible;}}
    void OnDestroy(){Ocultar();}
}
