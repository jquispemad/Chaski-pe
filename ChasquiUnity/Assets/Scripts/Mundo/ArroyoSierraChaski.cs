using UnityEngine;

public sealed class ArroyoSierraChaski : MonoBehaviour
{
    public static float Centro(float x)=>280+(6*Mathf.Sin(x*.055f)+3*Mathf.Sin(x*.13f))*Mathf.SmoothStep(0,1,Mathf.InverseLerp(10,24,Mathf.Abs(x)));
    public static float Agua(float x)=>x>=0?-.7f+18*Mathf.SmoothStep(0,1,Mathf.Clamp01((x-14)/74)):-.7f-3*Mathf.Pow(Mathf.Abs(x)/110,1.3f);
    public static float Ancho(float x)=>(2.25f+Mathf.PerlinNoise(x*.07f+15,8)*1.4f)*Mathf.SmoothStep(0,1,Mathf.Clamp01((88-x)/7));
    public bool BajoAgua(Vector3 p)=>p.x<=88 && Mathf.Abs(p.z-Centro(p.x))<Ancho(p.x) && p.y<Agua(p.x)+.04f;
}
