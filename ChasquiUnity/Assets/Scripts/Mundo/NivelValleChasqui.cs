using UnityEngine;

public sealed class NivelValleChasqui : MonoBehaviour
{
    public ChasquiMovimiento jugador;
    public Transform[] controles, recuerdos;
    public Transform llegada;
    public Terrain terreno;
    int ultimo=-1, recogidos; bool completo;
    bool[] encontrados;
    Vector3 salida, reaparicion;
    float avisoHasta;
    string aviso="Lleva el mensaje hasta el tambo del otro lado del valle.";
    public int UltimoControl=>ultimo;
    public bool Completado=>completo;
    public int Recogidos=>recogidos;
    void Start(){if(GetComponent<InterfazIncaChasqui>()==null)gameObject.AddComponent<InterfazIncaChasqui>();ColisionesValleChasqui.Aplicar(gameObject.scene);LimitesCaminoInca.Crear(transform);salida=reaparicion=jugador.transform.position;encontrados=new bool[recuerdos.Length];avisoHasta=Time.time+7;}
    void Update()
    {
        if(jugador==null)return;
        Vector3 p=jugador.transform.position;
        if(Input.GetKeyDown(KeyCode.R)) {if(completo)Reiniciar();else Reaparecer();}
        if(p.y<-.1f || p.z<3 || p.z>348 || Mathf.Abs(p.x)>100){Reaparecer();return;}
        for(int i=ultimo+1;i<controles.Length;i++)if(Vector3.Distance(p,controles[i].position)<3.4f){ultimo=i;reaparicion=controles[i].position+Vector3.up*.12f;Avisar("Punto de control "+(i+1)+" guardado");break;}
        for(int i=0;i<recuerdos.Length;i++)if(!encontrados[i]){
            recuerdos[i].Rotate(0,45*Time.deltaTime,0,Space.World);
            if(Vector3.Distance(p+Vector3.up*.9f,recuerdos[i].position)<1.45f){encontrados[i]=true;recogidos++;recuerdos[i].gameObject.SetActive(false);Avisar("Quipu encontrado · "+recogidos+" / "+recuerdos.Length);}
        }
        if(!completo&&Vector3.Distance(p,llegada.position)<4.2f){completo=true;Avisar("Mensaje entregado. ¡El tambo te recibe!");}
    }
    public void Reaparecer(){jugador.Reubicar(reaparicion);Avisar("De vuelta al último punto seguro");}
    public void Reiniciar(){ultimo=-1;recogidos=0;completo=false;reaparicion=salida;for(int i=0;i<recuerdos.Length;i++){encontrados[i]=false;recuerdos[i].gameObject.SetActive(true);}jugador.Reubicar(salida);}
    void Avisar(string texto){aviso=texto;avisoHasta=Time.time+3.5f;}
    public string AvisoActual => aviso;
    public bool MostrarAviso => Time.time < avisoHasta;
}

