using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class ProbarControlesChasqui
{
    const string Clave = "Chasqui.ComprobarControles";
    static ChasquiMovimiento jugador;
    static float tiempo, alturaMaxima, velocidadMaxima;
    static Vector3 inicio, antesRegreso, despuesAbajo, despuesIzquierda;
    static bool salto, aterrizo, regreso, abajo, izquierda, activo;
    static int ultimoFrame = -1;
    static Transform pierna, capa;
    static Quaternion piernaInicio, capaInicio;
    static float giroPierna, giroCapa, velocidadSinShift, velocidadPasoInicial;
    static bool basePose, vioSalto, fotoCaminar, fotoSalto, fotoCorrer;
    static ProbarControlesChasqui() { EditorApplication.playModeStateChanged += Cambio; }
    [MenuItem("Chasqui/Comprobar movimiento y salto")]
    public static void Ejecutar()
    {
        if (EditorApplication.isPlaying) throw new Exception("Sal de Play antes de comprobar los controles.");
        if (UnityEngine.Object.FindFirstObjectByType<ChasquiMovimiento>() == null) ConfigurarControlesChasqui.Aplicar();
        SessionState.SetBool(Clave,true); EditorApplication.isPlaying = true;
    }
    static void Cambio(PlayModeStateChange estado)
    {
        if (estado == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Clave,false))
        {
            jugador = UnityEngine.Object.FindFirstObjectByType<ChasquiMovimiento>();
            if (jugador == null) { Terminar("ERROR: falta controlador."); return; }
            jugador.enabled = false;
            inicio = jugador.transform.position; tiempo = 0; alturaMaxima = inicio.y; velocidadMaxima = 0;
            salto = aterrizo = regreso = abajo = izquierda = false; activo = true; ultimoFrame = -1;
            pierna = jugador.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name.StartsWith("thigh"));
            capa = jugador.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "cape_04");
            basePose = vioSalto = fotoCaminar = fotoSalto = fotoCorrer = false; giroPierna = giroCapa = velocidadSinShift = velocidadPasoInicial = 0;
            EditorApplication.update += Paso;
        }
        if (estado == PlayModeStateChange.ExitingPlayMode) { EditorApplication.update -= Paso; activo = false; }
    }
    static void Paso()
    {
        if (!activo || jugador == null || Time.frameCount == ultimoFrame) return;
        ultimoFrame = Time.frameCount;
        float dt = Time.deltaTime; if (dt <= 0) return;
        tiempo += dt;
        Vector2 entrada = Vector2.zero;
        if (tiempo < .65f) entrada = Vector2.up;
        else if (tiempo < 1.3f) entrada = Vector2.right;
        if (tiempo > 2.7f && tiempo < 3f && jugador.EnSuelo) aterrizo = true;
        if (tiempo >= 3f && !regreso) { antesRegreso = jugador.transform.position; regreso = true; }
        if (tiempo >= 3f && tiempo < 3.65f) entrada = Vector2.down;
        if (tiempo >= 3.65f && !abajo) { despuesAbajo = jugador.transform.position; abajo = true; }
        if (tiempo >= 3.65f && tiempo < 4.3f) entrada = Vector2.left;
        if (tiempo >= 4.3f && !izquierda) { despuesIzquierda = jugador.transform.position; izquierda = true; }
        if (tiempo >= 4.3f && tiempo < 5f) entrada = Vector2.one;
        bool pedirSalto = tiempo >= 1.45f && !salto;
        if (pedirSalto) salto = true;
        Vector3 anterior = jugador.transform.position;
        jugador.AplicarEntrada(entrada,pedirSalto,dt,tiempo >= 4.3f && tiempo < 5f);
        if (jugador.animador != null && jugador.animador.GetCurrentAnimatorStateInfo(0).IsName("Salto")) vioSalto = true;
        if (!basePose && tiempo >= .2f && pierna != null && capa != null)
        {
            basePose = true; piernaInicio = pierna.localRotation; capaInicio = capa.localRotation;
        }
        if (basePose)
        {
            giroPierna = Mathf.Max(giroPierna,Quaternion.Angle(piernaInicio,pierna.localRotation));
            giroCapa = Mathf.Max(giroCapa,Quaternion.Angle(capaInicio,capa.localRotation));
        }
        if (!fotoCaminar && tiempo > .5f) { Foto("caminar"); fotoCaminar = true; }
        if (!fotoSalto && tiempo > 1.7f) { Foto("salto"); fotoSalto = true; }
        if (!fotoCorrer && tiempo > 4.8f) { Foto("correr"); fotoCorrer = true; }
        Vector3 actual = jugador.transform.position;
        alturaMaxima = Mathf.Max(alturaMaxima,actual.y);
        float rapidez=Vector3.ProjectOnPlane(actual-anterior,Vector3.up).magnitude/dt;
        velocidadMaxima = Mathf.Max(velocidadMaxima,rapidez);
        if(tiempo<.16f)velocidadPasoInicial=Mathf.Max(velocidadPasoInicial,rapidez);
        if(tiempo>.4f && tiempo<1.3f)velocidadSinShift=Mathf.Max(velocidadSinShift,rapidez);
        if (tiempo < 5.2f) return;
        bool movimiento = antesRegreso.x-inicio.x > 1.3f && antesRegreso.z-inicio.z > 1.3f;
        bool regresoCorrecto = despuesAbajo.z < antesRegreso.z-1.3f && despuesIzquierda.x < despuesAbajo.x-1.3f;
        bool altura = alturaMaxima-inicio.y > .8f;
        bool diagonal = velocidadMaxima <= jugador.VelocidadMaxima+.2f;
        bool carreraAutomatica = velocidadSinShift > jugador.VelocidadMaxima-.25f && velocidadPasoInicial < 2.8f;
        bool animaciones = vioSalto && giroPierna > 8f && giroCapa > 10f;
        string resultado = (movimiento && regresoCorrecto && altura && aterrizo && diagonal && animaciones && carreraAutomatica ? "CORRECTO" : "ERROR")
            + "\nMovimiento adelante/derecha: " + movimiento + "\nMovimiento atras/izquierda: " + regresoCorrecto
            + "\nAltura salto: " + (alturaMaxima-inicio.y).ToString("F3") + "\nAterrizaje: " + aterrizo
            + "\nVelocidad horizontal maxima: " + velocidadMaxima.ToString("F3") + "\nDiagonal limitada: " + diagonal + "\nEstado salto animado: " + vioSalto + "\nMovimiento pierna: " + giroPierna.ToString("F1") + " grados\nMovimiento capa: " + giroCapa.ToString("F1") + " grados\nAnimaciones: " + animaciones;
        Terminar(resultado);
    }
    static void Foto(string nombre)
    {
        var cam = Camera.main; if (cam == null) return;
        var rt = RenderTexture.GetTemporary(600,800,24);
        var anterior = cam.targetTexture; var activa = RenderTexture.active; float aspect = cam.aspect;
        var tex = new Texture2D(600,800,TextureFormat.RGB24,false);
        try
        {
            cam.targetTexture = rt; cam.aspect = .75f; cam.Render(); RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0,0,600,800),0,0); tex.Apply();
            File.WriteAllBytes(Path.Combine(Path.GetTempPath(),"chasqui_prueba_"+nombre+".png"),tex.EncodeToPNG());
        }
        finally
        {
            cam.targetTexture = anterior; cam.aspect = aspect; RenderTexture.active = activa;
            RenderTexture.ReleaseTemporary(rt); UnityEngine.Object.DestroyImmediate(tex);
        }
    }
    static void Terminar(string resultado)
    {
        EditorApplication.update -= Paso; activo = false; SessionState.SetBool(Clave,false);
        File.WriteAllText(Path.Combine(Path.GetTempPath(),"chasqui_prueba_controles.txt"),resultado);
        Debug.Log("PRUEBA_CONTROLES_CHASQUI: " + resultado);
        EditorApplication.isPlaying = false;
    }
}
