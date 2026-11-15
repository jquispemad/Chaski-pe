using System.Collections.Generic;
using UnityEngine;

// Simulate after the tunic has finished deforming, so contact follows its
// current surface rather than only the character's rest pose.
[DefaultExecutionOrder(135)]
public sealed class TelaCapaSinCruces : MonoBehaviour
{
    public CapaFisicaChasqui capa;
    public RopaDinamicaChasqui ropa;
    public float separacion=.028f;
    public int Penetraciones {get;private set;}
    public Vector3[] Vertices => locales;
    struct Enlace {public int a,b;public float largo,rigidez;}
    readonly List<Enlace> enlaces=new List<Enlace>();
    readonly Dictionary<Vector2Int,float> espalda=new Dictionary<Vector2Int,float>();
    SkinnedMeshRenderer origen;Mesh horneada,malla;Transform marco,salida;
    Vector3[] puntos,antes,objetivos,locales;float[] libre;int[] superficie;
    Vector3 ultima;Matrix4x4 marcoAnterior;float[] largoAnclaje;bool listo;
    Vector2[] coordenadas;
    float apertura;
    const float Celda=.065f;
    void Start()
    {
        if(capa==null||ropa==null){enabled=false;return;}
        marco=capa.marco;origen=capa.tela.GetComponent<SkinnedMeshRenderer>();
        capa.tela.enabled=false;origen.enabled=false;
        horneada=new Mesh();origen.BakeMesh(horneada);
        malla=Instantiate(origen.sharedMesh);malla.name="Capa flexible en contacto con tunica";malla.MarkDynamic();
        var o=new GameObject("Capa · tela simulada sin cruces");o.transform.SetParent(origen.transform,false);salida=o.transform;
        o.AddComponent<MeshFilter>().sharedMesh=malla;var r=o.AddComponent<MeshRenderer>();r.sharedMaterials=origen.sharedMaterials;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        var uv=malla.uv;coordenadas=uv;var v=horneada.vertices;int n=v.Length;
        puntos=new Vector3[n];antes=new Vector3[n];objetivos=new Vector3[n];locales=new Vector3[n];libre=new float[n];
        for(int i=0;i<n;i++){v[i].x*=1.16f;puntos[i]=antes[i]=origen.transform.TransformPoint(v[i]);libre[i]=uv[i].y<.075f?0:1;}
        const int ancho=33,alto=43;
        for(int y=0;y<alto;y++)for(int x=0;x<ancho;x++)
        {
            int a=y*ancho+x;
            if(x+1<ancho)Agregar(a,a+1,1);
            if(y+1<alto)Agregar(a,a+ancho,1);
            if(x+1<ancho&&y+1<alto){Agregar(a,a+ancho+1,.65f);Agregar(a+1,a+ancho,.65f);}
            if(y+2<alto)Agregar(a,a+ancho*2,.35f);
        }
        var usados=new HashSet<int>(ropa.salida.sharedMesh.triangles);superficie=new int[usados.Count];usados.CopyTo(superficie);
        largoAnclaje=new float[n];for(int i=0;i<n;i++)largoAnclaje[i]=Vector3.Distance(puntos[i],puntos[i%33+66]);
        ultima=transform.position;marcoAnterior=marco.localToWorldMatrix;listo=true;
    }
    void Agregar(int a,int b,float rigidez){enlaces.Add(new Enlace{a=a,b=b,largo=Vector3.Distance(puntos[a],puntos[b]),rigidez=rigidez});}
    void LateUpdate()
    {
        if(!listo)return;
        float dt=Mathf.Clamp(Time.deltaTime,.001f,.0333f);origen.BakeMesh(horneada);var basePose=horneada.vertices;
        Vector3 traslado=transform.position-ultima;ultima=transform.position;
        bool teleport=traslado.magnitude>2;
        apertura=Mathf.Lerp(apertura,Mathf.Clamp01(capa.Rapidez/4.8f),1-Mathf.Exp(-dt*3.5f));
        var transporte=marco.localToWorldMatrix*marcoAnterior.inverse;marcoAnterior=marco.localToWorldMatrix;
        ConstruirEspalda();
        for(int i=0;i<puntos.Length;i++)
        {
            basePose[i].x*=1.16f;objetivos[i]=origen.transform.TransformPoint(basePose[i]);
            if(libre[i]==0){var q=marco.InverseTransformPoint(objetivos[i]);if(Limite(q,out float z)){q.z=Mathf.Min(q.z,z);objetivos[i]=marco.TransformPoint(q);}}
            puntos[i]=transporte.MultiplyPoint3x4(puntos[i]);antes[i]=transporte.MultiplyPoint3x4(antes[i]);
            if(teleport){puntos[i]=antes[i]=objetivos[i];}
            if(libre[i]==0){puntos[i]=antes[i]=objetivos[i];continue;}
            Vector3 inercia=Vector3.ClampMagnitude(puntos[i]-antes[i],.09f)*Mathf.Exp(-dt*2.0f);
            antes[i]=puntos[i];
            float caida=coordenadas[i].y,ancho=coordenadas[i].x;
            float carrera=Mathf.Clamp01(capa.Rapidez/4.8f);
            // Distributed pressure travels from the shoulders towards the hem.
            // The hem is free to lift and flutter; the neck remains attached.
            float onda=Mathf.Sin(Time.time*8.3f-caida*7+ancho*2.4f);
            Vector3 viento=capa.tela.externalAcceleration
                +marco.up*(carrera*(24+onda*12)*caida)
                -marco.forward*(carrera*(14+onda*9)*caida)
                +marco.right*(Mathf.Sin(Time.time*6-caida*5)*carrera*3*caida);
            puntos[i]+=inercia+(Physics.gravity+viento)*dt*dt;
        }
        // Short substeps and structural/shear constraints retain the cloth's size.
        for(int paso=0;paso<16;paso++)
        {
            foreach(var e in enlaces)
            {
                Vector3 d=puntos[e.b]-puntos[e.a];float l=d.magnitude,w=libre[e.a]+libre[e.b];
                if(l<.000001f||w==0)continue;
                // Resist extension, but allow the sheet to gather and fold.
                // Pushing compressed links apart was keeping the idle corners rigid.
                float rigidez=l<e.largo?e.rigidez*.06f:e.rigidez;
                Vector3 ajuste=d*((l-e.largo)/l*rigidez/w);
                puntos[e.a]+=ajuste*libre[e.a];puntos[e.b]-=ajuste*libre[e.b];
            }
            for(int i=0;i<puntos.Length;i++)
            {
                if(libre[i]==0){puntos[i]=objetivos[i];continue;}
                // Keep the cloth spread across the back. Prevent adjacent rows
                // from folding through each other and bunching at the centre.
                var q=marco.InverseTransformPoint(puntos[i]);var refLocal=marco.InverseTransformPoint(objetivos[i]);
                float t=coordenadas[i].y;
                float desplazar=Mathf.Sin(Time.time*5.2f-t*4)*.025f*t*Mathf.Clamp01(capa.Rapidez/4.8f);
                float reposo=1-apertura;
                float recoger=Mathf.Lerp(.66f,1,apertura);
                q.x=Mathf.Lerp(q.x,refLocal.x*recoger+desplazar,Mathf.Lerp(.075f,.20f,apertura));
                float borde=Mathf.Pow(Mathf.Abs(coordenadas[i].x-.5f)*2,3);
                if(t<.5f)q.y=Mathf.Lerp(q.y,Mathf.Min(q.y,refLocal.y-.065f*borde*reposo),.04f);
                if(i>=33){var filaAnterior=marco.InverseTransformPoint(puntos[i-33]);q.y=Mathf.Min(q.y,filaAnterior.y-.004f);}
                puntos[i]=marco.TransformPoint(q);
                var ancla=objetivos[i%33+66];var d=puntos[i]-ancla;float maximo=largoAnclaje[i];if(d.sqrMagnitude>maximo*maximo)puntos[i]=ancla+d.normalized*maximo;
                Contacto(i);
            }
        }
        Penetraciones=0;
        for(int i=0;i<puntos.Length;i++)
        {
            if(libre[i]>0){var q=marco.InverseTransformPoint(puntos[i]);if(Limite(q,out float z)&&q.z>z+.002f)Penetraciones++;}
            locales[i]=salida.InverseTransformPoint(puntos[i]);
        }
        malla.vertices=locales;malla.RecalculateNormals();malla.RecalculateBounds();
    }
    void ConstruirEspalda()
    {
        espalda.Clear();var v=ropa.salida.sharedMesh.vertices;var t=ropa.salida.transform;
        foreach(int i in superficie)
        {
            Vector3 q=marco.InverseTransformPoint(t.TransformPoint(v[i]));
            if(q.y<.40f||q.y>1.58f)continue;
            var key=new Vector2Int(Mathf.RoundToInt(q.x/Celda),Mathf.RoundToInt(q.y/Celda));
            if(!espalda.TryGetValue(key,out float z)||q.z<z)espalda[key]=q.z;
        }
    }
    bool Limite(Vector3 q,out float limite)
    {
        limite=float.PositiveInfinity;var key=new Vector2Int(Mathf.RoundToInt(q.x/Celda),Mathf.RoundToInt(q.y/Celda));bool encontro=false;
        for(int x=-1;x<=1;x++)for(int y=-1;y<=1;y++)
            if(espalda.TryGetValue(key+new Vector2Int(x,y),out float z)){limite=Mathf.Min(limite,z-separacion);encontro=true;}
        return encontro;
    }
    void Contacto(int i)
    {
        Vector3 q=marco.InverseTransformPoint(puntos[i]);
        if(Limite(q,out float z)&&q.z>z)
        {
            q.z=z;puntos[i]=marco.TransformPoint(q);
            // Remove velocity into the garment; retain sliding along its surface.
            Vector3 anteriorLocal=marco.InverseTransformPoint(antes[i]);anteriorLocal.z=Mathf.Min(anteriorLocal.z,z);antes[i]=marco.TransformPoint(anteriorLocal);
        }
    }
    void OnDestroy(){if(horneada!=null)Destroy(horneada);if(malla!=null)Destroy(malla);}
}
