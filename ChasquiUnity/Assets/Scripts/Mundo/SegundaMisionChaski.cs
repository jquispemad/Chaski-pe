using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SegundaMisionChaski : MonoBehaviour
{
    public ChasquiMovimiento jugador;
    public Camera vista;
    public Transform responsable, entrega, mirador;
    public AudioSource arroyo;
    public bool EnCinematica { get; set; }
    Vector3 control;
    int etapa, dialogo = -1;
    bool anunciado, completo, registro, pausa;
    float quieto, bloqueoPututu, avisoHasta;
    string aviso;
    CamaraChasqui seguimiento;
    AudioPrimeraMision audioNivel;
    Texture2D manta;
    Font titulo, texto;
    Camera mapa;
    RenderTexture imagen;
    float actualizarMapa;
    ArroyoSierraChaski cauce;
    readonly float[] controles = { 82, 245, 302 };
    readonly string[] conversacion = {
        "WAYRA\nTraigo el quipu de la posta del pueblo. El paso del río sigue abierto y pueden enviar provisiones.",
        "RESPONSABLE DE LA POSTA\nEsperábamos a otro corredor para tomar el relevo. Todavía no ha regresado.",
        "WAYRA\n¿Cuándo salió?",
        "RESPONSABLE DE LA POSTA\nAntes del amanecer. Lleva este aviso hasta la estación del valle. Pregunta si llegó allí."
    };
    public bool Completado => completo;
    public int PuntosControl => etapa;

    void Start()
    {
        Time.timeScale = 1;
        cauce=FindFirstObjectByType<ArroyoSierraChaski>();
        control = jugador.transform.position;
        seguimiento = vista.GetComponent<CamaraChasqui>();
        audioNivel = GetComponent<AudioPrimeraMision>();
        titulo = Resources.Load<Font>("EstiloInca/TituloTradicional");
        texto = Resources.Load<Font>("EstiloInca/TextoTradicional");
        entrega.gameObject.SetActive(false);
        Avisar("Lleva el quipu a la siguiente posta. Allí anuncia tu llegada con el pututu.", 9);
        var o = new GameObject("Minimapa · segundo recorrido");
        mapa = o.AddComponent<Camera>();
        mapa.enabled = false; mapa.orthographic = true; mapa.orthographicSize = 258;
        mapa.aspect = 1; mapa.nearClipPlane = .1f; mapa.farClipPlane = 240;
        mapa.allowHDR = false; mapa.allowMSAA = false;
        mapa.transform.SetPositionAndRotation(new Vector3(0,160,230), Quaternion.Euler(90,0,0));
        mapa.clearFlags = CameraClearFlags.SolidColor; mapa.backgroundColor = new Color(.31f,.36f,.23f);
        imagen = new RenderTexture(384,384,16); mapa.targetTexture = imagen;
        if (PlayerPrefs.HasKey("Chaski.Nivel02.Control") && PlayerPrefs.GetInt("Chaski.Nivel02.Completado",0)==0)
        {
            etapa=Mathf.Clamp(PlayerPrefs.GetInt("Chaski.Nivel02.Control"),0,controles.Length);
            control=new Vector3(PlayerPrefs.GetFloat("Chaski.Nivel02.X"),PlayerPrefs.GetFloat("Chaski.Nivel02.Y"),PlayerPrefs.GetFloat("Chaski.Nivel02.Z"));
            jugador.Reubicar(control);
            Avisar("Continúas desde el último punto de control, con el quipu a salvo.",6);
        }
    }

    void Update()
    {
        if (completo || EnCinematica) return;
        if (Input.GetKeyDown(KeyCode.Escape) && dialogo < 0)
        {
            pausa = !pausa; Time.timeScale = pausa ? 0 : 1;
            seguimiento.enabled = !pausa;
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        }
        if (pausa) return;
        if (jugador.transform.position.y < -3f || (cauce!=null ? cauce.BajoAgua(jugador.transform.position) :
            jugador.transform.position.z > 274 && jugador.transform.position.z < 286 && jugador.transform.position.y < -.5f))
        {
            jugador.Reubicar(control);
            Avisar("Has vuelto al último punto de control. Conservas el quipu.", 5);
        }
        if (dialogo >= 0)
        {
            if (Input.GetKeyDown(KeyCode.E)) AvanzarDialogo();
            return;
        }
        var p = jugador.transform.position;
        if (etapa < controles.Length && p.z >= controles[etapa] && jugador.EnSuelo)
        {
            // Stages are monotonic and only record a safe grounded position.
            control = p + Vector3.up * .12f; etapa++;
            PlayerPrefs.SetInt("Chaski.Nivel02.Control", etapa); PlayerPrefs.Save();
            PlayerPrefs.SetFloat("Chaski.Nivel02.X",control.x);PlayerPrefs.SetFloat("Chaski.Nivel02.Y",control.y);PlayerPrefs.SetFloat("Chaski.Nivel02.Z",control.z);PlayerPrefs.Save();
            Avisar("Punto de control · quipu a salvo", 4);
        }
        if (!registro && Vector3.Distance(p,mirador.position) < 3f && Input.GetKeyDown(KeyCode.E))
        {
            registro = true;
            PlayerPrefs.SetInt("Chaski.Nivel02.Registro",1);
            Avisar("LOS RELEVOS\nLos chaskis transmitían recados entre postas. El quipu ayudaba a registrar información.", 12);
        }
        if (Vector3.Distance(p,responsable.position) < 18 && Input.GetKeyDown(KeyCode.E) && Time.time >= bloqueoPututu)
        {
            anunciado = true; bloqueoPututu = Time.time + (audioNivel.pututu!=null?audioNivel.pututu.length:11f)+3f;
            var gesto=jugador.GetComponent<TocarPututuChaski>();
            if(gesto==null)gesto=jugador.gameObject.AddComponent<TocarPututuChaski>();
            gesto.Iniciar(audioNivel);
            entrega.gameObject.SetActive(true);
            Avisar("El pututu anuncia tu llegada. Acércate al círculo y detente junto a la responsable.", 10);
            responsable.LookAt(new Vector3(p.x,responsable.position.y,p.z));
        }
        float distancia = Vector2.Distance(new Vector2(p.x,p.z),new Vector2(entrega.position.x,entrega.position.z));
        quieto = anunciado && jugador.enabled && etapa == controles.Length && distancia < 1.3f && jugador.EnSuelo &&
            jugador.GetComponent<CharacterController>().velocity.sqrMagnitude < .09f ? quieto + Time.deltaTime : 0;
        if (quieto > 1f)
        {
            dialogo = 0; jugador.enabled = false; seguimiento.enabled = false;
            if (jugador.animador != null) jugador.animador.SetFloat("Movimiento",0);
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        }
    }

    void AvanzarDialogo()
    {
        audioNivel.Confirmar();
        if (++dialogo < conversacion.Length) return;
        completo = true;
        PlayerPrefs.SetInt("Chaski.Nivel02.Completado",1); PlayerPrefs.Save();
        audioNivel.Completar();
    }

    void LateUpdate()
    {
        if (mapa != null && Time.unscaledTime >= actualizarMapa)
        { actualizarMapa = Time.unscaledTime + .3f; mapa.Render(); }
    }

    void Avisar(string mensaje, float segundos) { aviso = mensaje; avisoHasta = Time.time + segundos; }

    GUIStyle Estilo(int tamano, bool grande = false) => new GUIStyle(GUI.skin.label) {
        font = grande ? titulo : texto, fontSize = tamano, wordWrap = true,
        normal = {textColor = new Color(.99f,.9f,.7f)} };
    static void Panel(Rect r)
    {
        var c = GUI.color; GUI.color = new Color(.14f,.075f,.035f,.94f);
        GUI.DrawTexture(r,Texture2D.whiteTexture); GUI.color = c;
    }

    void OnGUI()
    {
        if(EnCinematica)return;
        float escala = Mathf.Min(Screen.width/1280f,Screen.height/800f);
        var matriz = GUI.matrix; GUI.matrix = Matrix4x4.Scale(Vector3.one * escala);
        float w=Screen.width/escala, h=Screen.height/escala;
        if (completo) Final(w,h);
        else if (dialogo >= 0)
        {
            Panel(new Rect((w-800)/2,h-245,800,220));
            GUI.Label(new Rect((w-740)/2,h-225,740,138),conversacion[dialogo],Estilo(24));
            if(GUI.Button(new Rect(w/2-130,h-75,260,40),"Continuar [E]",Boton())) AvanzarDialogo();
        }
        else if (pausa)
        {
            Panel(new Rect(w/2-230,h/2-120,460,240));
            GUI.Label(new Rect(w/2-190,h/2-90,380,45),"DESCANSO EN EL CAMINO",Estilo(25,true));
            if(GUI.Button(new Rect(w/2-160,h/2-20,320,50),"Reanudar",Boton()))
            { pausa=false; Time.timeScale=1; seguimiento.enabled=true; }
            if(GUI.Button(new Rect(w/2-160,h/2+45,320,45),"Menú principal",Boton())) Cargar("InicioChaski");
        }
        else
        {
            Panel(new Rect(20,20,Mathf.Min(750,w-270),108));
            GUI.Label(new Rect(36,30,700,30),"02 · EL PRIMER RECORRIDO",Estilo(24,true));
            string pista = Time.time < avisoHasta ? aviso : Pista();
            GUI.Label(new Rect(36,68,Mathf.Min(710,w-310),60),pista,Estilo(18));
            DibujarMapa(new Rect(w-238,20,218,264));
            Panel(new Rect(20,h-53,w-40,35));
            GUI.Label(new Rect(33,h-50,w-66,32),"WASD / Flechas · Mover    Ratón · Mirar    Espacio · Saltar    C · Agacharse    E · Interactuar    Esc · Pausa",Estilo(16));
        }
        GUI.matrix = matriz;
    }

    string Pista()
    {
        var p=jugador.transform.position;
        if(Vector3.Distance(p,mirador.position)<3 && !registro) return "[E] Leer el registro del mirador";
        if(Vector3.Distance(p,responsable.position)<18) return anunciado ? "Detente en el círculo amarillo junto a la responsable." : "[E] Anuncia tu llegada con el pututu";
        if(p.z<50) return "Entrega el mensaje en la siguiente posta. Clic para mirar con el ratón.";
        if(p.z>175 && p.z<218) return "Paso bajo · mantén C para pasar bajo la viga.";
        if(p.z>245 && p.z<300) return "Cruza el arroyo por la pasarela. Si caes, vuelves al punto de control.";
        return "Sigue el sendero. Quipu de misión · 1 / 1    Registros · "+(registro?"1":"0")+" / 1";
    }

    void DibujarMapa(Rect r)
    {
        Panel(r); GUI.Label(new Rect(r.x+10,r.y+7,200,25),"CAMINO A LA POSTA · N ↑",Estilo(15,true));
        var area=new Rect(r.x+10,r.y+38,198,180);
        if(imagen!=null) GUI.DrawTexture(area,imagen,ScaleMode.StretchToFill);
        Marcador(area,entrega.position,new Color(1,.78f,.15f),8);
        Marcador(area,jugador.transform.position,Color.white,7);
        GUI.Label(new Rect(r.x+10,r.y+226,200,30),Mathf.CeilToInt(Vector3.Distance(jugador.transform.position,entrega.position))+" m · Posta",Estilo(18));
    }
    void Marcador(Rect r,Vector3 p,Color color,float lado)
    {
        if(mapa==null)return; var v=mapa.WorldToViewportPoint(p);var c=GUI.color;GUI.color=color;
        GUI.DrawTexture(new Rect(r.x+Mathf.Clamp01(v.x)*r.width-lado/2,r.y+(1-Mathf.Clamp01(v.y))*r.height-lado/2,lado,lado),Texture2D.whiteTexture);GUI.color=c;
    }
    GUIStyle Boton() => new GUIStyle(GUI.skin.button){font=texto,fontSize=20};
    void Final(float w,float h)
    {
        if(manta==null)manta=InterfazIncaChasqui.CrearTejido();
        var r=new Rect(w/2-440,h/2-265,880,530);GUI.DrawTexture(r,manta);
        var estilo=Estilo(31,true);estilo.alignment=TextAnchor.MiddleCenter;
        GUI.Label(new Rect(r.x+60,r.y+70,760,55),"MENSAJE ENTREGADO",estilo);
        estilo=Estilo(23);estilo.alignment=TextAnchor.MiddleCenter;
        GUI.Label(new Rect(r.x+75,r.y+160,730,130),"La posta recibió tu aviso.\nEl relevista no ha regresado.\nUn nuevo encargo te llevará a la estación del valle.",estilo);
        if(GUI.Button(new Rect(r.x+65,r.y+335,230,60),"↻ Repetir",Boton()))
        { PlayerPrefs.DeleteKey("Chaski.Nivel02.Control"); Cargar("Nivel02_PrimerRecorrido"); }
        if(GUI.Button(new Rect(r.x+325,r.y+335,230,60),"⌂ Inicio",Boton())) Cargar("InicioChaski");
        GUI.enabled=Application.CanStreamedLevelBeLoaded("Nivel03_ValleVientos");
        if(GUI.Button(new Rect(r.x+585,r.y+335,230,60),"→ Siguiente",Boton())) Cargar("Nivel03_ValleVientos");
        GUI.enabled=true;
        GUI.Label(new Rect(r.x+65,r.y+425,750,55),"Nivel 3 en preparación · Registro del mirador: "+(registro?"encontrado":"pendiente"),estilo);
    }
    static void Cargar(string escena) {Time.timeScale=1;SceneManager.LoadScene(escena);}
    void OnDestroy()
    {
        Time.timeScale=1;
        if(mapa!=null)Destroy(mapa.gameObject);
        if(imagen!=null){imagen.Release();Destroy(imagen);}
        if(manta!=null)Destroy(manta);
    }
}
