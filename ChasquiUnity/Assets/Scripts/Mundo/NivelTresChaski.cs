using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(40)]
public sealed class NivelTresChaski:MonoBehaviour
{
    public ChasquiMovimiento jugador;
    public Camera vista;
    public Transform encargado,circulo,registro,descanso,caminante,agricultor,responsableSalida;
    public Transform[] controles,abrigos;
    public RecursoValleVientos[] recursos;
    public AudioSource viento,agua;
    public static float CentroValle(float z)=>11*Mathf.Sin(z*.018f)+5*Mathf.Sin(z*.043f)+18*Mathf.Sin(z*.035f)*(1-Mathf.SmoothStep(0,1,Mathf.Clamp01(z/110)));
    public static float BaseValle(float z)=>z<110?7-5*Mathf.SmoothStep(0,1,Mathf.Clamp01(z/110)): z<195?2+5*Mathf.SmoothStep(0,1,(z-110)/85):z<235?7:z<270?7-6*Mathf.SmoothStep(0,1,(z-235)/35):z<305?1:1+3*Mathf.SmoothStep(0,1,Mathf.Clamp01((z-305)/95));
    public static float Centro(float z)
    {
        float a=z+446;float previo=14*Mathf.Sin(a*.021f)+7*Mathf.Sin(a*.047f)-(14*Mathf.Sin(450*.021f)+7*Mathf.Sin(450*.047f));
        return Mathf.Lerp(previo,CentroValle(z),Mathf.SmoothStep(0,1,Mathf.InverseLerp(28,75,z)));
    }
    public static float Base(float z)=>Mathf.Lerp(7,BaseValle(z),Mathf.SmoothStep(0,1,Mathf.InverseLerp(28,75,z)));
    public static float CentroAgua(float x)=>281+Mathf.Sin(x*.075f)*3*Mathf.SmoothStep(0,1,Mathf.Clamp01((Mathf.Abs(x)-12)/12));
    public static float AguaY(float x)=>.28f+3.2f*Mathf.SmoothStep(0,1,Mathf.InverseLerp(20,88,x));
    public static float AnchoAgua(float x)=>(4.1f+.45f*Mathf.Sin(x*.09f))*Mathf.SmoothStep(0,1,Mathf.InverseLerp(88,78,x));
    public float Resistencia{get;private set;}=100;
    public float Rafaga{get;private set;}
    public bool Protegido{get;private set;}
    public bool Terminado=>terminado;
    public float Introduccion=>intro;
    public int Etapa=>etapa;
    public int[] Cantidades=>cantidad;
    public float CocaRestante=>coca;
    public bool EnInterfaz=>pausa||selector||dialogo>=0||intro>0||terminado;
    readonly int[] cantidad=new int[4];
    readonly string[] nombres={"Coca","Cancha","Ch’arki","Agua"};
    readonly string[] efectos={"+10 resistencia · gasto −20 % durante 20 s","+20 resistencia · 2,8 s","+45 resistencia · 3,6 s","+30 resistencia · 3 s"};
    readonly float[] duraciones={2.2f,2.8f,3.6f,3f};
    readonly int[] recuperacion={10,20,45,30};
    readonly string[] charla={"WAYRA\nTraigo un aviso de la posta anterior. Busco al corredor que salió antes del amanecer.","ENCARGADO\nNo llegó a tomar el relevo. Un caminante lo vio seguir hacia la quebrada.","WAYRA\n¿Hacia el puente?","ENCARGADO\nSí. Y otros hombres preguntaron por él. Lleva este registro a la posta del otro lado. Allí podrán decirte más."};
    AudioPrimeraMision audioNivel;
    CharacterController cuerpo;
    AgacharseChasqui bajo;
    CamaraChasqui camara;
    ConsumoChaski gesto;
    CapaFisicaChasqui capaFisica;
    Transform pechoViento;
    Font titulo,texto;
    readonly System.Collections.Generic.Dictionary<int, GUIStyle> estilosCache = new System.Collections.Generic.Dictionary<int, GUIStyle>();
    GUIStyle botonCache;
    Texture2D manta;
    Camera mapa;RenderTexture imagenMapa;
    float siguienteMapa,quieto,coca,ultimoEsfuerzo,proximoPututu,intro=3,tiempoConsumo,mantenerE,tiempoDescanso;
    float relojViento;
    int etapa,dialogo=-1,elegido,consumiendo=-1;
    bool pausa,selector,anunciado,terminado,registroLeido,comentario,recogidaTecla;
    string aviso="",pistaContextual="",conversacion="";float avisoHasta,conversacionHasta;
    public string MensajeVisible=>Time.time<conversacionHasta?conversacion:Time.time<avisoHasta?aviso:pistaContextual;
    void Decir(string t){conversacion=t;conversacionHasta=Time.time+12;}
    public void HablarConCaminante(){comentario=true;Decir("CAMINANTE: Vi a dos hombres en la loma. Miraban a quienes pasaban, pero no bajaron al camino. Sigue con cuidado.");}
    Vector3 puntoControl,posicionConsumo;
    Quaternion vistaIntro;
    const string Prefijo="Chaski.Nivel03.";
    void Start()
    {
        Time.timeScale=1;audioNivel=GetComponent<AudioPrimeraMision>();cuerpo=jugador.GetComponent<CharacterController>();bajo=jugador.GetComponent<AgacharseChasqui>();camara=vista.GetComponent<CamaraChasqui>();capaFisica=jugador.GetComponentInChildren<CapaFisicaChasqui>();
        gesto=jugador.gameObject.AddComponent<ConsumoChaski>();jugador.SaltoIniciado+=GastarSalto;
        foreach(var t in jugador.GetComponentsInChildren<Transform>())if(t.name=="chest"){pechoViento=t;break;}
        titulo=FuenteCastellanaChaski.Obtener();texto=FuenteCastellanaChaski.Obtener();manta=InterfazIncaChasqui.CrearTejido();
        circulo.gameObject.SetActive(false);puntoControl=jugador.transform.position;
        etapa=Mathf.Clamp(PlayerPrefs.GetInt(Prefijo+"Control",0),0,controles.Length);
        if(etapa>0){puntoControl=controles[etapa-1].position;intro=0;jugador.Reubicar(puntoControl);}
        for(int i=0;i<4;i++)cantidad[i]=Mathf.Clamp(PlayerPrefs.GetInt(Prefijo+"Racion"+i,0),0,i==1?4:2);
        Resistencia=Mathf.Clamp(PlayerPrefs.GetFloat(Prefijo+"Resistencia",100),0,100);
        coca=Mathf.Clamp(PlayerPrefs.GetFloat(Prefijo+"Coca",0),0,20);registroLeido=PlayerPrefs.GetInt(Prefijo+"Registro",0)==1;
        foreach(var r in recursos)if(PlayerPrefs.GetInt(Prefijo+"Recogido."+r.identificador,0)==1)r.gameObject.SetActive(false);
        CrearMapa();vistaIntro=vista.transform.rotation;
        if(intro>0){Bloquear(true);Avisar("Dejas atrás la segunda posta. El aviso sigue en tu chuspa: cruza el valle hasta el siguiente pueblo.",8);}
    }
    public void EstablecerPausa(bool valor)
    {
        pausa=valor;Time.timeScale=valor?0:1;
        camara.enabled=!valor&&!EnInterfaz&&consumiendo<0;
        camara.CapturarRaton(false);
    }
    void GastarSalto(){Resistencia=Mathf.Max(0,Resistencia-3);ultimoEsfuerzo=Time.time;}
    void Update()
    {
        if(terminado)return;
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(selector){CerrarSelector();return;}
            EstablecerPausa(!pausa);
        }
        if(pausa){camara.CapturarRaton(false);return;}
        if(intro>0)
        {
            intro-=Time.deltaTime;var p=jugador.transform.position;
            vista.transform.position=Vector3.Lerp(p+new Vector3(-3,2,-2),p+new Vector3(0,2.3f,-4.5f),Mathf.SmoothStep(0,1,1-intro/3));vista.transform.LookAt(p+Vector3.up*1.1f);
            if(Input.GetKeyDown(KeyCode.E)||intro<=0){intro=0;Bloquear(false);Avisar("Sigue el sendero hasta la estación del valle. C protege del viento.",6);}return;
        }
        if(selector)
        {
            if(Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.UpArrow))elegido=(elegido+3)%4;
            if(Input.GetKeyDown(KeyCode.RightArrow)||Input.GetKeyDown(KeyCode.DownArrow))elegido=(elegido+1)%4;
            if(Input.GetKeyDown(KeyCode.E))Consumir(elegido);return;
        }
        if(dialogo>=0){if(Input.GetKeyDown(KeyCode.E))AvanzarDialogo();return;}
        var pos=jugador.transform.position;
        ActualizarViento(pos);
        if(pos.y<-3 || pos.x<80&&Mathf.Abs(pos.z-CentroAgua(pos.x))<3.4f&&pos.y<AguaY(pos.x)+.04f)
        {CancelarConsumo();jugador.Reubicar(puntoControl);Avisar("Vuelves al último abrigo. Las provisiones conservan su estado.",5);return;}
        if(consumiendo>=0)
        {
            if(QuiereMoverse()){CancelarConsumo();return;}
            tiempoConsumo+=Time.deltaTime;gesto.Actualizar(tiempoConsumo/duraciones[consumiendo]);
            if(tiempoConsumo>=duraciones[consumiendo])
            {
                int tipo=consumiendo;float antes=Resistencia;cantidad[tipo]--;if(tipo==0)coca=20;Resistencia=Mathf.Min(100,Resistencia+recuperacion[tipo]);
                CancelarConsumo();Guardar();Avisar(nombres[tipo]+" · +"+Mathf.RoundToInt(Resistencia-antes)+" resistencia · "+Mathf.RoundToInt(Resistencia)+" / 100",5);
            }
            return;
        }
        if(coca>0)coca=Mathf.Max(0,coca-Time.deltaTime);
        float rapidez=Vector3.ProjectOnPlane(cuerpo.velocity,Vector3.up).magnitude;
        bool carrera=jugador.enabled&&jugador.EnSuelo&&rapidez>3.1f;
        if(carrera){Resistencia=Mathf.Max(0,Resistencia-Time.deltaTime*(coca>0?1.2f:1.5f));ultimoEsfuerzo=Time.time;}
        jugador.limitarCarrera=Resistencia<=.1f || (jugador.limitarCarrera&&Resistencia<15);
        if(etapa<controles.Length && pos.z>=controles[etapa].position.z && jugador.EnSuelo)
        {puntoControl=controles[etapa].position;etapa++;Guardar();Avisar("Punto de control · mensaje a salvo",4);}
        pistaContextual="";bool contexto=false;
        RecursoValleVientos cercano=null;float distancia=3.2f;
        foreach(var r in recursos)
        {if(!r.gameObject.activeSelf||Mathf.Abs(pos.y-r.transform.position.y)>2)continue;float d=Vector2.Distance(new Vector2(pos.x,pos.z),new Vector2(r.transform.position.x,r.transform.position.z));if(d<distancia){cercano=r;distancia=d;}}
        if(responsableSalida!=null&&Vector3.Distance(pos,responsableSalida.position)<5)
        {
            contexto=true;pistaContextual="E · Hablar con la responsable de la posta";
            if(Input.GetKeyDown(KeyCode.E))Decir("RESPONSABLE: Continúa por el valle. El aviso ya está en tu chuspa. Pregunta por el relevista en la siguiente estación.");
        }
        else if(cercano==null&&agricultor!=null&&Vector3.Distance(pos,agricultor.position)<5)
        {
            contexto=true;pistaContextual="E · Hablar con el pastor";
            if(Input.GetKeyDown(KeyCode.E))Decir("PASTOR: El camino baja al valle. Más adelante el viento llega de costado: busca abrigo junto a las rocas.");
        }
        else if(cercano==null&&Vector3.Distance(pos,caminante.position)<5)
        {
            contexto=true;pistaContextual="E · Hablar con el caminante";
            if(Input.GetKeyDown(KeyCode.E))HablarConCaminante();
        }
        else if(cercano!=null)
        {
            contexto=true;pistaContextual="E · Recoger "+cercano.Nombre+" — "+efectos[cercano.tipo];
            if(Input.GetKeyDown(KeyCode.E))
            {
                int max=cercano.tipo==1?4:2;
                if(cantidad[cercano.tipo]>=max)Avisar("Tu chuspa ya tiene suficientes raciones de "+cercano.Nombre,3);
                else{cantidad[cercano.tipo]++;cercano.gameObject.SetActive(false);PlayerPrefs.SetInt(Prefijo+"Recogido."+cercano.identificador,1);Guardar();audioNivel.Confirmar();Avisar(cercano.Nombre+" guardado · mantén E para abrir la chuspa",5);}
            }
        }
        else if(Vector3.Distance(pos,descanso.position)<3)
        {
            contexto=true;pistaContextual="Refugio del viento · mantén E para abrir la chuspa";
        }
        else if(Vector3.Distance(pos,registro.position)<3)
        {
            contexto=true;pistaContextual="E · Registro de los relevos";
            if(Input.GetKeyDown(KeyCode.E)){registroLeido=true;Guardar();Avisar("LOS RELEVOS\nLos recados recorrían largas distancias mediante mensajeros que conocían sus caminos y postas.",12);}
        }
        else if(Vector3.Distance(pos,encargado.position)<16)
        {
            contexto=true;
            pistaContextual=anunciado?"Acércate al encargado y detente en el círculo de entrega":"E · Anunciar llegada con el pututu";
            if(Input.GetKeyDown(KeyCode.E)&&Time.time>proximoPututu&&jugador.enabled)
            {
                proximoPututu=Time.time+(audioNivel.pututu!=null?audioNivel.pututu.length:11)+3;
                var tocar=jugador.GetComponent<TocarPututuChaski>();if(tocar==null)tocar=jugador.gameObject.AddComponent<TocarPututuChaski>();tocar.Iniciar(audioNivel);
                anunciado=true;circulo.gameObject.SetActive(true);Avisar("[Suena el pututu] · Guarda la caracola y detente en el círculo.",12);
            }
        }
        mantenerE=Input.GetKey(KeyCode.E)&&jugador.enabled&&jugador.EnSuelo?mantenerE+Time.deltaTime:0;
        if(mantenerE>.6f){mantenerE=0;modoRatonChuspa=camara.RatonCapturado;selector=true;Bloquear(true);Time.timeScale=0;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
        quieto=anunciado&&etapa==controles.Length&&jugador.enabled&&jugador.EnSuelo&&Vector3.Distance(pos,circulo.position)<1.4f&&rapidez<.1f?quieto+Time.deltaTime:0;
        if(quieto>=1){dialogo=0;Bloquear(true);}
    }
    void ActualizarViento(Vector3 p)
    {
        relojViento+=Time.deltaTime;float fase=Mathf.Repeat(relojViento,10);
        float fuerza=fase<1.5f?Mathf.SmoothStep(0,1,fase/1.5f):fase<4.5f?1:1-Mathf.SmoothStep(0,1,(fase-4.5f)/1.5f);
        bool expuesto=p.z>115&&p.z<230 || p.z>360&&p.z<425;
        Protegido=false;
        foreach(var abrigo in abrigos)if(Mathf.Abs(p.z-abrigo.position.z)<3.2f && p.x<abrigo.position.x && p.x>abrigo.position.x-5)Protegido=true;
        Rafaga=expuesto&&!Protegido?fuerza:0;
        var capa=capaFisica;
        if(capa!=null)capa.vientoAmbiente=Vector3.left*(Rafaga*5)+Vector3.up*Rafaga*.6f;
        jugador.empujeExterior=jugador.enabled?Vector3.left*(Rafaga*(bajo.Agachado?.10f:.42f)):Vector3.zero;
        if(viento!=null)viento.volume=Mathf.Lerp(.045f,.13f,expuesto&&!Protegido?fuerza:.1f);
    }
    bool modoRatonChuspa;
    public void Consumir(int tipo)
    {
        if(cantidad[tipo]<=0){Avisar("No tienes "+nombres[tipo],3);return;}
        if(tipo==0&&coca>0){Avisar("La coca sigue activa.",3);return;}
        if(tipo!=0&&Resistencia>=99.9f){Avisar("La resistencia ya está completa.",3);return;}
        if(!jugador.EnSuelo||Rafaga>.45f){Avisar("Busca suelo seguro y abrigo para consumir.",3);return;}
        bool raton=selector?modoRatonChuspa:camara.RatonCapturado;CerrarSelector();consumiendo=tipo;tiempoConsumo=0;posicionConsumo=jugador.transform.position;Bloquear(true);camara.CapturarRaton(raton);gesto.Mostrar(tipo);
    }
    void CancelarConsumo(){consumiendo=-1;tiempoConsumo=0;gesto.Ocultar();if(!EnInterfaz)Bloquear(false);}
    void CerrarSelector(){selector=false;Time.timeScale=1;Bloquear(false);}
    void Bloquear(bool valor)
    {
        jugador.enabled=!valor;camara.enabled=!valor;
        if(valor){jugador.empujeExterior=Vector3.zero;if(jugador.animador!=null)jugador.animador.SetFloat("Movimiento",0);Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
    }
    void AvanzarDialogo()
    {
        audioNivel.Confirmar();
        if(++dialogo<charla.Length)return;
        terminado=true;PlayerPrefs.SetInt(Prefijo+"Completado",1);Guardar();audioNivel.Completar();
    }
    void Guardar()
    {
        PlayerPrefs.SetFloat(Prefijo+"Resistencia",Resistencia);
        PlayerPrefs.SetFloat("Chaski.Campana.Resistencia",Resistencia);PlayerPrefs.SetFloat("Chaski.Campana.Coca",coca);for(int i=0;i<4;i++)PlayerPrefs.SetInt("Chaski.Campana.Racion"+i,cantidad[i]);
        PlayerPrefs.SetInt(Prefijo+"Control",etapa);PlayerPrefs.SetInt(Prefijo+"Registro",registroLeido?1:0);
        for(int i=0;i<4;i++)PlayerPrefs.SetInt(Prefijo+"Racion"+i,cantidad[i]);PlayerPrefs.SetFloat(Prefijo+"Coca",coca);PlayerPrefs.Save();
    }
    void Avisar(string t,float segundos){aviso=t;avisoHasta=Time.time+segundos;}
    static bool QuiereMoverse()=>Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.UpArrow)||Input.GetKey(KeyCode.DownArrow)||Input.GetKey(KeyCode.LeftArrow)||Input.GetKey(KeyCode.RightArrow)||Input.GetKey(KeyCode.Space);
    void CrearMapa()
    {
        mapa=new GameObject("Minimapa · valle de los vientos").AddComponent<Camera>();mapa.enabled=false;mapa.orthographic=true;mapa.orthographicSize=275;mapa.aspect=1;mapa.farClipPlane=260;mapa.nearClipPlane=.1f;mapa.allowHDR=false;mapa.allowMSAA=false;
        mapa.transform.SetPositionAndRotation(new Vector3(0,170,250),Quaternion.Euler(90,0,0));imagenMapa=new RenderTexture(384,384,16);mapa.targetTexture=imagenMapa;
    }
    void LateUpdate()
    {
        if(pausa||selector||terminado||dialogo>=0){camara.enabled=false;camara.CapturarRaton(false);}
        if(mapa!=null&&Time.unscaledTime>siguienteMapa){siguienteMapa=Time.unscaledTime+.35f;mapa.Render();}
        if(!EnInterfaz&&consumiendo<0&&Rafaga>0&&jugador.animador!=null)
        {
            var pecho=pechoViento;
            if(pecho!=null)pecho.rotation=Quaternion.AngleAxis(Rafaga*4,jugador.visual.forward)*pecho.rotation;
        }
    }
    GUIStyle Estilo(int n,bool grande=false){int key=(n<<1)|(grande?1:0);if(!estilosCache.TryGetValue(key,out var st)){st=new GUIStyle(GUI.skin.label){font=grande?titulo:texto,fontSize=n,wordWrap=true,normal={textColor=new Color(1,.91f,.73f)}};estilosCache[key]=st;}return st;}
    static void Panel(Rect r){var c=GUI.color;GUI.color=new Color(.12f,.06f,.025f,.94f);GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=c;}
    bool Boton(Rect r,string t){if(botonCache==null)botonCache=new GUIStyle(GUI.skin.button){font=texto,fontSize=22};return GUI.Button(r,t,botonCache);}
    void OnGUI()
    {
        var antes=GUI.matrix;float s=Mathf.Min(Screen.width/1400f,Screen.height/850f);GUI.matrix=Matrix4x4.Scale(Vector3.one*s);float w=Screen.width/s,h=Screen.height/s;
        if(terminado||pausa||selector||dialogo>=0)
        {
            Panel(new Rect(0,0,w,h));var r=new Rect(w/2-460,h/2-285,920,570);GUI.DrawTexture(r,manta);Panel(new Rect(r.x+35,r.y+35,r.width-70,r.height-70));
            if(terminado)
            {
                GUI.Label(new Rect(r.x+65,r.y+65,790,60),"AVISO ENTREGADO",Estilo(34,true));
                GUI.Label(new Rect(r.x+65,r.y+155,790,150),"El relevista no llegó al valle.\nSu rastro continúa hacia la quebrada.\nUn nuevo registro te espera al otro lado del puente.",Estilo(26));
                if(Boton(new Rect(r.x+60,r.y+350,250,60),"Repetir nivel"))Reiniciar();
                if(Boton(new Rect(r.x+335,r.y+350,250,60),"Menú principal"))Cargar("InicioChaski");
                GUI.enabled=Application.CanStreamedLevelBeLoaded("Nivel04_PuenteAbismo");if(Boton(new Rect(r.x+610,r.y+350,250,60),"Siguiente nivel"))SiguienteNivel();GUI.enabled=true;
                GUI.Label(new Rect(r.x+65,r.y+445,790,60),"Nivel 4 en preparación · Registro: "+(registroLeido?"encontrado":"pendiente"),Estilo(21));
            }
            else if(pausa)
            {
                GUI.Label(new Rect(r.x+65,r.y+65,790,60),"DESCANSO EN EL CAMINO",Estilo(32,true));
                if(Boton(new Rect(r.x+200,r.y+180,520,65),"Reanudar [Esc]")){EstablecerPausa(false);}
                if(Boton(new Rect(r.x+200,r.y+265,520,65),"Volver al punto de control")){EstablecerPausa(false);CancelarConsumo();jugador.Reubicar(puntoControl);}
                if(Boton(new Rect(r.x+200,r.y+350,520,65),"Menú principal")){Guardar();Cargar("InicioChaski");}
            }
            else if(selector)
            {
                GUI.Label(new Rect(r.x+65,r.y+55,790,45),"CHUSPA · ELIGE UNA RACIÓN",Estilo(30,true));
                for(int i=0;i<4;i++)
                {
                    string t=(elegido==i?"→ ":"")+nombres[i]+" × "+cantidad[i]+" · "+efectos[i];
                    if(Boton(new Rect(r.x+65,r.y+120+i*72,790,60),t)){elegido=i;Consumir(i);}
                }
                GUI.Label(new Rect(r.x+65,r.y+430,790,40),avisoHasta>Time.time?aviso:"Flechas · elegir    E · consumir    Esc · cerrar",Estilo(21));
                if(Boton(new Rect(r.x+310,r.y+480,300,45),"Cerrar"))CerrarSelector();
            }
            else
            {
                GUI.Label(new Rect(r.x+65,r.y+90,790,260),charla[dialogo],Estilo(28));
                if(Boton(new Rect(r.x+290,r.y+405,340,65),"Continuar [E]"))AvanzarDialogo();
            }
        }
        else
        {
            Panel(new Rect(18,18,w-280,120));GUI.Label(new Rect(35,30,w-320,38),"03 · EL VALLE DE LOS VIENTOS",Estilo(28,true));
            string pista=Time.time<conversacionHasta?conversacion:Time.time<avisoHasta?aviso:Rafaga>.7f?"Ráfaga fuerte · mantén C y busca abrigo":Protegido?"Estás al abrigo del viento":"Lleva el aviso a la posta de la quebrada y pregunta por el relevista.";
            GUI.Label(new Rect(35,75,w-320,64),pista,Estilo(21));
            if(!string.IsNullOrEmpty(pistaContextual)){Panel(new Rect(w/2-400,h-205,800,55));GUI.Label(new Rect(w/2-385,h-195,770,45),pistaContextual,Estilo(22,true));}
            if(intro>0)GUI.Label(new Rect(35,150,800,55),"E · Omitir introducción",Estilo(22));
            var r=new Rect(w-245,18,225,255);Panel(r);GUI.Label(new Rect(r.x+12,26,205,25),"QUEBRADA · N ↑",Estilo(17,true));
            var area=new Rect(r.x+12,60,201,170);if(imagenMapa!=null)GUI.DrawTexture(area,imagenMapa,ScaleMode.StretchToFill);Marcador(area,jugador.transform.position,Color.white);Marcador(area,circulo.position,new Color(1,.77f,.15f));
            GUI.Label(new Rect(r.x+12,235,205,30),Mathf.CeilToInt(Vector3.Distance(jugador.transform.position,circulo.position))+" m · Posta",Estilo(20));
            Panel(new Rect(20,h-132,410,65));GUI.Label(new Rect(30,h-128,390,26),"Resistencia · "+Mathf.RoundToInt(Resistencia)+" / 100",Estilo(19));
            GUI.color=new Color(.65f,.74f,.3f);GUI.DrawTexture(new Rect(32,h-93,Mathf.Max(0,Resistencia)*3.8f,12),Texture2D.whiteTexture);GUI.color=Color.white;
            if(coca>0)GUI.Label(new Rect(450,h-125,490,35),"Coca · esfuerzo −20 % · "+Mathf.CeilToInt(coca)+" s",Estilo(21));
            if(consumiendo>=0)GUI.Label(new Rect(450,h-90,550,35),"Usando "+nombres[consumiendo]+" · moverte cancela",Estilo(21));
            Panel(new Rect(20,h-55,w-40,36));GUI.Label(new Rect(30,h-52,w-60,35),"WASD / Flechas · Mover    Ratón · Mirar    Espacio · Saltar    C · Agacharse    E · Interactuar / mantener para chuspa    Esc · Pausa",Estilo(17));
        }
        GUI.matrix=antes;
    }
    void Marcador(Rect r,Vector3 p,Color color){if(mapa==null)return;var q=mapa.WorldToViewportPoint(p);GUI.color=color;GUI.DrawTexture(new Rect(r.x+q.x*r.width-4,r.y+(1-q.y)*r.height-4,8,8),Texture2D.whiteTexture);GUI.color=Color.white;}
    void Reiniciar()
    {
        PlayerPrefs.DeleteKey(Prefijo+"Control");PlayerPrefs.DeleteKey(Prefijo+"Coca");PlayerPrefs.DeleteKey(Prefijo+"Resistencia");for(int i=0;i<4;i++)PlayerPrefs.DeleteKey(Prefijo+"Racion"+i);
        foreach(var r in recursos)PlayerPrefs.DeleteKey(Prefijo+"Recogido."+r.identificador);PlayerPrefs.Save();Cargar("Nivel03_ValleVientos");
    }
    public void SiguienteNivel()
    {
        Guardar();var p=jugador.transform.position;PlayerPrefs.SetFloat("Chaski.Enlace34.X",p.x);PlayerPrefs.SetFloat("Chaski.Enlace34.Y",p.y);PlayerPrefs.SetFloat("Chaski.Enlace34.Z",p.z);
        PlayerPrefs.SetFloat("Chaski.Enlace34.GiroPersonaje",jugador.visual.eulerAngles.y);PlayerPrefs.SetFloat("Chaski.Enlace34.GiroCamara",camara.Giro);PlayerPrefs.SetFloat("Chaski.Enlace34.Inclinacion",camara.Inclinacion);PlayerPrefs.SetInt("Chaski.Enlace34.Pendiente",1);PlayerPrefs.Save();Cargar("Nivel04_PuenteAbismo");
    }
    static void Cargar(string n){Time.timeScale=1;SceneManager.LoadScene(n);}
    void OnDestroy()
    {
        if(jugador!=null){jugador.SaltoIniciado-=GastarSalto;jugador.empujeExterior=Vector3.zero;jugador.limitarCarrera=false;}
        Time.timeScale=1;if(manta!=null)Destroy(manta);if(mapa!=null)Destroy(mapa.gameObject);if(imagenMapa!=null){imagenMapa.Release();Destroy(imagenMapa);}
    }
}
