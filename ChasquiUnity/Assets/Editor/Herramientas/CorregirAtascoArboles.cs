using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CorregirAtascoArboles
{
    static CorregirAtascoArboles(){EditorApplication.delayCall+=Aplicar;EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredEditMode)EditorApplication.delayCall+=Aplicar;};}
    static void Aplicar()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||SessionState.GetBool("ArbolesSinAtascoV1",false))return;
        var nivel=Object.FindFirstObjectByType<NivelValleChasqui>();if(nivel==null)return;
        ColisionesValleChasqui.Aplicar(nivel.gameObject.scene);
        var troncos=nivel.gameObject.scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<MeshFilter>()).Where(f=>f.name=="Tronco").ToArray();
        int validos=troncos.Count(f=>f.GetComponent<CapsuleCollider>()?.enabled==true&&!f.GetComponents<MeshCollider>().Any(c=>c.enabled));
        int salidas=0,bloqueos=0;
        var arbol=new GameObject("Prueba temporal del tronco");arbol.hideFlags=HideFlags.HideAndDontSave;arbol.transform.position=Vector3.up*150;
        var col=arbol.AddComponent<CapsuleCollider>();col.center=new Vector3(.09f,1.6f,.05f);col.height=3.2f;col.radius=.23f;
        var sujeto=new GameObject("Prueba temporal de salida");sujeto.hideFlags=HideFlags.HideAndDontSave;
        var cc=sujeto.AddComponent<CharacterController>();cc.height=2;cc.center=Vector3.up;cc.radius=.23f;cc.skinWidth=.025f;cc.stepOffset=.22f;cc.enableOverlapRecovery=true;
        try
        {
            Physics.SyncTransforms();
            // Enter toward the trunk, then reverse: every approach must allow an exit.
            for(int i=0;i<16;i++)
            {
                Vector3 d=Quaternion.Euler(0,i*22.5f,0)*Vector3.forward;
                Vector3 centro=arbol.transform.position+new Vector3(.09f,0,.05f);
                cc.enabled=false;sujeto.transform.position=centro+d*2;cc.enabled=true;Physics.SyncTransforms();
                for(int j=0;j<80;j++)cc.Move(-d*.04f);
                if(Vector3.ProjectOnPlane(sujeto.transform.position-centro,Vector3.up).magnitude>.35f)bloqueos++;
                for(int j=0;j<100;j++)cc.Move(d*.04f);
                if(Vector3.Dot(sujeto.transform.position-centro,d)>3.5f)salidas++;
            }
        }
        finally{Object.DestroyImmediate(sujeto);Object.DestroyImmediate(arbol);}
        EditorSceneManager.MarkSceneDirty(nivel.gameObject.scene);EditorSceneManager.SaveScene(nivel.gameObject.scene);
        var resultado=(validos==troncos.Length&&salidas==16&&bloqueos==16?"CORRECTO":"REVISAR")+
            "\nArboles con tronco suave y ramas sin atrapamiento: "+validos+" / "+troncos.Length+
            "\nTronco bloquea entrada: "+bloqueos+" / 16\nSe puede retroceder y salir: "+salidas+" / 16";
        File.WriteAllText("Temp/chasqui_arboles_sin_atasco.txt",resultado);Debug.Log(resultado);
        SessionState.SetBool("ArbolesSinAtascoV1",true);
    }
}
