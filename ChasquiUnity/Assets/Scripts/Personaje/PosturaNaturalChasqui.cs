using UnityEngine;

[DefaultExecutionOrder(80)]
public sealed class PosturaNaturalChasqui : MonoBehaviour
{
 public ChasquiMovimiento movimiento;public Transform marco,columna,pecho,cabeza;
 Vector3 anterior,velocidadAnterior;Quaternion giroAnterior;float inclinacion,balanceo;
 void Start(){anterior=transform.position;giroAnterior=marco.rotation;}
 void LateUpdate(){if(movimiento==null||marco==null||pecho==null)return;float dt=Mathf.Clamp(Time.deltaTime,.001f,.05f);Vector3 v=(transform.position-anterior)/dt;if(v.magnitude>15){anterior=transform.position;velocidadAnterior=Vector3.zero;giroAnterior=marco.rotation;return;}float rapidez=Vector3.ProjectOnPlane(v,Vector3.up).magnitude;float aceleracion=Vector3.Dot((v-velocidadAnterior)/dt,marco.forward);float curva=Mathf.DeltaAngle(giroAnterior.eulerAngles.y,marco.eulerAngles.y)/dt;float correr=Mathf.InverseLerp(2.5f,4.8f,rapidez);float target=movimiento.EnSuelo?correr*3.5f+Mathf.Clamp(aceleracion*.25f,-3,3):0;inclinacion=Mathf.Lerp(inclinacion,target,1-Mathf.Exp(-dt*10));balanceo=Mathf.Lerp(balanceo,Mathf.Clamp(-curva*.018f,-3.5f,3.5f)*Mathf.Clamp01(rapidez/2.5f),1-Mathf.Exp(-dt*9));float respiracion=Mathf.Sin(Time.time*2.6f)*.3f*Mathf.Clamp01(1-rapidez/2);if(columna!=null)columna.rotation=Quaternion.AngleAxis(inclinacion*.35f,marco.right)*columna.rotation;pecho.rotation=Quaternion.AngleAxis(inclinacion*.65f+respiracion,marco.right)*Quaternion.AngleAxis(balanceo,marco.forward)*pecho.rotation;if(cabeza!=null)cabeza.rotation=Quaternion.AngleAxis(-inclinacion*.45f,marco.right)*cabeza.rotation;anterior=transform.position;velocidadAnterior=v;giroAnterior=marco.rotation;}
}
