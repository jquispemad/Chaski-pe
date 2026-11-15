using UnityEngine;

[DefaultExecutionOrder(110)]
public sealed class CapaFisicaChasqui : MonoBehaviour
{
    public Cloth tela;
    public Transform marco;
    Vector3 anterior,velocidad;
    bool listo;
    public float Rapidez {get;private set;}
    void OnEnable(){anterior=transform.position;velocidad=Vector3.zero;listo=false;}
    void LateUpdate()
    {
        if(tela==null||marco==null)return;
        float dt=Mathf.Max(.001f,Time.deltaTime);
        Vector3 v=(transform.position-anterior)/dt;anterior=transform.position;
        if(!listo||v.magnitude>18)
        {
            tela.ClearTransformMotion();velocidad=Vector3.zero;listo=true;return;
        }
        velocidad=Vector3.Lerp(velocidad,v,1-Mathf.Exp(-dt*10));
        Rapidez=Vector3.ProjectOnPlane(velocidad,Vector3.up).magnitude;
        float rafaga=Mathf.Sin(Time.time*2.3f)*.45f+Mathf.Sin(Time.time*4.7f)*.18f;
        Vector3 arrastre=-Vector3.ProjectOnPlane(velocidad,Vector3.up)*(.45f+Rapidez*.65f);
        tela.externalAcceleration=arrastre+marco.right*rafaga+Vector3.up*(Mathf.Clamp01(Rapidez/4.8f)*2.0f);
        tela.randomAcceleration=new Vector3(.35f,.45f,.35f)*(1+Rapidez*.2f);
    }
}
