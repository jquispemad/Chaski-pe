using System;
using System.IO;
using UnityEngine;
using UnityEditor;
[InitializeOnLoad]
public static class ProbarSaltosYRopa
{
 const string Key="Chasqui.PruebaRopa";
 static ChasquiMovimiento p; static RopaDinamicaChasqui ropa;static CharacterController cc;
 static int fases,fotosFases; static float t,maxD;static string detalle;static int frame,penetraciones,stage;static bool cubo,plataforma,jump;static Vector3 spawn;
 static ProbarSaltosYRopa(){EditorApplication.playModeStateChanged+=Cambio;}
 [MenuItem("Chasqui/Comprobar plataforma y ropa")]
 public static void Iniciar(){SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;}
 static void Cambio(PlayModeStateChange s){
 if(s==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key,false)){
 p=UnityEngine.Object.FindFirstObjectByType<ChasquiMovimiento>();ropa=p.GetComponent<RopaDinamicaChasqui>();cc=p.GetComponent<CharacterController>();p.enabled=false;spawn=p.transform.position;t=maxD=0;fases=fotosFases=0;detalle="";frame=-1;stage=0;penetraciones=0;cubo=plataforma=jump=false;Mover(new Vector3(-.9f,spawn.y,.8f));EditorApplication.update+=Paso;
 }if(s==PlayModeStateChange.ExitingPlayMode)EditorApplication.update-=Paso;}
 static void Mover(Vector3 pos){cc.enabled=false;p.transform.position=pos;cc.enabled=true;jump=false;}
 static void Paso(){if(p==null||frame==Time.frameCount)return;frame=Time.frameCount;float dt=Mathf.Min(Time.deltaTime,1f/30f);t+=dt;
 if(ropa!=null&&t>.15f){maxD=Mathf.Max(maxD,ropa.UltimaDeformacionMaxima);penetraciones=Mathf.Max(penetraciones,ropa.ContarPenetraciones());}
 var natural=p.GetComponent<SaltoNaturalChasqui>(); if(natural!=null){int bit=1<<natural.Fase;if((fotosFases&bit)==0&&natural.Fase>0&&natural.TiempoFase>(natural.Fase==1?.035f:natural.Fase==2?.17f:.09f)){FotoLateral("salto_fase_"+natural.Fase);fotosFases|=bit;}fases|=bit;}
 bool salto=t>.5f&&!jump;if(salto)jump=true;
 float destino=stage==0?1.95f:5.8f;
 Vector2 input=t>.5f&&p.transform.position.z<destino?Vector2.up:Vector2.zero;
 if(stage>=2){input=t<1.4f?Vector2.right:Vector2.left;salto=false;}
 p.AplicarEntrada(input,salto,dt);
 if(stage<2&&t>1.65f){float top=stage==0?.4f:.9f;bool ok=p.EnSuelo&&Mathf.Abs(cc.bounds.min.y-top)<.08f;
 detalle+="\nEtapa "+stage+" pies="+cc.bounds.min.y+" posicion="+p.transform.position+" grounded="+p.EnSuelo;if(stage==0){cubo=ok;Foto("cubo");}else{plataforma=ok;Foto("plataforma");}
 stage++;t=0;if(stage==1)Mover(new Vector3(0,spawn.y,4.35f));else Mover(new Vector3(5,spawn.y,0));}
 if(stage==2&&t>2.5f){Foto("ropa");string result=(cubo&&plataforma&&maxD>.005f&&penetraciones==0&&(natural==null||(fases&30)==30&&natural.Aterrizajes>=2)?"CORRECTO":"REVISAR")+"\nCubo: "+cubo+"\nPlataforma: "+plataforma+"\nDeformacion ropa: "+maxD+"\nVertices penetrando volumen corporal: "+penetraciones+"\nVertices tela: "+(ropa!=null?ropa.CantidadVertices:0)+detalle+"\nFases salto: "+fases+" Aterrizajes: "+(natural!=null?natural.Aterrizajes:0);File.WriteAllText(Path.Combine(Path.GetTempPath(),"chasqui_prueba_ropa.txt"),result);Debug.Log(result);EditorApplication.update-=Paso;SessionState.SetBool(Key,false);EditorApplication.isPlaying=false;}
 }
 static void FotoLateral(string n){var cam=Camera.main;var pos=cam.transform.position;var rot=cam.transform.rotation;cam.transform.position=p.transform.position+new Vector3(3,1.25f,-3.5f);cam.transform.LookAt(p.transform.position+Vector3.up);Foto(n);cam.transform.SetPositionAndRotation(pos,rot); }
 static void Foto(string n){var cam=Camera.main;var rt=RenderTexture.GetTemporary(900,900,24);var prev=cam.targetTexture;var active=RenderTexture.active;float a=cam.aspect;var tex=new Texture2D(900,900,TextureFormat.RGB24,false);cam.targetTexture=rt;cam.aspect=1;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,900,900),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(Path.GetTempPath(),"chasqui_"+n+".png"),tex.EncodeToPNG());cam.targetTexture=prev;cam.aspect=a;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.DestroyImmediate(tex);}
}



