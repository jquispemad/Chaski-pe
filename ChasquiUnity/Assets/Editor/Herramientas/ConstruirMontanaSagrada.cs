using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ConstruirMontanaSagrada
{
 public const string Ruta="Assets/Scenes/Campana/Nivel05_MontanaSagrada.unity";
 [MenuItem("Chasqui/Construir nivel 5 · La montaña sagrada")]
 [InitializeOnLoadMethod]static void Preparar()=>EditorApplication.delayCall+=()=>{if(!EditorApplication.isPlayingOrWillChangePlaymode&&AssetDatabase.LoadAssetAtPath<SceneAsset>(Ruta)==null)Crear();};
 public static void Crear()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode)return;
  if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Ruta)==null)
  {
   AssetDatabase.CopyAsset(ConstruirValleVientos.Ruta,Ruta);
   var e=EditorSceneManager.OpenScene(Ruta);
   var previo=Object.FindFirstObjectByType<NivelTresChaski>();
   var raiz=previo.gameObject;var nivel=raiz.AddComponent<NivelCincoChaski>();
   foreach(var campo in typeof(NivelTresChaski).GetFields(System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Instance)){var destino=typeof(NivelCincoChaski).GetField(campo.Name);if(destino!=null)destino.SetValue(nivel,campo.GetValue(previo));}
   Object.DestroyImmediate(previo);
   raiz.name="Misión 05 · La montaña sagrada";
   foreach(var o in e.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Transform>()).ToArray())
   {
    if(o==null)continue;
    if(o.name.StartsWith("Arroyo")||o.name.StartsWith("Río")||o.name.StartsWith("Andén")||o.name.StartsWith("Cultivos"))Object.DestroyImmediate(o.gameObject);
   }
   ConstruirRelieve(e,nivel);
   nivel.encargado.name="Viajero herido · encuentro del tambo";
   nivel.jugador.name="Wayra · ascenso a la montaña";
   RenderSettings.fog=true;RenderSettings.fogDensity=.008f;RenderSettings.fogColor=new Color(.65f,.70f,.74f);
   EditorSceneManager.SaveScene(e);
   EditorBuildSettings.scenes=EditorBuildSettings.scenes.Where(o=>o.path!=Ruta).Concat(new[]{new EditorBuildSettingsScene(Ruta,true)}).ToArray();
  }
  EditorSceneManager.OpenScene(Ruta);EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(Ruta);AssetDatabase.SaveAssets();
 }
 static void ConstruirRelieve(Scene e,NivelCincoChaski nivel)
 {
  const string dir="Assets/Niveles/MontanaSagrada/";
  if(!AssetDatabase.IsValidFolder(dir.TrimEnd('/')))AssetDatabase.CreateFolder("Assets/Niveles","MontanaSagrada");
  var terreno=e.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Terrain>()).First();
  var viejo=terreno.terrainData;var nuevo=Object.Instantiate(viejo);nuevo.name="Ascenso de montaña";AssetDatabase.CreateAsset(nuevo,dir+"Terreno.asset");
  float Altura(float x,float z){float distancia=Mathf.Abs(x-NivelCincoChaski.Centro(z));return 3+Mathf.Max(0,z)*.055f+Mathf.SmoothStep(0,1,Mathf.InverseLerp(3,65,distancia))*(12+18*Mathf.PerlinNoise(x*.026f+7,z*.023f));}
  float Delta(Vector3 p)=>Altura(p.x,p.z)-(terreno.SampleHeight(p)+terreno.transform.position.y);
  var objetos=e.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Transform>()).ToArray();
  foreach(var o in objetos)
  {
   if(o==null||o==terreno.transform||o.IsChildOf(nivel.jugador.transform)||o==nivel.jugador.transform)continue;
   if(o.GetComponent<MeshFilter>() is MeshFilter f && f!=null && f.sharedMesh!=null && o.position.sqrMagnitude<.01f && f.sharedMesh.bounds.size.z>60)
   {
    var copia=Object.Instantiate(f.sharedMesh);var vs=copia.vertices;for(int i=0;i<vs.Length;i++){var q=o.TransformPoint(vs[i]);q.y+=Delta(q);vs[i]=o.InverseTransformPoint(q);}copia.vertices=vs;copia.RecalculateNormals();copia.RecalculateBounds();AssetDatabase.CreateAsset(copia,AssetDatabase.GenerateUniqueAssetPath(dir+"Paisaje.asset"));f.sharedMesh=copia;if(o.GetComponent<MeshCollider>() is MeshCollider col && col!=null)col.sharedMesh=copia;
   }
  }
  // Mover conjuntos completos una sola vez, conservando puertas, techos y colliders.
  foreach(var root in e.GetRootGameObjects())
  {
   if(root==nivel.gameObject||root==terreno.gameObject||root==nivel.vista.gameObject)continue;
   if(root.GetComponentInChildren<Terrain>()!=null){foreach(Transform hijo in root.transform){if(hijo==terreno.transform||hijo.position.sqrMagnitude<.01f)continue;var p=hijo.position;p.y+=Delta(p);hijo.position=p;}}
   else {var p=root.transform.position;p.y+=Delta(p);root.transform.position=p;}
  }
  int n=nuevo.heightmapResolution;var h=new float[n,n];for(int z=0;z<n;z++)for(int x=0;x<n;x++){float xx=terreno.transform.position.x+x*nuevo.size.x/(n-1),zz=terreno.transform.position.z+z*nuevo.size.z/(n-1);h[z,x]=Mathf.Clamp01((Altura(xx,zz)-terreno.transform.position.y)/nuevo.size.y);}nuevo.SetHeights(0,0,h);terreno.terrainData=nuevo;terreno.GetComponent<TerrainCollider>().terrainData=nuevo;
  var inicio=nivel.jugador.transform.position;inicio.y=Altura(inicio.x,inicio.z)+.12f;nivel.jugador.transform.position=inicio;
  var zs=new[]{85f,145f,200f,250f,310f,365f,410f};var tipos=new[]{1,3,2,0,1,3,2};var anteriores=nivel.recursos.ToArray();var recursos=new RecursoValleVientos[7];
  for(int i=0;i<7;i++){var modelo=anteriores.First(o=>o.tipo==tipos[i]);var o=Object.Instantiate(modelo.gameObject,modelo.transform.parent);var r=o.GetComponent<RecursoValleVientos>();r.identificador="montana-racion-"+i;float x=NivelCincoChaski.Centro(zs[i])+(i%2==0?-2.1f:2.1f);o.transform.position=new Vector3(x,Altura(x,zs[i])+.12f,zs[i]);recursos[i]=r;}
  foreach(var r in anteriores)Object.DestroyImmediate(r.gameObject);nivel.recursos=recursos;
  nivel.agua.volume=0;EditorUtility.SetDirty(nuevo);
 }

}
