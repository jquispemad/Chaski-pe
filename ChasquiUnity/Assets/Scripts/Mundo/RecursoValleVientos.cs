using UnityEngine;

public sealed class RecursoValleVientos:MonoBehaviour
{
    public int tipo;
    public string identificador;
    public string Nombre=>new[]{"Coca","Cancha","Ch’arki","Agua"}[Mathf.Clamp(tipo,0,3)];
}
