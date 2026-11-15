using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ProbarEstiloInca
{
    const string Key="Chasqui.ProbarEstiloInca";
    static NivelValleChasqui nivel;static InterfazIncaChasqui ui;static int fase,frame;static float t;
    static ProbarEstiloInca(){EditorApplication.playModeStateChanged+=Cambio;}
    [MenuItem("Chasqui/Comprobar tejido, quipu y pantalla final")]
    public static void Iniciar(){if(EditorApplication.isPlaying)return;SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;}
    static void Cambio(PlayModeStateChange s)
    {
        if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Key,false))
        {
            SessionState.SetBool(Key,false);nivel=Object.FindFirstObjectByType<NivelValleChasqui>();ui=nivel.GetComponent<InterfazIncaChasqui>();
            nivel.jugador.enabled=false;fase=0;frame=-1;t=0;EditorApplication.update+=Paso;
        }
        if(s==PlayModeStateChange.ExitingPlayMode){EditorApplication.update-=Paso;SessionState.SetBool(Key,false);}
    }
    static void Paso()
    {
        if(nivel==null||frame==Time.frameCount)return;frame=Time.frameCount;t+=Time.deltaTime;
        if(fase==0&&t>.6f){nivel.jugador.Reubicar(nivel.recuerdos[0].position-Vector3.up*.85f);fase=1;t=0;}
        else if(fase==1&&t>.6f){ScreenCapture.CaptureScreenshot("Temp/chasqui_aviso_tejido.png");fase=2;t=0;}
        else if(fase==2&&t>.5f){nivel.jugador.Reubicar(nivel.llegada.position+Vector3.up*.1f);fase=3;t=0;}
        else if(fase==3&&t>.6f)
        {
            ScreenCapture.CaptureScreenshot("Temp/chasqui_manta_final.png");fase=4;t=0;
            bool shaders=!ShaderUtil.ShaderHasError(Shader.Find("Chasqui/Texto de letreros"))&&!ShaderUtil.ShaderHasError(Shader.Find("Chasqui/Paja de ichu"));
            File.WriteAllText("Temp/chasqui_prueba_estilo.txt",(nivel.Recogidos==1&&nivel.Completado&&ui!=null&&shaders?"CORRECTO":"REVISAR")+
                "\nQuipu recogido y aviso mostrado: "+nivel.Recogidos+"\nManta al completar: "+nivel.Completado+"\nShaders sin errores: "+shaders+"\nBotones: decorativos");
        }
        else if(fase==4&&t>.6f){EditorApplication.update-=Paso;EditorApplication.isPlaying=false;}
    }
}
