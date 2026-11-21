using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class AsentarYAbastecerMontana
{
 [InitializeOnLoadMethod]static void Preparar()=>EditorApplication.delayCall+=Aplicar;
 static void Aplicar()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode||AssetDatabase.LoadAssetAtPath<SceneAsset>(ConstruirMontanaSagrada.Ruta)==null)return;
  var e=SceneManager.GetSceneByPath(ConstruirMontanaSagrada.Ruta);bool cerrar=!e.isLoaded;if(cerrar)e=EditorSceneManager.OpenScene(ConstruirMontanaSagrada.Ruta,OpenSceneMode.Additive);
  var m=e.GetRootGameObjects().Select(o=>o.GetComponent<NivelCincoChaski>()).FirstOrDefault(o=>o!=null);if(m==null)return;
  var t=e.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Terrain>()).First();int asentados=0;
  // Asentar conjuntos completos con el collider del piso como referencia, sin separar techos ni puertas.
  var objetos=e.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Transform>()).ToArray();
  var casas=objetos.Where(o=>o.name=="Muro posterior"||o.name=="Muro de piedra"||o.name.ToLowerInvariant().Contains("pilar")||o.name.ToLowerInvariant().Contains("columna")).Select(o=>o.parent).Where(o=>o!=null&&!o.name.StartsWith("Nivel ")).Distinct().ToArray();
  var d=t.terrainData;int n=d.heightmapResolution;var alturas=d.GetHeights(0,0,n,n);
  foreach(var casa in casas)
  {
   var rs=casa.GetComponentsInChildren<Renderer>();if(rs.Length==0)continue;var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);if(b.size.x>25||b.size.z>25)continue;var centro=b.center;float baseY=t.SampleHeight(centro)+t.transform.position.y;
   for(int z=0;z<n;z++)for(int x=0;x<n;x++){float xx=t.transform.position.x+x*d.size.x/(n-1),zz=t.transform.position.z+z*d.size.z/(n-1);float afuera=Mathf.Max(Mathf.Max(b.min.x-.5f-xx,xx-b.max.x-.5f),Mathf.Max(b.min.z-.5f-zz,zz-b.max.z-.5f));if(afuera>3)continue;float peso=1-Mathf.SmoothStep(0,1,Mathf.Clamp01(afuera/3));alturas[z,x]=Mathf.Lerp(alturas[z,x],(baseY-t.transform.position.y)/d.size.y,peso);}
   casa.position+=Vector3.up*(baseY-b.min.y-.04f);asentados++;
  }
  d.SetHeights(0,0,alturas);EditorUtility.SetDirty(d);
  foreach(var cartel in objetos.Where(o=>o!=null).Select(o=>o.GetComponent<TextMesh>()).Where(o=>o!=null))
  {cartel.font=FuenteCastellanaChaski.Obtener();cartel.GetComponent<Renderer>().sharedMaterial=cartel.font.material;}
  if(!e.GetRootGameObjects().Any(o=>o.name=="Montaña · abastecimiento adicional"))
  {
   var lista=m.recursos.ToList();float[] zs={110,170,225,280,335,390,450};int[] tipos={0,1,3,0,2,0,3};
   for(int i=0;i<zs.Length;i++){var original=lista.First(o=>o.tipo==tipos[i]);var copia=Object.Instantiate(original.gameObject,original.transform.parent);var r=copia.GetComponent<RecursoValleVientos>();r.identificador="montana-extra-"+i;float x=NivelCincoChaski.Centro(zs[i])+(i%2==0?2.1f:-2.1f);var p=new Vector3(x,0,zs[i]);p.y=t.SampleHeight(p)+t.transform.position.y+.08f;copia.transform.position=p;lista.Add(r);}
   m.recursos=lista.ToArray();var marca=new GameObject("Montaña · abastecimiento adicional");SceneManager.MoveGameObjectToScene(marca,e);
  }
  foreach(var f in objetos.Where(o=>o!=null).Select(o=>o.GetComponent<MeshFilter>()).Where(o=>o!=null&&(o.name.ToLowerInvariant().Contains("hojas")||o.name.ToLowerInvariant().Contains("copa"))))if(f.GetComponent<RamasVientoChaski>()==null)f.gameObject.AddComponent<RamasVientoChaski>();
  string apoyos="";Physics.SyncTransforms();
  foreach(var actor in new[]{m.caminante,m.agricultor,m.encargado,m.responsableSalida}.Where(o=>o!=null).Distinct())
  {
   var p=actor.position;float suelo=t.SampleHeight(p)+t.transform.position.y;
   foreach(var hit in Physics.RaycastAll(new Vector3(p.x,suelo+3,p.z),Vector3.down,6,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance))if(!hit.transform.IsChildOf(actor)&&!hit.transform.IsChildOf(m.jugador.transform)&&hit.point.y<=suelo+.5f){suelo=hit.point.y;break;}
   actor.position=new Vector3(p.x,suelo+.06f,p.z);apoyos+=actor.name+" · separación del suelo: "+(actor.position.y-suelo).ToString("F3")+" m\n";
  }
  EditorSceneManager.SaveScene(e);AssetDatabase.SaveAssets();System.IO.File.WriteAllText("Temp/nivel05_apoyos_alimentos.txt",$"Conjuntos asentados: {asentados}. Recursos: {m.recursos.Length}. Coca: {m.recursos.Count(o=>o.tipo==0)}.\n"+apoyos);if(cerrar)EditorSceneManager.CloseScene(e,true);
 }
}
