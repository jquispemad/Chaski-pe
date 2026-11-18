using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ConstruirPrimeraMision
{
    const string Escena = "Assets/Scenes/Campana/Nivel01_PrimeraMision.unity";
    const string Materiales = "Assets/Niveles/PrimeraMision";

    [InitializeOnLoadMethod]
    static void PrepararPrimeraImportacion()
    {
        EditorApplication.delayCall += () =>
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode &&
                AssetDatabase.LoadAssetAtPath<SceneAsset>(Escena) == null && AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Nivel01_PrimeraMision.unity") == null)
                Crear();
        };
    }

    [MenuItem("Chasqui/Abrir primera misión")]
    public static void Abrir()
    {
        if (EditorApplication.isPlaying || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Escena) == null && AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Nivel01_PrimeraMision.unity") == null) Crear();
        EditorSceneManager.OpenScene(Escena);
    }

    static void Crear()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Personajes/Chasqui/ChasquiJugadorRealista.prefab");
        if (prefab == null) return;
        if (!AssetDatabase.IsValidFolder(Materiales)) AssetDatabase.CreateFolder("Assets/Niveles", "PrimeraMision");
        var tierra = Material("Tierra", new Color(.39f, .25f, .13f));
        var piedra = Material("Piedra", new Color(.49f, .43f, .34f));
        var paja = Material("Paja", new Color(.69f, .48f, .21f));
        var madera = Material("Madera", new Color(.26f, .14f, .07f));
        var verde = Material("Cultivos", new Color(.28f, .44f, .15f));
        var fondo = Material("Laderas", new Color(.31f, .40f, .24f));
        var escenaAnterior = SceneManager.GetActiveScene();
        var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(escena);
        var pueblo = new GameObject("Pueblo · vivienda, cultivos y posta").transform;
        Bloque("Suelo", new Vector3(0,-.5f,130), new Vector3(62,1,264), verde, pueblo);
        Bloque("Sendero", new Vector3(0,-.08f,130), new Vector3(4,.16f,260), tierra, pueblo);
        Bloque("Límite inicio", new Vector3(0,1,-1), new Vector3(4,2,.5f), piedra, pueblo);
        Bloque("Límite posta", new Vector3(0,1,258), new Vector3(4,2,.5f), piedra, pueblo);
        for (int i=0;i<10;i++)
        {
            float z = i*26+9;
            Bloque("Ladera", new Vector3(26,1,z), new Vector3(18,5+i%3*2,36),fondo,pueblo,false)
                .transform.rotation = Quaternion.Euler(0,13,16);
            if (i<4 || i>=8) Casa(i==9?"Chaskiwasi":"Vivienda", z, piedra,paja,madera,pueblo);
            else
            {
                for(int a=0;a<3;a++)
                {
                    Bloque("Andén",new Vector3(5+a*2,.25f+a*.4f,z),new Vector3(2,.5f+a*.8f,22),piedra,pueblo,false);
                    Bloque("Cultivo",new Vector3(5+a*2,.54f+a*.8f,z),new Vector3(1.8f,.12f,21),verde,pueblo,false);
                    for(int b=0;b<12;b++)
                        Bloque("Maíz",new Vector3(5+a*2,.8f+a*.8f,z-10+b*1.8f),new Vector3(.24f,.6f,.24f),verde,pueblo,false);
                }
            }
        }
        var roca = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        roca.name="Piedra de práctica"; roca.transform.SetParent(pueblo);
        roca.transform.position=new Vector3(0,.18f,37); roca.transform.localScale=new Vector3(1.5f,.88f,1.8f);
        roca.GetComponent<Renderer>().sharedMaterial=piedra;
        roca.GetComponent<SphereCollider>().enabled=false;
        var obstaculo=roca.AddComponent<BoxCollider>(); obstaculo.size=new Vector3(.75f,.8f,.66f);
        Bloque("Viga · mantener C",new Vector3(0,1.87f,116),new Vector3(3,.44f,.65f),madera,pueblo);
        foreach(float x in new[]{-1.1f,1.1f})
            Bloque("Soporte de viga",new Vector3(x,.9f,116),new Vector3(.2f,1.8f,.25f),madera,pueblo);
        Bloque("Escalón de piedra",new Vector3(0,.11f,188),new Vector3(2,.22f,1.25f),piedra,pueblo);
        var jugador = (GameObject)PrefabUtility.InstantiatePrefab(prefab, escena);
        jugador.name="Wayra"; jugador.transform.position = new Vector3(0,.10f,2);
        var movimiento = jugador.GetComponent<ChasquiMovimiento>();
        movimiento.recorridoLateral = true;
        var relevista = (GameObject)PrefabUtility.InstantiatePrefab(prefab, escena);
        relevista.name="Relevista de la posta"; relevista.transform.position=new Vector3(.88f,.10f,249);
        foreach(var componente in relevista.GetComponentsInChildren<MonoBehaviour>(true)) componente.enabled=false;
        foreach(var collider in relevista.GetComponentsInChildren<Collider>(true)) collider.enabled=false;
        foreach(var tela in relevista.GetComponentsInChildren<Cloth>(true)) tela.enabled=false;
        var camara = new GameObject("Cámara lateral fija").AddComponent<Camera>();
        camara.tag="MainCamera"; camara.orthographic=true; camara.orthographicSize=9.1f;
        camara.gameObject.AddComponent<AudioListener>();
        var mision = new GameObject("Primera misión").AddComponent<PrimeraMisionChaski>();
        mision.jugador=movimiento; mision.vista=camara; mision.llegada=245;
        camara.transform.position=new Vector3(-18,4.4f,3.1f);
        camara.transform.rotation=Quaternion.LookRotation(new Vector3(18,-2.3f,0));
        movimiento.camara=camara.transform;
        var luz=new GameObject("Sol de la mañana").AddComponent<Light>();
        luz.type=LightType.Directional; luz.intensity=1.15f; luz.shadows=LightShadows.Soft;
        luz.transform.rotation=Quaternion.Euler(36,-42,0);
        RenderSettings.ambientLight=new Color(.63f,.62f,.57f);
        EditorSceneManager.SaveScene(escena,Escena);
        if (escenaAnterior.IsValid()) SceneManager.SetActiveScene(escenaAnterior);
        EditorSceneManager.CloseScene(escena,true);
        EditorBuildSettings.scenes = new[]{new EditorBuildSettingsScene(Escena,true)}
            .Concat(EditorBuildSettings.scenes.Where(s=>s.path!=Escena && !s.path.EndsWith("/Nivel01_Valle.unity"))).ToArray();
        AssetDatabase.SaveAssets();
        Debug.Log("PRIMERA_MISION_CREADA: "+Escena);
    }

    static Material Material(string nombre, Color color)
    {
        var m = new Material(Shader.Find("Standard")) {color=color};
        m.SetFloat("_Glossiness",.05f);
        AssetDatabase.CreateAsset(m,$"{Materiales}/{nombre}.mat");
        return m;
    }

    static GameObject Bloque(string nombre, Vector3 posicion, Vector3 tamaño, Material material, Transform padre, bool solido=true)
    {
        var o=GameObject.CreatePrimitive(PrimitiveType.Cube);
        o.name=nombre; o.transform.SetParent(padre); o.transform.position=posicion; o.transform.localScale=tamaño;
        o.GetComponent<Renderer>().sharedMaterial=material; o.GetComponent<Collider>().enabled=solido;
        return o;
    }

    static void Casa(string nombre,float z,Material piedra,Material paja,Material madera,Transform padre)
    {
        var casa = new GameObject(nombre).transform;
        casa.SetParent(padre);
        Bloque("Muros",new Vector3(5,1.3f,z),new Vector3(4,2.6f,6),piedra,casa,false);
        Bloque("Puerta",new Vector3(2.96f,.92f,z),new Vector3(.12f,1.84f,1.1f),madera,casa,false);
        foreach(int lado in new[]{-1,1})
            Bloque("Techo de paja",new Vector3(5+lado*1.12f,3.1f,z),new Vector3(2.65f,.22f,6.6f),paja,casa,false)
                .transform.rotation=Quaternion.Euler(0,0,-lado*24);
    }
}
