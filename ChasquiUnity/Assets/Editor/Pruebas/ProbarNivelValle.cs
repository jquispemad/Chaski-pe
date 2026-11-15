using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[InitializeOnLoad]
public static class ProbarNivelValle
{
 const string Key="Chasqui.ProbarValle";
 static NivelValleChasqui n;static ChasquiMovimiento p;static float t,global,ultimoSalto,anteriorZ,atascado;static int frame,etapa;static bool iniciado;static List<string> pasos;
 static ProbarNivelValle(){EditorApplication.playModeStateChanged+=Cambio;}
 [MenuItem("Chasqui/Comprobar nivel 1 completo")]
 public static void Ejecutar(){if(EditorApplication.isPlaying)return;if(UnityEngine.Object.FindFirstObjectByType<NivelValleChasqui>()==null)throw new Exception("Abre Nivel01_Valle");SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;}
 static Vector3 Suelo(float z){float x=ConstruirNivelValle.Ruta(z);return new Vector3(x,n.terreno.SampleHeight(new Vector3(x,0,z))+n.terreno.transform.position.y+.06f,z);}
 static void Cambio(PlayModeStateChange s){if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Key,false)){n=UnityEngine.Object.FindFirstObjectByType<NivelValleChasqui>();p=n.jugador;p.enabled=false;t=global=0;ultimoSalto=-5;etapa=0;frame=-1;iniciado=false;pasos=new List<string>();EditorApplication.update+=Paso;}if(s==PlayModeStateChange.ExitingPlayMode){EditorApplication.update-=Paso;SessionState.SetBool(Key,false);p=null;n=null;}}
 static void Fase(int v,Vector3 pos){etapa=v;t=0;atascado=0;anteriorZ=pos.z;p.Reubicar(pos);}
 static void Paso(){if(p==null||frame==Time.frameCount)return;frame=Time.frameCount;float dt=Mathf.Min(Time.deltaTime,.04f);if(dt<=0)return;t+=dt;global+=dt;if(global>55){Fin("ERROR: tiempo agotado en etapa "+etapa+" posicion="+p.transform.position);return;}
 if(!iniciado){if(t<.25f)return;iniciado=true;int soportes=0;for(float z=20;z<=318;z+=1.5f){float x=ConstruirNivelValle.Ruta(z);RaycastHit hit;if(Physics.Raycast(new Vector3(x,24,z),Vector3.down,out hit,35)&&hit.collider!=null)soportes++;}pasos.Add("Suelo y puente con colision bajo el recorrido: "+soportes+" muestras");Fase(0,Suelo(59));}
 Vector2 entrada=Vector2.zero;bool saltar=false;
 if(etapa==0||etapa==3){float destino=etapa==0?100:185;Vector3 target=new Vector3(ConstruirNivelValle.Ruta(p.transform.position.z+2),0,p.transform.position.z+2);Vector3 d=Vector3.ProjectOnPlane(target-p.transform.position,Vector3.up).normalized;Vector3 f=Vector3.ProjectOnPlane(p.camara.forward,Vector3.up).normalized;Vector3 r=Vector3.Cross(Vector3.up,f);entrada=new Vector2(Vector3.Dot(d,r),Vector3.Dot(d,f));if(t<.25f)entrada=Vector2.zero;
 if(etapa==0&&p.EnSuelo&&global-ultimoSalto>.74f){foreach(float obst in new[]{65f,86f,90f,94f})if(obst-p.transform.position.z>.15f&&obst-p.transform.position.z<1.8f){saltar=true;ultimoSalto=global;break;}}
 if(Mathf.Abs(p.transform.position.z-anteriorZ)<.004f&&t>.4f)atascado+=dt;else atascado=0;anteriorZ=p.transform.position.z;if(atascado>2.5f){Fin("ERROR: bloqueo en etapa "+etapa+" "+p.transform.position);return;}
 if(p.transform.position.z>=destino){pasos.Add(etapa==0?"Tronco y escalones superados con controlador real":"Puente cruzado de extremo a extremo");if(etapa==0)Fase(1,n.controles[0].position+Vector3.up*.1f);else Fase(4,n.recuerdos[0].position-Vector3.up*.85f);entrada=Vector2.zero;}
 }
 else if(etapa==1&&t>.45f){if(n.UltimoControl!=0){Fin("ERROR: punto de control");return;}pasos.Add("Primer punto de control guardado");Fase(2,new Vector3(ConstruirNivelValle.Ruta(164)+9,-.3f,164));}
 else if(etapa==2&&t>.45f){if(Vector3.Distance(p.transform.position,n.controles[0].position)>2){Fin("ERROR: reaparicion en rio");return;}pasos.Add("Caida al rio recupera el punto seguro");Fase(3,Suelo(145));}
 else if(etapa>=4&&etapa<=6&&t>.30f){int i=etapa-4;if(n.Recogidos<i+1){Fin("ERROR: quipu "+i);return;}if(i<2)Fase(etapa+1,n.recuerdos[i+1].position-Vector3.up*.85f);else{pasos.Add("Tres quipus recogidos");Fase(7,n.controles[1].position+Vector3.up*.1f);}}
 else if(etapa==7&&t>.35f){Fase(8,n.controles[2].position+Vector3.up*.1f);}
 else if(etapa==8&&t>.35f){if(n.UltimoControl!=2){Fin("ERROR: faltan puntos seguros");return;}pasos.Add("Tres puntos seguros funcionan");Fase(9,n.llegada.position+Vector3.up*.1f);}
 else if(etapa==9&&t>.5f){if(!n.Completado){Fin("ERROR: la meta no termina el nivel");return;}pasos.Add("Mensaje entregado y nivel completado");Fin("CORRECTO");return;}
 p.AplicarEntrada(entrada,saltar,dt);
 }
 static void Fin(string resultado){EditorApplication.update-=Paso;SessionState.SetBool(Key,false);File.WriteAllText(Path.Combine(Path.GetTempPath(),"chasqui_prueba_valle.txt"),resultado+"\n"+string.Join("\n",pasos));Debug.Log("PRUEBA_VALLE: "+resultado+"\n"+string.Join("\n",pasos));EditorApplication.isPlaying=false;}
}

