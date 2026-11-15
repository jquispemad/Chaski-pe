using UnityEngine;

[DefaultExecutionOrder(108)]
public sealed class CabelloDinamicoChasqui : MonoBehaviour
{
    public Transform cabeza,marco;
    public Transform[] mechones;
    Vector2[] angulos,velocidades;
    Vector3 anterior,vAnterior;
    Quaternion rotAnterior;
    void OnEnable()
    {
        if(cabeza==null)return;
        angulos=new Vector2[mechones.Length];velocidades=new Vector2[mechones.Length];
        anterior=transform.position;vAnterior=Vector3.zero;rotAnterior=cabeza.rotation;
    }
    void LateUpdate()
    {
        if(cabeza==null||angulos==null)return;
        float dt=Mathf.Clamp(Time.deltaTime,.001f,.04f);Vector3 v=(transform.position-anterior)/dt;
        if(v.magnitude>18){anterior=transform.position;vAnterior=Vector3.zero;rotAnterior=cabeza.rotation;return;}
        Vector3 a=Vector3.ClampMagnitude((v-vAnterior)/dt,22);
        Vector3 localV=marco.InverseTransformDirection(v),localA=marco.InverseTransformDirection(a);
        float giro=Mathf.DeltaAngle(rotAnterior.eulerAngles.y,cabeza.eulerAngles.y)/dt;
        for(int i=0;i<mechones.Length;i++)
        {
            float fase=Time.time*(5.5f+i*.13f)+i*1.7f;
            Vector2 destino=new Vector2(Mathf.Clamp(-localV.z*2.4f-localA.z*.32f+Mathf.Sin(fase)*(1+v.magnitude*.25f),-18,18),Mathf.Clamp(-giro*.025f-localA.x*.30f+Mathf.Sin(fase*.7f)*1.1f,-14,14));
            // Damped spring, integrated in small steps to remain stable on slower frames.
            int pasos=Mathf.Max(1,Mathf.CeilToInt(dt/.008f));float h=dt/pasos,frecuencia=20+i*.7f;
            for(int s=0;s<pasos;s++){velocidades[i]+=(destino-angulos[i])*(frecuencia*frecuencia*h);velocidades[i]*=Mathf.Exp(-1.65f*frecuencia*h);angulos[i]+=velocidades[i]*h;}
            mechones[i].localRotation=Quaternion.Euler(angulos[i].x,angulos[i].y,angulos[i].y*.3f);
        }
        anterior=transform.position;vAnterior=Vector3.Lerp(vAnterior,v,1-Mathf.Exp(-dt*15));rotAnterior=cabeza.rotation;
    }
}
