using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(95)]
[RequireComponent(typeof(ChasquiMovimiento))]
public sealed class AgacharseChasqui : MonoBehaviour
{
 public float alturaAgachado=1.5f;
 public float Factor {get;private set;}
 public bool Agachado {get;private set;}
 public bool LeerTeclado=true;
 CharacterController cc;ChasquiMovimiento movimiento;float alturaNormal,baseCapsula;Vector3 centroNormal;
 Transform pelvis,columna,pecho,cuello,pieL,pieR;Transform[] muslos,rodillas,pies;
 readonly Collider[] techo=new Collider[24];
 void Awake(){cc=GetComponent<CharacterController>();movimiento=GetComponent<ChasquiMovimiento>();alturaNormal=cc.height;centroNormal=cc.center;baseCapsula=centroNormal.y-alturaNormal*.5f;var t=GetComponentsInChildren<Transform>(true);pelvis=t.First(x=>x.name=="pelvis");columna=t.First(x=>x.name=="spine");pecho=t.First(x=>x.name=="chest");cuello=t.First(x=>x.name=="neck");muslos=t.Where(x=>x.name.StartsWith("thigh.")).ToArray();rodillas=t.Where(x=>x.name.StartsWith("shin.")).ToArray();pies=t.Where(x=>x.name.StartsWith("foot.")).ToArray();pieL=pies[0];pieR=pies[1];}
 void Update(){if(LeerTeclado)Solicitar(Input.GetKey(KeyCode.C),Time.deltaTime);}
 public bool PuedeLevantarse(){Vector3 baseMundo=transform.TransformPoint(new Vector3(centroNormal.x,baseCapsula,centroNormal.z));float r=cc.radius*.94f;Vector3 a=baseMundo+Vector3.up*(alturaAgachado-r+.05f),b=baseMundo+Vector3.up*(alturaNormal-r);int n=Physics.OverlapCapsuleNonAlloc(a,b,r,techo,~0,QueryTriggerInteraction.Ignore);for(int i=0;i<n;i++)if(techo[i]!=cc&&!techo[i].transform.IsChildOf(transform))return false;return true;}
 public void Solicitar(bool bajar,float dt){if(cc==null)return;Agachado=(bajar&&(movimiento.EnSuelo||Factor>.01f))||(Factor>.05f&&!PuedeLevantarse());Factor=Mathf.MoveTowards(Factor,Agachado?1:0,dt*6);float h=Mathf.Lerp(alturaNormal,alturaAgachado,Factor);if(Mathf.Abs(cc.height-h)>.0001f){cc.height=h;cc.center=new Vector3(centroNormal.x,baseCapsula+h*.5f,centroNormal.z);}}
 void LateUpdate(){if(Factor<.001f||pelvis==null)return;Vector3 derecha=movimiento.visual.right;float y=Mathf.Min(pieL.position.y,pieR.position.y);foreach(var h in muslos)h.rotation=Quaternion.AngleAxis(-48*Factor,derecha)*h.rotation;foreach(var h in rodillas)h.rotation=Quaternion.AngleAxis(96*Factor,derecha)*h.rotation;foreach(var h in pies)h.rotation=Quaternion.AngleAxis(-48*Factor,derecha)*h.rotation;columna.rotation=Quaternion.AngleAxis(18*Factor,derecha)*columna.rotation;pecho.rotation=Quaternion.AngleAxis(10*Factor,derecha)*pecho.rotation;cuello.rotation=Quaternion.AngleAxis(-16*Factor,derecha)*cuello.rotation;pelvis.position+=Vector3.up*(y-Mathf.Min(pieL.position.y,pieR.position.y));}
 void OnDisable(){if(cc!=null){cc.height=alturaNormal;cc.center=centroNormal;}Factor=0;Agachado=false;}
}



