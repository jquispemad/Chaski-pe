using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class CrearPuenteIchu
{
    const string D="Assets/Niveles/Valle/";
    static CrearPuenteIchu(){EditorApplication.delayCall+=Auto;}
    static void Auto(){if(!EditorApplication.isPlayingOrWillChangePlaymode&&!SessionState.GetBool("PuenteIchuV1",false))Crear();}
    [MenuItem("Chasqui/Crear puente colgante de ichu")]
    public static void Crear()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        var n=UnityEngine.Object.FindFirstObjectByType<NivelValleChasqui>();if(n==null)return;
        var viejo=GameObject.Find("Puente inca · fibras trenzadas");if(viejo!=null){SessionState.SetBool("PuenteIchuV1",true);return;}
        foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            if(r.name.StartsWith("Puente · tablones")||r.name=="Poste del puente"||r.name=="Soga del puente"||r.name=="Baranda · colision")
            {r.enabled=false;foreach(var c in r.GetComponents<Collider>())c.enabled=false;}
        float x=LimitesCaminoInca.Centro(164),y=ConstruirNivelValle.Base(164)+.1f;
        var root=new GameObject("Puente inca · fibras trenzadas");
        root.transform.position=new Vector3(x,y,151);
        var mat=AssetDatabase.LoadAssetAtPath<Material>(D+"SogasIchu.mat");if(mat==null){mat=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(mat,D+"SogasIchu.mat");}
        mat.color=new Color(.60f,.43f,.22f);mat.SetFloat("_Glossiness",.09f);mat.mainTexture=TexturaSoga();mat.mainTextureScale=new Vector2(3,12);EditorUtility.SetDirty(mat);
        var m=new Geometria();
        // Four load-bearing braided cables under a continuous woven walking surface.
        foreach(float off in new[]{-1.55f,-.52f,.52f,1.55f})Trenza(m,t=>new Vector3(off,Curva(t)-.13f,t),26,.052f,.038f);
        for(int i=0;i<=236;i++)
        {
            float z=i*26/236f;float yy=Curva(z);
            Tubo(m,new Vector3(-1.66f,yy,z),new Vector3(1.66f,yy,z),.057f,6);
        }
        for(int lado=-1;lado<=1;lado+=2)
        {
            float xx=lado*1.72f;
            Trenza(m,t=>new Vector3(xx,Curva(t)+1.25f,t),26,.055f,.043f);
            Trenza(m,t=>new Vector3(xx,Curva(t)+.55f,t),26,.025f,.022f);
            for(int i=0;i<=52;i++)
            {
                float z=i*.5f;
                Tubo(m,new Vector3(xx,Curva(z)-.05f,z),new Vector3(xx,Curva(z)+1.23f,z),.027f,5);
                if(i<52)
                {
                    float b=z+.5f;Tubo(m,new Vector3(xx,Curva(z)+.12f,z),new Vector3(xx,Curva(b)+1.2f,b),.018f,5);
                    Tubo(m,new Vector3(xx,Curva(z)+1.2f,z),new Vector3(xx,Curva(b)+.12f,b),.018f,5);
                }
            }
            // Deep anchoring posts remain at the abutments, away from the walking corridor.
            foreach(float z in new[]{0f,26f})
            {
                var ancla=GameObject.CreatePrimitive(PrimitiveType.Cube);ancla.name="Anclaje de piedra del puente";ancla.transform.SetParent(root.transform,false);
                ancla.transform.localPosition=new Vector3(lado*2.35f,.40f,z);ancla.transform.localScale=new Vector3(.75f,1.7f,1.0f);
                ancla.GetComponent<MeshRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(D+"PiedraMuro.mat");
                Tubo(m,new Vector3(xx,1.25f,z),new Vector3(lado*2.35f,.85f,z),.11f,9);
            }
            for(int i=0;i<52;i++)
            {
                float z=i*.5f,b=z+.5f;var c=new GameObject("Limite lateral del puente");c.transform.SetParent(root.transform,false);
                c.transform.localPosition=new Vector3(lado*1.83f,(Curva(z)+Curva(b))*.5f+.58f,(z+b)*.5f);
                c.AddComponent<BoxCollider>().size=new Vector3(.16f,1.4f,.53f);
            }
        }
        Agregar(root,"Ichu entretejido · cuerdas y piso",m.Mesh("Puente de sogas trenzadas"),mat,"PuenteIchuVisual.asset",false);
        // Smooth closed mesh avoids catching feet on every individual strand.
        var piso=new Geometria();
        for(int i=0;i<=104;i++)
        {
            float z=i*.25f,yy=Curva(z)+.05f;
            piso.v.AddRange(new[]{new Vector3(-1.69f,yy,z),new Vector3(1.69f,yy,z),new Vector3(-1.69f,yy-.20f,z),new Vector3(1.69f,yy-.20f,z)});
        }
        for(int i=0;i<104;i++){int a=i*4;piso.Quad(a,a+4,a+5,a+1);piso.Quad(a+2,a+3,a+7,a+6);piso.Quad(a,a+2,a+6,a+4);piso.Quad(a+1,a+5,a+7,a+3);}
        piso.Quad(0,1,3,2);piso.Quad(416,418,419,417);
        Agregar(root,"Superficie continua del puente",piso.Mesh("Colision del puente tejido"),mat,"PuenteIchuColision.asset",true);
        root.isStatic=true;Physics.SyncTransforms();
        int soportes=0;for(int i=0;i<=104;i++){float z=151+i*.25f;if(Physics.Raycast(new Vector3(x,y+3,z),Vector3.down,out var h,8)&&h.collider.transform.IsChildOf(root.transform))soportes++;}
        EditorSceneManager.MarkSceneDirty(n.gameObject.scene);EditorSceneManager.SaveScene(n.gameObject.scene);AssetDatabase.SaveAssets();SessionState.SetBool("PuenteIchuV1",true);
        File.WriteAllText("Temp/chasqui_puente_ichu.txt","Puente de ichu creado\nApoyo continuo: "+soportes+" / 105\nLargo: 26 m\nAncho transitable: 3.38 m");
        Debug.Log("PUENTE_ICHU_LISTO: "+soportes+" / 105 apoyos");
        Foto(new Vector3(x+10,y+6,145),new Vector3(x,y+.6f,164));
    }
    static float Curva(float z)=>-.65f*Mathf.Sin(Mathf.Clamp01(z/26)*Mathf.PI);
    class Geometria
    {
        public List<Vector3> v=new List<Vector3>();public List<int> t=new List<int>();public List<Vector2> uv=new List<Vector2>();
        public void Quad(int a,int b,int c,int d){t.AddRange(new[]{a,b,c,a,c,d});}
        public Mesh Mesh(string n){var m=new Mesh{name=n,indexFormat=IndexFormat.UInt32};m.SetVertices(v);m.SetTriangles(t,0);if(uv.Count==v.Count)m.SetUVs(0,uv);m.RecalculateNormals();m.RecalculateBounds();return m;}
    }
    static void Tubo(Geometria m,Vector3 a,Vector3 b,float radio,int lados)
    {
        Vector3 dir=(b-a).normalized;Vector3 eje=Vector3.Cross(dir,Mathf.Abs(dir.y)>.9f?Vector3.right:Vector3.up).normalized,otro=Vector3.Cross(dir,eje);int k=m.v.Count;
        for(int j=0;j<2;j++)for(int i=0;i<=lados;i++){float ang=i*2*Mathf.PI/lados;m.v.Add((j==0?a:b)+(eje*Mathf.Cos(ang)+otro*Mathf.Sin(ang))*radio);m.uv.Add(new Vector2(i/(float)lados,j*Vector3.Distance(a,b)));}
        for(int i=0;i<lados;i++)m.Quad(k+i,k+i+1,k+i+lados+2,k+i+lados+1);
    }
    static void Trenza(Geometria m,Func<float,Vector3> centro,float largo,float vuelta,float radio)
    {
        for(int hebra=0;hebra<3;hebra++)for(int i=0;i<260;i++)
        {
            float a=i*largo/260,b=(i+1)*largo/260,ang=a*25+hebra*Mathf.PI*2/3,ang2=b*25+hebra*Mathf.PI*2/3;
            Tubo(m,centro(a)+new Vector3(Mathf.Cos(ang)*vuelta,Mathf.Sin(ang)*vuelta,0),centro(b)+new Vector3(Mathf.Cos(ang2)*vuelta,Mathf.Sin(ang2)*vuelta,0),radio,5);
        }
    }
    static void Agregar(GameObject root,string nombre,Mesh mesh,Material mat,string archivo,bool colision)
    {
        string path=D+archivo;var existente=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(existente==null)AssetDatabase.CreateAsset(mesh,path);else{EditorUtility.CopySerialized(mesh,existente);UnityEngine.Object.DestroyImmediate(mesh);mesh=existente;}
        var o=new GameObject(nombre);o.transform.SetParent(root.transform,false);o.isStatic=true;
        if(colision)o.AddComponent<MeshCollider>().sharedMesh=mesh;
        else{o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=mat;}
    }
    static Texture2D TexturaSoga()
    {
        int N=256;var tex=new Texture2D(N,N,TextureFormat.RGB24,true);var p=new Color[N*N];
        for(int y=0;y<N;y++)for(int x=0;x<N;x++){float f=.70f+.13f*Mathf.Sin(x*.62f+y*.32f)+.17f*Mathf.PerlinNoise(x*.8f,y*.25f);p[y*N+x]=new Color(f,f*.90f,f*.68f);}
        tex.SetPixels(p);tex.Apply();string path=D+"SogaIchu.png";File.WriteAllBytes(path,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path);return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    static void Foto(Vector3 pos,Vector3 objetivo)
    {
        var o=new GameObject("Vista temporal del puente");var c=o.AddComponent<Camera>();c.transform.position=pos;c.transform.LookAt(objetivo);c.fieldOfView=55;c.farClipPlane=600;
        var rt=RenderTexture.GetTemporary(1400,900,24);var anterior=RenderTexture.active;var tex=new Texture2D(1400,900,TextureFormat.RGB24,false);
        try{c.targetTexture=rt;c.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1400,900),0,0);tex.Apply();File.WriteAllBytes("Temp/chasqui_puente_ichu.png",tex.EncodeToPNG());}
        finally{RenderTexture.active=anterior;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(o);}
    }
}
