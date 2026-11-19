using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ContinuidadYSalidaNivelCuatro
{
    const string Marca="Nivel 4 · continuidad exacta y camino hacia montaña";
    [InitializeOnLoadMethod]static void Preparar()=>EditorApplication.delayCall+=Aplicar;
    public static void Aplicar()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||AssetDatabase.LoadAssetAtPath<SceneAsset>(ConstruirPuenteAbismo.Ruta)==null)return;
        var e=SceneManager.GetSceneByPath(ConstruirPuenteAbismo.Ruta);bool cerrar=!e.isLoaded;if(cerrar)e=EditorSceneManager.OpenScene(ConstruirPuenteAbismo.Ruta,OpenSceneMode.Additive);
        if(e.GetRootGameObjects().Any(o=>o.name==Marca)){if(cerrar)EditorSceneManager.CloseScene(e,true);return;}
        var fuente=SceneManager.GetSceneByPath(ConstruirValleVientos.Ruta);bool cerrarFuente=!fuente.isLoaded;if(cerrarFuente)fuente=EditorSceneManager.OpenScene(ConstruirValleVientos.Ruta,OpenSceneMode.Additive);
        var m3=fuente.GetRootGameObjects().Select(o=>o.GetComponent<NivelTresChaski>()).First(o=>o!=null);var m=e.GetRootGameObjects().Select(o=>o.GetComponent<NivelCuatroChaski>()).First(o=>o!=null);
        Vector3 offset=new Vector3(-NivelTresChaski.Centro(485),0,-481);m.desplazamientoDesdeNivel3=offset;
        var mundo=e.GetRootGameObjects().First(o=>o.name.StartsWith("Nivel 04 ·")).transform;var terreno=mundo.GetComponentInChildren<Terrain>();var t3=fuente.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Terrain>()).First();
        var enlace=mundo.Cast<Transform>().First(o=>o.name=="Enlace compartido · mirador final del nivel 3");foreach(var hijo in enlace.Cast<Transform>().ToArray())Object.DestroyImmediate(hijo.gameObject);
        var mundo3=fuente.GetRootGameObjects().First(o=>o.name.StartsWith("Nivel 03 · valle")).transform;
        int copias=0;foreach(Transform p in mundo3)
        {
            if(p.GetComponent<Terrain>()!=null||p.name.Contains("Barrera")||p.name.Contains("Límite")||p.name.StartsWith("Sendero")||p.name.StartsWith("Arroyo"))continue;
            if(p.position.z>=453&&p.position.z<=510){Copiar(p.gameObject,enlace,offset,e);copias++;}
        }
        m.responsable=Copiar(m3.encargado.gameObject,enlace,offset,e).transform;m.responsable.name="Encargado · misma posta del final del nivel 3";
        m.jugador.transform.position=m3.circulo.position+offset+Vector3.up*.10f;m.controles[0].position=m.jugador.transform.position;
        m.vista.transform.position=m.jugador.transform.position+new Vector3(0,2.3f,-4.5f);m.vista.transform.LookAt(m.jugador.transform.position+Vector3.up*1.1f);
        var d=terreno.terrainData;int n=d.heightmapResolution;var hs=d.GetHeights(0,0,n,n);
        for(int z=0;z<n;z++)
        {
            float zz=-25+z*450f/(n-1);if(zz>50)break;float mezcla=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(28,50,zz));
            for(int x=0;x<n;x++){float xx=-120+x*240f/(n-1);var q=new Vector3(xx,0,zz)-offset;float y=t3.SampleHeight(q)+t3.transform.position.y;hs[z,x]=Mathf.Lerp(hs[z,x],(y+30)/90,mezcla);}
        }
        d.SetHeights(0,0,hs);d.SyncHeightmap();terreno.Flush();EditorUtility.SetDirty(d);
        var sd=t3.terrainData;var colores=sd.GetAlphamaps(0,0,sd.alphamapWidth,sd.alphamapHeight);int an=d.alphamapResolution;var alpha=d.GetAlphamaps(0,0,an,an);
        for(int z=0;z<an;z++)
        {
            float zz=-25+z*450f/(an-1);if(zz>50)break;float mezcla=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(28,50,zz));
            for(int x=0;x<an;x++){var q=new Vector3(-120+x*240f/(an-1),0,zz)-offset-t3.transform.position;int sx=Mathf.Clamp(Mathf.RoundToInt(q.x/sd.size.x*(sd.alphamapWidth-1)),0,sd.alphamapWidth-1),sz=Mathf.Clamp(Mathf.RoundToInt(q.z/sd.size.z*(sd.alphamapHeight-1)),0,sd.alphamapHeight-1);for(int k=0;k<d.terrainLayers.Length;k++)alpha[z,x,k]=Mathf.Lerp(alpha[z,x,k],colores[sz,sx,k],mezcla);}
        }
        d.SetAlphamaps(0,0,alpha);
        var f=mundo.GetComponentsInChildren<MeshFilter>().First(o=>o.name=="Sendero · piedra y tierra");var mesh=f.sharedMesh;var v=mesh.vertices.ToList();var uv=mesh.uv.ToList();var tr=mesh.triangles.ToList();
        for(int i=0;i<v.Count;i++)if(v[i].z<65){var p=v[i];float ancho=Mathf.Lerp(2.6f,1.6f,Mathf.SmoothStep(0,1,Mathf.InverseLerp(28,65,p.z)));p.x=NivelCuatroChaski.Centro(p.z)+(p.x-NivelCuatroChaski.Centro(p.z))*ancho/1.6f;p.y=terreno.SampleHeight(p)+terreno.transform.position.y+.023f;v[i]=p;}
        int previo=v.Count-7;
        for(int fila=1;fila<=66;fila++)
        {
            float z=390+fila*.5f,x=CentroSalida(z);int actual=v.Count;for(int j=0;j<7;j++){float u=j/6f;var p=new Vector3(x+(u*2-1)*1.6f,0,z);p.y=terreno.SampleHeight(p)+terreno.transform.position.y+.025f;v.Add(p);uv.Add(new Vector2(u,z/3));if(j<6)tr.AddRange(new[]{previo+j,actual+j,previo+j+1,previo+j+1,actual+j,actual+j+1});}previo=actual;
        }
        mesh.Clear();mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(tr,0);mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
        for(int z=0;z<an;z++){float zz=-25+z*450f/(an-1);if(zz<390)continue;for(int x=0;x<an;x++){float xx=-120+x*240f/(an-1),peso=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.6f,2.4f,Mathf.Abs(xx-CentroSalida(zz))));for(int k=0;k<d.terrainLayers.Length;k++)alpha[z,x,k]=Mathf.Lerp(alpha[z,x,k],k==1?1:0,peso);}}d.SetAlphamaps(0,0,alpha);
        foreach(var r in m.recursos)
        {
            var anterior=r.transform.position;float lado=anterior.x<NivelCuatroChaski.Centro(anterior.z)?-2.1f:2.1f;var nuevo=new Vector3(NivelCuatroChaski.Centro(anterior.z)+lado,0,anterior.z);nuevo.y=terreno.SampleHeight(nuevo)+terreno.transform.position.y+.45f;
            foreach(var pedestal in mundo.Cast<Transform>().Where(o=>o.name=="Piedra de abastecimiento"&&Mathf.Abs(o.position.z-anterior.z)<.1f&&Mathf.Abs(o.position.x-anterior.x)<.1f))pedestal.position+=nuevo-anterior;r.transform.position=nuevo;
        }
        foreach(var limite in mundo.Cast<Transform>().Where(o=>o.name=="Límite del recorrido"&&o.position.z>350))limite.position=new Vector3(0,20,419);
        var roca=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Niveles/Valle/Roca0.asset");var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Personajes/Chasqui/MaterialPiedra.mat");
        foreach(int lado in new[]{-1,1})
        {
            var o=new GameObject("Montaña · recodo del siguiente nivel");o.transform.SetParent(mundo);var p=new Vector3(CentroSalida(421)+lado*6,0,423);p.y=terreno.SampleHeight(p)+terreno.transform.position.y;var b=roca.bounds;o.transform.localScale=new Vector3(11/b.size.x,15/b.size.y,16/b.size.z);o.transform.position=p-Vector3.up*(b.min.y*o.transform.localScale.y+.25f);o.transform.rotation=Quaternion.Euler(0,lado*23,0);o.AddComponent<MeshFilter>().sharedMesh=roca;o.AddComponent<MeshRenderer>().sharedMaterial=mat;
        }
        if(cerrarFuente)EditorSceneManager.CloseScene(fuente,true);var marca=new GameObject(Marca);SceneManager.MoveGameObjectToScene(marca,e);EditorSceneManager.SaveScene(e);AssetDatabase.SaveAssets();
        System.IO.File.WriteAllText("Temp/nivel04_continuidad.txt",$"Elementos compartidos del mirador: {copias}\nAncho inicial del camino: 5,2 m\nSendero prolongado hasta: 423 m\nProvisiones accesibles a 2,1 m del eje\n");if(cerrar)EditorSceneManager.CloseScene(e,true);
    }
    static float CentroSalida(float z)=>NivelCuatroChaski.Centro(390)+(z-390)*.30f+8*Mathf.Sin((z-390)*.055f);
    static GameObject Copiar(GameObject original,Transform padre,Vector3 offset,Scene e){var o=Object.Instantiate(original);SceneManager.MoveGameObjectToScene(o,e);o.transform.SetParent(padre,true);o.transform.position+=offset;return o;}
}
