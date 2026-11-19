using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class ConstruirValleVientos
{
    public const string Ruta="Assets/Scenes/Campana/Nivel03_ValleVientos.unity";
    const string Dir="Assets/Niveles/ValleVientos/";
    static Transform mundo;static Terrain terreno;static Material roca,madera;static Scene escena;
    static T Recurso<T>(string ruta)where T:Object=>AssetDatabase.LoadAssetAtPath<T>(ruta);
    static Vector3 Camino(float z,float lateral=0)=>new Vector3(NivelTresChaski.Centro(z)+lateral,NivelTresChaski.Base(z),z);
    static float Altura(float x,float z)
    {
        float margen=Mathf.Abs(x-NivelTresChaski.Centro(z));
        float y=NivelTresChaski.Base(z)+Mathf.SmoothStep(0,1,Mathf.InverseLerp(10,85,margen))*(12+18*Mathf.PerlinNoise(x*.025f+13,z*.02f));
        if(x<80)
        {
            float dist=Mathf.Abs(z-NivelTresChaski.CentroAgua(x));
            float canal=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(3,6.5f,dist));
            y=Mathf.Lerp(y,NivelTresChaski.AguaY(x)-1.1f,canal);
        }
        return y;
    }
    [InitializeOnLoadMethod]
    static void Preparar()=>EditorApplication.delayCall+=()=>{if(!EditorApplication.isPlayingOrWillChangePlaymode){if(Recurso<SceneAsset>(Ruta)==null)Crear();Ajustar();}};
    static void Ajustar()
    {
        if(SessionState.GetBool("ValleVientos.Ajuste3",false))return;
        var e=SceneManager.GetSceneByPath(Ruta);bool cerrar=!e.isLoaded;if(cerrar)e=EditorSceneManager.OpenScene(Ruta,OpenSceneMode.Additive);
        var todos=e.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Transform>(true)).ToArray();
        var t=todos.First(o=>o.GetComponent<Terrain>()!=null).GetComponent<Terrain>();
        foreach(var p in todos.Where(o=>o.name=="Piedra del arroyo · superficie de apoyo"))
        {
            float y=Mathf.Max(.62f+Mathf.Sin((p.position.z-273)*.4f)*.12f,t.SampleHeight(p.position)+t.transform.position.y+.12f);
            p.position=new Vector3(p.position.x,y-.35f,p.position.z);
            p.localScale=new Vector3(2.5f,y+2.1f,1.65f);p.position=new Vector3(p.position.x,(y-2.1f)*.5f,p.position.z);
            var piedra=todos.FirstOrDefault(o=>o.name=="Roca · abrigo y paisaje"&&Mathf.Abs(o.position.z-p.position.z)<.05f&&Mathf.Abs(o.position.x-p.position.x)<.05f);
            if(piedra!=null){var mb=piedra.GetComponent<MeshFilter>().sharedMesh.bounds;var escala=piedra.localScale;escala.y=(y+2.1f)/mb.size.y;piedra.localScale=escala;piedra.position=new Vector3(piedra.position.x,-2.1f-mb.min.y*escala.y,piedra.position.z);}
        }
        foreach(var p in todos.Where(o=>o.name.StartsWith("Agricultor ·")))
        {
            var rs=p.GetComponentsInChildren<Renderer>();if(rs.Length==0)continue;var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);
            var destino=Camino(62,6);p.position+=new Vector3(destino.x-b.center.x,destino.y-b.min.y+.03f,destino.z-b.center.z);
        }
        foreach(var p in todos.Where(o=>o.name.StartsWith("Queñua ·")&&o.position.x>NivelTresChaski.Centro(o.position.z)&&Mathf.Abs(o.position.z-325)<14))Object.DestroyImmediate(p.gameObject);
        EditorSceneManager.SaveScene(e);Validar(e);if(cerrar)EditorSceneManager.CloseScene(e,true);SessionState.SetBool("ValleVientos.Ajuste3",true);
    }
    [MenuItem("Chasqui/Abrir nivel 3 · Valle de los vientos")]
    public static void Abrir()
    {
        if(EditorApplication.isPlaying)return;if(Recurso<SceneAsset>(Ruta)==null)Crear();EditorSceneManager.OpenScene(Ruta);EditorSceneManager.playModeStartScene=Recurso<SceneAsset>(Ruta);
    }
    public static void Crear()
    {
        if(Recurso<SceneAsset>(Ruta)!=null)return;
        if(!AssetDatabase.IsValidFolder(Dir.TrimEnd('/')))AssetDatabase.CreateFolder("Assets/Niveles","ValleVientos");
        var anterior=SceneManager.GetActiveScene();escena=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(escena);
        mundo=new GameObject("Nivel 03 · valle, laderas y abrigos").transform;
        roca=Recurso<Material>("Assets/Personajes/Chasqui/MaterialPiedra.mat");madera=Recurso<Material>("Assets/Niveles/Valle/Madera.mat");
        CrearTerreno();CrearSendero();CrearArroyo();
        var m=new GameObject("Misión · el valle de los vientos").AddComponent<NivelTresChaski>();
        var prefab=Recurso<GameObject>("Assets/Personajes/Chasqui/ChasquiJugadorRealista.prefab");
        var jugador=(GameObject)PrefabUtility.InstantiatePrefab(prefab,escena);jugador.name="Wayra · valle de los vientos";jugador.transform.position=Camino(4)+Vector3.up*.12f;
        m.jugador=jugador.GetComponent<ChasquiMovimiento>();m.jugador.recorridoLateral=false;
        var vista=new GameObject("Cámara · tercera persona").AddComponent<Camera>();vista.tag="MainCamera";vista.fieldOfView=58;vista.nearClipPlane=.12f;vista.farClipPlane=360;vista.gameObject.AddComponent<AudioListener>();
        vista.transform.position=jugador.transform.position+new Vector3(0,2.3f,-4.5f);vista.transform.LookAt(jugador.transform.position+Vector3.up*1.1f);
        vista.gameObject.AddComponent<CamaraChasqui>().objetivo=jugador.transform;m.jugador.camara=vista.transform;m.vista=vista;
        var copia=SceneManager.GetSceneByPath(ConstruirSegundoRecorrido.Ruta);bool cerrar=!copia.isLoaded;if(cerrar)copia=EditorSceneManager.OpenScene(ConstruirSegundoRecorrido.Ruta,OpenSceneMode.Additive);
        var casas=copia.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Transform>()).Where(t=>new[]{"Chaskiwasi de salida","Refugio junto al arroyo","Posta sin relevista"}.Contains(t.name)).ToArray();
        int nc=0;
        foreach(var original in casas)
        {
            var o=Object.Instantiate(original.gameObject);SceneManager.MoveGameObjectToScene(o,escena);o.transform.SetParent(mundo,true);
            float z=nc==0?7:nc==1?325:488;float x=NivelTresChaski.Centro(z)+10;
            o.name=nc==0?"Posta de salida":nc==1?"Refugio del caminante":"Estación del valle";o.transform.position=new Vector3(x,NivelTresChaski.Base(z),z);
            o.transform.rotation=Quaternion.identity;nc++;
        }
        var origenAudio=copia.GetRootGameObjects().Select(o=>o.GetComponent<AudioPrimeraMision>()).First(a=>a!=null);
        EditorUtility.CopySerialized(origenAudio,m.gameObject.AddComponent<AudioPrimeraMision>());
        var campesino=copia.GetRootGameObjects().First(o=>o.name.StartsWith("Campesino ·"));
        var agricultor=Object.Instantiate(campesino);SceneManager.MoveGameObjectToScene(agricultor,escena);agricultor.name="Agricultor · trabajando junto a los cultivos";agricultor.transform.position=Camino(62,6)+Vector3.up*.05f;
        if(cerrar)EditorSceneManager.CloseScene(copia,true);
        m.encargado=NPC(prefab,Camino(487,3.4f),"Encargado de la estación").transform;
        m.caminante=NPC(prefab,Camino(330,4.8f),"Caminante del refugio").transform;
        m.circulo=new GameObject("Círculo · entrega del aviso").transform;m.circulo.position=Camino(486,1.3f)+Vector3.up*.045f;
        m.circulo.gameObject.AddComponent<MeshFilter>().sharedMesh=Recurso<Mesh>("Assets/Niveles/PrimeraMision/AroEntrega.asset");m.circulo.gameObject.AddComponent<MeshRenderer>().sharedMaterial=Recurso<Material>("Assets/Niveles/PrimeraMision/OroEntrega.mat");
        m.descanso=Punto("Descanso junto al refugio",Camino(323,5.6f));m.registro=Punto("Mirador · registro opcional",Camino(219,-7));
        m.controles=new[]{100f,213f,304f,438f}.Select(z=>Punto("Control seguro · "+z,Camino(z)+Vector3.up*.14f)).ToArray();
        var abrigos=new List<Transform>();
        foreach(float z in new[]{139f,174f,205f,374f,405f})abrigos.Add(Roca(Camino(z,3.8f),new Vector3(2.3f,2.3f,5),true).transform);
        m.abrigos=abrigos.ToArray();
        foreach(float z in new[]{36f,82f,352f})Bloque("Tronco · salto",Camino(z)+Vector3.up*.21f,new Vector3(4,.42f,.55f),madera);
        foreach(float z in new[]{69f,155f,393f})
        {
            for(int i=-2;i<=2;i++){float zz=z+i*1.3f,h=1.05f-Mathf.Abs(i)*.34f;Roca(Camino(zz),new Vector3(2.7f,h,1.6f),false);Bloque("Piedra · apoyo estable",Camino(zz)+Vector3.up*h*.5f,new Vector3(2.5f,h,1.45f),null);}
        }
        CrearCruce();CrearArboles();CrearCultivos();CrearLimites();
        Cartel(15,"ESTACIÓN DEL VALLE\nSIGUE EL SENDERO");Cartel(112,"PASO DEL VIENTO\nC · AGACHARSE");Cartel(245,"PASO DEL ARROYO\nBUSCA APOYO EN LAS PIEDRAS");Cartel(312,"REFUGIO DEL CAMINANTE\nDESCANSA Y REVISA TU CHUSPA");Cartel(358,"LADERA EXPUESTA\nOBSERVA LAS RÁFAGAS");Cartel(466,"POSTA DEL VALLE\nE · TOCAR EL PUTUTU");
        Cartel(219,"LOS RELEVOS\nE · LEER",-7);
        var raciones=new List<RecursoValleVientos>();
        raciones.Add(Racion(1,Camino(92,-3.3f),"cancha-ladera"));raciones.Add(Racion(1,Camino(319,3.4f),"cancha-refugio"));raciones.Add(Racion(3,Camino(326,3.5f),"agua-refugio"));raciones.Add(Racion(2,Camino(333,-6),"charki-desvio"));raciones.Add(Racion(0,Camino(350,-3.3f),"coca-ladera"));m.recursos=raciones.ToArray();
        m.viento=Sonido("Viento de la ladera",Camino(180),"Assets/Audio/Nivel01/Ambiente/Viento-arboles-aves.mp3",.055f,false);
        m.agua=Sonido("Arroyo suave",new Vector3(NivelTresChaski.Centro(281),-.4f,281),"Assets/Audio/Nivel02/Arroyo.mp3",.14f,true);
        var sol=new GameObject("Sol del valle").AddComponent<Light>();sol.type=LightType.Directional;sol.transform.rotation=Quaternion.Euler(42,-38,0);sol.intensity=1.15f;sol.shadows=LightShadows.Soft;sol.color=new Color(1,.95f,.85f);
        RenderSettings.sun=sol;RenderSettings.skybox=Recurso<Material>("Assets/Niveles/Valle/Cielo.mat");RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.56f,.66f,.77f);RenderSettings.ambientEquatorColor=new Color(.44f,.45f,.37f);RenderSettings.ambientGroundColor=new Color(.23f,.25f,.2f);RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogColor=new Color(.64f,.72f,.78f);RenderSettings.fogDensity=.0035f;
        EditorSceneManager.SaveScene(escena,Ruta);AssetDatabase.SaveAssets();
        EditorBuildSettings.scenes=EditorBuildSettings.scenes.Where(s=>s.path!=Ruta).Concat(new[]{new EditorBuildSettingsScene(Ruta,true)}).ToArray();
        Validar(escena);if(anterior.IsValid())SceneManager.SetActiveScene(anterior);EditorSceneManager.CloseScene(escena,true);
        Debug.Log("NIVEL03_CREADO: valle propio, cuatro controles, cinco raciones, viento, cruce de piedras, misión y manta final.");
    }
    static void CrearTerreno()
    {
        const int n=513,a=512;var d=new TerrainData{heightmapResolution=n,alphamapResolution=a,size=new Vector3(220,65,550),terrainLayers=Recurso<TerrainData>("Assets/Niveles/Valle/ValleTerreno.asset").terrainLayers};var h=new float[n,n];
        for(int z=0;z<n;z++)for(int x=0;x<n;x++)h[z,x]=(Altura(-110+x*220f/(n-1),-20+z*550f/(n-1))+10)/65;
        d.SetHeights(0,0,h);var alpha=new float[a,a,d.terrainLayers.Length];
        for(int z=0;z<a;z++)for(int x=0;x<a;x++)
        {float xx=-110+x*220f/(a-1),zz=-20+z*550f/(a-1),dist=Mathf.Abs(xx-NivelTresChaski.Centro(zz));float tierra=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(2.2f,3.4f,dist));float piedra=(1-tierra)*Mathf.SmoothStep(0,1,Mathf.InverseLerp(25,50,d.GetSteepness(x/(a-1f),z/(a-1f))));alpha[z,x,0]=1-tierra-piedra;alpha[z,x,1]=tierra;alpha[z,x,2]=piedra;}
        d.SetAlphamaps(0,0,alpha);AssetDatabase.CreateAsset(d,Dir+"ValleVientosTerreno.asset");var o=Terrain.CreateTerrainGameObject(d);o.name="Terreno · valle de los vientos";o.transform.SetParent(mundo);o.transform.position=new Vector3(-110,-10,-20);terreno=o.GetComponent<Terrain>();terreno.materialTemplate=Recurso<Material>("Assets/Niveles/Valle/Terreno.mat");terreno.drawInstanced=true;terreno.heightmapPixelError=8;terreno.detailObjectDistance=0;
    }
    static void CrearSendero()
    {
        var v=new List<Vector3>();var uv=new List<Vector2>();var idx=new List<int>();
        for(int i=0;i<=1020;i++)for(int k=0;k<7;k++)
        {float z=-4+i*.5f,u=k/6f,x=NivelTresChaski.Centro(z)+(u*2-1)*2.6f;v.Add(new Vector3(x,Altura(x,z)+.025f,z));uv.Add(new Vector2(u*1.4f,z/3));if(i==0||k==6||z>273&&z<289)continue;int a=(i-1)*7+k,b=i*7+k;idx.AddRange(new[]{a,b,a+1,a+1,b,b+1});}
        CrearMesh("Sendero · tierra compactada",v,idx,uv,Recurso<Material>("Assets/Niveles/SegundoRecorrido/CaminoTierra.mat"),"Camino.asset",false);
    }
    static void CrearArroyo()
    {
        var v=new List<Vector3>();var uv=new List<Vector2>();var idx=new List<int>();var colors=new List<Color>();
        for(int i=0;i<=380;i++)for(int k=0;k<9;k++)
        {float x=-110+i*.5f,u=k/8f;float ancho=3.25f*Mathf.SmoothStep(0,1,Mathf.Clamp01((80-x)/7));v.Add(new Vector3(x,NivelTresChaski.AguaY(x),NivelTresChaski.CentroAgua(x)+(u*2-1)*ancho));uv.Add(new Vector2(u,x*.2f));colors.Add(new Color(Mathf.Sin(u*Mathf.PI),0,0,1));if(i==0||k==8)continue;int a=(i-1)*9+k,b=i*9+k;idx.AddRange(new[]{a,a+1,b,a+1,b+1,b});}
        var o=CrearMesh("Arroyo · naciente del cerro",v,idx,uv,Recurso<Material>("Assets/Niveles/SegundoRecorrido/AguaSierra.mat"),"Arroyo.asset",false);var mesh=o.GetComponent<MeshFilter>().sharedMesh;mesh.SetColors(colors);EditorUtility.SetDirty(mesh);
        for(int i=0;i<48;i++){float x=-85+i*3.3f;if(Mathf.Abs(x-NivelTresChaski.Centro(281))<5)continue;float z=NivelTresChaski.CentroAgua(x)+(i%2==0?-1:1)*4.8f;Roca(new Vector3(x,Altura(x,z),z),new Vector3(1.1f,.6f,1.3f),true);}
    }
    static void CrearCruce()
    {
        for(int i=0;i<9;i++)
        {
            float z=273+i*2f;float x=NivelTresChaski.Centro(z);float y=i==0||i==8?.8f:.62f+Mathf.Sin(i*.8f)*.12f;
            // Wide flat supports: visible stone and exact collision share the same top surface.
            var p=new Vector3(x,y-.35f,z);Roca(new Vector3(x,y-.7f,z),new Vector3(2.8f,.7f,1.8f),false);
            Bloque("Piedra del arroyo · superficie de apoyo",p,new Vector3(2.5f,.7f,1.65f),roca);
        }
    }
    static void CrearArboles()
    {
        for(int i=0;i<90;i++)
        {
            float z=12+i*5.3f;if(z<95||Mathf.Abs(z-281)<25||z>467)continue;
            float lado=(i%2==0?-1:1)*(11+i%5*5);float x=NivelTresChaski.Centro(z)+lado;
            if(lado>0&&Mathf.Abs(z-325)<14)continue;
            if(Mathf.Abs(lado)<13&&new[]{112f,219,245,312,358,466}.Any(a=>Mathf.Abs(z-a)<9))continue;
            var p=new Vector3(x,Altura(x,z),z);float s=.65f+i%4*.15f;
            var tronco=Malla("Queñua · tronco",Recurso<Mesh>("Assets/Niveles/Valle/Tronco"+i%2+".asset"),Recurso<Material>("Assets/Niveles/Valle/Corteza.mat"),p,Vector3.one*s);
            var c=tronco.AddComponent<CapsuleCollider>();c.center=Vector3.up*1.3f;c.height=2.6f;c.radius=.18f;
            var hojas=Malla("Queñua · hojas",Recurso<Mesh>("Assets/Niveles/Valle/Copa"+i%2+".asset"),Recurso<Material>("Assets/Niveles/Valle/Follaje.mat"),p,Vector3.one*s);hojas.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.TwoSided;
            if(i%3==0)Roca(new Vector3(x+3,Altura(x+3,z),z),new Vector3(1.6f,.8f,1.5f),true);
        }
    }
    static void CrearCultivos()
    {
        var hoja=Recurso<Mesh>("Assets/Niveles/PrimeraMision/HojasMaiz.asset");
        for(int grupo=0;grupo<2;grupo++)for(int fila=0;fila<3;fila++)
        {
            float z=30+grupo*27,x=NivelTresChaski.Centro(z)+10+fila*3,y=NivelTresChaski.Base(z)+fila*.35f;
            Bloque("Andén · piedra",new Vector3(x,y+.2f,z),new Vector3(2.8f,.4f,18),Recurso<Material>("Assets/Niveles/SegundoRecorrido/PiedraViviendas.mat"));
            var comb=new List<CombineInstance>();for(int i=0;i<15;i++)foreach(float lado in new[]{-.65f,.65f})comb.Add(new CombineInstance{mesh=hoja,transform=Matrix4x4.TRS(new Vector3(x+lado,y+.45f,z-7+i),Quaternion.Euler(0,i*57,0),Vector3.one*1.2f)});
            var mesh=new Mesh{name="Maíz · hojas del valle"};mesh.CombineMeshes(comb.ToArray());AssetDatabase.CreateAsset(mesh,Dir+"Maiz"+grupo+fila+".asset");Malla("Cultivos · sin árboles",mesh,Recurso<Material>("Assets/Niveles/PrimeraMision/HojaMaiz.mat"),Vector3.zero,Vector3.one);
        }
    }
    static void CrearLimites()
    {
        foreach(int lado in new[]{-1,1})for(int z=-8;z<512;z+=3)
        {var a=Camino(z,lado*32.6f);var b=Camino(z+3,lado*32.6f);a.y=b.y=25;var o=new GameObject("Barrera · 30 m del borde");o.transform.SetParent(mundo);o.transform.SetPositionAndRotation((a+b)*.5f,Quaternion.LookRotation(b-a));o.AddComponent<BoxCollider>().size=new Vector3(.6f,90,Vector3.Distance(a,b)+.3f);}
        Bloque("Límite de salida",new Vector3(0,25,-8),new Vector3(150,90,1),null);Bloque("Límite de posta",new Vector3(0,25,510),new Vector3(150,90,1),null);
    }
    static RecursoValleVientos Racion(int tipo,Vector3 p,string id)
    {
        var o=new GameObject("Ración · "+new[]{"coca","cancha","ch’arki","agua"}[tipo]);o.transform.SetParent(mundo);o.transform.position=p+Vector3.up*.45f;
        var r=o.AddComponent<RecursoValleVientos>();r.tipo=tipo;r.identificador=id;
        var mat=new Material(Shader.Find("Standard")){color=tipo==0?new Color(.2f,.34f,.1f):tipo==1?new Color(.75f,.55f,.22f):tipo==2?new Color(.32f,.13f,.08f):new Color(.5f,.26f,.13f)};AssetDatabase.CreateAsset(mat,Dir+id+".mat");
        var cuenco=GameObject.CreatePrimitive(PrimitiveType.Cylinder);cuenco.name="Recipiente · "+r.Nombre;cuenco.transform.SetParent(o.transform,false);cuenco.transform.localScale=new Vector3(.38f,.10f,.38f);cuenco.GetComponent<Renderer>().sharedMaterial=mat;Object.DestroyImmediate(cuenco.GetComponent<Collider>());
        Bloque("Piedra de abastecimiento",p+Vector3.up*.2f,new Vector3(.7f,.4f,.65f),roca);
        for(int i=0;i<7&&tipo!=3;i++)
        {var item=GameObject.CreatePrimitive(PrimitiveType.Sphere);item.name="Ración visible";item.transform.SetParent(o.transform,false);item.transform.localPosition=new Vector3(Mathf.Sin(i*2.4f)*.11f,.11f,Mathf.Cos(i*2.4f)*.11f);item.transform.localScale=tipo==0?new Vector3(.08f,.015f,.13f):tipo==1?Vector3.one*.05f:new Vector3(.13f,.025f,.045f);item.GetComponent<Renderer>().sharedMaterial=mat;Object.DestroyImmediate(item.GetComponent<Collider>());}
        return r;
    }
    static GameObject NPC(GameObject prefab,Vector3 p,string nombre)
    {
        var o=(GameObject)PrefabUtility.InstantiatePrefab(prefab,escena);o.name=nombre;o.transform.position=p+Vector3.up*.06f;o.transform.rotation=Quaternion.Euler(0,180,0);
        foreach(var c in o.GetComponentsInChildren<MonoBehaviour>(true))c.enabled=false;foreach(var c in o.GetComponentsInChildren<Collider>(true))c.enabled=false;foreach(var c in o.GetComponentsInChildren<Cloth>(true))c.enabled=false;
        var col=o.AddComponent<CapsuleCollider>();col.center=Vector3.up;col.height=1.9f;col.radius=.3f;return o;
    }
    static void Cartel(float z,string texto,float lado=-4.7f)
    {
        var raiz=new GameObject("Letrero · "+texto.Split('\n')[0]).transform;raiz.SetParent(mundo);raiz.position=Camino(z,lado);
        Bloque("Poste tallado",raiz.position+Vector3.up*.8f,new Vector3(.16f,1.6f,.16f),madera);
        Bloque("Tabla tallada",raiz.position+Vector3.up*1.8f,new Vector3(3.6f,.9f,.16f),Recurso<Material>("Assets/Niveles/Valle/MaderaLetrerosTallados.mat"));
        var o=new GameObject("Texto tallado");o.transform.SetParent(raiz,false);o.transform.localPosition=new Vector3(0,1.8f,-.095f);var t=o.AddComponent<TextMesh>();t.text=texto;t.font=Resources.Load<Font>("EstiloInca/TituloTradicional");t.fontSize=80;t.characterSize=.035f;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;o.GetComponent<Renderer>().sharedMaterial=Recurso<Material>("Assets/Niveles/Valle/LetraCarteles.mat");o.AddComponent<TextoCartelChasqui>();var b=o.GetComponent<Renderer>().bounds.size;if(b.x>.01f)o.transform.localScale*=Mathf.Min(3.2f/b.x,.68f/b.y);
    }
    static Transform Punto(string nombre,Vector3 p){var o=new GameObject(nombre);SceneManager.MoveGameObjectToScene(o,escena);o.transform.position=p;return o.transform;}
    static GameObject Roca(Vector3 p,Vector3 medida,bool solida)
    {
        var mesh=Recurso<Mesh>("Assets/Niveles/Valle/Roca0.asset");var b=mesh.bounds;var s=new Vector3(medida.x/b.size.x,medida.y/b.size.y,medida.z/b.size.z);
        var o=Malla("Roca · abrigo y paisaje",mesh,roca,p-Vector3.up*(b.min.y*s.y+.03f),s);
        if(solida){float gap=float.PositiveInfinity;foreach(var v in mesh.vertices){var q=o.transform.TransformPoint(v);gap=Mathf.Min(gap,q.y-Altura(q.x,q.z));}o.transform.position-=Vector3.up*(gap+.10f);var c=o.AddComponent<BoxCollider>();c.center=b.center;c.size=b.size*.9f;}return o;
    }
    static GameObject Bloque(string n,Vector3 p,Vector3 tam,Material mat)
    {var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=n;o.transform.SetParent(mundo);o.transform.position=p;o.transform.localScale=tam;if(mat==null)o.GetComponent<Renderer>().enabled=false;else o.GetComponent<Renderer>().sharedMaterial=mat;return o;}
    static GameObject Malla(string n,Mesh mesh,Material mat,Vector3 p,Vector3 s)
    {var o=new GameObject(n);o.transform.SetParent(mundo);o.transform.position=p;o.transform.localScale=s;o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=mat;return o;}
    static GameObject CrearMesh(string n,List<Vector3> v,List<int> idx,List<Vector2> uv,Material mat,string archivo,bool colision)
    {var mesh=new Mesh{name=n};mesh.SetVertices(v);mesh.SetTriangles(idx,0);mesh.SetUVs(0,uv);mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,Dir+archivo);var o=Malla(n,mesh,mat,Vector3.zero,Vector3.one);if(colision)o.AddComponent<MeshCollider>().sharedMesh=mesh;return o;}
    static AudioSource Sonido(string n,Vector3 p,string clip,float volumen,bool espacial)
    {var o=new GameObject("Audio · "+n);SceneManager.MoveGameObjectToScene(o,escena);o.transform.position=p;var a=o.AddComponent<AudioSource>();a.clip=Recurso<AudioClip>(clip);a.loop=true;a.playOnAwake=true;a.volume=volumen;a.spatialBlend=espacial?1:0;a.minDistance=4;a.maxDistance=27;a.dopplerLevel=0;a.rolloffMode=AudioRolloffMode.Linear;return a;}
    [MenuItem("Chasqui/Comprobar nivel 3")]
    public static void Comprobar(){var e=SceneManager.GetSceneByPath(Ruta);if(e.isLoaded)Validar(e);}
    static void Validar(Scene e)
    {
        Physics.SyncTransforms();int puntos=0;var errores=new List<string>();var m=e.GetRootGameObjects().Select(o=>o.GetComponent<NivelTresChaski>()).First(o=>o!=null);
        for(float z=5;z<500;z+=2){if(z>273&&z<289)continue;if(Physics.Raycast(Camino(z)+Vector3.up*20,Vector3.down,30))puntos++;else errores.Add("Sin suelo "+z);}
        for(int i=0;i<9;i++){float z=273+i*2;var hits=Physics.RaycastAll(new Vector3(NivelTresChaski.Centro(z),5,z),Vector3.down,10).Where(h=>h.collider.gameObject.scene==e).OrderBy(h=>h.distance).ToArray();if(hits.Length==0||!hits[0].collider.name.Contains("arroyo"))errores.Add("Apoyo del arroyo "+i);}
        if(m.recursos.Length!=5||m.controles.Length!=4)errores.Add("Distribución de recursos o controles");
        foreach(var r in m.recursos)if(r==null)errores.Add("Recurso sin referencia");
        System.IO.File.WriteAllText("Temp/nivel03_revision.txt",$"Muestras de suelo: {puntos}\nRecursos: {m.recursos.Length}\nControles: {m.controles.Length}\nErrores: {errores.Count}\n"+string.Join("\n",errores));
        Debug.Log("NIVEL03_VALIDACION: "+errores.Count+" errores. "+puntos+" apoyos; 9 piedras de cruce, 5 raciones y 4 controles.");
    }
}
