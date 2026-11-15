using UnityEngine;

// Cada fase sigue la velocidad y el contacto reales, sin limitar el tiempo en el aire.
public sealed class SaltoNaturalChasqui : MonoBehaviour
{
    public Animator animador;
    public const float Anticipacion = .09f;
    int fase; float tiempo, sinSuelo;
    public int Fase => fase; public float TiempoFase => tiempo;
    public int Aterrizajes { get; private set; }
    void Estado(string nombre,float mezcla){animador.CrossFadeInFixedTime(nombre,mezcla,0,0);tiempo=0;}
    public void Reiniciar(){fase=0;tiempo=sinSuelo=0;if(animador!=null)Estado("Caminar y correr",.06f);}
    public void Preparar(){if(animador==null)return;fase=1;Estado("Preparar salto",.035f);}
    public void Despegar(){if(animador==null)return;fase=2;Estado("Salto",.045f);}
    public void Actualizar(bool suelo,float vertical,float dt)
    {
        if(animador==null)return;tiempo+=dt;sinSuelo=suelo?0:sinSuelo+dt;
        if(fase==0&&sinSuelo>.08f&&vertical<-.5f){fase=3;Estado("Caida natural",.10f);}
        else if(fase==2&&vertical<=0){fase=3;Estado("Caida natural",.12f);}
        else if((fase==2||fase==3)&&suelo&&tiempo>.055f){fase=4;Aterrizajes++;Estado("Recepcion salto",.045f);}
        else if(fase==4&&tiempo>.25f){fase=0;Estado("Caminar y correr",.12f);}
    }
}


