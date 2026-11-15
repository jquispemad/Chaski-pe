using System;
using System.IO;
using UnityEngine;
using UnityEditor;
[InitializeOnLoad]
public static class ProbarCamaraRaton
{
 const string Key="Chasqui.PruebaRaton";
 static CamaraChasqui c;static ChasquiMovimiento p;static Vector3 inicio;static float t;static int frame;static bool limites,orientacion,cursor;static GameObject pared;
 static ProbarCamaraRaton(){EditorApplication.playModeStateChanged+=Cambio;}
 [MenuItem("Chasqui/Comprobar camara con raton")]
 public static void Iniciar(){if(EditorApplication.isPlaying)return;SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;}
 static void Cambio(PlayModeStateChange s){if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Key,false)){c=UnityEngine.Object.FindFirstObjectByType<CamaraChasqui>();p=UnityEngine.Object.FindFirstObjectByType<ChasquiMovimiento>();p.enabled=false;t=0;frame=-1;pared=null;limites=orientacion=cursor=false;EditorApplication.update+=Paso;}if(s==PlayModeStateChange.ExitingPlayMode)EditorApplication.update-=Paso;}
 static void Paso(){if(p==null||frame==Time.frameCount)return;frame=Time.frameCount;float dt=Mathf.Min(Time.deltaTime,.04f);t+=dt;if(t<.2f)return;
 if(!limites){c.Mirar(new Vector2(0,10000));bool a=c.Inclinacion>=-8.01f;c.Mirar(new Vector2(0,-10000));bool b=c.Inclinacion<=65.01f;c.Mirar(new Vector2(90f/2.2f,(65-18)/2.2f));limites=a&&b;inicio=p.transform.position;c.CapturarRaton(true);bool locked=c.RatonCapturado;c.CapturarRaton(false);cursor=locked&&!c.RatonCapturado;}
 if(t<1.5f){p.AplicarEntrada(Vector2.up,false,dt);return;}
 if(pared==null){Vector3 delta=p.transform.position-inicio;orientacion=delta.x>1.6f&&delta.x>Mathf.Abs(delta.z);p.AplicarEntrada(Vector2.zero,false,dt);var so=new SerializedObject(c);float altura=so.FindProperty("alturaMirada").floatValue;Vector3 centro=p.transform.position+Vector3.up*altura;Vector3 direccion=Quaternion.Euler(c.Inclinacion,c.Giro,0)*Vector3.back;pared=GameObject.CreatePrimitive(PrimitiveType.Cube);pared.name="Prueba temporal · obstruccion";pared.transform.position=centro+direccion*3;pared.transform.localScale=new Vector3(1,4,4);Physics.SyncTransforms();return;}
 p.AplicarEntrada(Vector2.zero,false,dt);
 if(t<1.9f)return;
 float distancia=Vector3.Distance(c.transform.position,p.transform.position+Vector3.up*1.3f);bool colision=distancia<2.9f;string result=(limites&&orientacion&&cursor&&colision?"CORRECTO":"REVISAR")+"\nLimites verticales: "+limites+"\nW avanza en la direccion del raton: "+orientacion+"\nCapturar y liberar cursor: "+cursor+"\nCamara evita pared: "+colision+" (distancia "+distancia.ToString("F2")+" m)";File.WriteAllText(Path.Combine(Path.GetTempPath(),"chasqui_prueba_raton.txt"),result);Debug.Log(result);EditorApplication.update-=Paso;SessionState.SetBool(Key,false);EditorApplication.isPlaying=false;
 }
}
