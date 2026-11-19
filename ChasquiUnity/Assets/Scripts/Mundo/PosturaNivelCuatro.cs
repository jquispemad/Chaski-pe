using UnityEngine;
[DefaultExecutionOrder(106)]public sealed class PosturaNivelCuatro:MonoBehaviour
{
 NivelCuatroChaski nivel;void Start(){nivel=GetComponent<NivelCuatroChaski>();}
 void LateUpdate(){if(nivel!=null&&!nivel.EnPausa)nivel.AplicarPostura();}
}
