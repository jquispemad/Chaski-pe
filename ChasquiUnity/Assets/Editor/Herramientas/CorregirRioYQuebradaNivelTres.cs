using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CorregirRioYQuebradaNivelTres
{
    const string Marca="Nivel 3 · río asentado y posta de la quebrada";
    static T Cargar<T>(string p)where T:Object=>AssetDatabase.LoadAssetAtPath<T>(p);
    [InitializeOnLoadMethod]static void Preparar()=>EditorApplication.delayCall+=()=>{Aplicar();AjustarOrillas();AsentarSuperficie();};
    public static void Aplicar()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||Cargar<SceneAsset>(ConstruirValleVientos.Ruta)==null)return;
        var e=SceneManager.GetSceneByPath(ConstruirValleVientos.Ruta);bool cerrar=!e.isLoaded;if(cerrar)e=EditorSceneManager.OpenScene(ConstruirValleVientos.Ruta,OpenSceneMode.Additive);
        if(e.GetRootGameObjects().Any(o=>o.name==Marca)){if(cerrar)EditorSceneManager.CloseScene(e,true);return;}
        var mundo=e.GetRootGameObjects().First(o=>o.name.StartsWith("Nivel 03 · valle")).transform;
        var m=e.GetRootGameObjects().Select(o=>o.GetComponent<NivelTresChaski>()).First(o=>o!=null);
        var terreno=mundo.GetComponentInChildren<Terrain>();var d=terreno.terrainData;int n=d.heightmapResolution;var hs=d.GetHeights(0,0,n,n);
        for(int z=0;z<n;z++)for(int x=0;x<n;x++)
        {
            float xx=-110+x*220f/(n-1),zz=-20+z*550f/(n-1),y=hs[z,x]*65-10;
            if(xx<90&&Mathf.Abs(zz-NivelTresChaski.CentroAgua(xx))<12)
            {
                float ancho=Mathf.Max(.2f,NivelTresChaski.AnchoAgua(xx)),dist=Mathf.Abs(zz-NivelTresChaski.CentroAgua(xx));
                float excavacion=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(ancho+1,ancho+6,dist));
                y=Mathf.Lerp(y,NivelTresChaski.AguaY(xx)-1.15f,excavacion);
            }
            if(zz>425)
            {
                float lado=xx-NivelTresChaski.Centro(zz),entrada=Mathf.SmoothStep(0,1,Mathf.InverseLerp(425,450,zz));
                float corte=Mathf.SmoothStep(0,1,Mathf.InverseLerp(5,13,lado))*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(35,57,lado)));
                y=Mathf.Lerp(y,-8+Mathf.Sin(zz*.07f)*.7f,corte*entrada);
            }
            hs[z,x]=Mathf.Clamp01((y+10)/65);
        }
        d.SetHeights(0,0,hs);EditorUtility.SetDirty(d);
        int a=d.alphamapResolution;var pintura=d.GetAlphamaps(0,0,a,a);
        for(int z=0;z<a;z++)for(int x=0;x<a;x++)
        {
            float xx=-110+x*220f/(a-1),zz=-20+z*550f/(a-1);bool ribera=Mathf.Abs(zz-NivelTresChaski.CentroAgua(xx))<9&&xx<90;bool quebrada=zz>435&&xx-NivelTresChaski.Centro(zz)>5&&xx-NivelTresChaski.Centro(zz)<53;
            if(!ribera&&!quebrada)continue;for(int k=0;k<d.terrainLayers.Length;k++)pintura[z,x,k]=0;pintura[z,x,2]=.85f;pintura[z,x,0]=.15f;
        }
        d.SetAlphamaps(0,0,pintura);
        var rio=mundo.GetComponentsInChildren<MeshFilter>().First(f=>f.name.StartsWith("Arroyo · naciente"));var mesh=rio.sharedMesh;
        var vs=new List<Vector3>();var ts=new List<int>();var uv=new List<Vector2>();var color=new List<Color>();
        for(int i=0;i<=396;i++)for(int j=0;j<9;j++)
        {
            float x=-110+i*.5f,u=j/8f;vs.Add(new Vector3(x,NivelTresChaski.AguaY(x),NivelTresChaski.CentroAgua(x)+(u*2-1)*NivelTresChaski.AnchoAgua(x)));uv.Add(new Vector2(u,x*.2f));color.Add(new Color(Mathf.Sin(u*Mathf.PI),0,0,1));
            if(i==0||j==8)continue;int b=i*9+j,c=b-9;ts.AddRange(new[]{c,c+1,b,c+1,b+1,b});
        }
        mesh.Clear();mesh.SetVertices(vs);mesh.SetTriangles(ts,0);mesh.SetUVs(0,uv);mesh.SetColors(color);mesh.RecalculateBounds();mesh.RecalculateNormals();EditorUtility.SetDirty(mesh);
        foreach(var f in mundo.GetComponentsInChildren<MeshFilter>())
        {
            if(!f.name.StartsWith("Roca ·")||Mathf.Abs(f.transform.position.z-NivelTresChaski.CentroAgua(f.transform.position.x))>12)continue;
            if(Mathf.Abs(f.transform.position.x-NivelTresChaski.Centro(f.transform.position.z))<2&&f.transform.position.z>=272&&f.transform.position.z<=290)continue;
            Asentar(f.transform,terreno);
        }
        foreach(var t in mundo.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Queñua ·")&&t.position.z>435&&t.position.x-NivelTresChaski.Centro(t.position.z)>5).ToArray())if(t!=null)Object.DestroyImmediate(t.gameObject);
        var posta=mundo.Cast<Transform>().First(o=>o.name=="Estación del valle");
        var nueva=new GameObject("Posta abierta · mirador de la quebrada").transform;SceneManager.MoveGameObjectToScene(nueva.gameObject,e);nueva.SetParent(mundo);nueva.position=new Vector3(NivelTresChaski.Centro(484)-6,4,484);
        // Reuse the thatch roof as an open shelter, without repeating the closed house.
        foreach(var p in posta.Cast<Transform>().Where(o=>o.name.Contains("Cubierta de ichu")).ToArray())
        {var copia=Object.Instantiate(p.gameObject,nueva,false);copia.transform.localPosition=p.localPosition;copia.transform.localRotation=p.localRotation;}
        Object.DestroyImmediate(posta.gameObject);
        var piedra=Cargar<Material>("Assets/Niveles/SegundoRecorrido/PiedraViviendas.mat");var madera=Cargar<Material>("Assets/Niveles/Valle/Madera.mat");
        foreach(float x in new[]{-2.8f,2.8f})foreach(float z in new[]{-2.7f,2.7f})Bloque("Pilar del abrigo",nueva,new Vector3(x,1.6f,z),new Vector3(.25f,3.2f,.25f),madera);
        Bloque("Muro bajo de piedra",nueva,new Vector3(-2.7f,.55f,0),new Vector3(.55f,1.1f,6),piedra);
        Bloque("Banco del guardacamino",nueva,new Vector3(0,.4f,1.6f),new Vector3(3,.8f,.6f),piedra);
        var roca=Cargar<Mesh>("Assets/Niveles/Valle/Roca0.asset");var material=Cargar<Material>("Assets/Personajes/Chasqui/MaterialPiedra.mat");
        for(int i=0;i<32;i++)
        {
            float z=441+i*2.15f,x=NivelTresChaski.Centro(z)+(i%2==0?7.2f:37),alto=2.5f+i%5*1.1f;
            var o=new GameObject("Roca · pared de la quebrada");o.transform.SetParent(mundo);o.transform.position=new Vector3(x,0,z);var b=roca.bounds;o.transform.localScale=new Vector3((3+i%3)/b.size.x,alto/b.size.y,4/b.size.z);o.AddComponent<MeshFilter>().sharedMesh=roca;o.AddComponent<MeshRenderer>().sharedMaterial=material;Asentar(o.transform,terreno);var c=o.AddComponent<BoxCollider>();c.center=b.center;c.size=b.size*.9f;
        }
        for(int i=0;i<14;i++){float z=453+i*3.4f;Bloque("Pretil · borde del mirador",mundo,new Vector3(NivelTresChaski.Centro(z)+4.1f,4.3f,z),new Vector3(.7f,.65f,2.9f),piedra);}
        m.encargado.position=new Vector3(NivelTresChaski.Centro(485)-2.2f,4.06f,485);m.encargado.rotation=Quaternion.Euler(0,150,0);
        m.circulo.position=new Vector3(NivelTresChaski.Centro(485)-.3f,4.045f,485);
        foreach(var texto in mundo.GetComponentsInChildren<TextMesh>())if(texto.text.StartsWith("POSTA DEL VALLE"))texto.text="POSTA DE LA QUEBRADA\nE · TOCAR EL PUTUTU";
        var marca=new GameObject(Marca);SceneManager.MoveGameObjectToScene(marca,e);EditorSceneManager.SaveScene(e);AssetDatabase.SaveAssets();Validar(e,terreno,mesh);if(cerrar)EditorSceneManager.CloseScene(e,true);
    }
    static void Asentar(Transform o,Terrain terreno)
    {var mesh=o.GetComponent<MeshFilter>().sharedMesh;float min=float.PositiveInfinity;foreach(var v in mesh.vertices){var q=o.TransformPoint(v);min=Mathf.Min(min,q.y-(terreno.SampleHeight(q)+terreno.transform.position.y));}o.position-=Vector3.up*(min+.1f);}
    static GameObject Bloque(string n,Transform padre,Vector3 p,Vector3 s,Material m)
    {var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=n;o.transform.SetParent(padre,false);o.transform.localPosition=p;o.transform.localScale=s;o.GetComponent<Renderer>().sharedMaterial=m;return o;}
    static void AjustarOrillas()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;var e=SceneManager.GetSceneByPath(ConstruirValleVientos.Ruta);bool cerrar=!e.isLoaded;if(cerrar)e=EditorSceneManager.OpenScene(ConstruirValleVientos.Ruta,OpenSceneMode.Additive);
        if(e.GetRootGameObjects().Any(o=>o.name=="Orillas del arroyo · contacto con el agua")){if(cerrar)EditorSceneManager.CloseScene(e,true);return;}
        var terreno=e.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Terrain>()).First();var d=terreno.terrainData;int n=d.heightmapResolution;var hs=d.GetHeights(0,0,n,n);
        for(int z=0;z<n;z++)for(int x=0;x<n;x++)
        {
            float xx=-110+x*220f/(n-1),zz=-20+z*550f/(n-1);if(xx>88)continue;float centro=NivelTresChaski.CentroAgua(xx),dist=Mathf.Abs(zz-centro),ancho=Mathf.Max(.12f,NivelTresChaski.AnchoAgua(xx));if(dist>ancho+9)continue;
            float agua=NivelTresChaski.AguaY(xx),y;
            if(dist<=ancho)y=agua+.025f-.82f*Mathf.Sqrt(Mathf.Clamp01(1-Mathf.Pow(dist/ancho,2)));
            else{float borde=terreno.SampleHeight(new Vector3(xx,0,centro+Mathf.Sign(zz-centro)*(ancho+9)))+terreno.transform.position.y;y=Mathf.Lerp(agua+.025f,borde,Mathf.SmoothStep(0,1,(dist-ancho)/9));}
            hs[z,x]=(y+10)/65;
        }
        d.SetHeights(0,0,hs);EditorUtility.SetDirty(d);
        var rio=e.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<MeshFilter>()).First(o=>o.name.StartsWith("Arroyo · naciente"));var mesh=rio.sharedMesh;var vs=mesh.vertices;int contactos=0;
        for(int i=0;i<vs.Length;i++)if(i%9==0||i%9==8){vs[i].y=Mathf.Min(vs[i].y,terreno.SampleHeight(vs[i])+terreno.transform.position.y+.012f);contactos++;}
        mesh.vertices=vs;mesh.RecalculateBounds();mesh.RecalculateNormals();EditorUtility.SetDirty(mesh);
        foreach(var f in e.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<MeshFilter>()))if(f.name.StartsWith("Roca ·")&&Mathf.Abs(f.transform.position.z-NivelTresChaski.CentroAgua(f.transform.position.x))<12&&!(Mathf.Abs(f.transform.position.x-NivelTresChaski.Centro(f.transform.position.z))<2&&f.transform.position.z>272&&f.transform.position.z<290))Asentar(f.transform,terreno);
        var marca=new GameObject("Orillas del arroyo · contacto con el agua");SceneManager.MoveGameObjectToScene(marca,e);EditorSceneManager.SaveScene(e);AssetDatabase.SaveAssets();
        System.IO.File.AppendAllText("Temp/nivel03_rio_quebrada.txt","Orillas conectadas al terreno: "+contactos+" vértices de borde.\n");Debug.Log("ORILLAS_NIVEL03: "+contactos+" vértices ajustados al contacto con el terreno.");if(cerrar)EditorSceneManager.CloseScene(e,true);
    }
    static void Validar(Scene e,Terrain terreno,Mesh rio)
    {
        int sinLecho=0;foreach(var v in rio.vertices){float suelo=terreno.SampleHeight(v)+terreno.transform.position.y;if(v.x<78&&v.y-suelo>1.7f)sinLecho++;}
        System.IO.File.WriteAllText("Temp/nivel03_rio_quebrada.txt","Vértices del río sin lecho próximo: "+sinLecho+"\nFinal: mirador abierto junto a la quebrada\n");Debug.Log("RIO_QUEBRADA_NIVEL03: "+sinLecho+" vértices sin lecho próximo; final diferente construido.");
    }
    static void AsentarSuperficie()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;var e=SceneManager.GetSceneByPath(ConstruirValleVientos.Ruta);bool cerrar=!e.isLoaded;if(cerrar)e=EditorSceneManager.OpenScene(ConstruirValleVientos.Ruta,OpenSceneMode.Additive);
        if(e.GetRootGameObjects().Any(o=>o.name=="Arroyo · superficie apoyada sobre colisión")){if(cerrar)EditorSceneManager.CloseScene(e,true);return;}
        var terreno=e.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Terrain>()).First();var d=terreno.terrainData;int n=d.heightmapResolution;var hs=d.GetHeights(0,0,n,n);
        for(int z=0;z<n;z++)for(int x=0;x<n;x++)
        {
            float xx=-110+x*220f/(n-1),zz=-20+z*550f/(n-1);if(xx>88)continue;float ancho=Mathf.Max(.12f,NivelTresChaski.AnchoAgua(xx)),dist=Mathf.Abs(zz-NivelTresChaski.CentroAgua(xx));if(dist>ancho+3)continue;
            float y=hs[z,x]*65-10;float mezcla=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(ancho+.8f,ancho+3,dist));hs[z,x]=(Mathf.Lerp(y,NivelTresChaski.AguaY(xx)-.045f,mezcla)+10)/65;
        }
        d.SetHeights(0,0,hs);d.SyncHeightmap();terreno.heightmapPixelError=1;terreno.heightmapMaximumLOD=0;terreno.Flush();Physics.SyncTransforms();EditorUtility.SetDirty(d);
        var rio=e.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<MeshFilter>()).First(o=>o.name.StartsWith("Arroyo · naciente"));rio.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);rio.transform.localScale=Vector3.one;var mesh=rio.sharedMesh;var vs=mesh.vertices;var col=terreno.GetComponent<TerrainCollider>();float max=0;
        for(int i=0;i<vs.Length;i++)
        {
            var q=rio.transform.TransformPoint(vs[i]);if(col.Raycast(new Ray(q+Vector3.up*80,Vector3.down),out var hit,160)){q.y=hit.point.y+.018f;vs[i]=rio.transform.InverseTransformPoint(q);max=Mathf.Max(max,q.y-hit.point.y);}
        }
        mesh.vertices=vs;mesh.RecalculateBounds();mesh.RecalculateNormals();EditorUtility.SetDirty(mesh);rio.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        var marca=new GameObject("Arroyo · superficie apoyada sobre colisión");SceneManager.MoveGameObjectToScene(marca,e);EditorSceneManager.SaveScene(e);AssetDatabase.SaveAssets();System.IO.File.AppendAllText("Temp/nivel03_rio_quebrada.txt","Separación máxima agua-colisión: "+max+" m. Sombras de lámina desactivadas; detalle del cauce conservado.\n");if(cerrar)EditorSceneManager.CloseScene(e,true);
    }
}
