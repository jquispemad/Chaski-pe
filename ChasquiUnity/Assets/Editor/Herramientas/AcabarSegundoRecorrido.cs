using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

static class AcabarSegundoRecorrido
{
    const string Marca="Acabado nivel 2 · entrega, camino y puente del Valle";
    const string Carpeta="Assets/Niveles/SegundoRecorrido/";
    [InitializeOnLoadMethod]
    static void Programar()
    {
        EditorApplication.delayCall+=Aplicar;
        EditorApplication.playModeStateChanged+=estado=>{if(estado==PlayModeStateChange.EnteredEditMode)EditorApplication.delayCall+=Aplicar;};
    }
    [MenuItem("Chasqui/Aplicar mejoras del segundo recorrido")]
    public static void Aplicar()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode || AssetDatabase.LoadAssetAtPath<SceneAsset>(ConstruirSegundoRecorrido.Ruta)==null)return;
        var escena=SceneManager.GetSceneByPath(ConstruirSegundoRecorrido.Ruta);bool cerrar=!escena.isLoaded;
        if(cerrar)escena=EditorSceneManager.OpenScene(ConstruirSegundoRecorrido.Ruta,OpenSceneMode.Additive);
        var mundo=escena.GetRootGameObjects().First(o=>o.name.StartsWith("Nivel 02"));
        if(mundo.transform.Find(Marca)!=null){if(cerrar)EditorSceneManager.CloseScene(escena,true);return;}
        var suelo=mundo.GetComponentInChildren<Terrain>();
        foreach(var r in mundo.GetComponentsInChildren<MeshRenderer>())
        {
            if(r.name=="Queñua · hojas") {r.shadowCastingMode=ShadowCastingMode.TwoSided;r.receiveShadows=true;}
            if(r.name!="Roca erosionada" || r.GetComponent<BoxCollider>()==null)continue;
            // Use the actual transformed mesh, rather than its bounding box or an analytic hill approximation.
            var mesh=r.GetComponent<MeshFilter>().sharedMesh;
            float separacion=float.PositiveInfinity;
            foreach(var v in mesh.vertices)
            {
                var p=r.transform.TransformPoint(v);
                separacion=Mathf.Min(separacion,p.y-Altura(suelo,p.x,p.z));
            }
            r.transform.position-=Vector3.up*(separacion+Mathf.Clamp(r.bounds.size.y*.12f,.10f,.25f));
        }
        CrearCamino(suelo,mundo.transform);
        CrearLimites(mundo.transform);
        SustituirPuente(escena,mundo.transform);
        CrearEntrega(escena,suelo);
        new GameObject(Marca).transform.SetParent(mundo.transform,false);
        EditorSceneManager.MarkSceneDirty(escena);EditorSceneManager.SaveScene(escena);AssetDatabase.SaveAssets();
        Debug.Log("ACABADO_NIVEL02: rocas asentadas por vértices, sombra de hojas, camino continuo, límites a 30 m, puente del Valle y entrega inicial guardados.");
        if(cerrar)EditorSceneManager.CloseScene(escena,true);
    }
    static float Altura(Terrain t,float x,float z)
    {
        var p=t.transform.position;var d=t.terrainData;
        return d.GetInterpolatedHeight(Mathf.Clamp01((x-p.x)/d.size.x),Mathf.Clamp01((z-p.z)/d.size.z))+p.y;
    }
    static void CrearCamino(Terrain suelo,Transform padre)
    {
        var vertices=new List<Vector3>();var uv=new List<Vector2>();var indices=new List<int>();
        const int filas=953, columnas=9;
        for(int i=0;i<filas;i++)
        {
            float z=-6+i*.5f,ancho=2.5f+Mathf.PerlinNoise(z*.12f,8)*.3f;
            for(int k=0;k<columnas;k++)
            {
                float u=k/(columnas-1f),x=ConstruirSegundoRecorrido.Centro(z)+(u*2-1)*ancho;
                vertices.Add(new Vector3(x,Altura(suelo,x,z)+.045f,z));uv.Add(new Vector2(u*1.4f,z/3));
                if(i==0||k==columnas-1||z>267&&z<=293)continue;
                int a=(i-1)*columnas+k,b=i*columnas+k;
                indices.AddRange(new[]{a,b,a+1,a+1,b,b+1});
            }
        }
        var mesh=new Mesh{name="Sendero de tierra · nivel 2"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,Carpeta+"CaminoTierra.asset");
        var o=new GameObject("Sendero · tierra compactada visible");o.transform.SetParent(padre,false);
        o.AddComponent<MeshFilter>().sharedMesh=mesh;
        var mat=new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/Niveles/Valle/CaminoTierra.mat"));
        mat.name="Sendero del segundo recorrido";mat.color=new Color(.63f,.49f,.31f);mat.SetFloat("_Glossiness",.02f);
        AssetDatabase.CreateAsset(mat,Carpeta+"CaminoTierra.mat");
        o.AddComponent<MeshRenderer>().sharedMaterial=mat;o.GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.Off;
        // The terrain remains the collision surface; the strip only supplies its visible earth finish.
    }
    static void CrearLimites(Transform padre)
    {
        foreach(var c in padre.GetComponentsInChildren<BoxCollider>().Where(c=>c.name=="Límite del recorrido" && Mathf.Abs(c.transform.position.x)>80).ToArray())Object.DestroyImmediate(c.gameObject);
        var grupo=new GameObject("Barreras invisibles · 30 m desde los costados del sendero").transform;grupo.SetParent(padre,false);
        foreach(int lado in new[]{-1,1})for(int z=-10;z<476;z+=2)
        {
            // 2.8 m is the outer edge of the earth strip; another 30 m stays available to explore.
            Vector3 a=new Vector3(ConstruirSegundoRecorrido.Centro(z)+lado*32.8f,18,z),b=new Vector3(ConstruirSegundoRecorrido.Centro(z+2)+lado*32.8f,18,z+2);
            var o=new GameObject("Barrera lateral "+lado+" · "+z);o.transform.SetParent(grupo,false);
            o.transform.SetPositionAndRotation((a+b)*.5f+Vector3.right*lado*.3f,Quaternion.LookRotation(b-a));
            o.AddComponent<BoxCollider>().size=new Vector3(.6f,90,Vector3.Distance(a,b)+.2f);o.isStatic=true;
        }
    }
    static void SustituirPuente(Scene escena,Transform padre)
    {
        string[] nombres={"Puente · piso de ichu trenzado","Baranda firme del puente","Poste de pasarela","Soga de ichu · pasamanos"};
        foreach(var t in padre.GetComponentsInChildren<Transform>().Where(t=>nombres.Contains(t.name)).ToArray())Object.DestroyImmediate(t.gameObject);
        const string ruta="Assets/Scenes/Borradores/Nivel01_Valle.unity";
        var valle=SceneManager.GetSceneByPath(ruta);bool cerrar=!valle.isLoaded;
        if(cerrar)valle=EditorSceneManager.OpenScene(ruta,OpenSceneMode.Additive);
        var original=valle.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Transform>(true)).First(t=>t.name=="Puente inca · fibras trenzadas");
        var puente=Object.Instantiate(original.gameObject);puente.name="Puente Q’eswachaka · ichu trenzado del Valle";
        SceneManager.MoveGameObjectToScene(puente,escena);puente.transform.SetParent(padre,true);
        puente.transform.SetPositionAndRotation(new Vector3(ConstruirSegundoRecorrido.Centro(280),.95f,267),Quaternion.identity);
        puente.transform.localScale=Vector3.one;
        foreach(var r in puente.GetComponentsInChildren<Renderer>())r.shadowCastingMode=ShadowCastingMode.On;
        if(cerrar)EditorSceneManager.CloseScene(valle,true);
    }
    static void CrearEntrega(Scene escena,Terrain suelo)
    {
        var m=escena.GetRootGameObjects().Select(o=>o.GetComponent<SegundaMisionChaski>()).First(x=>x!=null);
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Personajes/Chasqui/ChasquiJugadorRealista.prefab");
        var emisor=(GameObject)PrefabUtility.InstantiatePrefab(prefab,escena);emisor.name="Relevista · entrega inicial del quipu";
        var p=m.jugador.transform.position+Vector3.forward*1.55f;p.y=Altura(suelo,p.x,p.z)+.03f;emisor.transform.position=p;
        foreach(var b in emisor.GetComponentsInChildren<MonoBehaviour>(true))b.enabled=false;
        foreach(var c in emisor.GetComponentsInChildren<Collider>(true))c.enabled=false;
        foreach(var c in emisor.GetComponentsInChildren<Cloth>(true))c.enabled=false;
        foreach(var a in emisor.GetComponentsInChildren<Animator>(true))a.enabled=false;
        var contacto=emisor.AddComponent<CapsuleCollider>();contacto.center=Vector3.up;contacto.height=1.9f;contacto.radius=.28f;
        emisor.transform.LookAt(new Vector3(m.jugador.transform.position.x,p.y,m.jugador.transform.position.z));
        m.gameObject.AddComponent<EntregaInicialChaski>().emisor=emisor.transform;
    }
}
