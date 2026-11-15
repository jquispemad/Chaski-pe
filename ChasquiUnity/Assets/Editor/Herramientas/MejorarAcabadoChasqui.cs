using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;

public static class MejorarAcabadoChasqui
{
 const string D="Assets/Personajes/Chasqui/";
 static Transform H(Transform t,string n)=>t.GetComponentsInChildren<Transform>(true).First(x=>x.name==n);
 [MenuItem("Chasqui/Mejorar acabado y movimiento del personaje")]
 public static void Aplicar(){
 if(EditorApplication.isPlaying)throw new Exception("Detener Play");var p=UnityEngine.Object.FindFirstObjectByType<ChasquiMovimiento>();
 var colores=new Dictionary<int,Color>{{0,new Color(.43f,.225f,.125f)},{4,new Color(.018f,.014f,.011f)},{6,new Color(.56f,.31f,.075f)},{7,new Color(.34f,.045f,.028f)},{8,new Color(.055f,.045f,.12f)},{10,new Color(.075f,.042f,.022f)},{11,new Color(.63f,.42f,.14f)},{13,new Color(.11f,.20f,.075f)}};
 foreach(string prefix in new[]{"Material_","Tela_"})for(int i=0;i<17;i++){
 var m=AssetDatabase.LoadAssetAtPath<Material>(D+"Materiales/"+prefix+i.ToString("D2")+".mat");if(m==null)continue;
 bool tejido=new[]{6,7,8,11,13}.Contains(i);bool piel=new[]{0,5,16}.Contains(i);
 if(tejido){m.shader=Shader.Find("Chasqui/Tejido dos caras");m.SetFloat("_Fibra",i==11?.55f:.9f);m.SetFloat("_Metallic",0);m.SetFloat("_Glossiness",i==11?.23f:.12f);}
 else if(piel){m.shader=Shader.Find("Chasqui/Piel natural");m.SetFloat("_Glossiness",i==5?.36f:.29f);}
 else {m.SetFloat("_Glossiness",i==1||i==2||i==3?.65f:i==4||i==9?.21f:i==10?.19f:.28f);m.SetFloat("_Metallic",0);}
 if(colores.ContainsKey(i))m.color=colores[i];EditorUtility.SetDirty(m);
 }
 var so=new SerializedObject(p);so.FindProperty("teclaSalto").intValue=(int)KeyCode.Space;so.FindProperty("aceleracion").floatValue=18f;so.FindProperty("giro").floatValue=11f;so.ApplyModifiedPropertiesWithoutUndo();
 var postura=p.GetComponent<PosturaNaturalChasqui>();if(postura==null)postura=p.gameObject.AddComponent<PosturaNaturalChasqui>();postura.movimiento=p;postura.marco=p.visual;postura.columna=H(p.transform,"spine");postura.pecho=H(p.transform,"chest");postura.cabeza=H(p.transform,"neck");EditorUtility.SetDirty(postura);
 MejorarCarrera(p);
 EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveOpenScenes();
 // La iluminacion de relleno pertenece al personaje y mantiene sus materiales legibles al girar.
 var luzT=p.visual.Find("Luz suave del personaje");var luz=luzT!=null?luzT.GetComponent<Light>():null;if(luz==null){var o=new GameObject("Luz suave del personaje");o.transform.SetParent(p.visual,false);luz=o.AddComponent<Light>();}luz.type=LightType.Point;luz.color=new Color(.71f,.80f,1);luz.intensity=.26f;luz.range=3.2f;luz.shadows=LightShadows.None;luz.transform.localPosition=new Vector3(.8f,2.1f,1.4f);
 EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
 Retrato(p,false);Retrato(p,true);
 File.WriteAllText(Path.Combine(Path.GetTempPath(),"chasqui_acabado.txt"),"ACABADO_LISTO: tejido, piel, cuero, pelo, zancada amplia, postura y tecla Espacio.");Debug.Log("ACABADO_CHASQUI_LISTO");
 }
 static void MejorarCarrera(ChasquiMovimiento p){
 var fuente=AssetDatabase.LoadAssetAtPath<AnimationClip>(D+"CorrerAdelante.anim");var idle=AssetDatabase.LoadAllAssetsAtPath(D+"ChasquiAnimado.fbx").OfType<AnimationClip>().First(c=>c.name=="Reposo");
 var modelo=UnityEngine.Object.Instantiate(p.animador.gameObject);modelo.hideFlags=HideFlags.HideAndDontSave;modelo.GetComponent<Animator>().enabled=false;idle.SampleAnimation(modelo,.2f);
 var piernas=modelo.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("thigh.")||t.name.StartsWith("shin.")).ToArray();var baseRot=piernas.Select(t=>t.localRotation).ToArray();
 var clip=UnityEngine.Object.Instantiate(fuente);clip.name="CarreraNatural";var curvas=new AnimationCurve[piernas.Length,4];for(int i=0;i<piernas.Length;i++)for(int k=0;k<4;k++)curvas[i,k]=new AnimationCurve();
 for(int j=0;j<=120;j++){float time=fuente.length*j/120;fuente.SampleAnimation(modelo,time);for(int i=0;i<piernas.Length;i++){float amplitud=piernas[i].name.StartsWith("thigh")?1.48f:1.18f;Quaternion delta=Quaternion.Inverse(baseRot[i])*piernas[i].localRotation;Quaternion q=baseRot[i]*Quaternion.SlerpUnclamped(Quaternion.identity,delta,amplitud);float[] vals={q.x,q.y,q.z,q.w};for(int k=0;k<4;k++)curvas[i,k].AddKey(time,vals[k]);}}
 string[] props={"m_LocalRotation.x","m_LocalRotation.y","m_LocalRotation.z","m_LocalRotation.w"};for(int i=0;i<piernas.Length;i++)for(int k=0;k<4;k++)AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve(AnimationUtility.CalculateTransformPath(piernas[i],modelo.transform),typeof(Transform),props[k]),curvas[i,k]);clip.EnsureQuaternionContinuity();
 string path=D+"CarreraNatural.anim";var old=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);if(old!=null){EditorUtility.CopySerialized(clip,old);UnityEngine.Object.DestroyImmediate(clip);clip=old;}else AssetDatabase.CreateAsset(clip,path);
 float apoyo=MedirApoyo(clip,modelo);float ritmo=Mathf.Clamp(4.8f/Mathf.Max(.8f,Mathf.Abs(apoyo)),1.3f,2.5f);
 var controller=(AnimatorController)p.animador.runtimeAnimatorController;foreach(var s in controller.layers[0].stateMachine.states){var b=s.state.motion as BlendTree;if(b==null)continue;var children=b.children;for(int i=0;i<children.Length;i++)if(children[i].threshold>.9f){children[i].motion=clip;children[i].timeScale=ritmo;}b.children=children;EditorUtility.SetDirty(b);}EditorUtility.SetDirty(controller);EditorUtility.SetDirty(clip);UnityEngine.Object.DestroyImmediate(modelo);File.WriteAllText(Path.Combine(Path.GetTempPath(),"chasqui_carrera_natural.txt"),"Velocidad pie en apoyo="+apoyo+" m/s; ritmo="+ritmo+"; ciclo="+clip.length/ritmo+" s");
 }
 static float MedirApoyo(AnimationClip clip,GameObject modelo){var pies=modelo.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("foot.")).ToArray();var pos=new Vector3[pies.Length,121];for(int j=0;j<=120;j++){clip.SampleAnimation(modelo,clip.length*j/120);for(int i=0;i<pies.Length;i++)pos[i,j]=pies[i].position;}float sum=0;int count=0;for(int i=0;i<pies.Length;i++){float min=Enumerable.Range(0,121).Min(j=>pos[i,j].y);for(int j=1;j<120;j++)if(pos[i,j].y<min+.035f&&pos[i,j-1].y<min+.035f){sum+=(pos[i,j].z-pos[i,j-1].z)/(clip.length/120);count++;}}return count>0?sum/count:-2.7f;}
 public static void Retrato(ChasquiMovimiento p,bool detalle){var go=new GameObject("Vista temporal Chasqui");var cam=go.AddComponent<Camera>();Vector3 centro=p.transform.position+p.visual.up*(detalle?1.43f:1.1f);cam.transform.position=centro+p.visual.forward*(detalle?1.8f:3.3f)+p.visual.right*(detalle?.5f:1.2f)+Vector3.up*.12f;cam.transform.LookAt(centro);cam.fieldOfView=detalle?34:42;cam.farClipPlane=100;var rt=RenderTexture.GetTemporary(1000,1200,24);var prev=RenderTexture.active;var tex=new Texture2D(1000,1200,TextureFormat.RGB24,false);try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1000,1200),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(Path.GetTempPath(),detalle?"chasqui_tejido_detalle.png":"chasqui_acabado_personaje.png"),tex.EncodeToPNG());}finally{RenderTexture.active=prev;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(go);}}
}
