using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
public static class CrearSaltoNatural
{
 const string Dir="Assets/Personajes/Chasqui/";
 struct Pose {public float contacto,drop,torso,hipL,hipR,kneeL,kneeR,ankle,armL,armR,elbow,cape;}
 static Pose Neutra=>new Pose{contacto=1};
 static Pose Agachada=>new Pose{contacto=1,drop=.115f,torso=14,hipL=-34,hipR=-34,kneeL=68,kneeR=68,ankle=-30,armL=16,armR=16,elbow=-24,cape=6};
 static Pose Impulso=>new Pose{torso=6,hipL=-8,hipR=-16,kneeL=16,kneeR=24,ankle=18,armL=-65,armR=-52,elbow=-35,cape=25};
 static Pose Aire=>new Pose{drop=-.015f,torso=8,hipL=-42,hipR=-27,kneeL=77,kneeR=58,ankle=12,armL=-48,armR=-35,elbow=-48,cape=40};
 static Pose Caida=>new Pose{torso=10,hipL=-18,hipR=-20,kneeL=30,kneeR=34,ankle=-8,armL=-26,armR=-20,elbow=-30,cape=28};
 static Pose Recepcion=>new Pose{contacto=1,drop=.15f,torso=20,hipL=-43,hipR=-43,kneeL=84,kneeR=84,ankle=-36,armL=-35,armR=-35,elbow=-42,cape=18};
 [MenuItem("Chasqui/Mejorar salto natural")]
 public static void Aplicar(){
 if(EditorApplication.isPlaying)throw new Exception("Detener Play primero");
 var jugador=UnityEngine.Object.FindFirstObjectByType<ChasquiMovimiento>();var anim=jugador.animador;
 var controlador=(AnimatorController)anim.runtimeAnimatorController;var sm=controlador.layers[0].stateMachine;
 var copia=UnityEngine.Object.Instantiate(anim.gameObject);copia.hideFlags=HideFlags.HideAndDontSave;copia.GetComponent<Animator>().enabled=false;
 var idle=AssetDatabase.LoadAllAssetsAtPath(Dir+"ChasquiAnimado.fbx").OfType<AnimationClip>().First(c=>c.name=="Reposo");idle.SampleAnimation(copia,.2f);
 var huesos=copia.GetComponentsInChildren<Transform>().Where(t=>t!=copia.transform&&t.GetComponent<Renderer>()==null&&t.GetComponent<MeshFilter>()==null).ToArray();
 // Usar solo la jerarquia del esqueleto, nunca las mallas dinamicas.
 huesos=huesos.Where(t=>t.name=="root"||t.name=="pelvis"||t.name=="spine"||t.name=="chest"||t.name=="neck"||t.name=="head"||t.name.Contains(".")||t.name.StartsWith("cape_")).ToArray();
 var pos=huesos.Select(t=>t.localPosition).ToArray();var rot=huesos.Select(t=>t.localRotation).ToArray();var scale=huesos.Select(t=>t.localScale).ToArray();
 Vector3 derecha=jugador.visual.right,arriba=jugador.visual.up; var pies=huesos.Where(h=>h.name=="foot.L"||h.name=="foot.R").ToArray();var pelvis=huesos.First(h=>h.name=="pelvis");float alturaPies=pies.Min(h=>Vector3.Dot(h.position,arriba));
 Action<Pose> poner=p=>{
 for(int i=0;i<huesos.Length;i++){huesos[i].localPosition=pos[i];huesos[i].localRotation=rot[i];huesos[i].localScale=scale[i];}
 foreach(var t in huesos){float grados=0;switch(t.name){case "pelvis":t.position-=arriba*p.drop;break;case "spine":grados=p.torso*.4f;break;case "chest":grados=p.torso*.6f;break;case "neck":grados=-p.torso*.45f;break;case "thigh.L":grados=p.hipL;break;case "thigh.R":grados=p.hipR;break;case "shin.L":grados=p.kneeL;break;case "shin.R":grados=p.kneeR;break;case "foot.L":case "foot.R":grados=p.ankle;break;case "upper_arm.L":grados=p.armL;break;case "upper_arm.R":grados=p.armR;break;case "forearm.L":case "forearm.R":grados=p.elbow;break;case "cape_01":grados=p.cape;break;case "cape_02":case "cape_03":case "cape_04":grados=3;break;}if(grados!=0)t.rotation=Quaternion.AngleAxis(grados,derecha)*t.rotation;}
 if(p.contacto>0)pelvis.position+=arriba*((alturaPies-pies.Min(h=>Vector3.Dot(h.position,arriba)))*p.contacto);
 };
 Action<string,float,float[],Pose[]> clip=(nombre,duracion,tiempos,poses)=>{
 var curvas=new Dictionary<string,AnimationCurve[]>();foreach(var h in huesos)curvas[AnimationUtility.CalculateTransformPath(h,copia.transform)]=Enumerable.Range(0,7).Select(_=>new AnimationCurve()).ToArray();
 int pasos=Mathf.CeilToInt(duracion*60);for(int j=0;j<=pasos;j++){float time=duracion*j/pasos;int k=0;while(k<tiempos.Length-2&&time>tiempos[k+1])k++;float blend=Mathf.SmoothStep(0,1,Mathf.InverseLerp(tiempos[k],tiempos[k+1],time));poner(Interpolar(poses[k],poses[k+1],blend));foreach(var h in huesos){var c=curvas[AnimationUtility.CalculateTransformPath(h,copia.transform)];Vector3 v=h.localPosition;Quaternion q=h.localRotation;float[] vals={v.x,v.y,v.z,q.x,q.y,q.z,q.w};for(int a=0;a<7;a++)c[a].AddKey(time,vals[a]);}}
 var nuevo=new AnimationClip{name=nombre,frameRate=60};string[] props={"m_LocalPosition.x","m_LocalPosition.y","m_LocalPosition.z","m_LocalRotation.x","m_LocalRotation.y","m_LocalRotation.z","m_LocalRotation.w"};foreach(var kv in curvas)for(int a=0;a<7;a++)nuevo.SetCurve(kv.Key,typeof(Transform),props[a],kv.Value[a]);nuevo.EnsureQuaternionContinuity();var settings=AnimationUtility.GetAnimationClipSettings(nuevo);settings.loopTime=false;AnimationUtility.SetAnimationClipSettings(nuevo,settings);
 string ruta=Dir+nombre.Replace(" ","")+".anim";var anterior=AssetDatabase.LoadAssetAtPath<AnimationClip>(ruta);if(anterior!=null){EditorUtility.CopySerialized(nuevo,anterior);UnityEngine.Object.DestroyImmediate(nuevo);nuevo=anterior;}else AssetDatabase.CreateAsset(nuevo,ruta);
 var estado=sm.states.Select(s=>s.state).FirstOrDefault(s=>s.name==nombre);if(estado==null)estado=sm.AddState(nombre);estado.motion=nuevo;estado.speed=1;foreach(var tr in estado.transitions.ToArray())estado.RemoveTransition(tr);EditorUtility.SetDirty(nuevo);
 };
 clip("Preparar salto",.09f,new[]{0f,.09f},new[]{Neutra,Agachada});
 clip("Salto",.30f,new[]{0f,.065f,.30f},new[]{Agachada,Impulso,Aire});
 clip("Caida natural",.24f,new[]{0f,.24f},new[]{Aire,Caida});
 clip("Recepcion salto",.30f,new[]{0f,.075f,.17f,.30f},new[]{Caida,Recepcion,Agachada,Neutra});
 foreach(var tr in sm.anyStateTransitions.ToArray())if(tr.destinationState!=null&&tr.destinationState.name=="Salto")sm.RemoveAnyStateTransition(tr);
 var salto=jugador.GetComponent<SaltoNaturalChasqui>();if(salto==null)salto=jugador.gameObject.AddComponent<SaltoNaturalChasqui>();salto.animador=anim;
 UnityEngine.Object.DestroyImmediate(copia);EditorUtility.SetDirty(controlador);EditorUtility.SetDirty(salto);EditorSceneManager.MarkSceneDirty(jugador.gameObject.scene);EditorSceneManager.SaveOpenScenes();PrefabUtility.SaveAsPrefabAsset(jugador.gameObject,Dir+"ChasquiJugador.prefab");AssetDatabase.SaveAssets();Debug.Log("SALTO_NATURAL_LISTO: preparacion, impulso, vuelo, caida y recepcion sincronizados.");
 }
 static Pose Interpolar(Pose a,Pose b,float t){return new Pose{contacto=Mathf.Lerp(a.contacto,b.contacto,t),drop=Mathf.Lerp(a.drop,b.drop,t),torso=Mathf.Lerp(a.torso,b.torso,t),hipL=Mathf.Lerp(a.hipL,b.hipL,t),hipR=Mathf.Lerp(a.hipR,b.hipR,t),kneeL=Mathf.Lerp(a.kneeL,b.kneeL,t),kneeR=Mathf.Lerp(a.kneeR,b.kneeR,t),ankle=Mathf.Lerp(a.ankle,b.ankle,t),armL=Mathf.Lerp(a.armL,b.armL,t),armR=Mathf.Lerp(a.armR,b.armR,t),elbow=Mathf.Lerp(a.elbow,b.elbow,t),cape=Mathf.Lerp(a.cape,b.cape,t)};}
}

