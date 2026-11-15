using System.IO;
using UnityEngine;
using UnityEditor;
[InitializeOnLoad]
public static class ProbarAgacharse
{
 const string Key="Chasqui.ProbarAgache";static ChasquiMovimiento p;static AgacharseChasqui a;static CharacterController cc;static RopaDinamicaChasqui ropa;static float t,altura,alturaCabeza,bajaCabeza;static int frame,penetraciones;static bool bajo,techoSeguro,arriba,foto;static GameObject techo;static Vector3 inicio;
 static ProbarAgacharse(){EditorApplication.playModeStateChanged+=Cambio;}
 [MenuItem("Chasqui/Comprobar agacharse y techo")]
 public static void Iniciar(){if(EditorApplication.isPlaying)return;SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;}
 static void Cambio(PlayModeStateChange s){if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Key,false)){p=UnityEngine.Object.FindFirstObjectByType<ChasquiMovimiento>();p.enabled=false;a=p.GetComponent<AgacharseChasqui>();a.LeerTeclado=false;cc=p.GetComponent<CharacterController>();ropa=p.GetComponent<RopaDinamicaChasqui>();t=0;frame=-1;penetraciones=0;techo=null;bajo=techoSeguro=arriba=foto=false;altura=cc.height;alturaCabeza=Buscar("head").position.y;EditorApplication.update+=Paso;}if(s==PlayModeStateChange.ExitingPlayMode)EditorApplication.update-=Paso;}
 static Transform Buscar(string n){foreach(var tr in p.GetComponentsInChildren<Transform>())if(tr.name==n)return tr;return p.transform;}
 static void Paso(){if(frame==Time.frameCount)return;frame=Time.frameCount;float dt=Mathf.Min(Time.deltaTime,.0333f);t+=dt;bool pedir=t>.2f&&t<1.0f||t>2.3f;a.Solicitar(pedir,dt);p.AplicarEntrada(t>2.6f?Vector2.up:Vector2.zero,false,dt);if(t>.7f&&t<1.0f){bajo=cc.height<1.51f;bajaCabeza=alturaCabeza-Buscar("head").position.y;if(!foto){Foto();foto=true;}}
 if(t>1&&t<1.1f&&techo==null){techo=GameObject.CreatePrimitive(PrimitiveType.Cube);techo.transform.position=p.transform.position+Vector3.up*1.78f;techo.transform.localScale=new Vector3(3,.2f,3);Physics.SyncTransforms();}
 if(t>1.3f&&t<1.55f)techoSeguro=a.Agachado&&!a.PuedeLevantarse()&&cc.height<1.51f;
 if(t>1.6f&&techo!=null){UnityEngine.Object.DestroyImmediate(techo);techo=null;Physics.SyncTransforms();}
 if(t>2.05f&&t<2.25f)arriba=Mathf.Abs(cc.height-altura)<.01f;
 if(t>2.55f&&t<2.6f)inicio=p.transform.position;
 if(t>.35f)penetraciones=Mathf.Max(penetraciones,ropa.ContarPenetraciones());
 if(t<3.5f)return;float distancia=Vector3.ProjectOnPlane(p.transform.position-inicio,Vector3.up).magnitude;bool velocidad=distancia>.5f&&distancia<1.7f;string resultado=(bajo&&techoSeguro&&arriba&&velocidad&&bajaCabeza>.3f&&penetraciones==0?"CORRECTO":"REVISAR")+"\nCollider bajo: "+bajo+"\nNo se levanta bajo techo: "+techoSeguro+"\nSe levanta al despejarse: "+arriba+"\nMovimiento lento: "+velocidad+" ("+distancia+" m)\nCabeza desciende: "+bajaCabeza+" m\nPenetraciones ropa: "+penetraciones;File.WriteAllText(Path.Combine(Path.GetTempPath(),"chasqui_prueba_agache.txt"),resultado);Debug.Log(resultado);EditorApplication.update-=Paso;SessionState.SetBool(Key,false);EditorApplication.isPlaying=false;}
 static void Foto(){var cam=Camera.main;var pos=cam.transform.position;var rot=cam.transform.rotation;Vector3 centro=p.transform.position+Vector3.up*.8f;cam.transform.position=centro+p.visual.right*2.6f+p.visual.forward*2+Vector3.up*.15f;cam.transform.LookAt(centro);var rt=RenderTexture.GetTemporary(900,900,24);var prev=cam.targetTexture;var active=RenderTexture.active;var tex=new Texture2D(900,900,TextureFormat.RGB24,false);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,900,900),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(Path.GetTempPath(),"chasqui_agachado.png"),tex.EncodeToPNG());cam.targetTexture=prev;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.DestroyImmediate(tex);cam.transform.SetPositionAndRotation(pos,rot);}
}
