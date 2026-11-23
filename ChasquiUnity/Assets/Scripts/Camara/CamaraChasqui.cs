using UnityEngine;

[DefaultExecutionOrder(-100)]
public sealed class CamaraChasqui : MonoBehaviour
{
    public Transform objetivo;
    [SerializeField] Vector3 distancia = new Vector3(0f, 2.3f, -4.5f);
    [SerializeField] float alturaMirada = 1.1f;
    [SerializeField] float suavidad = .12f;
    [Header("Raton")]
    [SerializeField, Range(.2f,6f)] float sensibilidad = 2.2f;
    [SerializeField] float inclinacionMinima = -8f;
    [SerializeField] float inclinacionMaxima = 65f;
    Vector3 velocidad;
    float giro, inclinacion, radio;
    float ultimoClick=float.NegativeInfinity;
    readonly RaycastHit[] contactos = new RaycastHit[24];
    Camera camaraCache;
    Transform objetivoCache;
    AgacharseChasqui bajoCache;
    public float Giro => giro;
    public float Inclinacion => inclinacion;
    public bool RatonCapturado => Cursor.lockState == CursorLockMode.Locked;

    void Start()
    {
        camaraCache = GetComponent<Camera>();
        Vector3 offset=distancia-Vector3.up*alturaMirada;
        radio=Mathf.Max(2f,offset.magnitude);
        giro=Mathf.Atan2(-offset.x,-offset.z)*Mathf.Rad2Deg;
        inclinacion=Mathf.Clamp(Mathf.Atan2(offset.y,new Vector2(offset.x,offset.z).magnitude)*Mathf.Rad2Deg,inclinacionMinima,inclinacionMaxima);
        if(objetivo!=null){transform.position=objetivo.position+distancia;transform.LookAt(objetivo.position+Vector3.up*alturaMirada);}
    }
    void Update()
    {
        if(Time.timeScale<=0){CapturarRaton(false);return;}
        if(Input.GetKeyDown(KeyCode.Escape)){CapturarRaton(false);return;}
        if(!RatonCapturado && Input.GetMouseButtonDown(0))
        {
            var cam=camaraCache != null ? camaraCache : (camaraCache = GetComponent<Camera>());
            if(cam!=null&&cam.pixelRect.Contains(Input.mousePosition))
            {
                if(Time.unscaledTime-ultimoClick<=.35f){CapturarRaton(true);ultimoClick=float.NegativeInfinity;}
                else ultimoClick=Time.unscaledTime;
            }
            return;
        }
        if(RatonCapturado)Mirar(new Vector2(Input.GetAxisRaw("Mouse X"),Input.GetAxisRaw("Mouse Y")));
    }
    public void RestaurarOrientacion(float horizontal,float vertical)
    {
        giro=horizontal;inclinacion=Mathf.Clamp(vertical,inclinacionMinima,inclinacionMaxima);velocidad=Vector3.zero;
        transform.rotation=Quaternion.Euler(inclinacion,giro,0);
        if(objetivo!=null)transform.position=objetivo.position+Vector3.up*alturaMirada+transform.rotation*Vector3.back*radio;
    }
    public void CapturarRaton(bool capturar){Cursor.lockState=capturar?CursorLockMode.Locked:CursorLockMode.None;Cursor.visible=!capturar;}
    public void Mirar(Vector2 delta)
    {
        giro=Mathf.Repeat(giro+delta.x*sensibilidad+180f,360f)-180f;
        inclinacion=Mathf.Clamp(inclinacion-delta.y*sensibilidad,inclinacionMinima,inclinacionMaxima);
        // Actualizar la orientacion antes del controlador: W sigue la mirada del mismo fotograma.
        transform.rotation=Quaternion.Euler(inclinacion,giro,0);
    }
    void LateUpdate()
    {
        if(objetivo==null)return;
        if(objetivo!=objetivoCache){objetivoCache=objetivo;bajoCache=objetivo.GetComponent<AgacharseChasqui>();}
        var bajo=bajoCache;
        Vector3 centro=objetivo.position+Vector3.up*(alturaMirada-(bajo!=null?bajo.Factor*.4f:0));
        Vector3 direccion=Quaternion.Euler(inclinacion,giro,0)*Vector3.back;
        float longitud=radio;
        int cantidad=Physics.SphereCastNonAlloc(centro,.22f,direccion,contactos,radio,~0,QueryTriggerInteraction.Ignore);
        for(int i=0;i<cantidad;i++)
        {
            if(contactos[i].transform==objetivo||contactos[i].transform.IsChildOf(objetivo))continue;
            longitud=Mathf.Min(longitud,Mathf.Max(.5f,contactos[i].distance-.12f));
        }
        Vector3 deseada=centro+direccion*longitud;
        // Acercamiento inmediato al obstaculo y retorno suave al alejarse.
        if(Vector3.Distance(transform.position,centro)>longitud+.15f){transform.position=deseada;velocidad=Vector3.zero;}
        else transform.position=Vector3.SmoothDamp(transform.position,deseada,ref velocidad,suavidad);
        transform.LookAt(centro);
    }
    void OnApplicationFocus(bool foco){if(!foco)CapturarRaton(false);}
    void OnDisable(){CapturarRaton(false);}
}

