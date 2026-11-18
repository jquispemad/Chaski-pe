using UnityEngine;

[RequireComponent(typeof(NivelValleChasqui))]
public sealed class InterfazIncaChasqui : MonoBehaviour
{
    NivelValleChasqui nivel;
    Font titulo,lectura;
    Texture2D tejido,iconoQuipu,repetir,casa,siguiente;
    GUIStyle grande,medio,texto,pequeno;
    bool estilos;
    public bool VistaFinalEditor;
    static readonly Color Marfil=new Color(.98f,.89f,.68f);
    void Awake()
    {
        nivel=GetComponent<NivelValleChasqui>();
        titulo=Resources.Load<Font>("EstiloInca/TituloTradicional");
        lectura=Resources.Load<Font>("EstiloInca/TextoTradicional");
        tejido=CrearTejido();iconoQuipu=Icono(0);repetir=Icono(1);casa=Icono(2);siguiente=Icono(3);
    }
    void Preparar()
    {
        if(estilos)return;
        grande=Estilo(titulo,34,TextAnchor.MiddleCenter);
        medio=Estilo(titulo,21,TextAnchor.MiddleLeft);
        texto=Estilo(lectura,19,TextAnchor.MiddleLeft);
        pequeno=Estilo(lectura,16,TextAnchor.MiddleLeft);estilos=true;
    }
    static GUIStyle Estilo(Font font,int tam,TextAnchor alineacion)
    {return new GUIStyle(GUI.skin.label){font=font,fontSize=tam,alignment=alineacion,wordWrap=true,normal={textColor=Marfil},padding=new RectOffset(0,0,0,0)};}
    static void ColorRect(Rect r,Color c){GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=Color.white;}
    void Panel(Rect r)
    {
        ColorRect(new Rect(r.x+7,r.y+9,r.width,r.height),new Color(.025f,.012f,.008f,.45f));
        GUI.DrawTexture(r,tejido,ScaleMode.StretchToFill);
        // Loose wool tassels below the embroidered edge.
        for(float x=r.x+12;x<r.xMax-8;x+=9)
        {
            float largo=7+3*Mathf.Sin(x*1.9f);
            ColorRect(new Rect(x,r.yMax,2,largo),new Color(.65f,.43f,.22f));
            ColorRect(new Rect(x+2,r.yMax,1,largo-1),new Color(.91f,.72f,.40f));
        }
    }
    public void Dibujar(float w,float h)
    {
        Preparar();
        if(nivel.Completado||VistaFinalEditor){Final(w,h);return;}
        Panel(new Rect(24,24,380,122));
        GUI.Label(new Rect(47,45,335,29),"EL CAMINO DEL VALLE",medio);
        GUI.Label(new Rect(47,82,335,44),"Entrega el mensaje en el tambo\nQuipus "+nivel.Recogidos+" / "+nivel.recuerdos.Length+"   ·   Hitos "+(nivel.UltimoControl+1)+" / "+nivel.controles.Length,pequeno);
        float distancia=Vector3.Distance(nivel.jugador.transform.position,nivel.llegada.position);
        Panel(new Rect(w-248,24,224,63));GUI.Label(new Rect(w-227,39,190,30),"TAMBO  ·  "+Mathf.CeilToInt(distancia)+" m",medio);
        // A narrow woven ribbon keeps keyboard instructions clear without competing with the scene.
        ColorRect(new Rect(0,h-47,w,47),new Color(.09f,.055f,.035f,.88f));
        GUI.Label(new Rect(25,h-37,w-50,24),"WASD / Flechas · Mover     Espacio · Saltar     C · Agacharse     Ratón · Mirar     Esc · Cursor     R · Volver",pequeno);
        if(nivel.MostrarAviso)
        {
            Rect r=new Rect(w*.5f-320,h-142,640,72);Panel(r);
            GUI.DrawTexture(new Rect(r.x+22,r.y+17,38,38),iconoQuipu);
            GUI.Label(new Rect(r.x+79,r.y+17,r.width-101,38),nivel.AvisoActual,texto);
        }
        else if(Cursor.lockState!=CursorLockMode.Locked)
            GUI.Label(new Rect(w*.5f-220,h-92,440,26),"Haz clic en el juego para mirar con el ratón",pequeno);
    }
    void OnGUI()
    {
        if(nivel==null||nivel.jugador==null)return;
        Matrix4x4 anterior=GUI.matrix;
        float escala=Mathf.Min(Screen.width/1600f,Screen.height/900f);
        GUI.matrix=Matrix4x4.Scale(Vector3.one*escala);
        Dibujar(Screen.width/escala,Screen.height/escala);GUI.matrix=anterior;
    }
    void Final(float w,float h)
    {
        ColorRect(new Rect(0,0,w,h),new Color(.025f,.018f,.012f,.62f));
        Rect r=new Rect(w*.5f-420,h*.5f-265,840,530);Panel(r);
        var centro=new GUIStyle(texto){alignment=TextAnchor.MiddleCenter};
        var sello=new GUIStyle(pequeno){alignment=TextAnchor.MiddleCenter,normal={textColor=new Color(.85f,.62f,.32f)}};
        GUI.Label(new Rect(r.x+75,r.y+54,690,28),"Q H A P A Q   Ñ A N   /   N I V E L   0 1",sello);
        GUI.Label(new Rect(r.x+60,r.y+102,720,54),"MENSAJE ENTREGADO",grande);
        ColorRect(new Rect(r.x+245,r.y+174,350,2),new Color(.74f,.45f,.20f));
        GUI.Label(new Rect(r.x+90,r.y+197,660,56),"El tambo recibe tu mensaje.\nHas completado el camino del valle.",centro);
        GUI.DrawTexture(new Rect(r.center.x-86,r.y+269,35,35),iconoQuipu);
        GUI.Label(new Rect(r.center.x-39,r.y+270,225,35),"Quipus  "+nivel.Recogidos+" / "+nivel.recuerdos.Length,medio);
        BotonVisual(new Rect(r.x+54,r.y+344,230,99),repetir,"Reanudar nivel");
        BotonVisual(new Rect(r.x+305,r.y+344,230,99),casa,"Inicio");
        BotonVisual(new Rect(r.x+556,r.y+344,230,99),siguiente,"Siguiente nivel");
        GUI.Label(new Rect(r.x+80,r.y+466,680,26),"DEL VALLE AL TAMBO · MENSAJE A SALVO",sello);
    }
    public void VerFinal(float w,float h){if(nivel==null)Awake();Preparar();Final(w,h);}
    void BotonVisual(Rect r,Texture2D icono,string etiqueta)
    {
        // These are deliberately decorative: navigation is not implemented yet.
        ColorRect(r,new Color(.10f,.065f,.043f));
        Color borde=new Color(.65f,.42f,.21f);
        ColorRect(new Rect(r.x,r.y,r.width,2),borde);ColorRect(new Rect(r.x,r.yMax-2,r.width,2),borde);
        ColorRect(new Rect(r.x,r.y,2,r.height),borde);ColorRect(new Rect(r.xMax-2,r.y,2,r.height),borde);
        GUI.DrawTexture(new Rect(r.center.x-18,r.y+12,36,36),icono);
        var estilo=new GUIStyle(texto){alignment=TextAnchor.MiddleCenter,fontSize=18};
        GUI.Label(new Rect(r.x+6,r.y+57,r.width-12,28),etiqueta,estilo);
    }
    public static Texture2D CrearTejido()
    {
        const int W=840,H=530;var t=new Texture2D(W,H,TextureFormat.RGB24,false);t.name="Manta bordada · cochinilla e ichu";
        var p=new Color[W*H];
        Color rojo=new Color(.26f,.065f,.045f),crema=new Color(.79f,.58f,.30f),azul=new Color(.075f,.13f,.14f);
        for(int y=0;y<H;y++)for(int x=0;x<W;x++)
        {
            int d=Mathf.Min(Mathf.Min(x,W-1-x),Mathf.Min(y,H-1-y));
            Color c=rojo;
            if(d<5)c=azul;else if(d<9)c=crema;else if(d<34)
            {
                bool horizontal=y<34||y>H-35;int a=horizontal?x:y,b=horizontal?(y<34?y:H-1-y):(x<34?x:W-1-x);
                int u=a%40;int escalon=(Mathf.Abs(u-20)/5)*3;
                c=Mathf.Abs(b-(15+escalon))<3?crema:azul;
            }
            else if(d<38)c=crema;else if(d<42)c=new Color(.14f,.035f,.023f);
            float hilo=((x%3==0)?.045f:0)+((y%3==0)?-.04f:0);
            float grano=(Mathf.PerlinNoise(x*.74f,y*.74f)-.5f)*.08f;
            c*=1+hilo+grano+.018f*Mathf.Sin(x*.13f);p[y*W+x]=c;
        }
        t.SetPixels(p);t.Apply();t.wrapMode=TextureWrapMode.Clamp;t.filterMode=FilterMode.Bilinear;return t;
    }
    static Texture2D Icono(int tipo)
    {
        const int N=64;var t=new Texture2D(N,N,TextureFormat.RGBA32,false);var p=new Color[N*N];
        for(int y=0;y<N;y++)for(int x=0;x<N;x++)
        {
            Vector2 v=new Vector2(x,y);bool tinta=false;
            if(tipo==0)
            {
                tinta=Segmento(v,new Vector2(11,49),new Vector2(53,49))<2;
                for(int i=0;i<5;i++){float a=15+i*8,b=12+Mathf.Abs(i-2)*4;tinta|=Segmento(v,new Vector2(a,48),new Vector2(a,b))<1.6f;for(int k=0;k<2;k++)tinta|=(v-new Vector2(a,b+5+k*8)).magnitude<3;}
            }
            else if(tipo==1){float a=Mathf.Atan2(y-32,x-32);tinta=Mathf.Abs((v-new Vector2(32,32)).magnitude-20)<2&&!(a>.9f&&a<2.0f);tinta|=Segmento(v,new Vector2(19,51),new Vector2(18,38))<2||Segmento(v,new Vector2(19,51),new Vector2(32,49))<2;}
            else if(tipo==2){tinta=Segmento(v,new Vector2(9,33),new Vector2(32,54))<2||Segmento(v,new Vector2(32,54),new Vector2(55,33))<2; tinta|=Segmento(v,new Vector2(16,36),new Vector2(16,12))<2||Segmento(v,new Vector2(48,36),new Vector2(48,12))<2||Segmento(v,new Vector2(16,12),new Vector2(48,12))<2;tinta|=(x>27&&x<37&&y>12&&y<29);}
            else tinta=Segmento(v,new Vector2(11,32),new Vector2(51,32))<2||Segmento(v,new Vector2(36,48),new Vector2(52,32))<2||Segmento(v,new Vector2(36,16),new Vector2(52,32))<2;
            p[y*N+x]=tinta?Marfil:Color.clear;
        }
        t.SetPixels(p);t.Apply();return t;
    }
    static float Segmento(Vector2 p,Vector2 a,Vector2 b){Vector2 d=b-a;return Vector2.Distance(p,a+d*Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude));}
    void OnDestroy(){foreach(var t in new[]{tejido,iconoQuipu,repetir,casa,siguiente})if(t!=null)Destroy(t);}
}
