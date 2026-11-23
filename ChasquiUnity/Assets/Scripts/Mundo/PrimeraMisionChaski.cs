using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class PrimeraMisionChaski : MonoBehaviour
{
    public ChasquiMovimiento jugador;
    public Camera vista;
    public float llegada = 122f;
    public Transform circuloEntrega;
    public float radioEntrega = 1.25f;
    int dialogo;
    float quieto;
    Font titulo, lectura;
    GUIStyle encabezado, aviso, controles;
    float control = 2f;
    float velocidadCamara;
    const float Adelanto = 1.1f;
    Texture2D manta;
    public string siguienteEscena = "Nivel02_PrimerRecorrido";
    Camera camaraMapa;
    RenderTexture imagenMapa;
    float proximaActualizacionMapa;
    bool pausa;
    bool movimientoAntesPausa;
    CharacterController ccJugador;

    void Update()
    {
        if(dialogo != 3 && Input.GetKeyDown(KeyCode.Escape)) CambiarPausa(!pausa);
    }

    void CambiarPausa(bool valor)
    {
        if(pausa == valor)return;
        pausa=valor;
        if(pausa)
        {
            movimientoAntesPausa=jugador.enabled;
            jugador.enabled=false;
            Cursor.lockState=CursorLockMode.None;
            Cursor.visible=true;
        }
        else jugador.enabled=movimientoAntesPausa;
        Time.timeScale=pausa?0:1;
    }

    void Start()
    {
        ccJugador = jugador != null ? jugador.GetComponent<CharacterController>() : null;
        titulo = Resources.Load<Font>("EstiloInca/TituloTradicional");
        lectura = Resources.Load<Font>("EstiloInca/TextoTradicional");
        var posicion = vista.transform.position;
        posicion.z = jugador.transform.position.z + Adelanto;
        vista.transform.position = posicion;
        imagenMapa = new RenderTexture(256,256,16);
        var objetoMapa = new GameObject("Minimapa · vista aérea");
        camaraMapa = objetoMapa.AddComponent<Camera>();
        camaraMapa.enabled = false;
        camaraMapa.orthographic = true;
        float finalCamino = circuloEntrega == null ? llegada : circuloEntrega.position.z;
        camaraMapa.orthographicSize = finalCamino * .5f + 12f;
        camaraMapa.aspect = 1;
        camaraMapa.nearClipPlane = .1f;
        camaraMapa.farClipPlane = 110;
        camaraMapa.clearFlags = CameraClearFlags.SolidColor;
        camaraMapa.backgroundColor = new Color(.29f,.35f,.22f);
        camaraMapa.targetTexture = imagenMapa;
        camaraMapa.allowHDR = false;
        camaraMapa.allowMSAA = false;
        var derechaPantalla = Vector3.ProjectOnPlane(vista.transform.right, Vector3.up).normalized;
        var arribaMapa = Vector3.Cross(Vector3.down, derechaPantalla);
        camaraMapa.transform.rotation = Quaternion.LookRotation(Vector3.down, arribaMapa);
        camaraMapa.transform.position = new Vector3(0,65,finalCamino*.5f);
    }

    void LateUpdate()
    {
        if(pausa)return;
        float z = jugador.transform.position.z;
        if (jugador.transform.position.y < -3f) jugador.Reubicar(new Vector3(0, .12f, control));
        if (z > 157f && jugador.EnSuelo) control = 157f;
        var posicion = vista.transform.position;
        float objetivo = jugador.transform.position.z + Adelanto;
        posicion.z = Mathf.Abs(posicion.z - objetivo) > 12f
            ? objetivo
            : Mathf.SmoothDamp(posicion.z, objetivo, ref velocidadCamara, .18f);
        vista.transform.position = posicion;
        if(Time.unscaledTime >= proximaActualizacionMapa)
        {
            proximaActualizacionMapa = Time.unscaledTime + .25f;
            camaraMapa.Render();
        }
        if (dialogo != 0 || circuloEntrega == null) return;
        var distancia = jugador.transform.position - circuloEntrega.position;
        bool dentro = distancia.x * distancia.x + distancia.z * distancia.z <= radioEntrega * radioEntrega;
        var velocidad = ccJugador != null ? ccJugador.velocity : Vector3.zero;
        quieto = dentro && jugador.EnSuelo && velocidad.sqrMagnitude < .09f && !QuiereMoverse()
            ? quieto + Time.deltaTime : 0f;
        if (quieto >= 1f)
        {
            dialogo = 3;
            GetComponent<AudioPrimeraMision>()?.Completar();
            jugador.enabled = false;
            if (jugador.animador != null) jugador.animador.SetFloat("Movimiento", 0);
            PlayerPrefs.SetInt("Chaski.PrimeraMisionCompletada", 1);
            PlayerPrefs.Save();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    static bool QuiereMoverse() => Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) ||
        Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.Space);

    void OnGUI()
    {
        if(pausa)
        {
            DibujarPausa();
            return;
        }
        float ancho = Mathf.Min(Screen.width - 32, Mathf.Max(280, Screen.width - 262));
        var caja = new Rect(16, 16, ancho, 82);
        string mensaje;
        float z = jugador.transform.position.z;
        if (dialogo == 3) mensaje = "Quipu recibido. Lleva el aviso a la próxima posta.";
        else if (z >= llegada) mensaje = "Detente en el círculo amarillo junto al relevista.";
        else if (z < 26f) mensaje = "Sigue el sendero junto a los cultivos hasta la posta.";
        else if (z < 41f) mensaje = "[Espacio] Salta sobre la piedra.";
        else if (z >= 96f && z <= 120f) mensaje = "[C] Mantén pulsado para pasar bajo la viga.";
        else mensaje = "La posta está al final del sendero.";
        if (encabezado == null)
        {
            encabezado = Estilo(titulo, 22);
            aviso = Estilo(lectura, 17);
            controles = Estilo(lectura, 14);
        }
        if (dialogo == 3)
        {
            DibujarFinal();
            return;
        }
        Panel(caja);
        GUI.Label(new Rect(30,24,ancho-28,29), "CHASKI · La primera misión", encabezado);
        GUI.Label(new Rect(30,53,ancho-28,40), mensaje, aviso);
        var barra = new Rect(16, Screen.height - 49, ancho, 32);
        Panel(barra);
        GUI.Label(new Rect(29,barra.y+3,ancho-26,26),
            "W / ↑ Avanzar    S / ↓ Retroceder    Espacio Saltar    C Agacharse    Esc Pausa", controles);
        DibujarMapa();
    }

    void DibujarMapa()
    {
        var r = new Rect(Screen.width-226,16,210,210);
        Panel(r);
        GUI.Label(new Rect(r.x+12,r.y+8,186,24),"CAMINO A LA POSTA",controles);
        var anterior = GUI.color;
        var mapa = new Rect(r.x+12,r.y+35,186,126);
        if(imagenMapa != null) GUI.DrawTexture(mapa,imagenMapa,ScaleMode.StretchToFill);
        float destino = circuloEntrega == null ? llegada : circuloEntrega.position.z;
        var posicionDestino = camaraMapa.WorldToViewportPoint(new Vector3(0,0,destino));
        float destinoX = Mathf.Clamp01(posicionDestino.x);
        float destinoY = Mathf.Clamp01(1-posicionDestino.y)*mapa.height;
        GUI.color = new Color(1,.78f,.16f);
        GUI.DrawTexture(new Rect(mapa.x+Mathf.Clamp(destinoX*186,8,178)-5,mapa.y+destinoY-5,10,10),Texture2D.whiteTexture);
        GUI.color = new Color(.65f,.85f,1);
        var ubicacion = camaraMapa.WorldToViewportPoint(jugador.transform.position);
        GUI.DrawTexture(new Rect(mapa.x+Mathf.Clamp01(ubicacion.x)*mapa.width-4,
            mapa.y+(1-Mathf.Clamp01(ubicacion.y))*mapa.height-4,8,8),Texture2D.whiteTexture);
        GUI.color = anterior;
        float falta = Mathf.Max(0,destino-jugador.transform.position.z);
        GUI.Label(new Rect(r.x+12,r.y+165,190,21),"Posta · "+Mathf.CeilToInt(falta)+" m",aviso);
    }

    void DibujarFinal()
    {
        if (manta == null) manta = InterfazIncaChasqui.CrearTejido();
        var matriz = GUI.matrix;
        float escala = Mathf.Min(Screen.width / 940f, Screen.height / 650f);
        GUI.matrix = Matrix4x4.Scale(Vector3.one * escala);
        float w = Screen.width / escala, h = Screen.height / escala;
        GUI.color = new Color(0,0,0,.6f);
        GUI.DrawTexture(new Rect(0,0,w,h),Texture2D.whiteTexture);
        GUI.color = Color.white;
        var r = new Rect((w-840)/2,(h-530)/2,840,530);
        GUI.DrawTexture(r,manta);
        var centro = Estilo(lectura,23);
        centro.alignment = TextAnchor.MiddleCenter;
        var grande = Estilo(titulo,32);
        grande.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(r.x+60,r.y+55,720,38),"LA PRIMERA MISIÓN",centro);
        GUI.Label(new Rect(r.x+60,r.y+115,720,52),"QUIPU RECIBIDO",grande);
        GUI.Label(new Rect(r.x+90,r.y+195,660,90),
            "Has llegado a la posta.\nTu primer mensaje te espera en el camino.",centro);
        var boton = new GUIStyle(GUI.skin.button) {font=lectura,fontSize=21};
        if(GUI.Button(new Rect(r.x+70,r.y+335,320,65),"↻  Repetir nivel",boton))
        {
            GetComponent<AudioPrimeraMision>()?.Confirmar();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        bool disponible = Application.CanStreamedLevelBeLoaded(siguienteEscena);
        GUI.enabled = disponible;
        if(GUI.Button(new Rect(r.x+450,r.y+335,320,65),"→  Siguiente nivel",boton))
            SceneManager.LoadScene(siguienteEscena);
        GUI.enabled = true;
        if(!disponible)
            GUI.Label(new Rect(r.x+70,r.y+425,700,42),"El siguiente nivel está en preparación.",centro);
        GUI.matrix = matriz;
    }

    void DibujarPausa()
    {
        if(manta==null)manta=InterfazIncaChasqui.CrearTejido();
        var anterior=GUI.matrix;
        float escala=Mathf.Min(Screen.width/940f,Screen.height/650f);
        GUI.matrix=Matrix4x4.Scale(Vector3.one*escala);
        float w=Screen.width/escala,h=Screen.height/escala;
        GUI.color=new Color(0,0,0,.65f);
        GUI.DrawTexture(new Rect(0,0,w,h),Texture2D.whiteTexture);GUI.color=Color.white;
        var r=new Rect((w-700)/2,(h-420)/2,700,420);GUI.DrawTexture(r,manta);
        var tituloPausa=Estilo(titulo,30);tituloPausa.alignment=TextAnchor.MiddleCenter;
        GUI.Label(new Rect(r.x+60,r.y+55,580,55),"DESCANSO EN EL CAMINO",tituloPausa);
        var ayuda=Estilo(lectura,21);ayuda.alignment=TextAnchor.MiddleCenter;
        GUI.Label(new Rect(r.x+70,r.y+125,560,55),"Pulsa Esc para continuar tu misión.",ayuda);
        var boton=new GUIStyle(GUI.skin.button){font=lectura,fontSize=23};
        if(GUI.Button(new Rect(r.x+170,r.y+205,360,60),"Reanudar [Esc]",boton))CambiarPausa(false);
        if(GUI.Button(new Rect(r.x+170,r.y+280,360,55),"Menú principal",boton))
        {
            Time.timeScale=1;
            SceneManager.LoadScene("InicioChaski");
        }
        GUI.matrix=anterior;
    }

    void OnDestroy()
    {
        if(pausa)Time.timeScale=1;
        if(manta != null) Destroy(manta);
        if(camaraMapa != null) Destroy(camaraMapa.gameObject);
        if(imagenMapa != null) { imagenMapa.Release(); Destroy(imagenMapa); }
    }

    static GUIStyle Estilo(Font fuente, int tamano) => new GUIStyle(GUI.skin.label)
    {
        font = fuente, fontSize = tamano, wordWrap = true,
        normal = {textColor = new Color(.98f,.9f,.7f)}
    };

    static void Panel(Rect caja)
    {
        Color anterior = GUI.color;
        GUI.color = new Color(.38f,.24f,.11f,.96f);
        GUI.DrawTexture(caja, Texture2D.whiteTexture);
        GUI.color = new Color(.16f,.09f,.055f,.98f);
        GUI.DrawTexture(new Rect(caja.x+3,caja.y+3,caja.width-6,caja.height-6), Texture2D.whiteTexture);
        GUI.color = anterior;
    }
}
