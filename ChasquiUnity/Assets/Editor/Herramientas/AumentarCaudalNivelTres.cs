using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

public static class AumentarCaudalNivelTres
{
    [InitializeOnLoadMethod]static void Preparar()=>EditorApplication.delayCall+=Aplicar;
    public static void Aplicar()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||AssetDatabase.LoadAssetAtPath<SceneAsset>(ConstruirValleVientos.Ruta)==null)return;
        var e=SceneManager.GetSceneByPath(ConstruirValleVientos.Ruta);bool cerrar=!e.isLoaded;if(cerrar)e=EditorSceneManager.OpenScene(ConstruirValleVientos.Ruta,OpenSceneMode.Additive);
        if(e.GetRootGameObjects().Any(o=>o.name=="Arroyo · caudal alto y piedras irregulares")){if(cerrar)EditorSceneManager.CloseScene(e,true);return;}
        var todos=e.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Transform>()).ToArray();var terreno=todos.First(o=>o.GetComponent<Terrain>()!=null).GetComponent<Terrain>();var d=terreno.terrainData;int n=d.heightmapResolution;var h=d.GetHeights(0,0,n,n);
        for(int z=0;z<n;z++)for(int x=0;x<n;x++)
        {
            float xx=-110+x*220f/(n-1),zz=-20+z*550f/(n-1);if(xx>88)continue;float centro=NivelTresChaski.CentroAgua(xx),ancho=Mathf.Max(.1f,NivelTresChaski.AnchoAgua(xx)),dist=Mathf.Abs(zz-centro);if(dist>ancho+7)continue;
            float agua=NivelTresChaski.AguaY(xx),y;
            if(dist<ancho)y=agua+.06f-.95f*Mathf.Sqrt(Mathf.Clamp01(1-dist*dist/(ancho*ancho)));
            else{float exterior=terreno.SampleHeight(new Vector3(xx,0,centro+Mathf.Sign(zz-centro)*(ancho+7)))+terreno.transform.position.y;y=Mathf.Lerp(agua+.06f,Mathf.Max(agua+.1f,exterior),Mathf.SmoothStep(0,1,(dist-ancho)/7));}
            h[z,x]=(y+10)/65;
        }
        d.SetHeights(0,0,h);d.SyncHeightmap();terreno.Flush();Physics.SyncTransforms();EditorUtility.SetDirty(d);
        var rio=todos.First(t=>t.name.StartsWith("Arroyo · naciente")).GetComponent<MeshFilter>();rio.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);rio.transform.localScale=Vector3.one;
        var mesh=rio.sharedMesh;var vs=mesh.vertices;var col=terreno.GetComponent<TerrainCollider>();
        for(int i=0;i<vs.Length;i++)
        {
            float x=-110+(i/9)*.5f,u=(i%9)/8f;var q=new Vector3(x,NivelTresChaski.AguaY(x),NivelTresChaski.CentroAgua(x)+(u*2-1)*NivelTresChaski.AnchoAgua(x));
            if(i%9==0||i%9==8)if(col.Raycast(new Ray(q+Vector3.up*60,Vector3.down),out var hit,120))q.y=Mathf.Min(q.y,hit.point.y+.012f);vs[i]=q;
        }
        mesh.vertices=vs;mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
        var random=new System.Random(281);var apoyos=todos.Where(o=>o.name=="Piedra del arroyo · superficie de apoyo").OrderBy(o=>o.position.z).ToArray();float[] desplazamiento={-.1f,-.45f,.28f,.53f,-.16f,-.42f,.36f,-.22f,.12f};float min=float.PositiveInfinity;
        for(int i=0;i<apoyos.Length;i++)
        {
            var p=apoyos[i];var roca=todos.FirstOrDefault(t=>t.name=="Roca · abrigo y paisaje"&&Mathf.Abs(t.position.z-p.position.z)<.06f&&Mathf.Abs(t.position.x-p.position.x)<.06f);Vector3 cambio=new Vector3(desplazamiento[i%desplazamiento.Length],0,(float)(random.NextDouble()-.5)*.25f);float giro=(float)(random.NextDouble()-.5)*24;
            p.position+=cambio;p.rotation=Quaternion.Euler(0,giro,0);if(roca!=null){roca.position+=cambio;roca.rotation=Quaternion.Euler(0,giro,0);}
            float alto=p.GetComponent<Collider>().bounds.max.y;min=Mathf.Min(min,alto-NivelTresChaski.AguaY(p.position.x));
        }
        int cuenta=0;
        foreach(var p in todos.Where(t=>t.name=="Roca · abrigo y paisaje"&&Mathf.Abs(t.position.z-NivelTresChaski.CentroAgua(t.position.x))<14))
        {
            if(Mathf.Abs(p.position.x-NivelTresChaski.Centro(p.position.z))<3&&p.position.z>272&&p.position.z<291)continue;
            float x=p.position.x+(float)(random.NextDouble()-.5)*4;float ancho=NivelTresChaski.AnchoAgua(x);float lado=(cuenta++%2==0?-1:1)*(ancho*.55f+(float)random.NextDouble()*3.2f);
            p.position=new Vector3(x,0,NivelTresChaski.CentroAgua(x)+lado);p.rotation=Quaternion.Euler(0,(float)random.NextDouble()*360,0);p.localScale*=.75f+(float)random.NextDouble()*.5f;
            float diferencia=float.PositiveInfinity;foreach(var v in p.GetComponent<MeshFilter>().sharedMesh.vertices){var q=p.TransformPoint(v);if(col.Raycast(new Ray(q+Vector3.up*80,Vector3.down),out var hit,160))diferencia=Mathf.Min(diferencia,q.y-hit.point.y);}if(!float.IsInfinity(diferencia))p.position-=Vector3.up*(diferencia+.06f);
        }
        var marca=new GameObject("Arroyo · caudal alto y piedras irregulares");SceneManager.MoveGameObjectToScene(marca,e);EditorSceneManager.SaveScene(e);AssetDatabase.SaveAssets();
        System.IO.File.WriteAllText("Temp/nivel03_caudal.txt",$"Nivel del agua en el cruce: {NivelTresChaski.AguaY(-10):F2} m\nMargen mínimo de los apoyos: {min:F2} m\nApoyos: {apoyos.Length}; rocas distribuidas: {cuenta}\n");Debug.Log("CAUDAL_NIVEL03: agua más ancha y alta; apoyos secos con margen mínimo "+min.ToString("F2")+" m.");if(cerrar)EditorSceneManager.CloseScene(e,true);
    }
}
