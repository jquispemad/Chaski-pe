using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class ConstruirPuenteAbismo
{
 public const string Ruta="Assets/Scenes/Campana/Nivel04_PuenteAbismo.unity";const string D="Assets/Niveles/PuenteAbismo/";static Transform mundo;static Terrain terreno;static Scene escena;static Material piedra,madera;
 static T R<T>(string p)where T:Object=>AssetDatabase.LoadAssetAtPath<T>(p);
 static Vector3 RutaP(float z,float lado=0)=>new Vector3(NivelCuatroChaski.Centro(z)+lado,NivelCuatroChaski.Suelo(z),z);
 [InitializeOnLoadMethod]static void Preparar()=>EditorApplication.delayCall+=()=>{if(!EditorApplication.isPlayingOrWillChangePlaymode&&R<SceneAsset>(Ruta)==null)Crear();};
 [MenuItem("Chasqui/Abrir nivel 4 · El puente del abismo")]public static void Abrir(){if(EditorApplication.isPlaying)return;if(R<SceneAsset>(Ruta)==null)Crear();EditorSceneManager.OpenScene(Ruta);EditorSceneManager.playModeStartScene=R<SceneAsset>(Ruta);}
 static float Altura(float x,float z,Terrain origen,Vector3 offset)
 {
  float lateral=Mathf.Abs(x-NivelCuatroChaski.Centro(z));float y=NivelCuatroChaski.Suelo(z)+Mathf.SmoothStep(0,1,Mathf.InverseLerp(5,75,lateral))*(10+23*Mathf.PerlinNoise(x*.021f+31,z*.023f));
  if(z>75&&z<140&&x>NivelCuatroChaski.Centro(z)+3){float caida=Mathf.SmoothStep(0,1,Mathf.InverseLerp(3,12,x-NivelCuatroChaski.Centro(z)));y=Mathf.Lerp(y,-14,caida);}
  float desfiladero=Mathf.SmoothStep(0,1,Mathf.InverseLerp(149,150,z))*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(186,187,z)));y=Mathf.Lerp(y,-22+Mathf.PerlinNoise(x*.07f,3)*.6f,desfiladero);
  float distanciaRio=Mathf.Abs(z-NivelCuatroChaski.CentroRio(x));if(distanciaRio<7){float cauce=distanciaRio<2.7f?-21.15f-.8f*Mathf.Sqrt(Mathf.Clamp01(1-distanciaRio*distanciaRio/7.29f)):Mathf.Lerp(-21.09f,y,Mathf.SmoothStep(0,1,(distanciaRio-2.7f)/4.3f));y=cauce;}
  if(origen!=null&&z<65){var q=new Vector3(x,0,z)-offset;float anterior=origen.SampleHeight(q)+origen.transform.position.y+offset.y;y=Mathf.Lerp(anterior,y,Mathf.SmoothStep(0,1,Mathf.InverseLerp(28,65,z)));}
  return y;
 }
 public static void Crear()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode||R<SceneAsset>(Ruta)!=null)return;
  if(!AssetDatabase.IsValidFolder(D.TrimEnd('/')))AssetDatabase.CreateFolder("Assets/Niveles","PuenteAbismo");
  var anterior=SceneManager.GetActiveScene();var fuente=SceneManager.GetSceneByPath(ConstruirValleVientos.Ruta);bool cerrar=!fuente.isLoaded;if(cerrar)fuente=EditorSceneManager.OpenScene(ConstruirValleVientos.Ruta,OpenSceneMode.Additive);
  var origen=fuente.GetRootGameObjects().Select(o=>o.GetComponent<NivelTresChaski>()).First(o=>o!=null);var tOrigen=fuente.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Terrain>()).First();var offset=new Vector3(-NivelTresChaski.Centro(485),0,-481);
  escena=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(escena);mundo=new GameObject("Nivel 04 · quebrada y sendero de montaña").transform;piedra=R<Material>("Assets/Personajes/Chasqui/MaterialPiedra.mat");madera=R<Material>("Assets/Niveles/Valle/Madera.mat");
  CrearTerreno(tOrigen,offset);CrearSendero();CrearRio();
  var m=new GameObject("Misión 04 · el puente del abismo").AddComponent<NivelCuatroChaski>();
  var prefab=R<GameObject>("Assets/Personajes/Chasqui/ChasquiJugadorRealista.prefab");var jugador=(GameObject)PrefabUtility.InstantiatePrefab(prefab,escena);jugador.name="Wayra · puente del abismo";jugador.transform.position=origen.circulo.position+offset+Vector3.up*.1f;m.jugador=jugador.GetComponent<ChasquiMovimiento>();m.jugador.recorridoLateral=false;
  m.vista=new GameObject("Cámara · cuarto recorrido").AddComponent<Camera>();m.vista.tag="MainCamera";m.vista.fieldOfView=58;m.vista.farClipPlane=420;m.vista.nearClipPlane=.10f;m.vista.gameObject.AddComponent<AudioListener>();m.vista.gameObject.AddComponent<CamaraChasqui>().objetivo=jugador.transform;m.jugador.camara=m.vista.transform;m.vista.transform.position=jugador.transform.position+new Vector3(0,2.3f,-4.5f);m.vista.transform.LookAt(jugador.transform.position+Vector3.up*1.1f);
  EditorUtility.CopySerialized(origen.GetComponent<AudioPrimeraMision>(),m.gameObject.AddComponent<AudioPrimeraMision>());
  var enlace=new GameObject("Enlace compartido · mirador final del nivel 3").transform;SceneManager.MoveGameObjectToScene(enlace.gameObject,escena);enlace.SetParent(mundo);
  var mundo3=fuente.GetRootGameObjects().First(o=>o.name.StartsWith("Nivel 03 · valle")).transform;
  foreach(Transform p in mundo3)
  {
   if(p.GetComponent<Terrain>()!=null||p.name.Contains("Barrera")||p.name.Contains("Límite")||p.name.StartsWith("Sendero")||p.name.StartsWith("Arroyo"))continue;
   if(p.position.z>=453&&p.position.z<=510){var o=Object.Instantiate(p.gameObject);SceneManager.MoveGameObjectToScene(o,escena);o.transform.SetParent(enlace,true);o.transform.position+=offset;}
  }
  var npc=Object.Instantiate(origen.encargado.gameObject);SceneManager.MoveGameObjectToScene(npc,escena);npc.name="Encargado · mismo mirador del nivel 3";npc.transform.SetParent(enlace,true);npc.transform.position+=offset;m.responsable=npc.transform;
  if(cerrar)EditorSceneManager.CloseScene(fuente,true);
  m.puente=CrearPuente();m.puente.GetComponentInChildren<MeshFilter>().gameObject.AddComponent<PuenteVientoChaski>().nivel=m;m.controles=new[]{Punto("Control · mirador inicial",jugador.transform.position),Punto("Control · estribo inicial",RutaP(137)+Vector3.up*.12f),Punto("Control · otra orilla",RutaP(202)+Vector3.up*.12f)};
  for(int i=0;i<12;i++){float z=48+i*4.2f;var p=RutaP(z);Bloque("Peldaño de piedra · descenso",p-Vector3.up*.1f,new Vector3(3,.2f,.8f),piedra);}
  foreach(float z in new[]{88f,111f}){var p=RutaP(z);var roca=Roca(p,new Vector3(2.1f,.45f,1.2f));roca.name="Piedra de paso · salto corto";}
  CrearPaisaje();CrearLimites();
  Cartel(34,-3.5f,"PASO DE LA QUEBRADA\nEL PUENTE ESTÁ MÁS ADELANTE");Cartel(130,-3.5f,"PUENTE DE ICHU\nAVANZA CON CALMA · C PARA BAJAR");Cartel(207,3.3f,"SENDERO DE LA MONTAÑA\nBUSCA EL RASTRO DEL CORREDOR");
  m.registro=Punto("Anclajes · registro opcional",RutaP(208,-3.2f));Cartel(208,-3.5f,"ANCLAJES DEL PUENTE\nE · OBSERVAR");
  m.recursos=new[]{Racion(1,RutaP(14,-2.8f),"cancha-mirador"),Racion(3,RutaP(139,-2.5f),"agua-estribo"),Racion(2,RutaP(219,3),"charki-orilla")};
  m.hallazgo=Punto("Pertenencias del corredor · repisa segura",RutaP(368,-2.5f)+Vector3.up*.02f);
  var conjunto=new GameObject("Hallazgo · chuspa, sandalia y faja").transform;conjunto.SetParent(mundo);conjunto.position=m.hallazgo.position;
  var chuspa=GameObject.CreatePrimitive(PrimitiveType.Sphere);chuspa.name="Chuspa gastada";chuspa.transform.SetParent(conjunto,false);chuspa.transform.localPosition=new Vector3(0,.13f,0);chuspa.transform.localScale=new Vector3(.42f,.23f,.31f);chuspa.GetComponent<Renderer>().sharedMaterial=R<Material>("Assets/Personajes/Chasqui/Materiales/Tela_13.mat")??madera;Object.DestroyImmediate(chuspa.GetComponent<Collider>());
  var suela=GameObject.CreatePrimitive(PrimitiveType.Cube);suela.name="Sandalia abandonada";suela.transform.SetParent(conjunto,false);suela.transform.localPosition=new Vector3(.4f,.04f,.18f);suela.transform.localRotation=Quaternion.Euler(0,37,0);suela.transform.localScale=new Vector3(.13f,.045f,.28f);suela.GetComponent<Renderer>().sharedMaterial=madera;Object.DestroyImmediate(suela.GetComponent<Collider>());
  var tela=new GameObject("Tejido encontrado");tela.transform.SetParent(conjunto,false);tela.transform.localPosition=new Vector3(-.34f,.05f,.1f);var tejido=new Mesh{name="Faja doblada"};tejido.vertices=new[]{new Vector3(-.14f,0,-.04f),new Vector3(.14f,0,-.04f),new Vector3(-.13f,.02f,.05f),new Vector3(.13f,.02f,.05f)};tejido.triangles=new[]{0,2,1,1,2,3,1,2,0,3,2,1};tejido.uv=new[]{Vector2.zero,Vector2.right,Vector2.up,Vector2.one};tejido.RecalculateNormals();AssetDatabase.CreateAsset(tejido,D+"FajaEncontrada.asset");tela.AddComponent<MeshFilter>().sharedMesh=tejido;tela.AddComponent<MeshRenderer>().sharedMaterial=R<Material>("Assets/Personajes/Chasqui/Materiales/Tela_07.mat")??madera;m.tejido=tela.transform;
  Roca(RutaP(373,-5),new Vector3(3.4f,2.4f,4));
  m.rio=Sonido("Río al fondo del abismo",new Vector3(0,-19,168),"Assets/Audio/Nivel02/Arroyo.mp3",.15f,70);m.viento=Sonido("Viento entre los estribos",new Vector3(0,0,168),"Assets/Audio/Nivel01/Ambiente/Viento-arboles-aves.mp3",.04f,120);
  var sol=new GameObject("Sol · tarde en la quebrada").AddComponent<Light>();sol.type=LightType.Directional;sol.transform.rotation=Quaternion.Euler(42,-35,0);sol.color=new Color(1,.94f,.84f);sol.intensity=1.1f;sol.shadows=LightShadows.Soft;
  RenderSettings.sun=sol;RenderSettings.skybox=R<Material>("Assets/Niveles/Valle/Cielo.mat");RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.54f,.65f,.77f);RenderSettings.ambientEquatorColor=new Color(.44f,.45f,.4f);RenderSettings.ambientGroundColor=new Color(.21f,.24f,.24f);RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.0035f;RenderSettings.fogColor=new Color(.62f,.70f,.77f);
  EditorSceneManager.SaveScene(escena,Ruta);AssetDatabase.SaveAssets();EditorBuildSettings.scenes=EditorBuildSettings.scenes.Where(o=>o.path!=Ruta).Concat(new[]{new EditorBuildSettingsScene(Ruta,true)}).ToArray();Validar(escena);if(anterior.IsValid())SceneManager.SetActiveScene(anterior);EditorSceneManager.CloseScene(escena,true);Debug.Log("NIVEL04_CREADO: quebrada, puente de ichu de 36 m, mirador compartido, controles y hallazgo final.");
 }
 static void CrearTerreno(Terrain fuente,Vector3 offset)
 {
  const int n=513,a=512;var d=new TerrainData{name="Quebrada del puente",heightmapResolution=n,alphamapResolution=a,size=new Vector3(240,90,450),terrainLayers=fuente.terrainData.terrainLayers};var h=new float[n,n];for(int z=0;z<n;z++)for(int x=0;x<n;x++)h[z,x]=(Altura(-120+x*240f/(n-1),-25+z*450f/(n-1),fuente,offset)+30)/90;d.SetHeights(0,0,h);
  var alpha=new float[a,a,d.terrainLayers.Length];for(int z=0;z<a;z++)for(int x=0;x<a;x++){float xx=-120+x*240f/(a-1),zz=-25+z*450f/(a-1),dist=Mathf.Abs(xx-NivelCuatroChaski.Centro(zz));float camino=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.6f,2.7f,dist));float pendiente=d.GetSteepness(x/(a-1f),z/(a-1f));float roca=(1-camino)*Mathf.SmoothStep(0,1,Mathf.InverseLerp(16,38,pendiente));alpha[z,x,0]=1-camino-roca;alpha[z,x,1]=camino;alpha[z,x,2]=roca;}
  d.SetAlphamaps(0,0,alpha);AssetDatabase.CreateAsset(d,D+"TerrenoQuebrada.asset");var o=Terrain.CreateTerrainGameObject(d);o.name="Terreno · laderas y barranco mortal";o.transform.SetParent(mundo);o.transform.position=new Vector3(-120,-30,-25);terreno=o.GetComponent<Terrain>();terreno.materialTemplate=R<Material>("Assets/Niveles/Valle/Terreno.mat");terreno.heightmapPixelError=3;terreno.drawInstanced=true;terreno.detailObjectDistance=0;
 }
 static void CrearSendero()
 {
  var v=new List<Vector3>();var idx=new List<int>();var uv=new List<Vector2>();for(int i=0;i<=790;i++)for(int j=0;j<7;j++){float z=-5+i*.5f,u=j/6f,x=NivelCuatroChaski.Centro(z)+(u*2-1)*1.6f;var p=new Vector3(x,0,z);p.y=terreno.SampleHeight(p)+terreno.transform.position.y+.023f;v.Add(p);uv.Add(new Vector2(u,z/3));if(i==0||j==6||z>149.5f&&z<186.5f)continue;int a=(i-1)*7+j,b=i*7+j;idx.AddRange(new[]{a,b,a+1,a+1,b,b+1});}
  MeshObjeto("Sendero · piedra y tierra",v,idx,uv,R<Material>("Assets/Niveles/SegundoRecorrido/CaminoTierra.mat"),"Sendero.asset",false);
 }
 static void CrearRio()
 {
  var v=new List<Vector3>();var idx=new List<int>();var uv=new List<Vector2>();var colors=new List<Color>();for(int i=0;i<=480;i++)for(int j=0;j<9;j++){float x=-120+i*.5f,u=j/8f;v.Add(new Vector3(x,-21.15f,NivelCuatroChaski.CentroRio(x)+(u*2-1)*2.7f));uv.Add(new Vector2(u,x*.23f));colors.Add(new Color(Mathf.Sin(u*Mathf.PI),0,0,1));if(i==0||j==8)continue;int a=(i-1)*9+j,b=i*9+j;idx.AddRange(new[]{a,a+1,b,a+1,b+1,b});}
  var o=MeshObjeto("Río · fondo de la quebrada",v,idx,uv,R<Material>("Assets/Niveles/SegundoRecorrido/AguaSierra.mat"),"RioAbismo.asset",false);o.GetComponent<MeshFilter>().sharedMesh.SetColors(colors);o.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
 }
 static Transform CrearPuente()
 {
  var raiz=new GameObject("Puente de ichu · cruce de 36 metros").transform;raiz.SetParent(mundo);var visual=R<Mesh>("Assets/Niveles/Valle/PuenteIchuVisual.asset");var col=R<Mesh>("Assets/Niveles/Valle/PuenteIchuColision.asset");
  foreach(bool collision in new[]{false,true})
  {
   var mesh=Object.Instantiate(collision?col:visual);mesh.name=collision?"Apoyo curvo continuo de ichu":"Cuerdas y tejido del abismo";var v=mesh.vertices;for(int i=0;i<v.Length;i++){float z=v[i].z;float curva=-.65f*Mathf.Sin(Mathf.Clamp01(z/26)*Mathf.PI);v[i]=new Vector3(v[i].x*.57f,-2.05f+v[i].y+curva*.8f,150+z*36/26);}mesh.vertices=v;mesh.RecalculateBounds();mesh.RecalculateNormals();AssetDatabase.CreateAsset(mesh,D+(collision?"PuenteApoyo.asset":"PuenteTejido.asset"));var o=new GameObject(collision?"Puente ichu · apoyo continuo":"Ichu trenzado · piso y pasamanos");o.transform.SetParent(raiz,false);if(collision)o.AddComponent<MeshCollider>().sharedMesh=mesh;else{o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=R<Material>("Assets/Niveles/Valle/SogasIchu.mat");}}
  foreach(int lado in new[]{-1,1})for(int i=0;i<72;i++){float z=150+i*.5f,y=NivelCuatroChaski.PisoPuente(z+.25f);var o=Bloque("Pasamanos de ichu · colisión",new Vector3(lado*1.02f,y+.65f,z+.25f),new Vector3(.12f,1.22f,.53f),null);o.transform.SetParent(raiz,true);}
  foreach(float z in new[]{149.5f,186.5f})foreach(int lado in new[]{-1,1})Bloque("Estribo de piedra · anclaje",new Vector3(lado*1.65f,-1.3f,z),new Vector3(.9f,1.6f,1.4f),piedra);
  foreach(float z in new[]{149f,187f})Bloque("Acceso al puente · piedra firme",new Vector3(0,-2.15f,z),new Vector3(2.4f,.3f,2),piedra);
  return raiz;
 }
 static void CrearPaisaje()
 {
  for(int i=0;i<90;i++){float z=39+i*3.8f;if(z>141&&z<196)continue;float lado=(i%2==0?-1:1)*(5.5f+i%4*3.7f);var p=RutaP(z,lado);if(z>75&&z<140&&lado>3)continue;p.y=terreno.SampleHeight(p)+terreno.transform.position.y;Roca(p,new Vector3(1.4f+i%4*.7f,1.3f+i%3*.8f,2+i%2));}
  for(int i=0;i<28;i++){float x=-108+i*8;foreach(float z in new[]{146f,191f}){var p=new Vector3(x,0,z);p.y=terreno.SampleHeight(p)+terreno.transform.position.y;Roca(p,new Vector3(5,5+i%4,5));}}
  for(int i=0;i<28;i++){float z=65+i*10.5f;if(z>135&&z<210)continue;float x=NivelCuatroChaski.Centro(z)-12-i%3*5;var p=new Vector3(x,0,z);p.y=terreno.SampleHeight(p)+terreno.transform.position.y;var tronco=Malla("Queñua · tronco",R<Mesh>("Assets/Niveles/Valle/Tronco"+i%2+".asset"),R<Material>("Assets/Niveles/Valle/Corteza.mat"),p,Vector3.one*.7f);var c=tronco.AddComponent<CapsuleCollider>();c.center=Vector3.up*1.3f;c.height=2.6f;c.radius=.18f;var hoja=Malla("Queñua · hojas",R<Mesh>("Assets/Niveles/Valle/Copa"+i%2+".asset"),R<Material>("Assets/Niveles/Valle/Follaje.mat"),p,Vector3.one*.7f);hoja.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.TwoSided;}
 }
 static void CrearLimites(){foreach(float z in new[]{-14f,400f})Bloque("Límite del recorrido",new Vector3(0,20,z),new Vector3(240,120,.5f),null);foreach(float x in new[]{-116f,116f})Bloque("Límite de la quebrada",new Vector3(x,10,190),new Vector3(.5f,120,450),null);}
 static RecursoValleVientos Racion(int tipo,Vector3 p,string id)
 {
  var o=new GameObject("Provisión · "+id);o.transform.SetParent(mundo);o.transform.position=p+Vector3.up*.45f;var r=o.AddComponent<RecursoValleVientos>();r.tipo=tipo;r.identificador=id;Bloque("Piedra de abastecimiento",p+Vector3.up*.2f,new Vector3(.65f,.4f,.65f),piedra);var v=AspectoRacionChaski.Crear(tipo,false);SceneManager.MoveGameObjectToScene(v,escena);v.transform.SetParent(o.transform,false);v.transform.localScale=Vector3.one*1.8f;var vistos=new HashSet<Object>();int k=0;foreach(var f in v.GetComponentsInChildren<MeshFilter>())if(string.IsNullOrEmpty(AssetDatabase.GetAssetPath(f.sharedMesh))&&vistos.Add(f.sharedMesh))AssetDatabase.CreateAsset(f.sharedMesh,D+id+(k++)+".asset");foreach(var f in v.GetComponentsInChildren<Renderer>())foreach(var a in f.sharedMaterials)if(string.IsNullOrEmpty(AssetDatabase.GetAssetPath(a))&&vistos.Add(a))AssetDatabase.CreateAsset(a,D+id+(k++)+".mat");Object.DestroyImmediate(v.GetComponent<AspectoRacionChaski>());return r;
 }
 static void Cartel(float z,float lado,string text){var root=new GameObject("Letrero · "+text.Split('\n')[0]).transform;root.SetParent(mundo);root.position=RutaP(z,lado);Bloque("Poste tallado",root.position+Vector3.up*.8f,new Vector3(.16f,1.6f,.16f),madera).transform.SetParent(root,true);Bloque("Tabla tallada",root.position+Vector3.up*1.8f,new Vector3(3.8f,.95f,.17f),R<Material>("Assets/Niveles/Valle/MaderaLetrerosTallados.mat")).transform.SetParent(root,true);var o=new GameObject("Inscripción legible");o.transform.SetParent(root,false);o.transform.localPosition=new Vector3(0,1.8f,-.10f);var t=o.AddComponent<TextMesh>();t.text=text;t.font=Resources.Load<Font>("EstiloInca/TituloTradicional");t.fontSize=80;t.characterSize=.033f;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;o.GetComponent<Renderer>().sharedMaterial=R<Material>("Assets/Niveles/Valle/LetraCarteles.mat");o.AddComponent<TextoCartelChasqui>();}
 static Transform Punto(string n,Vector3 p){var o=new GameObject(n);SceneManager.MoveGameObjectToScene(o,escena);o.transform.position=p;return o.transform;}
 static AudioSource Sonido(string n,Vector3 p,string archivo,float volumen,float alcance){var o=new GameObject("Audio · "+n);SceneManager.MoveGameObjectToScene(o,escena);o.transform.position=p;var a=o.AddComponent<AudioSource>();a.clip=R<AudioClip>(archivo);a.loop=true;a.playOnAwake=true;a.volume=volumen;a.spatialBlend=1;a.minDistance=5;a.maxDistance=alcance;a.dopplerLevel=0;a.rolloffMode=AudioRolloffMode.Linear;return a;}
 static GameObject Roca(Vector3 p,Vector3 s){var mesh=R<Mesh>("Assets/Niveles/Valle/Roca0.asset");var b=mesh.bounds;var o=Malla("Roca asentada · ladera",mesh,piedra,p,new Vector3(s.x/b.size.x,s.y/b.size.y,s.z/b.size.z));float suelo=float.PositiveInfinity;for(int i=-1;i<=1;i++)for(int j=-1;j<=1;j++){var q=p+new Vector3(i*s.x*.4f,0,j*s.z*.4f);suelo=Mathf.Min(suelo,terreno.SampleHeight(q)+terreno.transform.position.y);}o.transform.position=new Vector3(p.x,suelo-b.min.y*o.transform.localScale.y-.12f,p.z);var c=o.AddComponent<BoxCollider>();c.center=b.center;c.size=b.size*.9f;return o;}
 static GameObject Bloque(string n,Vector3 p,Vector3 s,Material m){var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=n;o.transform.SetParent(mundo);o.transform.position=p;o.transform.localScale=s;if(m==null)o.GetComponent<Renderer>().enabled=false;else o.GetComponent<Renderer>().sharedMaterial=m;return o;}
 static GameObject Bloque(string n,Transform padre,Vector3 p,Vector3 s,Material m){var o=Bloque(n,p,s,m);o.transform.SetParent(padre,true);return o;}
 static GameObject Malla(string n,Mesh mesh,Material mat,Vector3 p,Vector3 s){var o=new GameObject(n);o.transform.SetParent(mundo);o.transform.position=p;o.transform.localScale=s;o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=mat;return o;}
 static GameObject MeshObjeto(string n,List<Vector3> v,List<int> idx,List<Vector2> uv,Material mat,string archivo,bool colision){var mesh=new Mesh{name=n,indexFormat=IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetTriangles(idx,0);mesh.SetUVs(0,uv);mesh.RecalculateBounds();mesh.RecalculateNormals();AssetDatabase.CreateAsset(mesh,D+archivo);var o=Malla(n,mesh,mat,Vector3.zero,Vector3.one);if(colision)o.AddComponent<MeshCollider>().sharedMesh=mesh;return o;}
 [MenuItem("Chasqui/Comprobar nivel 4")]public static void Comprobar(){var e=SceneManager.GetSceneByPath(Ruta);if(e.isLoaded)Validar(e);}
 static void Validar(Scene e)
 {
  Physics.SyncTransforms();int fallos=0,apoyos=0;var m=e.GetRootGameObjects().Select(o=>o.GetComponent<NivelCuatroChaski>()).First(o=>o!=null);
  for(int i=0;i<=144;i++){float z=150+i*.25f;var hits=Physics.RaycastAll(new Vector3(0,10,z),Vector3.down,50).Where(h=>h.collider.gameObject.scene==e).OrderBy(h=>h.distance).ToArray();if(hits.Length>0&&hits[0].collider.name.Contains("ichu"))apoyos++;else fallos++;}
  if(m.recursos.Length!=3||m.controles.Length!=3||m.hallazgo==null)fallos++;System.IO.File.WriteAllText("Temp/nivel04_geometria.txt",$"Apoyos del puente: {apoyos}/145\nErrores: {fallos}\nControles: {m.controles.Length}\nRecursos: {m.recursos.Length}\n");Debug.Log("NIVEL04_GEOMETRIA: "+apoyos+"/145 apoyos del puente; "+fallos+" errores.");
 }
}
