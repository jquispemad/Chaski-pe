using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class ConstruirSegundoRecorrido
{
    public const string Ruta = "Assets/Scenes/Campana/Nivel02_PrimerRecorrido.unity";
    const string Carpeta = "Assets/Niveles/SegundoRecorrido";
    const string Valle = "Assets/Niveles/Valle/";
    static Transform mundo;
    static Terrain terreno;
    static Material piedra, madera, ichu;
    static T Cargar<T>(string ruta) where T:Object => AssetDatabase.LoadAssetAtPath<T>(ruta);

    [InitializeOnLoadMethod]
    static void Programar() => EditorApplication.delayCall += () => {
        if(!EditorApplication.isPlayingOrWillChangePlaymode && Cargar<SceneAsset>(Ruta)==null) Crear();
    };

    [MenuItem("Chasqui/Abrir segundo recorrido")]
    public static void Abrir()
    {
        if(EditorApplication.isPlaying || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        if(Cargar<SceneAsset>(Ruta)==null)Crear();
        EditorSceneManager.OpenScene(Ruta);
        EditorSceneManager.playModeStartScene=Cargar<SceneAsset>(Ruta);
    }

    public static float Centro(float z)
    {
        float curva=14*Mathf.Sin(z*.021f) + 7*Mathf.Sin(z*.047f);
        float recto=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(15,30,Mathf.Abs(z-280)));
        return Mathf.Lerp(curva,-1.5773f,recto);
    }
    public static float AlturaCamino(float z)
    {
        if(z<85)return 0;
        if(z<160)return 6*Mathf.SmoothStep(0,1,(z-85)/75);
        if(z<230)return 6;
        if(z<265)return Mathf.Lerp(6,1,Mathf.SmoothStep(0,1,(z-230)/35));
        if(z<295)return 1;
        return Mathf.Lerp(1,3,Mathf.SmoothStep(0,1,(z-295)/65));
    }
    static float Altura(float x,float z)
    {
        float lateral=Mathf.Abs(x-Centro(z));
        float colina=Mathf.SmoothStep(0,1,Mathf.InverseLerp(12,75,lateral));
        float canal=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(4,11,Mathf.Abs(z-280)));
        return AlturaCamino(z)+colina*(8+23*Mathf.PerlinNoise(x*.032f+17,z*.017f))-canal*5;
    }
    static Vector3 EnCamino(float z,float lado=0) => new Vector3(Centro(z)+lado,AlturaCamino(z),z);

    static void Crear()
    {
        var prefab=Cargar<GameObject>("Assets/Personajes/Chasqui/ChasquiJugadorRealista.prefab");
        if(prefab==null)return;
        if(!AssetDatabase.IsValidFolder(Carpeta))AssetDatabase.CreateFolder("Assets/Niveles","SegundoRecorrido");
        var anterior=SceneManager.GetActiveScene();
        var escena=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
        SceneManager.SetActiveScene(escena);
        mundo=new GameObject("Nivel 02 · paisaje y recorrido").transform;
        piedra=Cargar<Material>(Valle+"PiedraMuro.mat");
        madera=Cargar<Material>(Valle+"Madera.mat"); ichu=Cargar<Material>(Valle+"TechoIchu.mat");
        CrearTerreno();
        CrearVegetacion();
        CrearCultivos();
        Casa(EnCamino(5,11),"Chaskiwasi de salida");
        Casa(EnCamino(58,-14),"Vivienda de los cultivos");
        Casa(EnCamino(313,10),"Refugio junto al arroyo");
        Casa(EnCamino(451,12),"Posta sin relevista");
        Cartel(EnCamino(12,-3.8f),"QHAPAQ ÑAN\nSIGUE A LA POSTA");
        Cartel(EnCamino(100,-3.8f),"LADERA DE PIEDRA\nESPACIO · SALTAR");
        Cartel(EnCamino(183,-3.8f),"PASO BAJO\nMANTÉN C");
        Cartel(EnCamino(259,-3.8f),"PASO DEL ARROYO\nCRUZA LA PASARELA");
        Cartel(EnCamino(428,-3.8f),"CHASKIWASI\nE · TOCAR EL PUTUTU");
        foreach(float z in new[]{35f,73f,346f,395f})
            Bloque("Tronco bajo · saltar",EnCamino(z)+Vector3.up*.22f,new Vector3(4,.44f,.55f),madera);
        foreach(float z in new[]{127f,162f,365f}) Monton(z);
        CrearPasoBajo(199);
        CrearPuente();
        // Invisible boundaries remain well outside the route and its side activities.
        foreach(float x in new[]{-88f,88f}) Limite(new Vector3(x,12,230),new Vector3(1,60,525));
        Limite(new Vector3(0,12,-9),new Vector3(180,60,1));
        Limite(new Vector3(0,12,474),new Vector3(180,60,1));

        var jugador=(GameObject)PrefabUtility.InstantiatePrefab(prefab,escena);
        jugador.name="Wayra · jugador";jugador.transform.position=EnCamino(3)+Vector3.up*.15f;
        var movimiento=jugador.GetComponent<ChasquiMovimiento>();movimiento.recorridoLateral=false;
        var vista=new GameObject("Cámara de seguimiento 3D").AddComponent<Camera>();
        vista.tag="MainCamera";vista.fieldOfView=58;vista.nearClipPlane=.12f;vista.farClipPlane=380;
        vista.gameObject.AddComponent<AudioListener>();
        var camara=vista.gameObject.AddComponent<CamaraChasqui>();camara.objetivo=jugador.transform;
        movimiento.camara=vista.transform;
        var mision=new GameObject("Segunda misión · mensaje y relevo").AddComponent<SegundaMisionChaski>();
        mision.jugador=movimiento;mision.vista=vista;
        var npc=(GameObject)PrefabUtility.InstantiatePrefab(prefab,escena);
        npc.name="Responsable de la posta";npc.transform.position=EnCamino(451,3.6f)+Vector3.up*.12f;
        npc.transform.rotation=Quaternion.Euler(0,180,0);
        foreach(var b in npc.GetComponentsInChildren<MonoBehaviour>(true))b.enabled=false;
        foreach(var c in npc.GetComponentsInChildren<Collider>(true))c.enabled=false;
        foreach(var c in npc.GetComponentsInChildren<Cloth>(true))c.enabled=false;
        var contacto=npc.AddComponent<CapsuleCollider>();contacto.center=Vector3.up;contacto.height=1.9f;contacto.radius=.3f;
        mision.responsable=npc.transform;
        var aro=new GameObject("Círculo de entrega · nivel 2");aro.transform.position=EnCamino(450,1.6f)+Vector3.up*.045f;
        aro.AddComponent<MeshFilter>().sharedMesh=Cargar<Mesh>("Assets/Niveles/PrimeraMision/AroEntrega.asset");
        aro.AddComponent<MeshRenderer>().sharedMaterial=Cargar<Material>("Assets/Niveles/PrimeraMision/OroEntrega.mat");mision.entrega=aro.transform;
        var registro=EnCamino(216,-9);
        Bloque("Mirador · descanso",registro-Vector3.up*.12f,new Vector3(5,.24f,5),piedra);
        Cartel(registro+Vector3.forward*1.8f,"LOS RELEVOS\nE · LEER EL REGISTRO");
        mision.mirador=new GameObject("Registro educativo · mirador").transform;mision.mirador.position=registro;
        var agricultor=(GameObject)PrefabUtility.InstantiatePrefab(prefab,escena);
        agricultor.name="Agricultor junto al cultivo";agricultor.transform.position=EnCamino(61,-5)+Vector3.up*.1f;
        foreach(var b in agricultor.GetComponentsInChildren<MonoBehaviour>(true))b.enabled=false;
        foreach(var c in agricultor.GetComponentsInChildren<Collider>(true))c.enabled=false;
        foreach(var c in agricultor.GetComponentsInChildren<Cloth>(true))c.enabled=false;
        Cartel(EnCamino(62,-3.8f),"LA POSTA ESTÁ TRAS EL AGUA\nHOY NO RESPONDIÓ SU PUTUTU");
        ConfigurarAudio(mision);
        var sol=new GameObject("Sol de mañana").AddComponent<Light>();sol.type=LightType.Directional;
        sol.transform.rotation=Quaternion.Euler(44,-32,0);sol.intensity=1.2f;sol.color=new Color(1,.93f,.82f);sol.shadows=LightShadows.Soft;
        RenderSettings.sun=sol;RenderSettings.skybox=Cargar<Material>(Valle+"Cielo.mat");
        RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.58f,.68f,.79f);
        RenderSettings.ambientEquatorColor=new Color(.46f,.47f,.37f);RenderSettings.ambientGroundColor=new Color(.24f,.25f,.19f);
        RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.0035f;
        RenderSettings.fogColor=new Color(.64f,.73f,.79f);
        EditorSceneManager.SaveScene(escena,Ruta);
        EditorBuildSettings.scenes=EditorBuildSettings.scenes.Where(s=>s.path!=Ruta).Concat(new[]{new EditorBuildSettingsScene(Ruta,true)}).ToArray();
        AssetDatabase.SaveAssets();
        if(anterior.IsValid())SceneManager.SetActiveScene(anterior);
        EditorSceneManager.CloseScene(escena,true);
        Debug.Log("NIVEL02_CREADO: recorrido 3D, cultivos, piedras, puente, posta, misión y sonido guardados.");
    }

    static void CrearTerreno()
    {
        var original=Cargar<TerrainData>(Valle+"ValleTerreno.asset");
        const int n=513, a=512;
        var datos=new TerrainData{heightmapResolution=n,alphamapResolution=a,size=new Vector3(220,60,520),terrainLayers=original.terrainLayers};
        var alturas=new float[n,n];
        for(int z=0;z<n;z++)for(int x=0;x<n;x++)alturas[z,x]=(Altura(-110+220f*x/(n-1),-20+520f*z/(n-1))+10)/60;
        datos.SetHeights(0,0,alturas);
        var pintura=new float[a,a,datos.terrainLayers.Length];
        for(int z=0;z<a;z++)for(int x=0;x<a;x++)
        {
            float zz=-20+520f*z/(a-1),xx=-110+220f*x/(a-1);
            float camino=(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(2.2f,3.6f,Mathf.Abs(xx-Centro(zz)))))*.92f;
            float roca=(1-camino)*Mathf.SmoothStep(0,1,Mathf.InverseLerp(25,52,datos.GetSteepness(x/(a-1f),z/(a-1f))));
            pintura[z,x,0]=1-camino-roca;pintura[z,x,1]=camino;pintura[z,x,2]=roca;
        }
        datos.SetAlphamaps(0,0,pintura);AssetDatabase.CreateAsset(datos,Carpeta+"/TerrenoRecorrido.asset");
        var o=Terrain.CreateTerrainGameObject(datos);o.name="Laderas y sendero · terreno con colisión";o.transform.SetParent(mundo);o.transform.position=new Vector3(-110,-10,-20);
        terreno=o.GetComponent<Terrain>();terreno.materialTemplate=Cargar<Material>(Valle+"Terreno.mat");terreno.drawInstanced=true;
        terreno.detailObjectDistance=0;terreno.treeDistance=150;terreno.heightmapPixelError=8;
    }

    static void CrearVegetacion()
    {
        var grupo=new GameObject("Queñuas y rocas · laterales").transform;grupo.SetParent(mundo);
        for(int i=0;i<105;i++)
        {
            float z=7+i*4.35f;
            if(Mathf.Abs(z-280)<16)continue;
            float lado=(i%2==0?-1:1)*(9+(i%6)*6.2f),x=Centro(z)+lado;
            float y=Altura(x,z),s=.7f+(i%4)*.16f;
            var tronco=Malla("Queñua · tronco",Cargar<Mesh>(Valle+"Tronco"+i%2+".asset"),Cargar<Material>(Valle+"Corteza.mat"),new Vector3(x,y,z),Vector3.one*s,grupo);
            var col=tronco.AddComponent<CapsuleCollider>();col.center=Vector3.up*1.3f;col.height=2.6f;col.radius=.18f;
            var copa=Malla("Queñua · hojas",Cargar<Mesh>(Valle+"Copa"+i%2+".asset"),Cargar<Material>(Valle+"Follaje.mat"),new Vector3(x,y,z),Vector3.one*s,grupo);
            copa.GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.Off;
            if(i%2==0)Roca(new Vector3(x+3,y,z+1),new Vector3(1.8f,1.2f,2.1f),i,true,grupo);
        }
        for(int i=0;i<85;i++)
        {
            float z=10+i*5.2f;if(Mathf.Abs(z-280)<14 || z>430)continue;
            float x=Centro(z)+(i%2==0?-1:1)*(4.8f+(i%3)*1.3f);
            Roca(new Vector3(x,Altura(x,z),z),new Vector3(1.2f+i%3*.45f,.5f+i%4*.3f,1.4f),i,true,grupo);
        }
    }
    static void CrearCultivos()
    {
        var grupo=new GameObject("Andenes con maíz de hojas").transform;grupo.SetParent(mundo);
        var hoja=Cargar<Mesh>("Assets/Niveles/PrimeraMision/HojasMaiz.asset");
        for(int tramo=0;tramo<3;tramo++)
        {
            float z=22+tramo*21;
            for(int fila=0;fila<3;fila++)
            {
                float x=Centro(z)+10+fila*3,y=Altura(x,z)+fila*.32f;
                Bloque("Muro de andén",new Vector3(x,y+.2f,z),new Vector3(2.8f,.4f,15),piedra,grupo);
                Bloque("Tierra de cultivo",new Vector3(x,y+.43f,z),new Vector3(2.65f,.08f,14.8f),Cargar<Material>("Assets/Niveles/PrimeraMision/Cultivos.mat"),grupo);
                var combinaciones=new List<CombineInstance>();
                for(int k=0;k<13;k++)foreach(float dx in new[]{-.7f,.7f})
                    combinaciones.Add(new CombineInstance{mesh=hoja,transform=Matrix4x4.TRS(new Vector3(x+dx,y+.48f,z-6.7f+k*1.1f),Quaternion.Euler(0,k*67,0),Vector3.one*1.25f)});
                var mesh=new Mesh{name="Maíz de andén"};mesh.CombineMeshes(combinaciones.ToArray());AssetDatabase.CreateAsset(mesh,$"{Carpeta}/Maiz{tramo}_{fila}.asset");
                var o=Malla("Cultivo · hojas",mesh,Cargar<Material>("Assets/Niveles/PrimeraMision/HojaMaiz.mat"),Vector3.zero,Vector3.one,grupo);o.GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.Off;
            }
        }
    }
    static void Monton(float z)
    {
        float[] h={.30f,.66f,1.02f,.66f,.30f};
        for(int i=0;i<5;i++)
        {
            float zz=z+(i-2)*1.25f;
            Roca(EnCamino(zz),new Vector3(3.2f,h[i],1.6f),i,false,mundo);
            Bloque("Piedra · apoyo de salto",EnCamino(zz)+Vector3.up*(h[i]*.5f-.025f),new Vector3(2.8f,h[i]-.05f,1.4f),null);
        }
    }
    static GameObject Roca(Vector3 p,Vector3 medida,int variante,bool colision,Transform padre)
    {
        if(colision)
        {
            p.y=Mathf.Min(Altura(p.x-medida.x*.35f,p.z-medida.z*.35f),Altura(p.x+medida.x*.35f,p.z+medida.z*.35f),
                Altura(p.x-medida.x*.35f,p.z+medida.z*.35f),Altura(p.x+medida.x*.35f,p.z-medida.z*.35f));
        }
        var mesh=Cargar<Mesh>(Valle+"Roca"+variante%4+".asset");var size=mesh.bounds.size;
        var escala=new Vector3(medida.x/size.x,medida.y/size.y,medida.z/size.z);
        p.y-=mesh.bounds.min.y*escala.y+.035f;
        var o=Malla("Roca erosionada",mesh,Cargar<Material>("Assets/Personajes/Chasqui/MaterialPiedra.mat"),p,escala,padre);
        if(colision){var c=o.AddComponent<BoxCollider>();c.center=mesh.bounds.center;c.size=mesh.bounds.size*.85f;}
        return o;
    }
    static void CrearPasoBajo(float z)
    {
        var p=EnCamino(z);
        foreach(float x in new[]{-2.6f,2.6f})Bloque("Soporte de viga",p+new Vector3(x,1.1f,0),new Vector3(.4f,2.2f,1),madera);
        var v=Bloque("Viga inclinada · mantener C",p+Vector3.up*1.91f,new Vector3(6,.42f,.7f),madera);
        v.transform.rotation=Quaternion.Euler(0,16,0);
        Object.DestroyImmediate(v.GetComponent<Collider>());
        Bloque("Paso bajo · colisión a 1.66 m",p+Vector3.up*1.89f,new Vector3(5.6f,.46f,1.5f),null);
        foreach(float lado in new[]{-1,1})Bloque("Murete del paso",p+new Vector3(lado*3.05f,.55f,0),new Vector3(.5f,1.1f,7),piedra);
    }
    static void CrearPuente()
    {
        float x=Centro(280);
        // Safe solid woven deck over a real depression; broad geometry avoids fine-line artifacts.
        Bloque("Puente · piso de ichu trenzado",new Vector3(x,.88f,280),new Vector3(3.6f,.24f,22),ichu);
        var soga=Cargar<Material>(Valle+"SogasIchu.mat");
        foreach(float lado in new[]{-1,1})
        {
            Bloque("Baranda firme del puente",new Vector3(x+lado*1.9f,1.7f,280),new Vector3(.18f,1.4f,22),null);
            for(int z=270;z<=290;z+=5)Bloque("Poste de pasarela",new Vector3(x+lado*1.9f,1.65f,z),new Vector3(.19f,1.5f,.19f),madera);
            Bloque("Soga de ichu · pasamanos",new Vector3(x+lado*1.9f,2.3f,280),new Vector3(.13f,.13f,22),soga);
        }
        var agua=new Material(Shader.Find("Standard")){color=new Color(.22f,.43f,.43f)};agua.SetFloat("_Glossiness",.72f);
        AssetDatabase.CreateAsset(agua,Carpeta+"/AguaArroyo.mat");
        Bloque("Arroyo · superficie de agua",new Vector3(0,-.7f,280),new Vector3(170,.12f,9),agua,mundo,false);
        foreach(float z in new[]{271f,289f})
            for(int i=-4;i<=4;i++)
            {float xx=x+i*5;Roca(new Vector3(xx,Altura(xx,z),z),new Vector3(2,.65f,1.5f),i+4,true,mundo);}
    }
    static void Casa(Vector3 p,string nombre)
    {
        var raiz=new GameObject(nombre).transform;raiz.SetParent(mundo);raiz.position=p;
        Bloque("Cimiento de piedra",p+Vector3.up*.15f,new Vector3(6,.3f,7),piedra,raiz);
        foreach(float x in new[]{-2.8f,2.8f})Bloque("Muro de piedra",p+new Vector3(x,1.5f,0),new Vector3(.4f,2.7f,6.5f),piedra,raiz);
        Bloque("Muro posterior",p+new Vector3(0,1.5f,3.1f),new Vector3(6,2.7f,.4f),piedra,raiz);
        foreach(float x in new[]{-1.9f,1.9f})Bloque("Fachada · entrada despejada",p+new Vector3(x,1.5f,-3.1f),new Vector3(2.1f,2.7f,.4f),piedra,raiz);
        Bloque("Dintel",p+new Vector3(0,2.7f,-3.1f),new Vector3(1.8f,.3f,.4f),madera,raiz);
        foreach(int lado in new[]{-1,1})
        {var o=Bloque("Techo de ichu",p+new Vector3(lado*1.57f,3.26f,0),new Vector3(3.6f,.25f,7.5f),ichu,raiz);o.transform.rotation=Quaternion.Euler(0,0,-lado*24);}
    }
    static void Cartel(Vector3 p,string mensaje)
    {
        var raiz=new GameObject("Letrero · "+mensaje.Split('\n')[0]).transform;raiz.SetParent(mundo);raiz.position=p;
        Bloque("Poste de madera",p+Vector3.up*.8f,new Vector3(.18f,1.6f,.18f),madera,raiz);
        Bloque("Tabla tallada",p+Vector3.up*1.8f,new Vector3(3.5f,.9f,.18f),Cargar<Material>(Valle+"MaderaLetrerosTallados.mat"),raiz);
        var o=new GameObject("Inscripción tallada");o.transform.SetParent(raiz,false);o.transform.localPosition=new Vector3(0,1.8f,-.105f);
        var t=o.AddComponent<TextMesh>();t.text=mensaje;t.font=Resources.Load<Font>("EstiloInca/TituloTradicional");t.fontSize=72;t.characterSize=.038f;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;
        o.GetComponent<MeshRenderer>().sharedMaterial=Cargar<Material>(Valle+"LetraCarteles.mat");o.AddComponent<TextoCartelChasqui>();
        var size=o.GetComponent<MeshRenderer>().bounds.size;
        if(size.x>.001f)o.transform.localScale*=Mathf.Min(3.18f/size.x,.7f/Mathf.Max(.01f,size.y));
    }
    static GameObject Bloque(string nombre,Vector3 p,Vector3 tam,Material mat,Transform padre=null,bool solido=true)
    {
        var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=nombre;o.transform.SetParent(padre==null?mundo:padre);
        o.transform.position=p;o.transform.localScale=tam;
        if(mat==null)o.GetComponent<Renderer>().enabled=false;else o.GetComponent<Renderer>().sharedMaterial=mat;
        if(!solido)Object.DestroyImmediate(o.GetComponent<Collider>());
        return o;
    }
    static GameObject Malla(string nombre,Mesh mesh,Material mat,Vector3 p,Vector3 escala,Transform padre)
    {
        var o=new GameObject(nombre);o.transform.SetParent(padre);o.transform.position=p;o.transform.localScale=escala;
        o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=mat;return o;
    }
    static void Limite(Vector3 p,Vector3 size) => Bloque("Límite del recorrido",p,size,null);
    static void ConfigurarAudio(SegundaMisionChaski mision)
    {
        const string origen="Assets/Scenes/Campana/Nivel01_PrimeraMision.unity";
        var primera=SceneManager.GetSceneByPath(origen);bool cerrar=!primera.isLoaded;
        if(cerrar)primera=EditorSceneManager.OpenScene(origen,OpenSceneMode.Additive);
        var audio=primera.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<AudioPrimeraMision>()).FirstOrDefault();
        if(audio!=null)EditorUtility.CopySerialized(audio,mision.gameObject.AddComponent<AudioPrimeraMision>());
        if(cerrar)EditorSceneManager.CloseScene(primera,true);
        var clip=Cargar<AudioClip>("Assets/Audio/Nivel02/Arroyo.mp3");
        if(clip!=null)
        {
            var o=new GameObject("Audio · arroyo cercano");SceneManager.MoveGameObjectToScene(o,mision.gameObject.scene);
            o.transform.position=new Vector3(Centro(280),1,280);
            var f=o.AddComponent<AudioSource>();f.clip=clip;f.loop=true;f.playOnAwake=true;f.spatialBlend=1;f.volume=.35f;f.minDistance=8;f.maxDistance=35;f.rolloffMode=AudioRolloffMode.Linear;
            mision.arroyo=f;
        }
    }
}
