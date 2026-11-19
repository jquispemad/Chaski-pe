using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MejorarIdentidadValleVientos
{
    const string Marca="Nivel 03 · salida de la segunda posta · revisión 2";
    const string Dir="Assets/Niveles/ValleVientos/";
    static float CentroAnterior(float z)=>11*Mathf.Sin(z*.018f)+5*Mathf.Sin(z*.043f);
    static Vector3 Desplazar(Vector3 p){if(p.z>=110)return p;return p+new Vector3(NivelTresChaski.Centro(p.z)-CentroAnterior(p.z),NivelTresChaski.Base(p.z)-2,0);}
    static float Altura(float x,float z)
    {
        float margen=Mathf.Abs(x-NivelTresChaski.Centro(z));float y=NivelTresChaski.Base(z)+Mathf.SmoothStep(0,1,Mathf.InverseLerp(10,85,margen))*(12+18*Mathf.PerlinNoise(x*.025f+13,z*.02f));
        if(x<80){float dist=Mathf.Abs(z-NivelTresChaski.CentroAgua(x));y=Mathf.Lerp(y,NivelTresChaski.AguaY(x)-1.1f,1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(3,6.5f,dist)));}return y;
    }
    [InitializeOnLoadMethod]static void Preparar()=>EditorApplication.delayCall+=Aplicar;
    public static void Aplicar()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||AssetDatabase.LoadAssetAtPath<SceneAsset>(ConstruirValleVientos.Ruta)==null)return;
        var e=SceneManager.GetSceneByPath(ConstruirValleVientos.Ruta);bool cerrar=!e.isLoaded;if(cerrar)e=EditorSceneManager.OpenScene(ConstruirValleVientos.Ruta,OpenSceneMode.Additive);
        var roots=e.GetRootGameObjects();if(roots.Any(o=>o.name==Marca)){Retocar(e);if(cerrar)EditorSceneManager.CloseScene(e,true);return;}
        var mundo=roots.First(o=>o.name.StartsWith("Nivel 03 · valle")).transform;
        var m=roots.Select(o=>o.GetComponent<NivelTresChaski>()).First(x=>x!=null);
        var terreno=mundo.GetComponentInChildren<Terrain>();var d=terreno.terrainData;int n=d.heightmapResolution;var h=new float[n,n];
        for(int z=0;z<n;z++)for(int x=0;x<n;x++){float xx=-110+x*220f/(n-1),zz=-20+z*550f/(n-1);h[z,x]=(Altura(xx,zz)+10)/65;}d.SetHeights(0,0,h);
        int an=d.alphamapResolution;var alpha=new float[an,an,d.terrainLayers.Length];
        for(int z=0;z<an;z++)for(int x=0;x<an;x++){float xx=-110+x*220f/(an-1),zz=-20+z*550f/(an-1),dist=Mathf.Abs(xx-NivelTresChaski.Centro(zz)),ancho=Mathf.Lerp(1.5f,2.6f,Mathf.SmoothStep(0,1,Mathf.Clamp01(zz/110)));float tierra=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(ancho,ancho+.8f,dist));alpha[z,x,0]=1-tierra;alpha[z,x,1]=tierra;}d.SetAlphamaps(0,0,alpha);EditorUtility.SetDirty(d);
        foreach(var p in mundo.Cast<Transform>().ToArray())
        {
            if(p.GetComponent<Terrain>()!=null||p.name.StartsWith("Arroyo · naciente"))continue;
            if(p.name=="Posta de salida"||p.name.StartsWith("Viga inclinada")||p.name.StartsWith("Paso bajo")||p.name.StartsWith("Soporte del paso")||p.name.StartsWith("Tronco ·")||p.name.StartsWith("Andén ·")||p.name.StartsWith("Cultivos ·")){Object.DestroyImmediate(p.gameObject);continue;}
            if(p.name=="Sendero · tierra compactada")
            {
                var mesh=p.GetComponent<MeshFilter>().sharedMesh;var vs=mesh.vertices;
                for(int i=0;i<vs.Length;i++){float z=vs[i].z;float lado=vs[i].x-CentroAnterior(z);float ancho=Mathf.Lerp(1.5f,2.6f,Mathf.SmoothStep(0,1,Mathf.Clamp01(z/110)));vs[i].x=NivelTresChaski.Centro(z)+lado*ancho/2.6f;vs[i].y=Altura(vs[i].x,z)+.025f;}mesh.vertices=vs;mesh.RecalculateBounds();mesh.RecalculateNormals();EditorUtility.SetDirty(mesh);continue;
            }
            p.position=Desplazar(p.position);
        }
        m.jugador.transform.position=Desplazar(m.jugador.transform.position);m.vista.transform.position=m.jugador.transform.position+new Vector3(0,2.3f,-4.5f);m.vista.transform.LookAt(m.jugador.transform.position+Vector3.up*1.1f);
        var pastor=roots.FirstOrDefault(o=>o.name.StartsWith("Agricultor ·"));if(pastor!=null){var rs=pastor.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);var destino=new Vector3(NivelTresChaski.Centro(72)-4,NivelTresChaski.Base(72),72);pastor.transform.position+=new Vector3(destino.x-b.center.x,destino.y-b.min.y+.02f,destino.z-b.center.z);pastor.name="Pastor · junto al sendero de altura";}
        foreach(var texto in mundo.GetComponentsInChildren<TextMesh>())if(texto.text.StartsWith("ESTACIÓN DEL VALLE"))texto.text="SALIDA DE LA SEGUNDA POSTA\nHACIA EL VALLE DE LOS VIENTOS";
        var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Personajes/Chasqui/MaterialPiedra.mat");var piedra=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Niveles/Valle/Roca0.asset");
        for(int i=0;i<22;i++)
        {
            float z=9+i*4.2f,lado=(i%2==0?-1:1)*(4+i%3*1.3f),x=NivelTresChaski.Centro(z)+lado;float alto=1.6f+i%4*.65f;
            var o=new GameObject("Afloramiento · salida del valle");o.transform.SetParent(mundo);var b=piedra.bounds;var s=new Vector3((2+i%3*.5f)/b.size.x,alto/b.size.y,(2.8f+i%2)/b.size.z);o.transform.localScale=s;o.transform.position=new Vector3(x,Altura(x,z)-b.min.y*s.y-.15f,z);o.AddComponent<MeshFilter>().sharedMesh=piedra;o.AddComponent<MeshRenderer>().sharedMaterial=mat;var c=o.AddComponent<BoxCollider>();c.center=b.center;c.size=b.size*.9f;
        }
        foreach(var r in m.recursos)
        {
            foreach(var hijo in r.transform.Cast<Transform>().ToArray())Object.DestroyImmediate(hijo.gameObject);
            var visual=AspectoRacionChaski.Crear(r.tipo,false);SceneManager.MoveGameObjectToScene(visual,e);visual.transform.SetParent(r.transform,false);visual.transform.localScale=Vector3.one*(r.tipo==3?2.3f:1.8f);
            var vistos=new HashSet<Object>();int i=0;
            foreach(var f in visual.GetComponentsInChildren<MeshFilter>())if(string.IsNullOrEmpty(AssetDatabase.GetAssetPath(f.sharedMesh))&&vistos.Add(f.sharedMesh))AssetDatabase.CreateAsset(f.sharedMesh,Dir+"Racion-"+r.identificador+"-"+(i++)+".asset");
            foreach(var f in visual.GetComponentsInChildren<Renderer>())foreach(var a in f.sharedMaterials)if(string.IsNullOrEmpty(AssetDatabase.GetAssetPath(a))&&vistos.Add(a))AssetDatabase.CreateAsset(a,Dir+"Racion-"+r.identificador+"-"+(i++)+".mat");
            Object.DestroyImmediate(visual.GetComponent<AspectoRacionChaski>());
        }
        if(pastor!=null)m.agricultor=pastor.transform;
        var marca=new GameObject(Marca);SceneManager.MoveGameObjectToScene(marca,e);EditorSceneManager.SaveScene(e);AssetDatabase.SaveAssets();if(cerrar)EditorSceneManager.CloseScene(e,true);Debug.Log("VALLE03_REVISADO: salida diferente, sin arco ni entrega repetida, raciones detalladas.");
    }
    static void Retocar(Scene e)
    {
        if(e.GetRootGameObjects().Any(o=>o.name=="Raciones y carteles · acabado 3"))return;
        foreach(var texto in e.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<TextMesh>()))if(texto.text.StartsWith("SALIDA DE LA SEGUNDA"))
        {var b=texto.GetComponent<Renderer>().bounds.size;texto.transform.localScale*=Mathf.Min(3.25f/b.x,.65f/b.y);}
        foreach(string guid in AssetDatabase.FindAssets("t:Mesh",new[]{Dir.TrimEnd('/')}))
        {
            string ruta=AssetDatabase.GUIDToAssetPath(guid);if(!ruta.Contains("Racion-"))continue;var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(ruta);
            if(mesh.name=="Hoja · nervadura y curvatura"||mesh.name=="Ch’arki · tira fibrosa")
            {
                var v=mesh.vertices;if(v.Length!=27)continue;var prev=mesh.triangles;var tr=new List<int>();for(int i=0;i<prev.Length;i+=12)for(int k=0;k<6;k++)tr.Add(prev[i+k]);int count=tr.Count;
                var vs=v.Concat(v).ToArray();for(int i=0;i<count;i+=3)tr.AddRange(new[]{tr[i+2]+v.Length,tr[i+1]+v.Length,tr[i]+v.Length});mesh.Clear();mesh.vertices=vs;mesh.triangles=tr.ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
            }
            if(mesh.name=="Cerámica · interior y borde"&&ruta.Contains("agua-refugio")&&mesh.bounds.size.y<.001f)
            {var tr=mesh.triangles;for(int i=0;i<tr.Length;i+=3){int a=tr[i];tr[i]=tr[i+2];tr[i+2]=a;}mesh.triangles=tr;mesh.RecalculateNormals();EditorUtility.SetDirty(mesh);}
        }
        var marca=new GameObject("Raciones y carteles · acabado 3");SceneManager.MoveGameObjectToScene(marca,e);EditorSceneManager.SaveScene(e);AssetDatabase.SaveAssets();
    }
}
