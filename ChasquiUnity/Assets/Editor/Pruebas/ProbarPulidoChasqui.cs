using System;
using System.IO;
using UnityEngine;
using UnityEditor;
[InitializeOnLoad]
public static class ProbarPulidoChasqui
{
 const string Key="Chasqui.ProbarPulido";static ChasquiMovimiento p;static RopaDinamicaChasqui ropa;static float t,altura,maxAlt,maxDef;static int frame,penetraciones;static bool pidio,fotoRun,fotoJump,fotoLand,aterrizo;static Vector3 inicio;
 static ProbarPulidoChasqui(){EditorApplication.playModeStateChanged+=Cambio;}
 [MenuItem("Chasqui/Comprobar acabado carrera y salto")]
 public static void Iniciar(){if(EditorApplication.isPlaying)return;SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;}
 static void Cambio(PlayModeStateChange s){if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Key,false)){p=UnityEngine.Object.FindFirstObjectByType<ChasquiMovimiento>();ropa=p.GetComponent<RopaDinamicaChasqui>();p.enabled=false;t=maxDef=0;frame=-1;penetraciones=0;pidio=fotoRun=fotoJump=fotoLand=aterrizo=false;inicio=p.transform.position;altura=maxAlt=inicio.y;EditorApplication.update+=Paso;}if(s==PlayModeStateChange.ExitingPlayMode)EditorApplication.update-=Paso;}
 static void Paso(){if(p==null||frame==Time.frameCount)return;frame=Time.frameCount;float dt=Mathf.Min(Time.deltaTime,.0333f);t+=dt;bool jump=t>1.4f&&!pidio;if(jump){pidio=true;altura=p.transform.position.y;}p.AplicarEntrada(t<1.35f?Vector2.up:Vector2.zero,jump,dt);maxAlt=Mathf.Max(maxAlt,p.transform.position.y);if(t>2.2f&&p.EnSuelo)aterrizo=true;if(ropa!=null&&t>.2f){maxDef=Mathf.Max(maxDef,ropa.UltimaDeformacionMaxima);penetraciones=Mathf.Max(penetraciones,ropa.ContarPenetraciones());}
 if(!fotoRun&&t>.9f){Foto("carrera_mejorada");fotoRun=true;}if(!fotoJump&&t>1.72f){Foto("salto_mejorado");fotoJump=true;}if(!fotoLand&&t>2.3f){Foto("recepcion_mejorada");fotoLand=true;}
 if(t<3)return;bool key=new SerializedObject(p).FindProperty("teclaSalto").intValue==(int)KeyCode.Space;bool shaders=!ShaderUtil.ShaderHasError(Shader.Find("Chasqui/Tejido dos caras"))&&!ShaderUtil.ShaderHasError(Shader.Find("Chasqui/Piel natural"));string result=(maxAlt-altura>.9f&&aterrizo&&penetraciones==0&&key&&shaders?"CORRECTO":"REVISAR")+"\nAltura salto: "+(maxAlt-altura)+"\nAterrizo: "+aterrizo+"\nTecla Espacio: "+key+"\nMateriales sin errores: "+shaders+"\nPenetraciones de ropa: "+penetraciones+"\nDeformacion ropa: "+maxDef;File.WriteAllText(Path.Combine(Path.GetTempPath(),"chasqui_prueba_pulido.txt"),result);Debug.Log(result);EditorApplication.update-=Paso;SessionState.SetBool(Key,false);EditorApplication.isPlaying=false;
 }
 static void Foto(string nombre){var cam=Camera.main;var pos=cam.transform.position;var rot=cam.transform.rotation;float fov=cam.fieldOfView;Vector3 centro=p.transform.position+Vector3.up*1.05f;cam.transform.position=centro+p.visual.right*3.2f+p.visual.forward*1.9f+Vector3.up*.15f;cam.transform.LookAt(centro);cam.fieldOfView=42;var rt=RenderTexture.GetTemporary(900,1100,24);var prev=cam.targetTexture;var active=RenderTexture.active;var tex=new Texture2D(900,1100,TextureFormat.RGB24,false);try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,900,1100),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(Path.GetTempPath(),"chasqui_"+nombre+".png"),tex.EncodeToPNG());}finally{cam.targetTexture=prev;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.DestroyImmediate(tex);cam.transform.SetPositionAndRotation(pos,rot);cam.fieldOfView=fov;}}
}
