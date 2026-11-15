using UnityEngine;
[DefaultExecutionOrder(105)]
public sealed class VolumenTelaChasqui : MonoBehaviour
{
    public Transform inicio,fin,marco;
    public Vector3 desplazamientoInicio,desplazamientoFin;
    public CapsuleCollider colision;
    void LateUpdate(){Actualizar();}
    public void Actualizar()
    {
        if(inicio==null||fin==null||colision==null)return;
        Vector3 a=inicio.position+marco.TransformVector(desplazamientoInicio),b=fin.position+marco.TransformVector(desplazamientoFin),eje=b-a;
        transform.position=(a+b)*.5f;if(eje.sqrMagnitude>.000001f)transform.rotation=Quaternion.FromToRotation(Vector3.up,eje.normalized);
        colision.height=eje.magnitude+colision.radius*2;
    }
}
