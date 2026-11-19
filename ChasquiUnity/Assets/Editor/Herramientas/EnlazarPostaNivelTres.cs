using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class EnlazarPostaNivelTres
{
    [InitializeOnLoadMethod]static void Preparar()=>EditorApplication.delayCall+=Aplicar;
    public static void Aplicar()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        var e=SceneManager.GetSceneByPath(ConstruirValleVientos.Ruta);bool cerrar=!e.isLoaded;if(cerrar)e=EditorSceneManager.OpenScene(ConstruirValleVientos.Ruta,OpenSceneMode.Additive);
        if(e.GetRootGameObjects().Any(o=>o.name=="Continuidad · final exacto del nivel 2")){if(cerrar)EditorSceneManager.CloseScene(e,true);return;}
        var fuente=SceneManager.GetSceneByPath(ConstruirSegundoRecorrido.Ruta);bool cerrarFuente=!fuente.isLoaded;if(cerrarFuente)fuente=EditorSceneManager.OpenScene(ConstruirSegundoRecorrido.Ruta,OpenSceneMode.Additive);
        var src=fuente.GetRootGameObjects().Select(o=>o.GetComponent<SegundaMisionChaski>()).First(x=>x!=null);
        var mundo=e.GetRootGameObjects().First(o=>o.name.StartsWith("Nivel 03 · valle")).transform;
        var m=e.GetRootGameObjects().Select(o=>o.GetComponent<NivelTresChaski>()).First(x=>x!=null);
        var origenTerreno=fuente.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Terrain>()).First();var terreno=mundo.GetComponentInChildren<Terrain>();
        var offset=new Vector3(-ConstruirSegundoRecorrido.Centro(450),4,-446);
        foreach(var p in mundo.Cast<Transform>().ToArray())
        {
            if(p.GetComponent<Terrain>()!=null)continue;
            if(p.position.z<29&&(p.name.StartsWith("Afloramiento")||p.name.StartsWith("Letrero")||p.name.StartsWith("Poste")||p.name.StartsWith("Tabla"))){Object.DestroyImmediate(p.gameObject);continue;}
            if(p.name=="Sendero · tierra compactada")continue;
            if(p.position.z<75)p.position+=new Vector3(NivelTresChaski.Centro(p.position.z)-NivelTresChaski.CentroValle(p.position.z),NivelTresChaski.Base(p.position.z)-NivelTresChaski.BaseValle(p.position.z),0);
        }
        int n=terreno.terrainData.heightmapResolution;var hs=terreno.terrainData.GetHeights(0,0,n,n);
        for(int z=0;z<n;z++)for(int x=0;x<n;x++)
        {
            float zz=-20+z*550f/(n-1),xx=-110+x*220f/(n-1);if(zz>=100)continue;
            var p=new Vector3(xx,0,zz);var q=p-offset;float desde=origenTerreno.SampleHeight(q)+origenTerreno.transform.position.y+offset.y;
            float previo=xx-(NivelTresChaski.Centro(zz)-NivelTresChaski.CentroValle(zz));float ahora=terreno.SampleHeight(new Vector3(previo,0,zz))+terreno.transform.position.y+NivelTresChaski.Base(zz)-NivelTresChaski.BaseValle(zz);
            float y=Mathf.Lerp(desde,ahora,Mathf.SmoothStep(0,1,Mathf.InverseLerp(28,75,zz)));hs[z,x]=(y+10)/65;
        }
        terreno.terrainData.SetHeights(0,0,hs);EditorUtility.SetDirty(terreno.terrainData);
        var mesh=mundo.Cast<Transform>().First(o=>o.name=="Sendero · tierra compactada").GetComponent<MeshFilter>().sharedMesh;var vs=mesh.vertices;
        for(int i=0;i<vs.Length;i++)if(vs[i].z<110){float z=vs[i].z;float anchoAnterior=Mathf.Lerp(1.5f,2.6f,Mathf.SmoothStep(0,1,Mathf.Clamp01(z/110)));float ancho=Mathf.Lerp(3.3f,anchoAnterior,Mathf.SmoothStep(0,1,Mathf.InverseLerp(28,75,z)));vs[i].x=NivelTresChaski.Centro(z)+(vs[i].x-NivelTresChaski.CentroValle(z))*ancho/anchoAnterior;vs[i].y=terreno.SampleHeight(vs[i])+terreno.transform.position.y+.025f;}
        mesh.vertices=vs;mesh.RecalculateBounds();mesh.RecalculateNormals();EditorUtility.SetDirty(mesh);
        int a=terreno.terrainData.alphamapResolution;var alpha=terreno.terrainData.GetAlphamaps(0,0,a,a);var sd=origenTerreno.terrainData;var colores=sd.GetAlphamaps(0,0,sd.alphamapWidth,sd.alphamapHeight);
        for(int z=0;z<a;z++)for(int x=0;x<a;x++)
        {
            float zz=-20+z*550f/(a-1),xx=-110+x*220f/(a-1);if(zz>28)continue;var q=new Vector3(xx,0,zz)-offset-origenTerreno.transform.position;
            int ix=Mathf.Clamp(Mathf.RoundToInt(q.x/sd.size.x*(sd.alphamapWidth-1)),0,sd.alphamapWidth-1),iz=Mathf.Clamp(Mathf.RoundToInt(q.z/sd.size.z*(sd.alphamapHeight-1)),0,sd.alphamapHeight-1);for(int k=0;k<sd.terrainLayers.Length;k++)alpha[z,x,k]=colores[iz,ix,k];
        }
        terreno.terrainData.SetAlphamaps(0,0,alpha);
        var grupo=new GameObject("Continuidad · final exacto del nivel 2");SceneManager.MoveGameObjectToScene(grupo,e);
        var srcMundo=fuente.GetRootGameObjects().First(o=>o.name.StartsWith("Nivel 02 · paisaje")).transform;
        foreach(var p in srcMundo.Cast<Transform>())
        {
            if(p.name=="Posta sin relevista"||p.name.StartsWith("Letrero")&&p.position.z>426)Copiar(p.gameObject,grupo.transform,offset,e);
            else if(p.name.StartsWith("Queñuas y rocas")){foreach(Transform v in p)if(v.position.z>426&&v.position.z<474)Copiar(v.gameObject,grupo.transform,offset,e);}
        }
        var responsable=Copiar(src.responsable.gameObject,grupo.transform,offset,e);responsable.name="Responsable · misma posta del nivel 2";m.responsableSalida=responsable.transform;
        m.jugador.transform.position=src.entrega.position+offset+Vector3.up*.1f;
        m.vista.transform.position=m.jugador.transform.position+new Vector3(0,2.3f,-4.5f);m.vista.transform.LookAt(m.jugador.transform.position+Vector3.up*1.1f);
        if(cerrarFuente)EditorSceneManager.CloseScene(fuente,true);EditorSceneManager.SaveScene(e);AssetDatabase.SaveAssets();if(cerrar)EditorSceneManager.CloseScene(e,true);Debug.Log("POSTA_CONTINUA: final del nivel 2 reproducido al inicio del nivel 3; el camino nuevo continúa desde allí.");
    }
    static GameObject Copiar(GameObject original,Transform padre,Vector3 offset,Scene e)
    {var o=Object.Instantiate(original);SceneManager.MoveGameObjectToScene(o,e);o.transform.SetParent(padre,true);o.transform.position+=offset;return o;}
}
