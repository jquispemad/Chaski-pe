using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class AcabarPuebloChaski
{
    const string Escena = "Assets/Scenes/Campana/Nivel01_PrimeraMision.unity";
    const string Valle = "Assets/Niveles/Valle/";

    [InitializeOnLoadMethod]
    static void AplicarAlImportar() => EditorApplication.delayCall += Aplicar;

    [MenuItem("Chasqui/Aplicar acabado del pueblo")]
    static void Aplicar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            AssetDatabase.LoadAssetAtPath<SceneAsset>(Escena) == null) return;
        var anterior = SceneManager.GetActiveScene();
        var escena = SceneManager.GetSceneByPath(Escena);
        bool abrir = !escena.isLoaded;
        if (abrir) escena = EditorSceneManager.OpenScene(Escena, OpenSceneMode.Additive);
        if (escena.GetRootGameObjects().Any(o => o.name == "Paisaje andino"))
        {
            if (abrir) EditorSceneManager.CloseScene(escena, true);
            return;
        }

        SceneManager.SetActiveScene(escena);
        var mision = escena.GetRootGameObjects().Select(o => o.GetComponent<PrimeraMisionChaski>()).First(m => m != null);
        mision.vista.transform.position = new Vector3(-18f, 4.4f, 3.1f);
        mision.vista.transform.rotation = Quaternion.LookRotation(new Vector3(18,-2.3f,0));
        mision.vista.orthographicSize = 5.9f;
        foreach (Transform hijo in mision.transform.Cast<Transform>().Where(t => t.name.StartsWith("Encuadre ")).ToArray())
            Object.DestroyImmediate(hijo.gameObject);

        var pueblo = escena.GetRootGameObjects().First(o => o.name.StartsWith("Pueblo"));
        foreach (var render in pueblo.GetComponentsInChildren<MeshRenderer>())
        {
            switch (render.name)
            {
                case "Suelo": case "Sendero": case "Ladera": render.enabled = false; break;
                case "Muros": case "Andén": render.sharedMaterial = Recurso<Material>("PiedraMuro.mat"); break;
                case "Techo de paja": render.sharedMaterial = Recurso<Material>("TechoIchu.mat"); break;
                case "Puerta": case "Viga · mantener C": case "Soporte de viga":
                    render.sharedMaterial = Recurso<Material>("Madera.mat"); break;
                case "Piedra de práctica":
                    var filtro = render.GetComponent<MeshFilter>();
                    var roca = Recurso<Mesh>("Roca0.asset");
                    filtro.sharedMesh = roca;
                    var col = render.GetComponent<BoxCollider>();
                    var escala = new Vector3(1.25f,.73f,1.7f);
                    var limites = roca.bounds;
                    render.transform.localScale = new Vector3(escala.x/limites.size.x,escala.y/limites.size.y,escala.z/limites.size.z);
                    render.transform.position = new Vector3(0,-limites.min.y*render.transform.localScale.y,37f);
                    col.center = limites.center;
                    col.size = limites.size * .82f;
                    render.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Personajes/Chasqui/MaterialPiedra.mat");
                    break;
            }
        }
        var paisaje = new GameObject("Paisaje andino").transform;
        CrearTerreno(paisaje);
        for (int i = 0; i < 41; i++)
        {
            float z = 2 + 6.6f*i + Mathf.Sin(i*2.6f)*1.8f;
            float x = 10 + (i % 5)*4.2f;
            var p = new Vector3(x, Altura(x,z), z);
            float s = .65f + (i%4)*.16f;
            Malla("Queñua · tronco", "Tronco"+i%2+".asset","Corteza.mat", p, s, paisaje);
            Malla("Queñua · copa", "Copa"+i%2+".asset","Follaje.mat", p, s, paisaje);
            if(i%3==0)
                Malla("Roca erosionada","Roca"+i%4+".asset",null,new Vector3(x, p.y-.12f,z-2f),s,paisaje);
        }
        var sol = escena.GetRootGameObjects().Select(o=>o.GetComponent<Light>()).First(l=>l != null);
        sol.color = new Color(1f,.91f,.77f);
        sol.intensity = 1.25f;
        RenderSettings.sun = sol;
        RenderSettings.skybox = Recurso<Material>("Cielo.mat");
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(.53f,.64f,.75f);
        RenderSettings.ambientEquatorColor = new Color(.42f,.44f,.35f);
        RenderSettings.ambientGroundColor = new Color(.20f,.22f,.16f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(.61f,.70f,.76f);
        RenderSettings.fogDensity = .008f;
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        AssetDatabase.SaveAssets();
        if (abrir)
        {
            if (anterior.IsValid()) SceneManager.SetActiveScene(anterior);
            EditorSceneManager.CloseScene(escena, true);
        }
        Debug.Log("PUEBLO_CHASKI_ACABADO: terreno, recursos del Valle y cámara de seguimiento guardados.");
    }

    static T Recurso<T>(string archivo) where T : Object => AssetDatabase.LoadAssetAtPath<T>(Valle+archivo);

    static float Altura(float x,float z)
    {
        float borde = Mathf.SmoothStep(0,1,Mathf.InverseLerp(4,29,x));
        float montana = Mathf.SmoothStep(0,1,Mathf.InverseLerp(28,79,x));
        return -.045f + borde*(2f+7f*Mathf.PerlinNoise(x*.06f,z*.031f))
            + montana*(9f+22f*Mathf.PerlinNoise(x*.034f+9,z*.026f));
    }

    static void CrearTerreno(Transform padre)
    {
        var baseValle = Recurso<TerrainData>("ValleTerreno.asset");
        const int resolucion = 257;
        const int pintura = 256;
        var datos = new TerrainData
        {
            heightmapResolution = resolucion,
            size = new Vector3(132,48,330),
            alphamapResolution = pintura,
            terrainLayers = baseValle.terrainLayers
        };
        var altura = new float[resolucion,resolucion];
        for(int z=0;z<resolucion;z++)
            for(int x=0;x<resolucion;x++)
                altura[z,x] = (Altura(-24+132f*x/(resolucion-1),-34+330f*z/(resolucion-1))+1)/48;
        datos.SetHeights(0,0,altura);
        var capas = new float[pintura,pintura,datos.terrainLayers.Length];
        for(int z=0;z<pintura;z++)
            for(int x=0;x<pintura;x++)
            {
                float xx=-24+132f*x/(pintura-1), zz=-34+330f*z/(pintura-1);
                float tierra=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.65f,2.6f,Mathf.Abs(xx)+.5f*Mathf.PerlinNoise(xx,zz*.3f)));
                float roca=(1-tierra)*Mathf.SmoothStep(0,1,Mathf.InverseLerp(19,44,datos.GetSteepness(x/(pintura-1f),z/(pintura-1f))));
                capas[z,x,0]=1-tierra-roca;
                capas[z,x,1]=tierra;
                capas[z,x,2]=roca;
            }
        datos.SetAlphamaps(0,0,capas);
        AssetDatabase.CreateAsset(datos,"Assets/Niveles/PrimeraMision/PuebloTerreno.asset");
        var terreno = Terrain.CreateTerrainGameObject(datos);
        terreno.name = "Terreno · tierra y laderas";
        terreno.transform.SetParent(padre);
        terreno.transform.position = new Vector3(-24,-1,-34);
        terreno.GetComponent<Terrain>().materialTemplate = Recurso<Material>("Terreno.mat");
        terreno.GetComponent<Terrain>().drawInstanced = true;
        terreno.GetComponent<Terrain>().detailObjectDistance = 0f;
        terreno.GetComponent<TerrainCollider>().enabled = false;
    }

    static void Malla(string nombre,string malla,string material,Vector3 p,float escala,Transform padre)
    {
        var o = new GameObject(nombre);
        o.transform.SetParent(padre);
        o.transform.position = p;
        o.transform.localScale = Vector3.one*escala;
        o.AddComponent<MeshFilter>().sharedMesh = Recurso<Mesh>(malla);
        o.AddComponent<MeshRenderer>().sharedMaterial = material == null
            ? AssetDatabase.LoadAssetAtPath<Material>("Assets/Personajes/Chasqui/MaterialPiedra.mat")
            : Recurso<Material>(material);
    }
}
