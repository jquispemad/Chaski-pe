using System.Collections.Generic;
using UnityEngine;

public sealed class AspectoRacionChaski:MonoBehaviour
{
    readonly List<Object> temporales=new List<Object>();
    Material ColorMate(string nombre,Color color,float brillo=.08f){var m=new Material(Shader.Find("Standard")){name=nombre,color=color};m.SetFloat("_Glossiness",brillo);temporales.Add(m);return m;}
    void Malla(string nombre,Mesh mesh,Material mat,Vector3 p,Quaternion r)
    {temporales.Add(mesh);var o=new GameObject(nombre);o.transform.SetParent(transform,false);o.transform.localPosition=p;o.transform.localRotation=r;o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=mat;}
    static Mesh Torneado(Vector2[] perfil)
    {
        const int n=32;var v=new List<Vector3>();var uv=new List<Vector2>();var tr=new List<int>();
        for(int j=0;j<perfil.Length;j++)for(int i=0;i<=n;i++)
        {float a=i*Mathf.PI*2/n;v.Add(new Vector3(Mathf.Cos(a)*perfil[j].x,perfil[j].y,Mathf.Sin(a)*perfil[j].x));uv.Add(new Vector2(i/(float)n,j/(float)(perfil.Length-1)));if(j>0&&i<n){int k=j*(n+1)+i,b=k-n-1;tr.AddRange(new[]{b,k,b+1,b+1,k,k+1});}}
        var m=new Mesh{name="Cerámica · interior y borde"};m.SetVertices(v);m.SetUVs(0,uv);m.SetTriangles(tr,0);m.RecalculateNormals();return m;
    }
    static Mesh Alimento(bool hoja)
    {
        var v=new List<Vector3>();var t=new List<int>();
        for(int i=0;i<=8;i++)
        {float u=i/8f,w=hoja?Mathf.Sin(u*Mathf.PI)*.023f:.012f*(.8f+.2f*Mathf.Sin(i*2));for(int k=0;k<3;k++)v.Add(new Vector3((k-1)*w,Mathf.Sin(u*5)*.006f+(k==1?.007f:0),u*.10f));if(i>0)for(int k=0;k<2;k++){int a=(i-1)*3+k,b=i*3+k;t.AddRange(new[]{a,b,a+1,a+1,b,b+1});}}
        int frente=v.Count,indices=t.Count;v.AddRange(v.ToArray());for(int j=0;j<indices;j+=3)t.AddRange(new[]{t[j+2]+frente,t[j+1]+frente,t[j]+frente});
        var m=new Mesh{name=hoja?"Hoja · nervadura y curvatura":"Ch’arki · tira fibrosa"};m.SetVertices(v);m.SetTriangles(t,0);m.RecalculateNormals();return m;
    }
    public static GameObject Crear(int tipo,bool enMano){var o=new GameObject("Ración detallada · "+tipo);o.AddComponent<AspectoRacionChaski>().Construir(tipo,enMano);return o;}
    void Construir(int tipo,bool mano)
    {
        var arcilla=ColorMate("Arcilla cocida",new Color(.48f,.24f,.13f));var ocre=ColorMate("Maíz tostado y engobe",new Color(.75f,.52f,.25f));
        if(tipo==3)
        {
            Malla("Vaso abierto",Torneado(new[]{new Vector2(0,0),new Vector2(.041f,0),new Vector2(.052f,.13f),new Vector2(.045f,.133f),new Vector2(.035f,.018f),new Vector2(0,.018f)}),arcilla,Vector3.zero,Quaternion.identity);
            Malla("Banda ocre",Torneado(new[]{new Vector2(.051f,.10f),new Vector2(.052f,.11f)}),ocre,Vector3.zero,Quaternion.identity);
            Malla("Agua dentro del vaso",Torneado(new[]{new Vector2(.041f,.106f),new Vector2(0,.106f)}),ColorMate("Agua · reflejo",new Color(.23f,.39f,.37f),.85f),Vector3.zero,Quaternion.identity);return;
        }
        if(!mano)Malla("Cuenco abierto",Torneado(new[]{new Vector2(0,0),new Vector2(.075f,0),new Vector2(.14f,.062f),new Vector2(.131f,.069f),new Vector2(.066f,.014f),new Vector2(0,.014f)}),arcilla,Vector3.zero,Quaternion.identity);
        int n=mano?(tipo==1?5:2):tipo==1?30:9;
        var verde=ColorMate("Hoja natural",new Color(.25f,.34f,.085f));var carne=ColorMate("Carne deshidratada",new Color(.38f,.18f,.08f));
        for(int i=0;i<n;i++)
        {
            float ang=i*2.39996f,rad=mano?.012f:.085f*Mathf.Sqrt((i+.5f)/n);var p=new Vector3(Mathf.Cos(ang)*rad,mano?0:.045f+Mathf.Sin(i)*.008f,Mathf.Sin(ang)*rad);
            if(tipo!=1)Malla(tipo==0?"Hoja de coca":"Tira de ch’arki",Alimento(tipo==0),tipo==0?verde:carne,p,Quaternion.Euler(0,i*137,0));
            else{var grano=GameObject.CreatePrimitive(PrimitiveType.Sphere);grano.name="Grano de maíz tostado";grano.transform.SetParent(transform,false);grano.transform.localPosition=p;grano.transform.localScale=new Vector3(.016f,.014f,.021f);grano.transform.localRotation=Quaternion.Euler(i*23,i*47,0);grano.GetComponent<Renderer>().sharedMaterial=i%5==0?arcilla:ocre;DestroyImmediate(grano.GetComponent<Collider>());}
        }
    }
    void OnDestroy(){if(Application.isPlaying)foreach(var o in temporales)if(o!=null)Destroy(o);}
}
