using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class PrepararSaltosYRopa
{
    const string Folder="Assets/Personajes/Chasqui";
    struct V { public Vector3 p,n,pose;public Vector2 uv;public BoneWeight peso;public bool capa; }
    static Transform Hueso(Transform raiz,string nombre)
    { return raiz.GetComponentsInChildren<Transform>(true).First(t=>t.name==nombre); }
    static BoneWeight Mezclar(BoneWeight a,BoneWeight b)
    {
        var d=new Dictionary<int,float>();
        Action<int,float> poner=(i,w)=>{if(w>0)d[i]=(d.ContainsKey(i)?d[i]:0)+w*.5f;};
        poner(a.boneIndex0,a.weight0);poner(a.boneIndex1,a.weight1);poner(a.boneIndex2,a.weight2);poner(a.boneIndex3,a.weight3);
        poner(b.boneIndex0,b.weight0);poner(b.boneIndex1,b.weight1);poner(b.boneIndex2,b.weight2);poner(b.boneIndex3,b.weight3);
        var k=d.OrderByDescending(x=>x.Value).Take(4).ToArray();float total=k.Sum(x=>x.Value);var wv=new BoneWeight();
        for(int j=0;j<k.Length;j++)
        {
            float w=k[j].Value/total;
            if(j==0){wv.boneIndex0=k[j].Key;wv.weight0=w;}if(j==1){wv.boneIndex1=k[j].Key;wv.weight1=w;}
            if(j==2){wv.boneIndex2=k[j].Key;wv.weight2=w;}if(j==3){wv.boneIndex3=k[j].Key;wv.weight3=w;}
        }
        return wv;
    }
    static T Guardar<T>(T objeto,string path) where T:UnityEngine.Object
    {
        var previo=AssetDatabase.LoadAssetAtPath<T>(path);
        if(previo==null){AssetDatabase.CreateAsset(objeto,path);return objeto;}
        EditorUtility.CopySerialized(objeto,previo);UnityEngine.Object.DestroyImmediate(objeto);EditorUtility.SetDirty(previo);return previo;
    }
    static Material MaterialEscena(string nombre,Color color)
    {
        string path=Folder+"/Materiales/"+nombre+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(m==null){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path);}
        m.color=color;m.SetFloat("_Glossiness",.18f);EditorUtility.SetDirty(m);return m;
    }
    static GameObject Bloque(Transform padre,string nombre,Vector3 centro,Vector3 escala,Material material)
    {
        var t=padre.Find(nombre);var o=t!=null?t.gameObject:GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=nombre;
        o.transform.SetParent(padre,false);o.transform.localPosition=centro;o.transform.localRotation=Quaternion.identity;o.transform.localScale=escala;
        o.GetComponent<Renderer>().sharedMaterial=material;var c=o.GetComponent<BoxCollider>();if(c==null)c=o.AddComponent<BoxCollider>();c.isTrigger=false;c.enabled=true;
        return o;
    }
    [MenuItem("Chasqui/Preparar plataforma, cubos y ropa dinamica")]
    public static void Aplicar()
    {
        if(EditorApplication.isPlaying)throw new Exception("Sal de Play antes de preparar la escena.");
        var jugador=UnityEngine.Object.FindFirstObjectByType<ChasquiMovimiento>();if(jugador==null)throw new Exception("No se encuentra el jugador.");
        var zona=GameObject.Find("Zona de saltos")??new GameObject("Zona de saltos");
        var plataforma=Bloque(zona.transform,"Plataforma de practica",new Vector3(0,.45f,6.8f),new Vector3(4.8f,.9f,3.6f),MaterialEscena("Plataforma",new Color(.13f,.38f,.43f)));
        Bloque(zona.transform,"Cubo bajo",new Vector3(-.9f,.2f,2f),new Vector3(1.2f,.4f,1.2f),MaterialEscena("CuboBajo",new Color(.77f,.50f,.17f)));
        Bloque(zona.transform,"Cubo medio",new Vector3(1.5f,.4f,3.8f),new Vector3(1.25f,.8f,1.25f),MaterialEscena("CuboMedio",new Color(.22f,.43f,.65f)));
        Bloque(zona.transform,"Cubo alto",new Vector3(-3.3f,.6f,5.6f),new Vector3(1.4f,1.2f,1.4f),MaterialEscena("CuboAlto",new Color(.59f,.27f,.20f)));
        Bloque(zona.transform,"Cubo sobre plataforma",new Vector3(1.2f,1.2f,7.2f),new Vector3(1.25f,.6f,1.25f),MaterialEscena("CuboPlataforma",new Color(.30f,.51f,.27f)));
        var render=jugador.GetComponentsInChildren<SkinnedMeshRenderer>(true).First(r=>r.name=="Chasqui_Malla_Animada");
        var original=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"/ChasquiAnimado.fbx").GetComponentInChildren<SkinnedMeshRenderer>(true).sharedMesh;
        render.sharedMesh=original;
        var baked=new Mesh();render.BakeMesh(baked);var poses=baked.vertices;var normalPose=baked.normals;
        var source=original.vertices;var normals=original.normals;var uv=original.uv;var pesos=original.boneWeights;
        var frame=jugador.visual;
        var alturas=new float[source.Length];float suelo=float.MaxValue;
        for(int i=0;i<source.Length;i++){alturas[i]=frame.InverseTransformPoint(render.transform.TransformPoint(poses[i])).y;suelo=Mathf.Min(suelo,alturas[i]);}
        var indicesCapa=new HashSet<int>();for(int i=0;i<render.bones.Length;i++)if(render.bones[i].name.StartsWith("cape_"))indicesCapa.Add(i);
        var flags=new bool[source.Length];
        for(int i=0;i<source.Length;i++)
        {
            var w=pesos[i];float c=(indicesCapa.Contains(w.boneIndex0)?w.weight0:0)+(indicesCapa.Contains(w.boneIndex1)?w.weight1:0)+(indicesCapa.Contains(w.boneIndex2)?w.weight2:0)+(indicesCapa.Contains(w.boneIndex3)?w.weight3:0);flags[i]=c>.5f;
        }
        var ropaVertices=new List<V>();var mapa=new Dictionary<int,int>();var cuerpoTri=new List<int>[original.subMeshCount];var ropaTri=new List<int>[original.subMeshCount];
        Func<int,int> copiar=i=>
        {
            int indice;if(mapa.TryGetValue(i,out indice))return indice;
            indice=ropaVertices.Count;mapa[i]=indice;ropaVertices.Add(new V{p=source[i],n=normals[i],pose=poses[i],uv=uv.Length==source.Length?uv[i]:Vector2.zero,peso=pesos[i],capa=flags[i]});return indice;
        };
        for(int material=0;material<original.subMeshCount;material++)
        {
            cuerpoTri[material]=new List<int>();ropaTri[material]=new List<int>();string nombre=render.sharedMaterials[material].name;
            bool tejido=nombre.StartsWith("Uncu")||nombre.StartsWith("Lliclla")||nombre.StartsWith("Índigo")||nombre.StartsWith("Hilo ocre");
            var tris=original.GetTriangles(material);
            for(int k=0;k<tris.Length;k+=3)
            {
                int a=tris[k],b=tris[k+1],c=tris[k+2];float altura=(alturas[a]+alturas[b]+alturas[c])/3-suelo;
                bool capa=flags[a]||flags[b]||flags[c];bool ropa=capa||(tejido&&altura<1.55f);
                if(!ropa){cuerpoTri[material].AddRange(new[]{a,b,c});continue;}
                Vector3 normal=render.transform.TransformDirection((normalPose[a]+normalPose[b]+normalPose[c]).normalized);
                if(capa&&Vector3.Dot(normal,-frame.forward)<.08f)continue;
                if(nombre.StartsWith("Uncu")&&!capa)
                {
                    Vector3 centro=render.transform.TransformPoint((poses[a]+poses[b]+poses[c])/3);
                    Vector3 radial=Vector3.ProjectOnPlane(centro-frame.position,frame.up).normalized;
                    if(Vector3.Dot(normal,radial)<.08f)continue;
                }
                ropaTri[material].AddRange(new[]{copiar(a),copiar(b),copiar(c)});
            }
        }
        // Densificar los paños grandes para que los muslos no atraviesen entre vértices.
        for(int paso=0;paso<5;paso++)
        {
            var puntosMedios=new Dictionary<long,int>();bool cambio=false;
            Func<int,int,int> medio=(a,b)=>
            {
                long key=((long)Mathf.Min(a,b)<<32)|(uint)Mathf.Max(a,b);int i;if(puntosMedios.TryGetValue(key,out i))return i;
                V x=ropaVertices[a],y=ropaVertices[b];i=ropaVertices.Count;
                ropaVertices.Add(new V{p=(x.p+y.p)*.5f,n=(x.n+y.n).normalized,pose=(x.pose+y.pose)*.5f,uv=(x.uv+y.uv)*.5f,peso=Mezclar(x.peso,y.peso),capa=x.capa&&y.capa});puntosMedios[key]=i;return i;
            };
            for(int mat=0;mat<ropaTri.Length;mat++)
            {
                string n=render.sharedMaterials[mat].name;if(!n.StartsWith("Uncu")&&!n.StartsWith("Lliclla"))continue;
                var next=new List<int>();var tris=ropaTri[mat];
                for(int k=0;k<tris.Count;k+=3)
                {
                    int a=tris[k],b=tris[k+1],c=tris[k+2];
                    float ab=(ropaVertices[a].pose-ropaVertices[b].pose).sqrMagnitude,bc=(ropaVertices[b].pose-ropaVertices[c].pose).sqrMagnitude,ca=(ropaVertices[c].pose-ropaVertices[a].pose).sqrMagnitude;
                    if(Mathf.Max(ab,Mathf.Max(bc,ca))<.065f*.065f){next.AddRange(new[]{a,b,c});continue;}
                    if(ab>=bc&&ab>=ca){int m=medio(a,b);next.AddRange(new[]{a,m,c,m,b,c});}
                    else if(bc>=ca){int m=medio(b,c);next.AddRange(new[]{b,m,a,m,c,a});}
                    else {int m=medio(c,a);next.AddRange(new[]{c,m,b,m,a,b});}cambio=true;
                }
                ropaTri[mat]=next;
            }
            if(!cambio)break;
        }
        var cuerpo=UnityEngine.Object.Instantiate(original);cuerpo.name="Cuerpo sin superficies de ropa";
        for(int i=0;i<cuerpoTri.Length;i++)cuerpo.SetTriangles(cuerpoTri[i],i);
        cuerpo=Guardar(cuerpo,Folder+"/CuerpoSinRopa.asset");render.sharedMesh=cuerpo;
        var ropaBase=new Mesh{name="Ropa con pesos del esqueleto",indexFormat=IndexFormat.UInt32};
        ropaBase.SetVertices(ropaVertices.Select(v=>v.p).ToList());ropaBase.SetNormals(ropaVertices.Select(v=>v.n).ToList());ropaBase.SetUVs(0,ropaVertices.Select(v=>v.uv).ToList());
        ropaBase.boneWeights=ropaVertices.Select(v=>v.peso).ToArray();ropaBase.bindposes=original.bindposes;ropaBase.subMeshCount=ropaTri.Length;
        for(int i=0;i<ropaTri.Length;i++)ropaBase.SetTriangles(ropaTri[i],i);
        ropaBase.RecalculateBounds();ropaBase=Guardar(ropaBase,Folder+"/RopaBase.asset");
        var vista=UnityEngine.Object.Instantiate(ropaBase);vista.name="Ropa en la pose inicial";vista.SetVertices(ropaVertices.Select(v=>v.pose).ToList());vista.RecalculateNormals();vista.RecalculateBounds();vista=Guardar(vista,Folder+"/RopaVista.asset");
        var inputT=render.transform.Find("Ropa - piel de referencia");var input=inputT!=null?inputT.gameObject:new GameObject("Ropa - piel de referencia");input.transform.SetParent(render.transform,false);
        var skin=input.GetComponent<SkinnedMeshRenderer>();if(skin==null)skin=input.AddComponent<SkinnedMeshRenderer>();skin.sharedMesh=ropaBase;skin.bones=render.bones;skin.rootBone=render.rootBone;skin.updateWhenOffscreen=true;skin.enabled=false;
        var outputT=render.transform.Find("Ropa - tela dinamica");var output=outputT!=null?outputT.gameObject:new GameObject("Ropa - tela dinamica");output.transform.SetParent(render.transform,false);
        var filtro=output.GetComponent<MeshFilter>();if(filtro==null)filtro=output.AddComponent<MeshFilter>();filtro.sharedMesh=vista;
        var clothRender=output.GetComponent<MeshRenderer>();if(clothRender==null)clothRender=output.AddComponent<MeshRenderer>();
        var shader=Shader.Find("Chasqui/Tejido dos caras");if(shader==null)throw new Exception("No se ha importado el shader de tela.");
        var mats=new Material[render.sharedMaterials.Length];
        for(int i=0;i<mats.Length;i++)
        {
            string path=Folder+"/Materiales/Tela_"+i.ToString("D2")+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){m=new Material(render.sharedMaterials[i]);m.shader=shader;AssetDatabase.CreateAsset(m,path);}
            m.shader=shader;m.color=render.sharedMaterials[i].color;m.SetFloat("_Metallic",render.sharedMaterials[i].GetFloat("_Metallic"));m.SetFloat("_Glossiness",render.sharedMaterials[i].GetFloat("_Glossiness"));mats[i]=m;EditorUtility.SetDirty(m);
        }
        skin.sharedMaterials=mats;clothRender.sharedMaterials=mats;
        var simulacion=jugador.GetComponent<RopaDinamicaChasqui>();if(simulacion==null)simulacion=jugador.gameObject.AddComponent<RopaDinamicaChasqui>();
        simulacion.piel=skin;simulacion.salida=filtro;simulacion.marco=frame;simulacion.esCapa=ropaVertices.Select(v=>v.capa).ToArray();
        float maxCapa=ropaVertices.Where(v=>v.capa).Max(v=>frame.InverseTransformPoint(render.transform.TransformPoint(v.pose)).y);
        float minCapa=ropaVertices.Where(v=>v.capa).Min(v=>frame.InverseTransformPoint(render.transform.TransformPoint(v.pose)).y);
        simulacion.libertad=ropaVertices.Select(v=>
        {
            float y=frame.InverseTransformPoint(render.transform.TransformPoint(v.pose)).y;
            float t=v.capa?Mathf.InverseLerp(maxCapa,minCapa,y):Mathf.Clamp01((1.12f-(y-suelo))/.39f);
            return v.capa?Mathf.Clamp01((t-.08f)/.92f):t;
        }).ToArray();
        var volumenes=new List<RopaDinamicaChasqui.Volumen>();
        Action<string,string,float,float,Vector3,Vector3> volumen=(a,b,ra,rb,oa,ob)=>volumenes.Add(new RopaDinamicaChasqui.Volumen{inicio=Hueso(jugador.transform,a),fin=Hueso(jugador.transform,b),radioInicio=ra,radioFin=rb,desplazamientoInicio=oa,desplazamientoFin=ob});
        volumen("pelvis","spine",.155f,.125f,Vector3.zero,Vector3.zero);
        foreach(float x in new[]{-.035f,.035f})
        {
            volumen("spine","chest",.125f,.17f,new Vector3(x,0,0),new Vector3(x,0,0));
            volumen("chest","neck",.17f,.065f,new Vector3(x,0,0),new Vector3(x*.5f,0,0));
        }
        foreach(string side in new[]{"L","R"})
        {
            volumen("thigh."+side,"shin."+side,.105f,.088f,Vector3.zero,Vector3.zero);
            volumen("shin."+side,"foot."+side,.078f,.043f,Vector3.zero,Vector3.zero);
            volumen("upper_arm."+side,"forearm."+side,.078f,.054f,Vector3.zero,Vector3.zero);
            volumen("forearm."+side,"hand."+side,.056f,.037f,Vector3.zero,Vector3.zero);
        }
        simulacion.cuerpo=volumenes.ToArray();simulacion.entorno=zona.GetComponentsInChildren<BoxCollider>();simulacion.separacion=.014f;
        EditorUtility.SetDirty(simulacion);EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();
        PrefabUtility.SaveAsPrefabAsset(jugador.gameObject,Folder+"/ChasquiJugador.prefab");AssetDatabase.SaveAssets();UnityEngine.Object.DestroyImmediate(baked);
        string info="Ropa="+ropaVertices.Count+" vertices; volumenes="+volumenes.Count+"; obstaculos="+simulacion.entorno.Length;
        File.WriteAllText(Path.Combine(Path.GetTempPath(),"chasqui_ropa_preparada.txt"),info);Debug.Log("SALTOS_Y_ROPA_LISTOS "+info);
    }
}

