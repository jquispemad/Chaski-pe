using UnityEngine;
[RequireComponent(typeof(TextMesh))]
public sealed class TextoCartelChasqui : MonoBehaviour
{
 void OnEnable(){Font.textureRebuilt+=Actualizar;Actualizar(GetComponent<TextMesh>().font);}
 void OnDisable(){Font.textureRebuilt-=Actualizar;}
 void Actualizar(Font f){var t=GetComponent<TextMesh>();if(f==null||f!=t.font)return;var r=GetComponent<MeshRenderer>();if(r.sharedMaterial!=null)r.sharedMaterial.mainTexture=f.material.mainTexture;}
}
