using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
[InitializeOnLoad]
public static class InstalarAgacheCarteles
{
 static InstalarAgacheCarteles(){EditorApplication.delayCall+=InstalarSiFalta;}
 static void InstalarSiFalta(){if(EditorApplication.isPlayingOrWillChangePlaymode)return;var p=UnityEngine.Object.FindFirstObjectByType<ChasquiMovimiento>();if(p!=null&&p.GetComponent<AgacharseChasqui>()==null)Aplicar();}
 [MenuItem("Chasqui/Instalar agacharse y mejorar letreros")]
 public static void Aplicar(){if(EditorApplication.isPlaying)return;var p=UnityEngine.Object.FindFirstObjectByType<ChasquiMovimiento>();if(p==null)return;var a=p.GetComponent<AgacharseChasqui>();if(a==null)a=p.gameObject.AddComponent<AgacharseChasqui>();
 var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");const string path="Assets/Niveles/Valle/LetraCarteles.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat==null){mat=new Material(Shader.Find("Chasqui/Texto de letreros"));AssetDatabase.CreateAsset(mat,path);}mat.shader=Shader.Find("Chasqui/Texto de letreros");mat.mainTexture=font.material.mainTexture;
 int count=0;foreach(var t in UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsSortMode.None)){
 bool grande=t.text.Contains("QHAPAQ")||t.text.Contains("TAMBO DEL");
 if(t.text.Contains("QHAPAQ"))t.text="QHAPAQ ÑAN";else if(t.text.Contains("SIGUE EL SENDERO"))t.text="SALTA LOS OBSTÁCULOS\nESPACIO";else if(t.text.Contains("RIO DEL"))t.text="RÍO DEL VALLE\nCRUZA EL PUENTE";else if(t.text.Contains("EL MENSAJE"))t.text="TAMBO\nSIGUE EL CAMINO";
 t.font=font;t.fontSize=128;t.fontStyle=FontStyle.Bold;t.characterSize=.1f;t.lineSpacing=1.15f;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=new Color(1,.94f,.76f);t.transform.localScale=Vector3.one;t.transform.rotation=Quaternion.identity;font.RequestCharactersInTexture(t.text,128,FontStyle.Bold);var r=t.GetComponent<MeshRenderer>();r.sharedMaterial=mat;var bounds=r.bounds;float w=grande?5.8f:1.82f,h=grande?.44f:.48f;if(bounds.size.x>.001f&&bounds.size.y>.001f)t.transform.localScale=Vector3.one*Mathf.Min(w/bounds.size.x,h/bounds.size.y);if(t.GetComponent<TextoCartelChasqui>()==null)t.gameObject.AddComponent<TextoCartelChasqui>();EditorUtility.SetDirty(t);count++;
 }mat.mainTexture=font.material.mainTexture;EditorUtility.SetDirty(mat);EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();Debug.Log("AGACHE_Y_LETREROS_LISTOS: CTRL/C; "+count+" letreros legibles con profundidad");}
}
