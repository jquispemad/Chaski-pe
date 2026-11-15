using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class VerificarLimitesCamino
{
    static VerificarLimitesCamino(){EditorApplication.delayCall += ComprobarUnaVez;}
    static void ComprobarUnaVez()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode || SessionState.GetBool("LimitesCaminoVerificados1",false))return;
        var temporal=new GameObject("Verificacion temporal de limites");
        temporal.hideFlags=HideFlags.HideAndDontSave;
        try
        {
            LimitesCaminoInca.Crear(temporal.transform);
            Physics.SyncTransforms();
            int correctos=0;
            for(int z=1;z<350;z++)for(int lado=-1;lado<=1;lado+=2)
            {
                var origen=new Vector3(LimitesCaminoInca.Centro(z),70,z);
                if(Physics.Raycast(origen,Vector3.right*lado,out var hit,60,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore)
                    && hit.transform.IsChildOf(temporal.transform) && Mathf.Abs(hit.distance-50)<.5f)correctos++;
            }
            bool invisible=temporal.GetComponentsInChildren<Renderer>().Length==0;
            string resultado=(correctos==698&&invisible?"CORRECTO":"REVISAR")+
                "\nColisiones laterales a 50 m: "+correctos+" / 698\nSin superficies visibles: "+invisible;
            File.WriteAllText("Temp/chasqui_limites_camino.txt",resultado);
            Debug.Log(resultado);
            SessionState.SetBool("LimitesCaminoVerificados1",true);
        }
        finally{Object.DestroyImmediate(temporal);}
    }
}
