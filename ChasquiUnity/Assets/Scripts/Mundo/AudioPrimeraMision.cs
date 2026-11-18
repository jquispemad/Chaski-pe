using System.Collections;
using UnityEngine;

// One small audio rig for the level; footsteps are driven by travelled distance.
[DefaultExecutionOrder(200)]
public sealed class AudioPrimeraMision : MonoBehaviour
{
    public AudioClip musica, ambiente, pututu, confirmar, abrirPanel;
    public AudioClip[] pasosTierra, pasosPiedra, pasosMadera, tela, impulso, aterrizaje, caida, quipu;
    AudioSource fondo, exterior, efectos, ropa;
    ChasquiMovimiento jugador;
    CharacterController cuerpo;
    AgacharseChasqui agacharse;
    Vector3 anterior;
    bool suelo, bajo, terminado;
    float distanciaPaso, aire, caidaMaxima, siguienteTela;
    int ultimoPaso = -1;

    void Start()
    {
        var primera = GetComponent<PrimeraMisionChaski>();
        jugador = primera != null ? primera.jugador : GetComponent<SegundaMisionChaski>().jugador;
        cuerpo = jugador.GetComponent<CharacterController>();
        agacharse = jugador.GetComponent<AgacharseChasqui>();
        anterior = jugador.transform.position;
        suelo = jugador.EnSuelo;
        fondo = Fuente("Música", .18f, true);
        exterior = Fuente("Viento y aves", .11f, true);
        efectos = Fuente("Acciones", .65f, false);
        ropa = Fuente("Ropa", .35f, false);
        fondo.clip = musica;
        exterior.clip = ambiente;
        if (musica != null) fondo.Play();
        if (ambiente != null) exterior.Play();
        Debug.Log("AUDIO_NIVEL01: música, ambiente y efectos preparados.");
    }

    AudioSource Fuente(string nombre, float volumen, bool bucle)
    {
        var objeto = new GameObject("Audio · " + nombre);
        objeto.transform.SetParent(transform, false);
        var fuente = objeto.AddComponent<AudioSource>();
        fuente.playOnAwake = false;
        fuente.spatialBlend = 0; // Side camera remains far from the character.
        fuente.volume = volumen;
        fuente.loop = bucle;
        fuente.dopplerLevel = 0;
        return fuente;
    }

    void LateUpdate()
    {
        if (jugador == null || terminado || Time.deltaTime <= 0) return;
        var posicion = jugador.transform.position;
        var desplazamiento = posicion - anterior;
        anterior = posicion;
        // Checkpoint teleports must not produce a burst of steps or impacts.
        if (desplazamiento.sqrMagnitude > 9f)
        { distanciaPaso = aire = caidaMaxima = 0; suelo = jugador.EnSuelo; return; }
        bool enSuelo = jugador.EnSuelo;
        bool agachado = agacharse != null && agacharse.Agachado;
        if (agachado != bajo)
        {
            Reproducir(tela, ropa, .7f);
            bajo = agachado;
        }
        if (!enSuelo)
        {
            aire += Time.deltaTime;
            caidaMaxima = Mathf.Max(caidaMaxima, -cuerpo.velocity.y);
            if (suelo && cuerpo.velocity.y > 1f) Reproducir(impulso, efectos, .55f);
            distanciaPaso = 0;
        }
        else
        {
            if (!suelo && aire > .12f)
            {
                Reproducir(caidaMaxima > 8f ? caida : aterrizaje, efectos, .85f);
                Reproducir(tela, ropa, .45f);
            }
            aire = caidaMaxima = 0;
            distanciaPaso += new Vector2(desplazamiento.x, desplazamiento.z).magnitude;
            float rapidez = new Vector2(cuerpo.velocity.x, cuerpo.velocity.z).magnitude;
            float longitudPaso = bajo ? .7f : Mathf.Lerp(1.2f, 1.45f, Mathf.InverseLerp(2.5f, 4.8f, rapidez));
            if (jugador.enabled && distanciaPaso >= longitudPaso)
            {
                distanciaPaso = 0;
                var pasos = PasosDelSuelo();
                if (pasos != null && pasos.Length > 0)
                {
                    int i = Random.Range(0, pasos.Length);
                    if (pasos.Length > 1 && i == ultimoPaso) i = (i + 1) % pasos.Length;
                    ultimoPaso = i;
                    efectos.pitch = Random.Range(.98f, 1.02f);
                    efectos.PlayOneShot(pasos[i], bajo ? .25f : .5f);
                }
                if (Time.time >= siguienteTela)
                {
                    Reproducir(tela, ropa, .18f);
                    siguienteTela = Time.time + Random.Range(1.4f, 2.5f);
                }
            }
        }
        suelo = enSuelo;
    }

    AudioClip[] PasosDelSuelo()
    {
        if (Physics.Raycast(jugador.transform.position + Vector3.up * .25f,
            Vector3.down, out var hit, 1.2f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            string nombre = hit.collider.name.ToLowerInvariant();
            if (nombre.Contains("piedra") || nombre.Contains("roca") || nombre.Contains("montón")) return pasosPiedra;
            if (nombre.Contains("puente") || nombre.Contains("tablón")) return pasosMadera;
        }
        return pasosTierra;
    }

    static void Reproducir(AudioClip[] clips, AudioSource fuente, float volumen)
    {
        if (fuente == null || clips == null || clips.Length == 0) return;
        fuente.pitch = Random.Range(.96f, 1.04f);
        fuente.PlayOneShot(clips[Random.Range(0, clips.Length)], volumen);
    }

    public void Completar()
    {
        if (terminado || efectos == null) return;
        terminado = true;
        StartCoroutine(BajarFondoAlTerminar());
        Reproducir(quipu, ropa, .9f);
        efectos.pitch = 1;
        if (pututu != null) efectos.PlayOneShot(pututu, .75f);
        if (abrirPanel != null) ropa.PlayOneShot(abrirPanel, .4f);
    }

    public void Confirmar()
    {
        if (efectos != null && confirmar != null) efectos.PlayOneShot(confirmar, .5f);
    }

    public void TocarPututu()
    {
        if (efectos == null || pututu == null) return;
        efectos.pitch = 1f;
        efectos.PlayOneShot(pututu, .65f);
        StartCoroutine(AtenuarDurantePututu(pututu.length));
    }
    IEnumerator AtenuarDurantePututu(float duracion)
    {
        float a=fondo.volume,b=exterior.volume;
        for(float t=0;t<duracion+.65f;t+=Time.unscaledDeltaTime)
        {
            if(terminado)yield break;
            float mezcla=t<duracion?Mathf.SmoothStep(0,1,Mathf.Clamp01(t/.35f)):1-Mathf.SmoothStep(0,1,Mathf.Clamp01((t-duracion)/.65f));
            fondo.volume=Mathf.Lerp(a,a*.4f,mezcla);exterior.volume=Mathf.Lerp(b,b*.55f,mezcla);
            yield return null;
        }
        if(!terminado){fondo.volume=a;exterior.volume=b;}
    }

    IEnumerator BajarFondoAlTerminar()
    {
        float musicaInicial = fondo.volume;
        float ambienteInicial = exterior.volume;
        const float duracion = 1.2f;
        float tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float factor = Mathf.Lerp(1f, .5f, Mathf.SmoothStep(0f, 1f, tiempo / duracion));
            fondo.volume = musicaInicial * factor;
            exterior.volume = ambienteInicial * factor;
            yield return null;
        }
        fondo.volume = musicaInicial * .5f;
        exterior.volume = ambienteInicial * .5f;
    }
}
