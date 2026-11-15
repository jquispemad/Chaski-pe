using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class JugarChasquiManual
{
    static JugarChasquiManual(){EditorApplication.delayCall+=LimpiarAlInstalar;}
    static void LimpiarAlInstalar()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode || SessionState.GetBool("ManualPreparadoV1",false))return;
        Limpiar();SessionState.SetBool("ManualPreparadoV1",true);
    }
    static void Limpiar()
    {
        foreach(var key in new[]{"Chasqui.ProbarValle","Chasqui.ProbarAgache","Chasqui.PruebaRopa","Chasqui.PruebaRaton","Chasqui.ProbarPulido"})SessionState.SetBool(key,false);
    }
    [MenuItem("Chasqui/Jugar manualmente")]
    public static void Jugar()
    {
        if(EditorApplication.isPlaying)return;
        Limpiar();
        var jugador=Object.FindFirstObjectByType<ChasquiMovimiento>();if(jugador!=null)jugador.enabled=true;
        EditorApplication.isPlaying=true;
    }
}
