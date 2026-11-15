using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class ProbarRealismoIntegral
{
    const string Key="Chasqui.TestRealismoIntegral";
    static ChasquiMovimiento p;static CapaFisicaChasqui capa;static CabelloDinamicoChasqui pelo;static AgacharseChasqui agache;
    static float tiempo,maxPelo,maxCapa,alturaInicio,maxAltura;static int frame,invalidos;static bool salto,aterrizo;static Vector3[] reposo;static bool[] fotos;
    static ProbarRealismoIntegral(){EditorApplication.playModeStateChanged+=Cambio;}
    [MenuItem("Chasqui/Comprobar realismo de capa cabello y cuerpo")]
    public static void Iniciar(){if(EditorApplication.isPlaying)return;SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;}
    static void Cambio(PlayModeStateChange s)
    {
        if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Key,false))
        {
            Application.runInBackground=true;
            SessionState.SetBool(Key,false);p=UnityEngine.Object.FindFirstObjectByType<ChasquiMovimiento>();
            var nivel=UnityEngine.Object.FindFirstObjectByType<NivelValleChasqui>();nivel.enabled=false;
            p.enabled=false;p.camara=null;capa=p.GetComponent<CapaFisicaChasqui>();pelo=p.GetComponent<CabelloDinamicoChasqui>();agache=p.GetComponent<AgacharseChasqui>();agache.LeerTeclado=false;
            var piso=GameObject.CreatePrimitive(PrimitiveType.Cube);piso.name="Piso temporal de prueba";piso.transform.position=new Vector3(80,99.75f,40);piso.transform.localScale=new Vector3(40,.5f,60);
            p.Reubicar(new Vector3(80,100.02f,25));tiempo=maxCapa=maxPelo=0;maxAltura=alturaInicio=100.02f;frame=-1;invalidos=0;salto=aterrizo=false;reposo=null;fotos=new bool[6];EditorApplication.update+=Paso;
        }
        if(s==PlayModeStateChange.ExitingPlayMode){EditorApplication.update-=Paso;SessionState.SetBool(Key,false);}
    }
    static void Paso()
    {
        if(p==null||frame==Time.frameCount)return;frame=Time.frameCount;float dt=Mathf.Min(Time.deltaTime,.04f);tiempo+=dt;
        if(capa==null||capa.tela==null){Fin("ERROR: falta Cloth");return;}
        Vector2 entrada=tiempo>1.3f&&tiempo<4.4f?Vector2.up:tiempo>7.5f&&tiempo<8.3f?Vector2.right:Vector2.zero;
        bool saltar=tiempo>6&&!salto;if(saltar)salto=true;
        agache.Solicitar(tiempo>4.6f&&tiempo<5.6f,dt);p.AplicarEntrada(entrada,saltar,dt,true);
        maxAltura=Mathf.Max(maxAltura,p.transform.position.y);if(tiempo>7&&p.EnSuelo)aterrizo=true;
        var contacto=p.GetComponent<TelaCapaSinCruces>();
        var puntos=contacto!=null&&contacto.Vertices!=null?contacto.Vertices:capa.tela.vertices;
        if(tiempo>1.1f&&reposo==null)reposo=(Vector3[])puntos.Clone();
        foreach(var v in puntos)if(float.IsNaN(v.x)||float.IsInfinity(v.x)||v.sqrMagnitude>25)invalidos++;
        if(reposo!=null&&tiempo>2&&tiempo<4.3f)for(int i=puntos.Length/2;i<puntos.Length;i++)maxCapa=Mathf.Max(maxCapa,Vector3.Distance(puntos[i],reposo[i]));
        foreach(var m in pelo.mechones)maxPelo=Mathf.Max(maxPelo,Quaternion.Angle(Quaternion.identity,m.localRotation));
        float[] momentos={1.2f,3.5f,5.2f,6.45f,8.1f,9.3f};
        string[] nombres={"reposo","carrera","agachado","salto","giro","retrato"};
        for(int i=0;i<6;i++)if(!fotos[i]&&tiempo>momentos[i]){fotos[i]=true;Foto(nombres[i],i==5);}
        if(fotos[0]&&!fotos[5]&&tiempo>1.25f&&tiempo<1.35f){var rend=p.GetComponent<RopaDinamicaChasqui>().salida.GetComponent<Renderer>();rend.enabled=false;Foto("solo_capa",false);rend.enabled=true;}
        if(tiempo>10)
        {
            bool shader=!ShaderUtil.ShaderHasError(Shader.Find("Chasqui/Tejido dos caras"));
            string resultado=(invalidos==0&&maxCapa>.07f&&maxPelo>2&&maxAltura-alturaInicio>.8f&&aterrizo&&shader?"CORRECTO":"REVISAR")+
                "\nMovimiento capa: "+maxCapa+" m\nMovimiento cabello: "+maxPelo+" grados\nVertices invalidos: "+invalidos+
                "\nSalto: "+(maxAltura-alturaInicio)+" m\nAterrizo: "+aterrizo+"\nShader sin errores: "+shader+"\nPenetraciones ropa base: "+p.GetComponent<RopaDinamicaChasqui>().ContarPenetraciones()+"\nPenetraciones capa contra envolvente de tunica: "+(contacto!=null?contacto.Penetraciones:-1);Fin(resultado);
        }
    }
    static void Fin(string resultado){File.WriteAllText("Temp/chasqui_prueba_realismo.txt",resultado);Debug.Log(resultado);EditorApplication.update-=Paso;EditorApplication.isPlaying=false;}
    static void Foto(string nombre,bool frente)
    {
        var o=new GameObject("Retrato temporal");var c=o.AddComponent<Camera>();Vector3 mirar=p.transform.position+Vector3.up*(frente?1.35f:1.1f);
        c.transform.position=mirar+p.visual.forward*(frente?2.3f:-2.5f)+p.visual.right*1.5f+Vector3.up*.20f;c.transform.LookAt(mirar);c.fieldOfView=frente?36:43;c.farClipPlane=500;
        var rt=RenderTexture.GetTemporary(1100,1300,24);var antes=RenderTexture.active;var t=new Texture2D(1100,1300,TextureFormat.RGB24,false);
        try{c.targetTexture=rt;c.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1100,1300),0,0);t.Apply();File.WriteAllBytes("Temp/chasqui_realista_"+nombre+".png",t.EncodeToPNG());}
        finally{RenderTexture.active=antes;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(o);}
    }
}
