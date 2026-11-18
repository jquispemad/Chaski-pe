using System.Linq;
using UnityEngine;

// A short hand-off staged with the existing rig; control is returned after the camera settles.
[DefaultExecutionOrder(80)]
public sealed class EntregaInicialChaski : MonoBehaviour
{
    public Transform emisor;
    public Transform puerta, accesoCasa, interiorCasa;
    public AudioClip crujidoPuerta, cierrePuerta;
    SegundaMisionChaski mision;
    CamaraChasqui seguimiento;
    Animator animador;
    Transform brazoA, codoA, manoA, brazoB, codoB, manoB;
    Transform quipu;
    Quaternion[] reposo;
    Transform[] huesos;
    float reloj;
    bool activa, animadorActivo;
    Vector3 inicioCamara;
    Quaternion inicioRotacion;
    Material hilo;
    Font fuente;
    Animator animadorEmisor;
    Transform visualEmisor;
    AudioSource sonidoEmisor;
    AudioPrimeraMision audioNivel;
    Terrain terreno;
    Quaternion puertaCerrada;
    Vector3 esquinaAcceso, camaraRetorno;
    Quaternion rotacionRetorno;
    int retirada;
    float tiempoRetirada, distanciaPaso;
    const float FinEntrega=9.5f;
    CortinaIchuChaski cortina;
    Transform pechoA, cuelloA, pechoB, cuelloB;
    Quaternion pechoReposoA,cuelloReposoA,pechoReposoB,cuelloReposoB;
    Vector3 puntoA,puntoB,velocidadA,velocidadB;
    float velocidadMarcha;
    void Start()
    {
        mision=GetComponent<SegundaMisionChaski>();
        if(emisor==null || mision.PuntosControl>0)return;
        var jugador=mision.jugador;
        seguimiento=mision.vista.GetComponent<CamaraChasqui>();
        animador=jugador.animador;
        animadorActivo=animador!=null && animador.enabled;
        if(animador!=null){animador.SetFloat("Movimiento",0);animador.Update(0);animador.enabled=false;}
        huesos=jugador.GetComponentsInChildren<Transform>(true);
        brazoA=Buscar(huesos,"upper_arm.R");codoA=Buscar(huesos,"forearm.R");manoA=Buscar(huesos,"hand.R");
        var otros=emisor.GetComponentsInChildren<Transform>(true);
        var locomocionEmisor=emisor.GetComponent<ChasquiMovimiento>();
        visualEmisor=locomocionEmisor!=null && locomocionEmisor.visual!=null?locomocionEmisor.visual:emisor;
        animadorEmisor=locomocionEmisor!=null?locomocionEmisor.animador:emisor.GetComponentInChildren<Animator>();
        if(animadorEmisor!=null){animadorEmisor.enabled=false;animadorEmisor.applyRootMotion=false;animadorEmisor.SetBool("EnSuelo",true);animadorEmisor.SetFloat("Movimiento",0);animadorEmisor.Update(0);}
        terreno=Terrain.activeTerrains.FirstOrDefault(t=>t.gameObject.scene==gameObject.scene);
        audioNivel=GetComponent<AudioPrimeraMision>();
        sonidoEmisor=emisor.gameObject.AddComponent<AudioSource>();sonidoEmisor.playOnAwake=false;sonidoEmisor.spatialBlend=.45f;sonidoEmisor.volume=.5f;sonidoEmisor.dopplerLevel=0;
        if(puerta!=null){puertaCerrada=puerta.localRotation;cortina=puerta.GetComponent<CortinaIchuChaski>();}
        esquinaAcceso=emisor.position+new Vector3(3,0,-5.5f);
        brazoB=Buscar(otros,"upper_arm.R");codoB=Buscar(otros,"forearm.R");manoB=Buscar(otros,"hand.R");
        pechoA=Buscar(huesos,"chest");cuelloA=Buscar(huesos,"neck");pechoB=Buscar(otros,"chest");cuelloB=Buscar(otros,"neck");
        pechoReposoA=pechoA!=null?pechoA.localRotation:Quaternion.identity;cuelloReposoA=cuelloA!=null?cuelloA.localRotation:Quaternion.identity;
        pechoReposoB=pechoB!=null?pechoB.localRotation:Quaternion.identity;cuelloReposoB=cuelloB!=null?cuelloB.localRotation:Quaternion.identity;
        huesos=new[]{brazoA,codoA,brazoB,codoB,manoA,manoB};
        reposo=huesos.Select(t=>t==null?Quaternion.identity:t.localRotation).ToArray();
        jugador.enabled=false;seguimiento.enabled=false;mision.EnCinematica=true;
        jugador.visual.rotation=Quaternion.LookRotation(emisor.position-jugador.transform.position);
        emisor.LookAt(new Vector3(jugador.transform.position.x,emisor.position.y,jugador.transform.position.z));
        var centro=(jugador.transform.position+emisor.position)*.5f+Vector3.up*1.15f;
        inicioCamara=centro+new Vector3(-3.6f,1.05f,-2.4f);
        inicioRotacion=Quaternion.LookRotation(centro-inicioCamara);
        mision.vista.transform.SetPositionAndRotation(inicioCamara,inicioRotacion);
        fuente=Resources.Load<Font>("EstiloInca/TextoTradicional");
        CrearQuipu();activa=true;
        var encuentro=(jugador.transform.position+emisor.position)*.5f+Vector3.up*1.18f;
        puntoA=manoA!=null?manoA.position:encuentro;puntoB=manoB!=null?manoB.position:encuentro;
        Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
    }
    static Transform Buscar(Transform[] ts,string n)=>ts.FirstOrDefault(t=>t.name==n);
    void CrearQuipu()
    {
        quipu=new GameObject("Quipu · entrega animada").transform;
        hilo=new Material(Shader.Find("Standard")){color=new Color(.68f,.3f,.14f)};
        for(int i=-5;i<=5;i++)
        {
            var o=new GameObject("Cordón anudado");o.transform.SetParent(quipu,false);
            var l=o.AddComponent<LineRenderer>();l.useWorldSpace=false;l.sharedMaterial=hilo;l.widthMultiplier=.012f;l.positionCount=3;
            float x=i*.024f, largo=.15f+(i%3+3)%3*.035f;
            l.SetPositions(new[]{new Vector3(x,0,0),new Vector3(x,-largo*.5f,.012f),new Vector3(x+.006f,-largo,0)});
        }
        var travesaño=quipu.gameObject.AddComponent<LineRenderer>();travesaño.useWorldSpace=false;travesaño.sharedMaterial=hilo;travesaño.widthMultiplier=.018f;travesaño.positionCount=2;
        travesaño.SetPositions(new[]{new Vector3(-.14f,0,0),new Vector3(.14f,0,0)});
    }
    void LateUpdate()
    {
        if(!activa)return;
        reloj+=Time.deltaTime;
        if(reloj>=FinEntrega && puerta!=null && accesoCasa!=null && interiorCasa!=null)
        {
            ActualizarRetirada();
            return;
        }
        float gestoB=Suave(1.0f,3.3f,reloj)*(1-Suave(5.6f,7.1f,reloj));
        float gestoA=Suave(2.0f,4.0f,reloj)*(1-Suave(6.8f,8.4f,reloj));
        for(int i=0;i<huesos.Length;i++)if(huesos[i]!=null)huesos[i].localRotation=reposo[i];
        Respirar(pechoA,cuelloA,pechoReposoA,cuelloReposoA,0,reloj>=7.8f);
        Respirar(pechoB,cuelloB,pechoReposoB,cuelloReposoB,1.7f,reloj<7.8f);
        var centro=(mision.jugador.transform.position+emisor.position)*.5f+Vector3.up*1.18f;
        puntoA=Vector3.SmoothDamp(puntoA,centro+Vector3.right*.065f+Vector3.up*(Mathf.Sin(reloj*1.7f)*.006f),ref velocidadA,.19f);
        puntoB=Vector3.SmoothDamp(puntoB,centro-Vector3.right*.065f+Vector3.up*(Mathf.Sin(reloj*1.5f+.8f)*.008f),ref velocidadB,.23f);
        Apuntar(brazoA,codoA,manoA,puntoA,gestoA);
        Apuntar(brazoB,codoB,manoB,puntoB,gestoB);
        var desde=manoB!=null?manoB.position:centro;
        var hasta=manoA!=null?manoA.position:centro;
        quipu.position=Vector3.Lerp(desde,hasta,Suave(4.5f,5.3f,reloj));
        quipu.rotation=Quaternion.Slerp(quipu.rotation,Quaternion.Euler(Mathf.Sin(reloj*2.1f)*3,0,Mathf.Sin(reloj*1.4f)*2),1-Mathf.Exp(-5*Time.deltaTime));
        float cambio=puerta==null?Mathf.SmoothStep(0,1,Mathf.InverseLerp(8.1f,FinEntrega,reloj)):0;
        var p=mision.jugador.transform.position;
        mision.vista.transform.position=Vector3.Lerp(inicioCamara,p+new Vector3(0,2.3f,-4.5f),cambio);
        mision.vista.transform.rotation=Quaternion.Slerp(inicioRotacion,Quaternion.LookRotation(p+Vector3.up*1.1f-(p+new Vector3(0,2.3f,-4.5f))),cambio);
        if(reloj>=FinEntrega)Terminar();
    }
    void ActualizarRetirada()
    {
        if(retirada==0)
        {
            for(int i=0;i<huesos.Length;i++)if(huesos[i]!=null)huesos[i].localRotation=reposo[i];
            if(quipu!=null){Destroy(quipu.gameObject);quipu=null;}
            retirada=1;tiempoRetirada=0;
        }
        tiempoRetirada+=Time.deltaTime;
        bool andando=false;
        if(retirada==1 || retirada==2 || retirada==4)
        {
            Vector3 destino=retirada==1?esquinaAcceso:retirada==2?accesoCasa.position:interiorCasa.position;
            var anterior=emisor.position;
            Vector3 horizontal=Vector3.ProjectOnPlane(destino-anterior,Vector3.up);
            if(horizontal.magnitude>.035f)
            {
                float giro=Vector3.Angle(visualEmisor.forward,horizontal);
                float deseada=Mathf.Min(2.05f,Mathf.Sqrt(2*2.7f*horizontal.magnitude))*Mathf.Lerp(.15f,1,1-Mathf.Clamp01(giro/100));
                velocidadMarcha=Mathf.MoveTowards(velocidadMarcha,deseada,2.7f*Time.deltaTime);
                andando=velocidadMarcha>.08f;
                emisor.position=Vector3.MoveTowards(anterior,new Vector3(destino.x,anterior.y,destino.z),velocidadMarcha*Time.deltaTime);
                visualEmisor.rotation=Quaternion.RotateTowards(visualEmisor.rotation,Quaternion.LookRotation(horizontal),145*Time.deltaTime);
                float altura=0;
                if(terreno!=null){var origen=terreno.transform.position;var d=terreno.terrainData;altura=d.GetInterpolatedHeight((emisor.position.x-origen.x)/d.size.x,(emisor.position.z-origen.z)/d.size.z)+origen.y;}
                float subir=retirada==4?Mathf.SmoothStep(0,1,Mathf.InverseLerp(accesoCasa.position.z,accesoCasa.position.z+.5f,emisor.position.z)):0;
                var p=emisor.position;p.y=altura+.03f+subir*.3f;emisor.position=p;
                distanciaPaso+=Vector3.ProjectOnPlane(emisor.position-anterior,Vector3.up).magnitude;
                if(distanciaPaso>.95f && audioNivel!=null && audioNivel.pasosTierra.Length>0)
                {distanciaPaso=0;sonidoEmisor.pitch=Random.Range(.98f,1.02f);sonidoEmisor.PlayOneShot(audioNivel.pasosTierra[Random.Range(0,audioNivel.pasosTierra.Length)],.45f);}
            }
            else
            {
                retirada++;tiempoRetirada=0;
                if(retirada==3||retirada==5)velocidadMarcha=0;
                if(retirada==3 && crujidoPuerta!=null)sonidoEmisor.PlayOneShot(crujidoPuerta,.5f);
                if(retirada==5 && crujidoPuerta!=null)sonidoEmisor.PlayOneShot(crujidoPuerta,.35f);
            }
        }
        if(animadorEmisor!=null)
        {
            animadorEmisor.SetFloat("Movimiento",andando?velocidadMarcha/4.8f:0,.18f,Time.deltaTime);
            animadorEmisor.Update(Time.deltaTime);
        }
        if(retirada==3 || retirada==5)
        {
            Vector3 tirador=cortina!=null?cortina.PuntoDeAgarre:puerta.TransformPoint(new Vector3(1.35f,1.05f,-.10f));
            var mirar=Vector3.ProjectOnPlane(tirador-emisor.position,Vector3.up);
            if(mirar.sqrMagnitude>.001f)visualEmisor.rotation=Quaternion.Slerp(visualEmisor.rotation,Quaternion.LookRotation(mirar),1-Mathf.Exp(-5*Time.deltaTime));
            Apuntar(brazoB,codoB,manoB,tirador,Mathf.Sin(Mathf.Clamp01(tiempoRetirada/1.15f)*Mathf.PI)*.8f);
            float u=Mathf.SmoothStep(0,1,Mathf.Clamp01(tiempoRetirada/1.15f));
            float apertura=retirada==3?u:1-u;
            if(cortina!=null)cortina.Abrir(apertura);
            else puerta.localRotation=puertaCerrada*Quaternion.Euler(0,-105*apertura,0);
            if(tiempoRetirada>=1.15f)
            {
                if(retirada==5)
                {
                    if(cortina!=null)cortina.Abrir(0);else puerta.localRotation=puertaCerrada;
                    if(cierrePuerta!=null){sonidoEmisor.pitch=1;sonidoEmisor.PlayOneShot(cierrePuerta,.3f);}
                    camaraRetorno=mision.vista.transform.position;rotacionRetorno=mision.vista.transform.rotation;
                }
                retirada++;tiempoRetirada=0;
            }
        }
        var jugador=mision.jugador.transform.position;
        if(retirada<6)
        {
            var centro=Vector3.Lerp(jugador,emisor.position,.65f)+Vector3.up*1.15f;
            var deseada=centro+new Vector3(-5,2.2f,-6.5f);
            mision.vista.transform.position=Vector3.Lerp(mision.vista.transform.position,deseada,1-Mathf.Exp(-2*Time.deltaTime));
            mision.vista.transform.rotation=Quaternion.Slerp(mision.vista.transform.rotation,Quaternion.LookRotation(centro-mision.vista.transform.position),1-Mathf.Exp(-3*Time.deltaTime));
        }
        else
        {
            float t=Mathf.SmoothStep(0,1,Mathf.Clamp01(tiempoRetirada/1.5f));
            Vector3 destino=jugador+new Vector3(0,2.3f,-4.5f);
            mision.vista.transform.SetPositionAndRotation(Vector3.Lerp(camaraRetorno,destino,t),Quaternion.Slerp(rotacionRetorno,Quaternion.LookRotation(jugador+Vector3.up*1.1f-destino),t));
            if(tiempoRetirada>=1.5f)Terminar();
        }
    }
    public static void Apuntar(Transform brazo,Transform codo,Transform mano,Vector3 objetivo,float peso)
    {
        if(brazo==null||codo==null||mano==null)return;
        var a=brazo.localRotation;var b=codo.localRotation;
        // Stable two-bone reach with an outward/downward elbow; avoid the snapping of unconstrained CCD.
        Vector3 hombro=brazo.position;float superior=Vector3.Distance(hombro,codo.position),inferior=Vector3.Distance(codo.position,mano.position);
        Vector3 direccion=objetivo-hombro;
        if(direccion.sqrMagnitude<.0001f||superior<.001f||inferior<.001f)return;
        float distancia=Mathf.Clamp(direccion.magnitude,Mathf.Abs(superior-inferior)+.015f,(superior+inferior)*.94f);
        direccion.Normalize();
        var cuerpo=brazo.root;Vector3 polo=cuerpo.right*.6f+Vector3.down*.8f-cuerpo.forward*.15f;
        Vector3 lateral=Vector3.ProjectOnPlane(polo,direccion).normalized;
        if(lateral.sqrMagnitude<.01f)lateral=Vector3.Cross(direccion,Vector3.forward).normalized;
        float coseno=Mathf.Clamp((superior*superior+distancia*distancia-inferior*inferior)/(2*superior*distancia),-1,1);
        Vector3 codoDeseado=hombro+direccion*(superior*coseno)+lateral*(superior*Mathf.Sqrt(Mathf.Max(0,1-coseno*coseno)));
        brazo.rotation=Quaternion.FromToRotation(codo.position-hombro,codoDeseado-hombro)*brazo.rotation;
        Vector3 destino=hombro+direccion*distancia;
        codo.rotation=Quaternion.FromToRotation(mano.position-codo.position,destino-codo.position)*codo.rotation;
        var finalA=brazo.localRotation;var finalB=codo.localRotation;
        brazo.localRotation=Quaternion.Slerp(a,finalA,peso);codo.localRotation=Quaternion.Slerp(b,finalB,peso);
        var direccionMano=mano.position-codo.position;
        if(direccionMano.sqrMagnitude>.0001f)
            mano.rotation=Quaternion.Slerp(mano.rotation,Quaternion.FromToRotation(mano.up,direccionMano.normalized)*mano.rotation,peso*.65f);
    }
    static float Suave(float inicio,float final,float t)
    {
        float u=Mathf.InverseLerp(inicio,final,t);
        return u*u*u*(u*(u*6-15)+10);
    }
    void Respirar(Transform pecho,Transform cuello,Quaternion basePecho,Quaternion baseCuello,float fase,bool hablando)
    {
        float respiracion=Mathf.Sin(reloj*1.5f+fase);
        if(pecho!=null)pecho.localRotation=basePecho*Quaternion.Euler(respiracion*.45f,Mathf.Sin(reloj*.7f+fase)*.35f,0);
        float asentir=hablando?Mathf.Sin(reloj*2.7f+fase)*.85f:Mathf.Sin(reloj*.9f+fase)*.25f;
        if(cuello!=null)cuello.localRotation=baseCuello*Quaternion.Euler(asentir,Mathf.Sin(reloj*.6f+fase)*.7f,0);
    }
    void Terminar()
    {
        for(int i=0;i<huesos.Length;i++)if(huesos[i]!=null)huesos[i].localRotation=reposo[i];
        if(pechoA!=null)pechoA.localRotation=pechoReposoA;if(cuelloA!=null)cuelloA.localRotation=cuelloReposoA;
        if(animador!=null)animador.enabled=animadorActivo;
        mision.jugador.visual.rotation=Quaternion.identity;
        mision.jugador.enabled=true;seguimiento.enabled=true;mision.EnCinematica=false;activa=false;
        if(quipu!=null)Destroy(quipu.gameObject);
        Debug.Log("ENTREGA_INICIAL_COMPLETA: quipu entregado y control del jugador restaurado.");
    }
    void OnGUI()
    {
        if(!activa)return;
        GUI.color=Color.black;GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height*.10f),Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(0,Screen.height*.78f,Screen.width,Screen.height*.22f),Texture2D.whiteTexture);GUI.color=Color.white;
        var estilo=new GUIStyle(GUI.skin.label){font=fuente,fontSize=Mathf.Max(18,Screen.height/36),wordWrap=true,alignment=TextAnchor.MiddleCenter};
        string hablante=reloj<7.8f?"RELEVISTA":"WAYRA";
        string mensaje=reloj<2.8f?"Recibe este quipu y lleva el recado a la siguiente posta.":reloj<5.5f?"El paso del río está abierto. Ya pueden enviar las provisiones.":reloj<7.8f?"Anuncia tu llegada con el pututu. ¡Buen camino!":"Así lo haré. El mensaje llegará a la próxima posta.";
        if(retirada>0){hablante="EL PRIMER RECORRIDO";mensaje=retirada<5?"El relevista regresa al chaskiwasi. Wayra se prepara para partir.":"El encargo está en tus manos. Sigue el camino hacia la próxima posta.";}
        var nombre=new GUIStyle(estilo){fontSize=Mathf.Max(16,Screen.height/45),fontStyle=FontStyle.Bold};nombre.normal.textColor=new Color(1,.78f,.39f);
        GUI.Label(new Rect(Screen.width*.12f,Screen.height*.795f,Screen.width*.76f,Screen.height*.045f),hablante,nombre);
        GUI.Label(new Rect(Screen.width*.12f,Screen.height*.845f,Screen.width*.76f,Screen.height*.10f),mensaje,estilo);
    }
    void OnDestroy(){if(quipu!=null)Destroy(quipu.gameObject);if(hilo!=null)Destroy(hilo);}
}
