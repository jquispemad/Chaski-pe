using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(120)]
public sealed class RopaDinamicaChasqui : MonoBehaviour
{
    [Serializable] public class Volumen
    {
        public Transform inicio, fin;
        public Vector3 desplazamientoInicio, desplazamientoFin;
        public float radioInicio=.1f, radioFin=.1f;
    }
    struct Capsula { public Vector3 a,b,eje,min,max; public float ra,rb,inverso; }
    public SkinnedMeshRenderer piel;
    public MeshFilter salida;
    public Transform marco;
    [HideInInspector] public float[] libertad;
    [HideInInspector] public bool[] esCapa;
    public Volumen[] cuerpo;
    public BoxCollider[] entorno;
    public float separacion=.012f;
    Mesh horneada, deformada;
    readonly List<Vector3> lectura=new List<Vector3>();
    readonly List<Vector3> escritura=new List<Vector3>();
    Vector3[] posiciones, velocidades, objetivos, normales, suavizadas;
    int[][] vecinos;
    Capsula[] capsulas;
    Bounds[] cajas; int cantidadCajas;
    Matrix4x4 marcoAnterior;
    Vector3 posicionAnterior, velocidadAnterior;
    float reloj;
    bool iniciado;
    public float UltimaDeformacionMaxima { get; private set; }
    public int CantidadVertices => posiciones!=null?posiciones.Length:0;

    void Awake()
    {
        if(piel==null || salida==null || marco==null){enabled=false;return;}
        horneada=new Mesh();horneada.MarkDynamic();
        deformada=Instantiate(piel.sharedMesh);deformada.name="Ropa simulada en juego";deformada.MarkDynamic();salida.sharedMesh=deformada;
        int n=deformada.vertexCount;
        posiciones=new Vector3[n];velocidades=new Vector3[n];objetivos=new Vector3[n];normales=new Vector3[n];suavizadas=new Vector3[n];
        var enlaces=new HashSet<int>[n];for(int i=0;i<n;i++)enlaces[i]=new HashSet<int>();
        var triangles=deformada.triangles;
        for(int i=0;i<triangles.Length;i+=3)
        {
            int a=triangles[i],b=triangles[i+1],c=triangles[i+2];
            enlaces[a].Add(b);enlaces[a].Add(c);enlaces[b].Add(a);enlaces[b].Add(c);enlaces[c].Add(a);enlaces[c].Add(b);
        }
        vecinos=new int[n][];for(int i=0;i<n;i++){vecinos[i]=new int[enlaces[i].Count];enlaces[i].CopyTo(vecinos[i]);escritura.Add(Vector3.zero);}
        entorno=Array.FindAll(entorno??new BoxCollider[0],c=>c!=null);capsulas=new Capsula[cuerpo.Length];cajas=new Bounds[entorno.Length];
        marcoAnterior=marco.localToWorldMatrix;posicionAnterior=transform.position;
    }
    void LateUpdate()
    {
        if(horneada==null)return;
        float dt=Mathf.Clamp(Time.deltaTime,.001f,.04f);reloj+=dt;
        Matrix4x4 actual=marco.localToWorldMatrix,delta=actual*marcoAnterior.inverse;
        Vector3 velocidad=(transform.position-posicionAnterior)/dt;
        if(velocidad.magnitude>20f)iniciado=false;
        Vector3 aceleracion=Vector3.ClampMagnitude((velocidad-velocidadAnterior)/dt,8f);
        float fuerza=Mathf.Clamp01(Vector3.ProjectOnPlane(velocidad,Vector3.up).magnitude/4.8f);
        for(int i=0;i<cuerpo.Length;i++)
        {
            var v=cuerpo[i];
            Vector3 a=v.inicio.position+marco.TransformVector(v.desplazamientoInicio), b=v.fin.position+marco.TransformVector(v.desplazamientoFin), eje=b-a; Vector3 radio=Vector3.one*(Mathf.Max(v.radioInicio,v.radioFin)+separacion); capsulas[i]=new Capsula {a=a,b=b,eje=eje,inverso=eje.sqrMagnitude>.000001f?1f/eje.sqrMagnitude:0, min=Vector3.Min(a,b)-radio,max=Vector3.Max(a,b)+radio,ra=v.radioInicio,rb=v.radioFin};
        }
        cantidadCajas=0;Vector3 cerca=marco.position+Vector3.up;for(int i=0;i<entorno.Length;i++){if(entorno[i]==null)continue;var b=entorno[i].bounds;if(b.SqrDistance(cerca)<5f)cajas[cantidadCajas++]=b;}
        piel.BakeMesh(horneada);horneada.GetVertices(lectura);
        var ns=horneada.normals;
        Matrix4x4 aMundo=piel.transform.localToWorldMatrix,aLocal=salida.transform.worldToLocalMatrix;
        UltimaDeformacionMaxima=0;
        for(int i=0;i<lectura.Count;i++)
        {
            Vector3 basePose=aMundo.MultiplyPoint3x4(lectura[i]);
            normales[i]=piel.transform.TransformDirection(ns[i]).normalized;
            float f=libertad[i];Vector3 pLocal=marco.InverseTransformPoint(basePose);Vector3 deseada=basePose;
            if(esCapa[i])
            {
                float fase=f*8f-reloj*(7f+fuerza*7f);
                deseada-=marco.forward*(Mathf.Sin(pLocal.x*25f+f*.4f)*(.008f+.014f*f)*f);
                deseada+=marco.up*(Mathf.Sin(fase+pLocal.x*2f)*(.004f+.026f*fuerza)*f*f);
                deseada+=marco.right*(Mathf.Sin(fase*.75f)*(.003f+.012f*fuerza)*f*f);
                deseada-=aceleracion*(.0025f*f);
            }
            else
            {
                Vector3 radial=Vector3.ProjectOnPlane(basePose-(marco.position+marco.up*pLocal.y),marco.up).normalized;
                float angulo=Mathf.Atan2(pLocal.x,pLocal.z);
                deseada+=radial*(Mathf.Sin(angulo*9f+pLocal.y*3f)*.007f*f);
                deseada+=radial*(Mathf.Sin(angulo*7f-reloj*(4f+fuerza*4f))*(.003f+.012f*fuerza)*f*f);
                deseada-=aceleracion*(.0015f*f);
            }
            objetivos[i]=deseada;
            if(!iniciado){posiciones[i]=deseada;velocidades[i]=Vector3.zero;}
            else
            {
                posiciones[i]=delta.MultiplyPoint3x4(posiciones[i]);velocidades[i]=delta.MultiplyVector(velocidades[i]);
                if(f<.005f){posiciones[i]=deseada;velocidades[i]=Vector3.zero;}
                else posiciones[i]=Vector3.SmoothDamp(posiciones[i],deseada,ref velocidades[i],Mathf.Lerp(.018f,.065f,f),Mathf.Infinity,dt);
            }
            Vector3 desplazamiento=posiciones[i]-deseada;
            float limite=esCapa[i]?.13f:.22f;
            if(desplazamiento.sqrMagnitude>limite*limite)posiciones[i]=deseada+desplazamiento.normalized*limite;
            posiciones[i]=Resolver(posiciones[i],normales[i],true);
        }
        // Suavizar la corrección de contacto sin aplanar los detalles del tejido.
        for(int i=0;i<posiciones.Length;i++)
        {
            Vector3 suma=Vector3.zero;foreach(int vecino in vecinos[i])suma+=posiciones[vecino]-objetivos[vecino];
            Vector3 promedio=vecinos[i].Length>0?suma/vecinos[i].Length:Vector3.zero;
            suavizadas[i]=Vector3.Lerp(posiciones[i],objetivos[i]+promedio,.20f*libertad[i]);
        }
        for(int i=0;i<posiciones.Length;i++)
        {
            posiciones[i]=Resolver(suavizadas[i],normales[i],true);
            UltimaDeformacionMaxima=Mathf.Max(UltimaDeformacionMaxima,Vector3.Distance(posiciones[i],aMundo.MultiplyPoint3x4(lectura[i])));
            escritura[i]=aLocal.MultiplyPoint3x4(posiciones[i]);
        }
        deformada.SetVertices(escritura);deformada.RecalculateNormals();deformada.RecalculateBounds();
        marcoAnterior=actual;posicionAnterior=transform.position;velocidadAnterior=velocidad;iniciado=true;
    }
    Vector3 Resolver(Vector3 p,Vector3 normal,bool contraCajas)
    {
        if(contraCajas)
            for(int caja=0;caja<cantidadCajas;caja++)
            {
                var box=cajas[caja];if(!box.Contains(p))continue;
                Vector3 d=p-box.center,h=box.extents;float x=h.x-Mathf.Abs(d.x),y=h.y-Mathf.Abs(d.y),z=h.z-Mathf.Abs(d.z);
                if(x<y&&x<z)p.x=box.center.x+(d.x>=0?1:-1)*(h.x+separacion);
                else if(y<z)p.y=box.center.y+(d.y>=0?1:-1)*(h.y+separacion);
                else p.z=box.center.z+(d.z>=0?1:-1)*(h.z+separacion);
            }
        for(int paso=0;paso<20;paso++) { bool corregido=false; foreach(var c in capsulas)
            {
                if(p.x<c.min.x||p.x>c.max.x||p.y<c.min.y||p.y>c.max.y||p.z<c.min.z||p.z>c.max.z)continue; Vector3 eje=c.eje;float t=Mathf.Clamp01(Vector3.Dot(p-c.a,eje)*c.inverso);
                Vector3 centro=c.a+eje*t,d=p-centro;float r=Mathf.Lerp(c.ra,c.rb,t)+separacion;
                if(d.sqrMagnitude<r*r){p=centro+(d.sqrMagnitude>.0000001f?d.normalized:normal)*r;corregido=true;}
            }
            if(!corregido)break;
        }
        return p;
    }
    public int ContarPenetraciones()
    {
        if(!iniciado)return 0;int count=0;
        foreach(var p in posiciones)
            foreach(var c in capsulas)
            {
                if(p.x<c.min.x||p.x>c.max.x||p.y<c.min.y||p.y>c.max.y||p.z<c.min.z||p.z>c.max.z)continue; Vector3 eje=c.eje;float t=Mathf.Clamp01(Vector3.Dot(p-c.a,eje)*c.inverso);
                float r=Mathf.Lerp(c.ra,c.rb,t)-.002f;
                if((p-c.a-eje*t).sqrMagnitude<r*r){count++;break;}
            }
        return count;
    }
    void OnDestroy(){if(horneada!=null)Destroy(horneada);if(deformada!=null)Destroy(deformada);}
}








