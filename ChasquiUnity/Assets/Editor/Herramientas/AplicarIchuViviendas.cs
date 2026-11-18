using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

static class AplicarIchuViviendas
{
    const string Dir="Assets/Niveles/SegundoRecorrido/";
    [InitializeOnLoadMethod]
    static void Programar()
    {
        EditorApplication.delayCall+=Aplicar;
        EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredEditMode)EditorApplication.delayCall+=Aplicar;};
    }
    [MenuItem("Chasqui/Aplicar paja y cortinas en ambos niveles")]
    public static void Aplicar()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        var segunda=SceneManager.GetSceneByPath(ConstruirSegundoRecorrido.Ruta);bool cerrar2=!segunda.isLoaded;
        if(cerrar2)segunda=EditorSceneManager.OpenScene(ConstruirSegundoRecorrido.Ruta,OpenSceneMode.Additive);
        var mundo=segunda.GetRootGameObjects().First(o=>o.name.StartsWith("Nivel 02"));
        var intro=segunda.GetRootGameObjects().Select(o=>o.GetComponent<EntregaInicialChaski>()).First(i=>i!=null);
        var material=AssetDatabase.LoadAssetAtPath<Material>(Dir+"PajaVolumen.mat");
        if(material==null){material=new Material(Shader.Find("Chasqui/Paja agrupada de cubierta"));material.color=new Color(.68f,.54f,.32f);AssetDatabase.CreateAsset(material,Dir+"PajaVolumen.mat");}
        foreach(string guid in AssetDatabase.FindAssets("PajaTecho t:Mesh",new[]{Dir.TrimEnd('/')}))
        {
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath(guid));
            var v=mesh.vertices;var idx=mesh.triangles;bool cambiar=false;
            for(int i=0;i<idx.Length;i+=3)
            {
                int baseHaz=idx[i]/10*10;Vector3 centro=Vector3.zero;
                for(int j=0;j<10;j++)centro+=v[baseHaz+j];centro/=10;
                var a=v[idx[i]];var b=v[idx[i+1]];var c=v[idx[i+2]];
                if(Vector3.Dot(Vector3.Cross(b-a,c-a),(a+b+c)/3-centro)<0)
                {int k=idx[i+1];idx[i+1]=idx[i+2];idx[i+2]=k;cambiar=true;}
            }
            if(cambiar){mesh.triangles=idx;mesh.RecalculateNormals();EditorUtility.SetDirty(mesh);}
        }
        if(mundo.transform.Find("Ichu volumétrico y cortina de entrada")==null)
        {
            int numero=0;
            foreach(var r in mundo.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name=="Techo de ichu").ToArray())
            {
                var escala=r.transform.lossyScale;
                var mesh=Tejado(escala,numero);AssetDatabase.CreateAsset(mesh,Dir+"PajaTecho"+(numero++)+".asset");
                r.GetComponent<MeshFilter>().sharedMesh=mesh;r.sharedMaterial=material;
                r.name="Cubierta de ichu · haces superpuestos y alero irregular";
            }
            var casa=mundo.GetComponentsInChildren<Transform>().First(t=>t.name=="Chaskiwasi de salida");
            if(intro.puerta!=null)Object.DestroyImmediate(intro.puerta.gameObject);
            intro.puerta=CrearCortina(casa,material);
            intro.crujidoPuerta=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Nivel01/Ropa/Tela_03.ogg");
            intro.cierrePuerta=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Nivel01/Ropa/Tela_04.ogg");
            new GameObject("Ichu volumétrico y cortina de entrada").transform.SetParent(mundo.transform,false);
            EditorSceneManager.MarkSceneDirty(segunda);EditorSceneManager.SaveScene(segunda);
        }
        const string ruta1="Assets/Scenes/Campana/Nivel01_PrimeraMision.unity";
        var primera=SceneManager.GetSceneByPath(ruta1);bool cerrar1=!primera.isLoaded;
        if(cerrar1)primera=EditorSceneManager.OpenScene(ruta1,OpenSceneMode.Additive);
        var pueblo=primera.GetRootGameObjects().First(o=>o.name.StartsWith("Pueblo"));
        if(pueblo.transform.Find("Casas de piedra e ichu · acabado compartido")==null)
        {
            var modelo=mundo.GetComponentsInChildren<Transform>().First(t=>t.name=="Chaskiwasi de salida");
            foreach(var vieja in pueblo.GetComponentsInChildren<Transform>().Where(t=>t.name=="Vivienda" || t.name=="Chaskiwasi").ToArray())
            {
                if(!vieja.gameObject.activeInHierarchy)continue;
                var muro=vieja.GetComponentsInChildren<Renderer>().First(r=>r.name=="Muros");
                var centro=muro.bounds.center-Vector3.up*(muro.bounds.size.y*.5f);
                var nueva=Object.Instantiate(modelo.gameObject);nueva.name=vieja.name+" · piedra y paja";
                SceneManager.MoveGameObjectToScene(nueva,primera);nueva.transform.SetParent(pueblo.transform,true);
                nueva.transform.SetPositionAndRotation(centro,Quaternion.Euler(0,90,0));nueva.transform.localScale=Vector3.one*.85f;
                foreach(var t in nueva.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Entrada de la casa")||t.name.StartsWith("Interior de la casa")).ToArray())Object.DestroyImmediate(t.gameObject);
                vieja.gameObject.SetActive(false);
            }
            new GameObject("Casas de piedra e ichu · acabado compartido").transform.SetParent(pueblo.transform,false);
            EditorSceneManager.MarkSceneDirty(primera);EditorSceneManager.SaveScene(primera);
        }
        AssetDatabase.SaveAssets();
        if(cerrar1)EditorSceneManager.CloseScene(primera,true);
        if(cerrar2)EditorSceneManager.CloseScene(segunda,true);
        Debug.Log("ICHU_VIVIENDAS_LISTO: cubiertas con haces, cortina flexible sin bisagra y sonidos de fibras; casas compartidas con nivel 1.");
    }
    static Transform CrearCortina(Transform casa,Material mat)
    {
        var o=new GameObject("Entrada · estera de fibras atada al dintel");o.transform.SetParent(casa,false);o.transform.localPosition=new Vector3(0,.3f,-3.32f);
        var v=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();const int nx=16,ny=20;
        for(int y=0;y<=ny;y++)for(int x=0;x<=nx;x++)
        {
            float u=x/(float)nx,f=y/(float)ny;
            v.Add(new Vector3((u-.5f)*1.64f,f*2.12f+Mathf.Sin(x*1.8f)*.025f*(1-f),Mathf.Sin(u*Mathf.PI*8)*.024f));uv.Add(new Vector2(u*3,f*4));
            if(x<nx && y<ny){int a=y*(nx+1)+x,b=a+nx+1;t.AddRange(new[]{a,b,a+1,a+1,b,b+1});}
        }
        var m=new Mesh{name="Estera de fibra vegetal"};m.SetVertices(v);m.SetUVs(0,uv);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();AssetDatabase.CreateAsset(m,Dir+"CortinaEntrada.asset");
        o.AddComponent<MeshFilter>().sharedMesh=m;o.AddComponent<MeshRenderer>().sharedMaterial=mat;
        var c=o.AddComponent<BoxCollider>();c.center=Vector3.up*1.06f;c.size=new Vector3(1.64f,2.12f,.08f);o.AddComponent<CortinaIchuChaski>();
        for(int i=0;i<4;i++)
        {
            var cuerda=new GameObject("Lazada de fibra al dintel");cuerda.transform.SetParent(o.transform,false);
            var l=cuerda.AddComponent<LineRenderer>();l.useWorldSpace=false;l.sharedMaterial=mat;l.widthMultiplier=.025f;l.positionCount=5;float x=-.6f+i*.4f;
            l.SetPositions(new[]{new Vector3(x,2.03f,0),new Vector3(x,2.23f,-.08f),new Vector3(x,2.30f,.05f),new Vector3(x,2.15f,.09f),new Vector3(x,2.03f,0)});
        }
        return o.transform;
    }
    static Mesh Tejado(Vector3 escala,int semilla)
    {
        var v=new List<Vector3>();var t=new List<int>();
        int filas=Mathf.CeilToInt(escala.x/.5f),haces=Mathf.CeilToInt(escala.z/.17f);
        for(int fila=0;fila<filas;fila++)for(int haz=0;haz<haces;haz++)
        {
            float largo=escala.x/filas,ancho=escala.z/haces;
            float ruido=Mathf.PerlinNoise(fila*3.1f+semilla,haz*1.7f);
            float a=-escala.x*.5f+fila*largo-.09f,b=a+largo+.18f+(ruido-.5f)*.13f;
            float z=-escala.z*.5f+haz*ancho;
            float y=escala.y*.5f+.065f+(filas-fila)*.018f;
            int k=v.Count;
            // Broad overlapping straw bundles give thickness and rough eaves, without hair-like strands.
            var p=new[]{new Vector3(a,y,z),new Vector3(a,y+.065f,z+ancho*.5f),new Vector3(a,y,z+ancho),new Vector3(b,y-.025f,z),new Vector3(b,y+.025f,z+ancho*.5f),new Vector3(b,y-.025f,z+ancho),new Vector3(a,y-.045f,z),new Vector3(a,y-.045f,z+ancho),new Vector3(b,y-.075f,z),new Vector3(b,y-.075f,z+ancho)};
            foreach(var q in p)v.Add(new Vector3(q.x/escala.x,q.y/escala.y,q.z/escala.z));
            int[] idx={0,3,1,1,3,4,1,4,2,2,4,5,3,8,4,4,8,9,4,9,5,0,1,6,1,7,6,1,2,7,0,6,3,3,6,8,2,5,7,5,9,7};
            foreach(int i in idx)t.Add(k+i);
        }
        var m=new Mesh{name="Haces de ichu superpuestos"};m.SetVertices(v);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();return m;
    }
}
