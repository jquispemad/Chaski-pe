using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

static class DetallarPrimeraMision
{
    const string Ruta = "Assets/Scenes/Campana/Nivel01_PrimeraMision.unity";
    const string Valle = "Assets/Niveles/Valle/";
    const string Recursos = "Assets/Niveles/PrimeraMision/";

    [InitializeOnLoadMethod]
    static void Programar() => EditorApplication.delayCall += Aplicar;

    static void Aplicar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var anterior = SceneManager.GetActiveScene();
        var escena = SceneManager.GetSceneByPath(Ruta);
        bool cerrar = !escena.isLoaded;
        if (cerrar) escena = EditorSceneManager.OpenScene(Ruta, OpenSceneMode.Additive);
        var mision = escena.GetRootGameObjects().Select(o => o.GetComponent<PrimeraMisionChaski>()).FirstOrDefault(m => m != null);
        if (mision != null && mision.circuloEntrega == null)
        {
            SceneManager.SetActiveScene(escena);
            var cartel = new GameObject("Letreros de madera").transform;
            Letrero(cartel,12,"QHAPAQ ÑAN","LA PRIMERA MISIÓN");
            Letrero(cartel,30,"PASO DE PIEDRA","ESPACIO PARA SALTAR");
            Letrero(cartel,108,"PASO BAJO","MANTÉN C");
            Letrero(cartel,240,"CHASKIWASI","POSTA DE RELEVO");
            CrearMeta(mision);
            CrearRocas();
            var pueblo = escena.GetRootGameObjects().First(o=>o.name.StartsWith("Pueblo"));
            CrearMaiz(pueblo.transform);
            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            AssetDatabase.SaveAssets();
            Debug.Log("DETALLE_PRIMERA_MISION: carteles, rocas, maíz y círculo guardados.");
        }
        if (cerrar)
        {
            if (anterior.IsValid()) SceneManager.SetActiveScene(anterior);
            EditorSceneManager.CloseScene(escena,true);
        }
    }

    static void Letrero(Transform grupo,float z,string titulo,string linea)
    {
        var madera = AssetDatabase.LoadAssetAtPath<Material>(Valle+"MaderaLetrerosTallados.mat");
        var tinta = AssetDatabase.LoadAssetAtPath<Material>(Valle+"LetraCarteles.mat");
        var raiz = new GameObject(titulo).transform;
        raiz.SetParent(grupo);
        raiz.SetPositionAndRotation(new Vector3(2.1f,0,z),Quaternion.Euler(0,90,0));
        Tabla(raiz,madera,new Vector3(0,.75f,0),new Vector3(.15f,1.5f,.14f));
        Tabla(raiz,madera,new Vector3(0,1.64f,0),new Vector3(2.65f,.78f,.14f));
        var o = new GameObject("Texto tallado");
        o.transform.SetParent(raiz,false);
        o.transform.localPosition = new Vector3(0,1.64f,-.075f);
        var texto = o.AddComponent<TextMesh>();
        texto.text = titulo+"\n"+linea;
        var fuente = Resources.Load<Font>("EstiloInca/TituloTradicional");
        if (fuente != null) texto.font = fuente;
        texto.fontSize=96; texto.fontStyle=FontStyle.Bold;
        texto.characterSize=.032f; texto.anchor=TextAnchor.MiddleCenter;
        texto.alignment=TextAlignment.Center;
        o.GetComponent<MeshRenderer>().sharedMaterial=tinta;
        o.AddComponent<TextoCartelChasqui>();
        var medida = o.GetComponent<MeshRenderer>().bounds.size;
        float ancho = Mathf.Max(medida.x,medida.z);
        if (ancho > .001f && medida.y > .001f)
            o.transform.localScale *= Mathf.Min(2.36f/ancho,.61f/medida.y);
    }

    static void Tabla(Transform padre,Material material,Vector3 p,Vector3 escala)
    {
        var o=GameObject.CreatePrimitive(PrimitiveType.Cube);
        o.name="Madera tallada"; o.transform.SetParent(padre,false);
        o.transform.localPosition=p; o.transform.localScale=escala;
        o.GetComponent<Renderer>().sharedMaterial=material;
        Object.DestroyImmediate(o.GetComponent<Collider>());
    }

    static void CrearMeta(PrimeraMisionChaski mision)
    {
        var o = new GameObject("Círculo amarillo · entrega del quipu");
        o.transform.position = new Vector3(0,.035f,248f);
        mision.circuloEntrega=o.transform;
        mision.llegada=243;
        var vertices=new List<Vector3>(); var indices=new List<int>();
        const int segmentos=64;
        for(int i=0;i<segmentos;i++)
        {
            float angulo=i*2*Mathf.PI/segmentos;
            var dir=new Vector3(Mathf.Cos(angulo),0,Mathf.Sin(angulo));
            vertices.Add(dir*1.25f); vertices.Add(dir*1.08f);
            int a=i*2,b=(a+2)%(segmentos*2);
            indices.AddRange(new[]{a,a+1,b,b,a+1,b+1});
        }
        var aro=new Mesh{name="Aro dorado"}; aro.SetVertices(vertices);
        aro.SetTriangles(indices,0); aro.RecalculateNormals(); aro.RecalculateBounds();
        AssetDatabase.CreateAsset(aro,Recursos+"AroEntrega.asset");
        var oro=new Material(Shader.Find("Unlit/Color")){color=new Color(1,.76f,.14f)};
        AssetDatabase.CreateAsset(oro,Recursos+"OroEntrega.mat");
        o.AddComponent<MeshFilter>().sharedMesh=aro;
        o.AddComponent<MeshRenderer>().sharedMaterial=oro;
    }

    static void CrearRocas()
    {
        var terreno = Terrain.activeTerrains.FirstOrDefault(t=>t.gameObject.scene == SceneManager.GetActiveScene());
        var grupo = new GameObject("Rocas y piedras del sendero").transform;
        var piedra=AssetDatabase.LoadAssetAtPath<Material>("Assets/Personajes/Chasqui/MaterialPiedra.mat");
        for(int i=0;i<52;i++)
        {
            float z=7+4.6f*i+Mathf.Sin(i*3.4f);
            float x=(i%3==0 ? -1f:1f)*(2.65f+i%5*.65f);
            float escala=.17f+(i%6)*.14f;
            var m=AssetDatabase.LoadAssetAtPath<Mesh>(Valle+"Roca"+(i%4)+".asset");
            var o=new GameObject("Roca erosionada");
            o.transform.SetParent(grupo);
            float y=terreno==null?0:terreno.SampleHeight(new Vector3(x,0,z))+terreno.transform.position.y;
            o.transform.position=new Vector3(x,y-m.bounds.min.y*escala-.10f,z);
            o.transform.localScale=Vector3.one*escala;
            o.transform.rotation=Quaternion.Euler(0,i*67,0);
            o.AddComponent<MeshFilter>().sharedMesh=m;
            o.AddComponent<MeshRenderer>().sharedMaterial=piedra;
        }
    }

    static void CrearMaiz(Transform pueblo)
    {
        var mat=new Material(Shader.Find("Standard")) {color=new Color(.27f,.48f,.14f)};
        mat.SetFloat("_Glossiness",.16f);
        AssetDatabase.CreateAsset(mat,Recursos+"HojaMaiz.mat");
        var v=new List<Vector3>(); var t=new List<int>();
        for(int hoja=0;hoja<7;hoja++)
        {
            float ang=hoja*2.4f;
            var dir=new Vector3(Mathf.Cos(ang),0,Mathf.Sin(ang));
            var lado=Vector3.Cross(Vector3.up,dir);
            int baseHoja=v.Count;
            for(int j=0;j<=8;j++)
            {
                float u=j/8f;
                var centro=Vector3.up*(.14f+hoja*.084f+Mathf.Sin(u*Mathf.PI*.87f)*.29f)
                    +dir*(u*(.43f-hoja*.025f));
                float ancho=Mathf.Sin(Mathf.PI*u)*.068f+.001f;
                v.Add(centro-lado*ancho);
                v.Add(centro+Vector3.up*(ancho*.35f));
                v.Add(centro+lado*ancho);
                if (j==0) continue;
                for(int k=0;k<2;k++)
                {
                    int a=baseHoja+(j-1)*3+k,b=a+3;
                    t.AddRange(new[]{a,b,a+1,a+1,b,b+1});
                }
            }
        }
        int frente=v.Count, caras=t.Count;
        v.AddRange(v.ToArray());
        for(int i=0;i<caras;i+=3)
            t.AddRange(new[]{t[i]+frente,t[i+2]+frente,t[i+1]+frente});
        var hojas=new Mesh{name="Hojas curvas de maíz"};
        hojas.SetVertices(v); hojas.SetTriangles(t,0); hojas.RecalculateNormals(); hojas.RecalculateBounds();
        AssetDatabase.CreateAsset(hojas,Recursos+"HojasMaiz.asset");
        foreach(var planta in pueblo.GetComponentsInChildren<Transform>().Where(o=>o.name=="Maíz").ToArray())
        {
            float s=.82f+Mathf.PerlinNoise(planta.position.x,planta.position.z)*.38f;
            var p=planta.position-Vector3.up*.28f;
            planta.name="Maíz · tallo y hojas";
            planta.SetPositionAndRotation(p,Quaternion.Euler(0,p.z*73f,0));
            planta.localScale=Vector3.one*s;
            var hojasObj=new GameObject("Hojas");
            hojasObj.transform.SetParent(planta,false);
            hojasObj.AddComponent<MeshFilter>().sharedMesh=hojas;
            hojasObj.AddComponent<MeshRenderer>().sharedMaterial=mat;
            var tallo=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tallo.name="Tallo"; tallo.transform.SetParent(planta,false);
            tallo.transform.localPosition=Vector3.up*.39f;
            tallo.transform.localScale=new Vector3(.027f,.39f,.027f);
            tallo.GetComponent<MeshRenderer>().sharedMaterial=mat;
            Object.DestroyImmediate(tallo.GetComponent<Collider>());
            planta.GetComponent<MeshRenderer>().enabled=false;
        }
    }
}
