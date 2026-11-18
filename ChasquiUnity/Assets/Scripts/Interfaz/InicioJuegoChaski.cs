using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class InicioJuegoChaski:MonoBehaviour
{
    public Texture2D portada,menu;
    public Texture2D[] historieta;
    public AudioClip musica,click;
    Font titulo,texto;
    Texture2D tejido;
    AudioSource audioMenu;
    int pantalla,pagina;
    bool comenzar;
    float volumen;
    readonly string[][] dialogos={
        new[]{"Al amanecer, Wayra se presenta en el chaskiwasi.","RELEVISTA: Recibe este quipu y lleva el recado a la siguiente posta.","El mensaje también se transmite de palabra. Wayra escucha con atención."},
        new[]{"RELEVISTA: El paso del río está abierto. Ya pueden enviar las provisiones.","RELEVISTA: Sigue el sendero y anuncia tu llegada con el pututu.","WAYRA: Así lo haré. El mensaje llegará a la próxima posta."},
        new[]{"Wayra guarda el quipu en su chuspa y se despide.","El relevista regresa al chaskiwasi y aparta la estera de la entrada.","Una nueva misión comienza entre caminos, valles y puentes del Tahuantinsuyo."}
    };
    void Start()
    {
        Time.timeScale=1;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
        titulo=Resources.Load<Font>("EstiloInca/TituloTradicional");texto=Resources.Load<Font>("EstiloInca/TextoTradicional");
        tejido=InterfazIncaChasqui.CrearTejido();
        volumen=PlayerPrefs.GetFloat("Chaski.Audio.Volumen",.8f);AudioListener.volume=volumen;
        audioMenu=gameObject.AddComponent<AudioSource>();audioMenu.clip=musica;audioMenu.loop=true;audioMenu.volume=.18f;if(musica!=null)audioMenu.Play();
    }
    void Update()
    {
        if(pantalla==0 && Input.anyKeyDown)pantalla=1;
        else if(Input.GetKeyDown(KeyCode.Escape)){if(pantalla==4)GuardarOpciones();pantalla=1;}
        if(pantalla==3)
        {
            if(Input.GetKeyDown(KeyCode.RightArrow))Avanzar();
            if(Input.GetKeyDown(KeyCode.LeftArrow))pagina=Mathf.Max(0,pagina-1);
        }
    }
    GUIStyle Estilo(int n,bool destacado=false,TextAnchor alineacion=TextAnchor.UpperLeft)
    {
        var s=new GUIStyle(GUI.skin.label){font=destacado?titulo:texto,fontSize=n,wordWrap=true,alignment=alineacion};
        s.normal.textColor=new Color(.99f,.91f,.73f);return s;
    }
    bool Boton(Rect r,string mensaje,bool habilitado=true)
    {
        bool antes=GUI.enabled;GUI.enabled=habilitado;
        var s=new GUIStyle(GUI.skin.button){font=texto,fontSize=24};
        s.normal.textColor=new Color(1,.89f,.65f);s.hover.textColor=Color.white;
        bool pulsado=GUI.Button(r,mensaje,s);GUI.enabled=antes;
        if(pulsado&&click!=null)audioMenu.PlayOneShot(click,.4f);return pulsado;
    }
    static void Oscurecer(Rect r,float alpha)
    {GUI.color=new Color(.055f,.025f,.01f,alpha);GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=Color.white;}
    void OnGUI()
    {
        var anterior=GUI.matrix;float escala=Mathf.Min(Screen.width/1600f,Screen.height/900f);
        GUI.matrix=Matrix4x4.Scale(Vector3.one*escala);float w=Screen.width/escala,h=Screen.height/escala;
        var fondo=pantalla==0?portada:(menu!=null?menu:portada);
        if(fondo!=null)GUI.DrawTexture(new Rect(0,0,w,h),fondo,ScaleMode.ScaleAndCrop);
        if(pantalla==0)
        {
            Oscurecer(new Rect(0,0,w*.49f,h),.36f);
            GUI.Label(new Rect(65,h*.24f,w*.43f,115),"CHASKI",Estilo(96,true));
            GUI.Label(new Rect(70,h*.24f+120,w*.43f,85),"EL ÚLTIMO MENSAJE",Estilo(37,true));
            GUI.Label(new Rect(70,h*.74f,w*.43f,60),"Pulsa una tecla para comenzar",Estilo(26));
        }
        else if(pantalla==1)Menu(w,h);
        else if(pantalla==2)Historia(w,h);
        else if(pantalla==3)Historieta(w,h);
        else if(pantalla==4)Opciones(w,h);
        GUI.matrix=anterior;
    }
    void Marco(Rect r,string nombre)
    {
        GUI.DrawTexture(r,tejido);Oscurecer(new Rect(r.x+23,r.y+23,r.width-46,r.height-46),.83f);
        GUI.Label(new Rect(r.x+45,r.y+34,r.width-90,60),nombre,Estilo(32,true));
    }
    void Menu(float w,float h)
    {
        var r=new Rect(36,30,480,h-60);Marco(r,"CHASKI");
        GUI.Label(new Rect(81,116,390,65),"EL ÚLTIMO MENSAJE",Estilo(23,true));
        bool continuar=PlayerPrefs.GetInt("Chaski.Campana.Iniciada",0)==1 || PlayerPrefs.GetInt("Chaski.PrimeraMisionCompletada",0)==1 || PlayerPrefs.HasKey("Chaski.Nivel02.Control");
        if(Boton(new Rect(81,215,390,62),"Continuar",continuar))Cargar(PlayerPrefs.GetInt("Chaski.PrimeraMisionCompletada",0)==1?"Nivel02_PrimerRecorrido":"Nivel01_PrimeraMision");
        if(Boton(new Rect(81,291,390,62),"Nueva partida")){comenzar=true;pagina=0;pantalla=3;}
        if(Boton(new Rect(81,367,390,62),"Historia"))pantalla=2;
        if(Boton(new Rect(81,443,390,62),"Historieta")){comenzar=false;pagina=0;pantalla=3;}
        if(Boton(new Rect(81,519,390,62),"Opciones"))pantalla=4;
        if(Boton(new Rect(81,595,390,62),"Salir"))
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
            #else
            Application.Quit();
            #endif
        }
        GUI.Label(new Rect(81,h-138,390,62),"Una aventura de Wayra por los caminos del Tahuantinsuyo",Estilo(18));
    }
    void Historia(float w,float h)
    {
        var r=new Rect(w*.09f,45,w*.82f,h-90);Marco(r,"EL CAMINO DE WAYRA");
        string historia="En los Andes del siglo XV, las postas y sus relevos mantienen vivos los mensajes del Tahuantinsuyo. Wayra, un joven corredor de una comunidad cercana a Cusco, está a punto de recibir su primer encargo.\n\nSu camino comienza entre viviendas y cultivos. Más adelante deberá llevar un quipu y un recado oral a la siguiente posta: el paso del río está abierto y pueden enviar provisiones.\n\nPero al llegar descubrirá que el corredor encargado del relevo todavía no ha regresado. La búsqueda de una respuesta lo conducirá hacia los valles y los puentes del camino.\n\nCada entrega es una responsabilidad. Cada posta abre una nueva pregunta.\n\nWayra y su aventura son ficticios. El paisaje y el sistema de relevos inspiran el mundo del juego.";
        GUI.Label(new Rect(r.x+55,r.y+115,r.width-110,r.height-240),historia,Estilo(25));
        if(Boton(new Rect(r.x+55,r.yMax-100,240,55),"Volver"))pantalla=1;
        if(Boton(new Rect(r.xMax-355,r.yMax-100,300,55),"Ver historieta")){comenzar=false;pagina=0;pantalla=3;}
    }
    void Historieta(float w,float h)
    {
        Oscurecer(new Rect(0,0,w,h),.92f);
        GUI.Label(new Rect(40,14,w-80,46),"EL PRIMER ENCARGO · "+(pagina+1)+" / 3",Estilo(27,true));
        var area=new Rect(40,70,w-80,h-340);
        if(historieta!=null&&pagina<historieta.Length)GUI.DrawTexture(area,historieta[pagina],ScaleMode.ScaleToFit);
        float ancho=(w-110)/3;
        for(int i=0;i<3;i++)
        {
            var r=new Rect(40+i*(ancho+15),h-250,ancho,140);Oscurecer(r,.8f);
            GUI.Label(new Rect(r.x+12,r.y+10,r.width-24,r.height-20),(i+1)+" · "+dialogos[pagina][i],Estilo(22));
        }
        if(Boton(new Rect(40,h-87,220,55),"Volver al menú")){pantalla=1;comenzar=false;}
        if(Boton(new Rect(w/2-250,h-87,220,55),"← Anterior",pagina>0))pagina--;
        if(Boton(new Rect(w/2+30,h-87,220,55),pagina==2?(comenzar?"Comenzar →":"Terminar →"):"Siguiente →"))Avanzar();
        if(comenzar&&Boton(new Rect(w-260,h-87,220,55),"Omitir y jugar"))Nueva();
    }
    void Avanzar(){if(pagina<2)pagina++;else if(comenzar)Nueva();else pantalla=1;}
    void Nueva()
    {
        foreach(string clave in new[]{"Chaski.PrimeraMisionCompletada","Chaski.Nivel02.Control","Chaski.Nivel02.X","Chaski.Nivel02.Y","Chaski.Nivel02.Z","Chaski.Nivel02.Completado","Chaski.Nivel02.Registro"})PlayerPrefs.DeleteKey(clave);
        PlayerPrefs.SetInt("Chaski.Campana.Iniciada",1);PlayerPrefs.Save();Cargar("Nivel01_PrimeraMision");
    }
    void Opciones(float w,float h)
    {
        var r=new Rect(w/2-390,h/2-280,780,560);Marco(r,"OPCIONES");
        GUI.Label(new Rect(r.x+60,r.y+125,660,45),"Volumen general · "+Mathf.RoundToInt(volumen*100)+" %",Estilo(25));
        volumen=GUI.HorizontalSlider(new Rect(r.x+60,r.y+190,660,35),volumen,0,1);AudioListener.volume=volumen;
        GUI.Label(new Rect(r.x+60,r.y+255,660,160),"Nivel 1: W / ↑ avanzar · S / ↓ retroceder\nNivel 2: WASD / flechas · ratón para mirar\nEspacio: saltar · C: agacharse\nE: interactuar / pututu · Esc: pausa",Estilo(23));
        if(Boton(new Rect(r.x+230,r.y+450,320,55),"Guardar y volver")){GuardarOpciones();pantalla=1;}
    }
    void GuardarOpciones(){PlayerPrefs.SetFloat("Chaski.Audio.Volumen",volumen);PlayerPrefs.Save();}
    static void Cargar(string nombre){Time.timeScale=1;SceneManager.LoadScene(nombre);}
    void OnDestroy(){if(tejido!=null)Destroy(tejido);}
}
