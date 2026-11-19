using UnityEngine;
[RequireComponent(typeof(TextMesh))]
public sealed class TextoCartelChasqui : MonoBehaviour
{
 bool ajustar=true;Renderer tabla;Renderer letras;
 void OnEnable(){Font.textureRebuilt+=Actualizar;Actualizar(GetComponent<TextMesh>().font);ajustar=true;}
 void OnDisable(){Font.textureRebuilt-=Actualizar;}
 void Actualizar(Font f){var t=GetComponent<TextMesh>();if(f==null||f!=t.font)return;var r=GetComponent<MeshRenderer>();if(r.sharedMaterial!=null)r.sharedMaterial.mainTexture=f.material.mainTexture;ajustar=true;}
 void LateUpdate()
 {
  if(!ajustar)return;if(letras==null)letras=GetComponent<Renderer>();
  if(tabla==null){float cerca=1.2f;foreach(var r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)){if(r==letras||r.gameObject.scene!=gameObject.scene||!(r.name.ToLowerInvariant().Contains("tabla")||r.name.ToLowerInvariant().Contains("tablilla")))continue;float d=Vector3.Distance(r.bounds.center,letras.bounds.center);if(d<cerca){cerca=d;tabla=r;}}}
  var size=letras.bounds.size;if(size.x<.001f||size.y<.001f)return;
  Vector2 area=tabla!=null?new Vector2(tabla.bounds.size.x*.88f,tabla.bounds.size.y*.72f):new Vector2(3.1f,.62f);
  float factor=Mathf.Min(1,Mathf.Min(area.x/size.x,area.y/size.y));transform.localScale*=factor;ajustar=false;
 }
}
