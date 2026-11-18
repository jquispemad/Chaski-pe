using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public sealed class ChasquiMovimiento : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] float velocidad = 2.5f;
    [SerializeField] float velocidadCorrer = 4.8f;
    [SerializeField] float aceleracion = 22f;
    [SerializeField] float giro = 14f;
    [SerializeField] float duracionPasoBreve = .18f;
    [Header("Salto")]
    [SerializeField] KeyCode teclaSalto = KeyCode.Space;
    [SerializeField] float alturaSalto = 1.35f;
    [SerializeField] float gravedad = -24f;
    [SerializeField] float margenSalto = .12f;
    [Header("Referencias")]
    public Transform visual;
    public Transform camara;
    public Animator animador;
    public bool recorridoLateral;

    CharacterController cuerpo;
    Vector3 velocidadHorizontal;
    float velocidadVertical;
    float tiempoEnMovimiento;
    float ultimoSuelo = float.NegativeInfinity;
    float ultimoSalto = float.NegativeInfinity;
    Vector3 inicio;
    float ejeSendero;
    AgacharseChasqui agacharse;
    SaltoNaturalChasqui saltoVisual; float preparacion = -1f;
    public float VelocidadMaxima => velocidadCorrer;
    public bool EnSuelo => cuerpo != null && cuerpo.isGrounded;

    void Awake()
    {
        cuerpo = GetComponent<CharacterController>();
        inicio = transform.position; ejeSendero = inicio.x;
        saltoVisual = GetComponent<SaltoNaturalChasqui>();agacharse=GetComponent<AgacharseChasqui>();
        if (camara == null && Camera.main != null) camara = Camera.main.transform;
    }

    public void Reubicar(Vector3 punto)
    {
        if(cuerpo==null)cuerpo=GetComponent<CharacterController>();cuerpo.enabled=false;transform.position=punto;cuerpo.enabled=true;velocidadHorizontal=Vector3.zero;velocidadVertical=0;tiempoEnMovimiento=0;preparacion=-1;ultimoSalto=ultimoSuelo=float.NegativeInfinity;var natural=GetComponent<SaltoNaturalChasqui>();if(natural!=null)natural.Reiniciar();
    }

    void Update()
    {
        float x = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1f : 0f)
                - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
        float y = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1f : 0f)
                - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);
        AplicarEntrada(new Vector2(x, y), Input.GetKeyDown(teclaSalto), Time.deltaTime, Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
    }

    public void AplicarEntrada(Vector2 entrada, bool saltar, float dt, bool correr = false)
    {
        if (cuerpo == null) cuerpo = GetComponent<CharacterController>();
        if (dt <= 0f) return;
        if (recorridoLateral) entrada.x = 0f;
        entrada = Vector2.ClampMagnitude(entrada, 1f);
        Vector3 adelante = camara != null ? Vector3.ProjectOnPlane(camara.forward, Vector3.up).normalized : Vector3.forward;
        if (recorridoLateral) adelante = Vector3.forward;
        if (adelante.sqrMagnitude < .01f) adelante = Vector3.forward;
        Vector3 derecha = Vector3.Cross(Vector3.up, adelante);
        Vector3 direccion = adelante * entrada.y + derecha * entrada.x;
        if (entrada.sqrMagnitude > .001f) tiempoEnMovimiento += dt;
        else tiempoEnMovimiento = 0f;
        bool bajo=agacharse!=null&&(agacharse.Agachado||agacharse.Factor>.1f);if(bajo)saltar=false;
        bool carreraActiva = !recorridoLateral && !bajo && (correr || tiempoEnMovimiento >= duracionPasoBreve);
        velocidadHorizontal = Vector3.MoveTowards(velocidadHorizontal, direccion * (bajo ? 1.35f : carreraActiva ? velocidadCorrer : velocidad), aceleracion * dt);

        if (cuerpo.isGrounded && velocidadVertical <= 0f)
        {
            ultimoSuelo = Time.time;
            velocidadVertical = -2f;
        }
        if (saltar) ultimoSalto = Time.time;
        bool saltoIniciado = false;
        if (preparacion < 0f && Time.time - ultimoSalto <= margenSalto && Time.time - ultimoSuelo <= margenSalto)
        {
            preparacion = saltoVisual != null ? SaltoNaturalChasqui.Anticipacion : 0f; if(saltoVisual != null) saltoVisual.Preparar();
            ultimoSalto = ultimoSuelo = float.NegativeInfinity;
        }
        if(preparacion >= 0f) { preparacion -= dt; if(preparacion <= 0f){preparacion=-1f;velocidadVertical=Mathf.Sqrt(alturaSalto * -2f * gravedad);saltoIniciado=true;if(saltoVisual!=null)saltoVisual.Despegar();} }
        velocidadVertical += gravedad * dt;
        CollisionFlags contacto = cuerpo.Move((velocidadHorizontal + Vector3.up * velocidadVertical) * dt);
        if (recorridoLateral && transform.position.x != ejeSendero)
        {
            // CharacterController can slide sideways when it hits an angled surface.
            var posicion = transform.position;
            posicion.x = ejeSendero;
            transform.position = posicion;
        }
        if ((contacto & CollisionFlags.Above) != 0 && velocidadVertical > 0f) velocidadVertical = 0f;
        if (animador != null)
        {
            animador.SetFloat("Movimiento", velocidadHorizontal.magnitude / velocidadCorrer, .07f, dt);
            animador.SetBool("EnSuelo", cuerpo.isGrounded);
            if (saltoIniciado && saltoVisual == null) animador.SetTrigger("Saltar");
        }
        if(saltoVisual!=null)saltoVisual.Actualizar(cuerpo.isGrounded,velocidadVertical,dt);
        if (visual != null && direccion.sqrMagnitude > .001f)
            visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(direccion, Vector3.up), 1f - Mathf.Exp(-giro * dt));

        if (transform.position.y < -15f)
        {
            cuerpo.enabled = false;
            transform.position = inicio + Vector3.up * .1f;
            cuerpo.enabled = true;
            velocidadHorizontal = Vector3.zero;
            tiempoEnMovimiento = 0f;
            velocidadVertical = 0f;
            ultimoSalto = ultimoSuelo = float.NegativeInfinity;
        }
    }
}




