using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(45)]
public sealed class NivelCuatroChaski:MonoBehaviour
{
 public Vector3 desplazamientoDesdeNivel3;
 public ChasquiMovimiento jugador;public Camera vista;public Transform responsable,puente,hallazgo,tejido,registro;public Transform[] controles;public RecursoValleVientos[] recursos;public AudioSource rio,viento;
 public static float Centro(float z){float a=z+481;float previo=NivelTresChaski.Centro(a)-NivelTresChaski.Centro(485);float nuevo=z<205?0:8*Mathf.Sin((z-205)*.022f);return Mathf.Lerp(previo,nuevo,Mathf.SmoothStep(0,1,Mathf.InverseLerp(28,100,z)));}
 public static float Suelo(float z)=>z<110?4-6*Mathf.SmoothStep(0,1,Mathf.InverseLerp(28,110,z)):z<220?-2:-2+5*Mathf.SmoothStep(0,1,Mathf.InverseLerp(220,345,z));
 public static float PisoPuente(float z)=>-2-1.17f*Mathf.Sin(Mathf.Clamp01((z-150)/36)*Mathf.PI);
 public static float CentroRio(float x)=>168+Mathf.Sin(x*.035f)*4*Mathf.SmoothStep(0,1,Mathf.InverseLerp(12,40,Mathf.Abs(x)));
 public float Resistencia{get;private set;}=100;public float FuerzaViento{get;private set;}public bool SobrePuente=>jugador.transform.position.z>=149.8f&&jugador.transform.position.z<=186.2f&&Mathf.Abs(jugador.transform.position.x)<1.4f;
 public bool Muerto=>muerto;public bool Terminado=>terminado;public bool EnPausa=>pausa;public int Etapa=>etapa;public int[] Cantidades=>cantidades;
 readonly int[] cantidades=new int[4];readonly string[] nombres={"Coca","Cancha","Ch’arki","Agua"};readonly string[] efectos={"+10 resistencia · esfuerzo −20 % · 20 s","+20 resistencia","+45 resistencia","+30 resistencia"};readonly float[] duracion={2.2f,2.8f,3.6f,3};readonly int[] recupera={10,20,45,30};
 const string P="Chaski.Nivel04.";CamaraChasqui camara;CharacterController cc;AgacharseChasqui agacharse;ConsumoChaski gesto;AudioPrimeraMision audioNivel;Transform pecho,brazo,codo,mano,cabeza;
 Font titulo,texto;Texture2D manta;Camera mapa;RenderTexture imagen;float proximoMapa,intro=6,coca,ultimoEsfuerzo,caidaDesde,controlRes=100,mantenerE,consumoT,examenT,muerteT,tiempo;int etapa,consumiendo=-1,elegido;bool pausa,selector,muerto,terminado,examinando,leido,enAire;Vector3 puntoControl,tejidoInicio;Quaternion tejidoGiro;string aviso="",contexto="";float avisoHasta;
 public static int Capacidad(int t)=>t==1?4:2;
 bool modoRatonChuspa;
 public bool EnInterfaz=>pausa||selector||muerto||terminado||intro>0||examinando;
 void Start()
 {
  Time.timeScale=1;camara=vista.GetComponent<CamaraChasqui>();cc=jugador.GetComponent<CharacterController>();agacharse=jugador.GetComponent<AgacharseChasqui>();audioNivel=GetComponent<AudioPrimeraMision>();gesto=jugador.gameObject.AddComponent<ConsumoChaski>();gameObject.AddComponent<PosturaNivelCuatro>();jugador.reaparecerAutomaticamente=false;jugador.SaltoIniciado+=Salto;
  foreach(var t in jugador.GetComponentsInChildren<Transform>()){if(t.name=="chest")pecho=t;if(t.name=="upper_arm.R")brazo=t;if(t.name=="forearm.R")codo=t;if(t.name=="hand.R")mano=t;if(t.name=="head")cabeza=t;}
  titulo=FuenteCastellanaChaski.Obtener();texto=FuenteCastellanaChaski.Obtener();manta=InterfazIncaChasqui.CrearTejido();
  bool vieneDelTres=PlayerPrefs.GetInt("Chaski.Enlace34.Pendiente",0)==1;
  bool guardado=!vieneDelTres&&PlayerPrefs.GetInt(P+"Inventario",0)==1;
  for(int i=0;i<4;i++)cantidades[i]=Mathf.Clamp(PlayerPrefs.GetInt(guardado?P+"Racion"+i:"Chaski.Campana.Racion"+i,PlayerPrefs.GetInt("Chaski.Nivel03.Racion"+i,0)),0,Capacidad(i));
  if(vieneDelTres||PlayerPrefs.GetInt(P+"ChuspaPreparada",0)==0)
  {
   cantidades[0]=Mathf.Max(1,cantidades[0]);cantidades[2]=Mathf.Max(1,cantidades[2]);
   PlayerPrefs.SetInt(P+"EntradaRacion0",cantidades[0]);PlayerPrefs.SetInt(P+"EntradaRacion2",cantidades[2]);PlayerPrefs.SetInt(P+"ChuspaPreparada",1);
  }
  coca=Mathf.Clamp(PlayerPrefs.GetFloat(guardado?P+"Coca":"Chaski.Campana.Coca",0),0,20);Resistencia=Mathf.Clamp(PlayerPrefs.GetFloat(guardado?P+"Resistencia":"Chaski.Campana.Resistencia",100),0,100);
  if(vieneDelTres){PlayerPrefs.DeleteKey(P+"EntradaRes");PlayerPrefs.DeleteKey(P+"Completado");PlayerPrefs.DeleteKey(P+"Registro");foreach(var r in recursos)PlayerPrefs.DeleteKey(P+"Recogido."+r.identificador);}
  if(!PlayerPrefs.HasKey(P+"EntradaRes")){PlayerPrefs.SetFloat(P+"EntradaRes",Resistencia);PlayerPrefs.SetFloat(P+"EntradaCoca",coca);for(int i=0;i<4;i++)PlayerPrefs.SetInt(P+"EntradaRacion"+i,cantidades[i]);}
  etapa=vieneDelTres?0:Mathf.Clamp(PlayerPrefs.GetInt(P+"Control",0),0,controles.Length-1);puntoControl=controles[etapa].position;controlRes=vieneDelTres?Resistencia:PlayerPrefs.GetFloat(P+"ControlRes",Resistencia);leido=PlayerPrefs.GetInt(P+"Registro",0)==1;
  if(etapa>0){jugador.Reubicar(puntoControl);intro=0;}
  else if(PlayerPrefs.GetInt("Chaski.Enlace34.Pendiente",0)==1)
  {
   var p=new Vector3(PlayerPrefs.GetFloat("Chaski.Enlace34.X"),PlayerPrefs.GetFloat("Chaski.Enlace34.Y"),PlayerPrefs.GetFloat("Chaski.Enlace34.Z"))+desplazamientoDesdeNivel3;
   jugador.Reubicar(p);puntoControl=p;intro=0;
   jugador.visual.rotation=Quaternion.Euler(0,PlayerPrefs.GetFloat("Chaski.Enlace34.GiroPersonaje"),0);
   camara.RestaurarOrientacion(PlayerPrefs.GetFloat("Chaski.Enlace34.GiroCamara"),PlayerPrefs.GetFloat("Chaski.Enlace34.Inclinacion"));PlayerPrefs.SetInt("Chaski.Enlace34.Pendiente",0);
  }foreach(var r in recursos)if(PlayerPrefs.GetInt(P+"Recogido."+r.identificador,0)==1)r.gameObject.SetActive(false);
  tejidoInicio=tejido.position;tejidoGiro=tejido.rotation;CrearMapa();if(intro>0)Bloquear(true);caidaDesde=jugador.transform.position.y;Avisar("ENCARGADO: El paso sigue abierto. Busca al corredor en el camino de la montaña.",8);Guardar();
 }
 void Salto(){Resistencia=Mathf.Max(0,Resistencia-3);ultimoEsfuerzo=Time.time;}
 public void EstablecerPausa(bool valor){pausa=valor;Time.timeScale=valor?0:1;camara.enabled=!EnInterfaz&&consumiendo<0;camara.CapturarRaton(false);}
 void Bloquear(bool b){jugador.enabled=!b;camara.enabled=!b;if(b){jugador.empujeExterior=Vector3.zero;camara.CapturarRaton(false);if(jugador.animador!=null)jugador.animador.SetFloat("Movimiento",0);}}
 void Update()
 {
  if(Input.GetKeyDown(KeyCode.Escape)){if(selector){CerrarChuspa();return;}EstablecerPausa(!pausa);}if(pausa)return;
  if(muerto){muerteT+=Time.deltaTime;return;}if(terminado)return;
  if(intro>0){intro-=Time.deltaTime;float encuadre=Mathf.SmoothStep(0,1,1-intro/6);var p=jugador.transform.position;vista.transform.position=Vector3.Lerp(p+new Vector3(-3.4f,2,-1.5f),p+new Vector3(0,2.3f,-4.5f),encuadre);vista.transform.LookAt(p+Vector3.up*1.1f);if(Input.GetKeyDown(KeyCode.E)||intro<=0){intro=0;Bloquear(false);Avisar("Sigue la ladera hasta el puente de ichu. El barranco es mortal.",6);}return;}
  if(selector){if(Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.UpArrow))elegido=(elegido+3)%4;if(Input.GetKeyDown(KeyCode.RightArrow)||Input.GetKeyDown(KeyCode.DownArrow))elegido=(elegido+1)%4;if(Input.GetKeyDown(KeyCode.E))Consumir(elegido);return;}
  if(examinando){examenT+=Time.deltaTime;agacharse.Solicitar(true,Time.deltaTime);if(examenT>7||Input.GetKeyDown(KeyCode.E))Terminar();return;}
  var pos=jugador.transform.position;bool piso=jugador.EnSuelo;
  if(piso){if(enAire&&caidaDesde-pos.y>=6){Morir();return;}enAire=false;caidaDesde=pos.y;}else{if(!enAire){enAire=true;caidaDesde=pos.y;}caidaDesde=Mathf.Max(caidaDesde,pos.y);}
  if(pos.y<-9 || pos.z>149.5f&&pos.z<186.5f&&pos.y<PisoPuente(pos.z)-3){Morir();return;}
  tiempo+=Time.deltaTime;float fase=Mathf.Repeat(tiempo,11);float f=fase<1.5f?Mathf.SmoothStep(0,1,fase/1.5f):fase<4?1:1-Mathf.SmoothStep(0,1,(fase-4)/2);
  bool expuesto=pos.z>122&&pos.z<197;FuerzaViento=expuesto?f:0;jugador.empujeExterior=Vector3.right*(FuerzaViento*(agacharse.Agachado?.025f:.10f));jugador.factorVelocidad=SobrePuente?.65f:1;
  if(viento!=null)viento.volume=Mathf.Lerp(.035f,.095f,FuerzaViento);var capa=jugador.GetComponentInChildren<CapaFisicaChasqui>();if(capa!=null)capa.vientoAmbiente=Vector3.right*FuerzaViento*4;
  if(consumiendo>=0){if(QuiereMoverse()){CancelarConsumo();return;}consumoT+=Time.deltaTime;gesto.Actualizar(consumoT/duracion[consumiendo]);if(consumoT>=duracion[consumiendo]){int t=consumiendo;float antes=Resistencia;cantidades[t]--;if(t==0)coca=20;Resistencia=Mathf.Min(100,Resistencia+recupera[t]);CancelarConsumo();Guardar();Avisar(nombres[t]+" · +"+Mathf.RoundToInt(Resistencia-antes)+" resistencia · "+Mathf.RoundToInt(Resistencia)+" / 100",5);}return;}
  if(coca>0)coca=Mathf.Max(0,coca-Time.deltaTime);bool correr=cc.velocity.magnitude>3.1f&&piso;
  if(correr){Resistencia=Mathf.Max(0,Resistencia-Time.deltaTime*(coca>0?1.2f:1.5f));ultimoEsfuerzo=Time.time;}
  jugador.limitarCarrera=SobrePuente||Resistencia<=.1f||jugador.limitarCarrera&&!SobrePuente&&Resistencia<15;
  if(etapa+1<controles.Length&&pos.z>=controles[etapa+1].position.z&&piso){etapa++;puntoControl=controles[etapa].position;controlRes=Resistencia;Guardar();Avisar("Punto de control · apoyo seguro",4);}
  contexto="";var cercano=RecursoCercano();
  if(cercano!=null)
  {
   int max=Capacidad(cercano.tipo);contexto=cantidades[cercano.tipo]<max?"E · Recoger "+cercano.Nombre+" y guardar en la chuspa":"Chuspa llena de "+cercano.Nombre+" · mantén E para comer";
   if(Input.GetKeyDown(KeyCode.E))Recoger(cercano);
  }
  else if(Vector3.Distance(pos,hallazgo.position)<2.4f){contexto="E · Examinar las pertenencias";if(Input.GetKeyDown(KeyCode.E)&&piso)Examinar();}
  else if(Vector3.Distance(pos,registro.position)<3){contexto="E · Observar los anclajes";if(Input.GetKeyDown(KeyCode.E)){leido=true;Guardar();Avisar("PUENTES DE FIBRAS · Las cuerdas, el piso tejido y los anclajes trabajan como un conjunto. Su cuidado mantiene abierto el paso.",12);}}
  else if(Vector3.Distance(pos,responsable.position)<4){contexto="E · Hablar con el encargado";if(Input.GetKeyDown(KeyCode.E))Avisar("ENCARGADO: Cruza con calma. Si arrecia el viento, baja el cuerpo. El sendero continúa al otro lado.",10);}
  else if(pos.z>138&&pos.z<149){contexto="E · Tocar el pututu     C · Bajar el cuerpo durante las ráfagas";if(Input.GetKeyDown(KeyCode.E)&&jugador.enabled){var t=jugador.GetComponent<TocarPututuChaski>();if(t==null)t=jugador.gameObject.AddComponent<TocarPututuChaski>();t.Iniciar(audioNivel);}}
  mantenerE=Input.GetKey(KeyCode.E)&&piso&&jugador.enabled&&!SobrePuente?mantenerE+Time.deltaTime:0;
  if(mantenerE>.6f){mantenerE=0;modoRatonChuspa=camara.RatonCapturado;selector=true;Bloquear(true);Time.timeScale=0;}
 }
 public RecursoValleVientos RecursoCercano()
 {
  RecursoValleVientos mejor=null;float cerca=3.2f;var p=jugador.transform.position;
  foreach(var r in recursos){if(r==null||!r.gameObject.activeInHierarchy||Mathf.Abs(p.y-r.transform.position.y)>2)continue;float d=Vector2.Distance(new Vector2(p.x,p.z),new Vector2(r.transform.position.x,r.transform.position.z));if(d<cerca){cerca=d;mejor=r;}}
  return mejor;
 }
 public bool Recoger(RecursoValleVientos r)
 {
  if(r==null||!r.gameObject.activeInHierarchy||muerto||terminado||pausa||selector||intro>0||consumiendo>=0)return false;
  var p=jugador.transform.position;if(Vector2.Distance(new Vector2(p.x,p.z),new Vector2(r.transform.position.x,r.transform.position.z))>3.2f||Mathf.Abs(p.y-r.transform.position.y)>2)return false;
  int t=r.tipo;if(cantidades[t]>=Capacidad(t)){Avisar("Tu chuspa ya contiene "+nombres[t]+". Mantén E para abrirla y consumir.",5);return false;}
  cantidades[t]++;r.gameObject.SetActive(false);PlayerPrefs.SetInt(P+"Recogido."+r.identificador,1);Guardar();audioNivel.Confirmar();Avisar(r.Nombre+" guardado en la chuspa · mantén E para abrirla",5);return true;
 }
 public void Morir(){if(muerto||terminado)return;CancelarConsumo();muerto=true;muerteT=0;examinando=false;Bloquear(true);agacharse.LeerTeclado=false;agacharse.Solicitar(false,.3f);Guardar();}
 public void Reintentar(){if(!muerto)return;muerto=false;intro=0;muerteT=0;jugador.Reubicar(puntoControl);enAire=false;caidaDesde=puntoControl.y;agacharse.LeerTeclado=true;Bloquear(false);camara.RestaurarOrientacion(0,12);Guardar();Avisar("Has vuelto al último apoyo seguro.",4);}
 public void Examinar(){if(muerto||terminado||!jugador.EnSuelo)return;examinando=true;examenT=0;Bloquear(true);agacharse.LeerTeclado=false;var d=hallazgo.position-jugador.transform.position;d.y=0;if(d.sqrMagnitude>.01f)jugador.visual.rotation=Quaternion.LookRotation(d);}
 void Terminar(){tejido.SetPositionAndRotation(tejidoInicio,tejidoGiro);agacharse.LeerTeclado=true;agacharse.Solicitar(false,.3f);examinando=false;terminado=true;PlayerPrefs.SetInt(P+"Completado",1);Guardar();audioNivel.Completar(false);if(viento!=null)viento.volume*=.5f;}
 public void Consumir(int t)
 {
  if(t<0||t>3||cantidades[t]<=0){Avisar("No tienes esa ración.",3);return;}if(SobrePuente||!jugador.EnSuelo){Avisar("Come o bebe sobre suelo firme, fuera del puente.",4);return;}if(t==0&&coca>0||t!=0&&Resistencia>=99.9f){Avisar("No necesitas esa ración ahora.",3);return;}
  bool raton=selector?modoRatonChuspa:camara.RatonCapturado;CerrarChuspa();consumiendo=t;consumoT=0;Bloquear(true);camara.CapturarRaton(raton);gesto.Mostrar(t);
 }
 void CancelarConsumo(){consumiendo=-1;consumoT=0;if(gesto!=null)gesto.Ocultar();if(!EnInterfaz)Bloquear(false);}
 void CerrarChuspa(){selector=false;Time.timeScale=1;if(!EnInterfaz)Bloquear(false);}
 static bool QuiereMoverse()=>Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.UpArrow)||Input.GetKey(KeyCode.DownArrow)||Input.GetKey(KeyCode.LeftArrow)||Input.GetKey(KeyCode.RightArrow)||Input.GetKey(KeyCode.Space);
 void Avisar(string s,float t){aviso=s;avisoHasta=Time.time+t;}
 void Guardar(){PlayerPrefs.SetInt(P+"Inventario",1);PlayerPrefs.SetInt(P+"Control",etapa);PlayerPrefs.SetInt(P+"Registro",leido?1:0);PlayerPrefs.SetFloat(P+"ControlRes",controlRes);PlayerPrefs.SetFloat(P+"Resistencia",Resistencia);PlayerPrefs.SetFloat(P+"Coca",coca);for(int i=0;i<4;i++){PlayerPrefs.SetInt(P+"Racion"+i,cantidades[i]);PlayerPrefs.SetInt("Chaski.Campana.Racion"+i,cantidades[i]);}PlayerPrefs.SetFloat("Chaski.Campana.Resistencia",Resistencia);PlayerPrefs.SetFloat("Chaski.Campana.Coca",coca);PlayerPrefs.Save();}
 void Reiniciar(){PlayerPrefs.DeleteKey(P+"Control");PlayerPrefs.DeleteKey(P+"Completado");PlayerPrefs.DeleteKey(P+"Registro");for(int i=0;i<4;i++)PlayerPrefs.SetInt(P+"Racion"+i,PlayerPrefs.GetInt(P+"EntradaRacion"+i,0));PlayerPrefs.SetFloat(P+"Resistencia",PlayerPrefs.GetFloat(P+"EntradaRes",100));PlayerPrefs.SetFloat(P+"Coca",PlayerPrefs.GetFloat(P+"EntradaCoca",0));PlayerPrefs.SetFloat(P+"ControlRes",PlayerPrefs.GetFloat(P+"EntradaRes",100));foreach(var r in recursos)PlayerPrefs.DeleteKey(P+"Recogido."+r.identificador);PlayerPrefs.Save();Cargar("Nivel04_PuenteAbismo");}
 static void Cargar(string s){Time.timeScale=1;SceneManager.LoadScene(s);}
 void CrearMapa(){mapa=new GameObject("Minimapa · puente del abismo").AddComponent<Camera>();mapa.enabled=false;mapa.orthographic=true;mapa.orthographicSize=215;mapa.aspect=1;mapa.farClipPlane=260;mapa.nearClipPlane=.1f;mapa.allowHDR=mapa.allowMSAA=false;mapa.transform.SetPositionAndRotation(new Vector3(0,180,190),Quaternion.Euler(90,0,0));imagen=new RenderTexture(320,320,16);mapa.targetTexture=imagen;}
 void LateUpdate()
 {
  if(EnInterfaz){camara.enabled=false;camara.CapturarRaton(false);}if(mapa!=null&&Time.unscaledTime>proximoMapa){proximoMapa=Time.unscaledTime+.4f;mapa.Render();}
 }
 public void AplicarPostura()
 {
  if(examinando)
  {
   float w=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.5f,2,examenT))*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(5,7,examenT)));if(pecho!=null)pecho.rotation=Quaternion.AngleAxis(22*w,jugador.visual.right)*pecho.rotation;
   Vector3 objetivo=tejidoInicio+Vector3.up*.08f;if(examenT>2&&examenT<5)objetivo=jugador.transform.position+Vector3.up*1.05f+jugador.visual.forward*.38f;
   EntregaInicialChaski.Apuntar(brazo,codo,mano,objetivo,w);if(mano!=null&&examenT>2&&examenT<5)tejido.position=mano.position;else tejido.SetPositionAndRotation(tejidoInicio,tejidoGiro);
  }
  else if(SobrePuente&&!EnInterfaz&&pecho!=null){pecho.rotation=Quaternion.AngleAxis(Mathf.Sin(Time.time*1.7f)*1.5f+FuerzaViento*3,jugador.visual.forward)*pecho.rotation;var ts=jugador.GetComponentsInChildren<Transform>();foreach(var t in ts)if(t.name=="upper_arm.L")t.rotation=Quaternion.AngleAxis(-FuerzaViento*9,jugador.visual.forward)*t.rotation;}
 }
 GUIStyle Estilo(int n,bool grande=false)=>new GUIStyle(GUI.skin.label){font=grande?titulo:texto,fontSize=n,wordWrap=true,normal={textColor=new Color(1,.92f,.75f)}};
 static void Panel(Rect r){var c=GUI.color;GUI.color=new Color(.12f,.065f,.035f,.94f);GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=c;}
 bool Boton(Rect r,string t)=>GUI.Button(r,t,new GUIStyle(GUI.skin.button){font=texto,fontSize=23});
 void OnGUI()
 {
  var anterior=GUI.matrix;float escala=Mathf.Min(Screen.width/1400f,Screen.height/850f);GUI.matrix=Matrix4x4.Scale(Vector3.one*escala);float w=Screen.width/escala,h=Screen.height/escala;
  if(pausa||selector||terminado||muerto)
  {
   Panel(new Rect(0,0,w,h));var r=new Rect(w/2-450,h/2-275,900,550);GUI.DrawTexture(r,manta);Panel(new Rect(r.x+30,r.y+30,840,490));
   if(pausa){GUI.Label(new Rect(r.x+65,r.y+65,770,50),"DESCANSO EN EL CAMINO",Estilo(32,true));if(Boton(new Rect(r.x+190,r.y+170,520,65),"Reanudar [Esc]"))EstablecerPausa(false);if(Boton(new Rect(r.x+190,r.y+255,520,65),"Volver al último control")){EstablecerPausa(false);Morir();Reintentar();}if(Boton(new Rect(r.x+190,r.y+340,520,65),"Menú principal")){Guardar();Cargar("InicioChaski");}}
   else if(muerto){GUI.Label(new Rect(r.x+65,r.y+75,770,65),"HAS CAÍDO AL ABISMO",Estilo(34,true));GUI.Label(new Rect(r.x+65,r.y+165,770,70),"Tu recorrido continúa desde el último apoyo seguro.",Estilo(25));GUI.enabled=muerteT>.65f;if(Boton(new Rect(r.x+180,r.y+290,250,65),"Reintentar [E]")||muerteT>.65f&&Event.current.type==EventType.KeyDown&&Event.current.keyCode==KeyCode.E)Reintentar();GUI.enabled=true;if(Boton(new Rect(r.x+465,r.y+290,250,65),"Menú"))Cargar("InicioChaski");}
   else if(terminado){GUI.Label(new Rect(r.x+65,r.y+65,770,60),"AL OTRO LADO DEL ABISMO",Estilo(31,true));GUI.Label(new Rect(r.x+65,r.y+155,770,120),"El puente quedó atrás. Las pertenencias del corredor están abandonadas, pero su rastro continúa hacia la montaña.",Estilo(26));if(Boton(new Rect(r.x+60,r.y+320,240,65),"Repetir"))Reiniciar();if(Boton(new Rect(r.x+330,r.y+320,240,65),"Menú"))Cargar("InicioChaski");GUI.enabled=Application.CanStreamedLevelBeLoaded("Nivel05_MontanaSagrada");if(Boton(new Rect(r.x+600,r.y+320,240,65),"Siguiente"))SiguienteNivel();GUI.enabled=true;GUI.Label(new Rect(r.x+65,r.y+425,770,45),"Nivel 5 en preparación · Registro: "+(leido?"observado":"pendiente"),Estilo(21));}
   else{GUI.Label(new Rect(r.x+65,r.y+55,770,45),"CHUSPA · ELIGE UNA RACIÓN",Estilo(30,true));for(int i=0;i<4;i++)if(Boton(new Rect(r.x+60,r.y+120+i*72,780,60),(i==elegido?"→ ":"")+nombres[i]+" × "+cantidades[i]+" · "+efectos[i])){elegido=i;Consumir(i);}GUI.Label(new Rect(r.x+65,r.y+430,770,55),Time.time<avisoHasta?aviso:"Flechas · elegir    E · consumir    Esc · cerrar",Estilo(21));}
  }
  else
  {
   Panel(new Rect(18,18,w-280,116));GUI.Label(new Rect(35,28,w-320,40),"04 · EL PUENTE DEL ABISMO",Estilo(28,true));string ayuda=Time.time<avisoHasta?aviso:SobrePuente?"Marcha cuidadosa · C baja el cuerpo · no corras hacia el borde":"Sigue el sendero y busca noticias del corredor.";GUI.Label(new Rect(35,77,w-320,60),ayuda,Estilo(21));
   Panel(new Rect(w-244,18,225,240));GUI.Label(new Rect(w-230,28,200,28),"QUEBRADA · N ↑",Estilo(17,true));var area=new Rect(w-232,66,201,150);if(imagen!=null)GUI.DrawTexture(area,imagen,ScaleMode.StretchToFill);Marca(area,jugador.transform.position,Color.white);if(jugador.transform.position.z>315)Marca(area,hallazgo.position,new Color(1,.77f,.16f));GUI.Label(new Rect(w-230,225,205,30),Mathf.CeilToInt(Vector3.Distance(jugador.transform.position,hallazgo.position))+" m · Sendero",Estilo(18));
   Panel(new Rect(20,h-127,390,60));GUI.Label(new Rect(30,h-122,370,26),"Resistencia · "+Mathf.RoundToInt(Resistencia)+" / 100",Estilo(19));GUI.color=new Color(.65f,.74f,.3f);GUI.DrawTexture(new Rect(32,h-89,Resistencia*3.6f,12),Texture2D.whiteTexture);GUI.color=Color.white;
   string abajo=examinando?(examenT<3.5f?"WAYRA: Una chuspa de corredor… ¿por qué la habrá dejado aquí?":"WAYRA: El sendero continúa hacia la montaña. Tengo que seguir."):intro>0?"E · Omitir presentación":consumiendo>=0?"Usando "+nombres[consumiendo]+" · moverte cancela":contexto;
   if(!string.IsNullOrEmpty(abajo)){Panel(new Rect(w/2-430,h-220,860,70));GUI.Label(new Rect(w/2-410,h-210,820,62),abajo,Estilo(23));}
   GUI.Label(new Rect(435,h-87,w-470,27),"CHUSPA · Coca "+cantidades[0]+"   Cancha "+cantidades[1]+"   Ch’arki "+cantidades[2]+"   Agua "+cantidades[3],Estilo(18));
   if(coca>0)GUI.Label(new Rect(440,h-117,450,35),"Coca · esfuerzo −20 % · "+Mathf.CeilToInt(coca)+" s",Estilo(21));
   Panel(new Rect(20,h-52,w-40,34));GUI.Label(new Rect(30,h-49,w-60,32),"WASD / Flechas · Mover    Doble clic / Ratón · Mirar    Espacio · Saltar    C · Agacharse    E · Interactuar / mantener para chuspa    Esc · Pausa",Estilo(16));
  }
  GUI.matrix=anterior;
 }
 void Marca(Rect r,Vector3 p,Color c){if(mapa==null)return;var q=mapa.WorldToViewportPoint(p);GUI.color=c;GUI.DrawTexture(new Rect(r.x+q.x*r.width-4,r.y+(1-q.y)*r.height-4,8,8),Texture2D.whiteTexture);GUI.color=Color.white;}
 public void SiguienteNivel(){Guardar();var p=jugador.transform.position;PlayerPrefs.SetFloat("Chaski.Enlace45.X",p.x);PlayerPrefs.SetFloat("Chaski.Enlace45.Y",p.y);PlayerPrefs.SetFloat("Chaski.Enlace45.Z",p.z);PlayerPrefs.SetFloat("Chaski.Enlace45.Giro",camara.Giro);PlayerPrefs.SetFloat("Chaski.Enlace45.Inclinacion",camara.Inclinacion);PlayerPrefs.SetInt("Chaski.Enlace45.Pendiente",1);PlayerPrefs.Save();Cargar("Nivel05_MontanaSagrada");}
 void OnDestroy(){Time.timeScale=1;if(jugador!=null){jugador.SaltoIniciado-=Salto;jugador.factorVelocidad=1;jugador.reaparecerAutomaticamente=true;jugador.empujeExterior=Vector3.zero;jugador.limitarCarrera=false;}if(manta!=null)Destroy(manta);if(mapa!=null)Destroy(mapa.gameObject);if(imagen!=null){imagen.Release();Destroy(imagen);}}
}
