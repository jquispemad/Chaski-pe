using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AsentarRocasDelRioNivelTres
{
    [InitializeOnLoadMethod]static void Preparar()=>EditorApplication.delayCall+=Aplicar;
    [MenuItem("Chasqui/Asentar rocas del río")]
    public static void Aplicar()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        var e=SceneManager.GetSceneByPath(ConstruirValleVientos.Ruta);bool cerrar=!e.isLoaded;if(cerrar)e=EditorSceneManager.OpenScene(ConstruirValleVientos.Ruta,OpenSceneMode.Additive);
        if(e.GetRootGameObjects().Any(o=>o.name=="Río · rocas sumergidas y orillas sin flotantes")){if(cerrar)EditorSceneManager.CloseScene(e,true);return;}
        var todos=e.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Transform>()).ToArray();var terreno=todos.First(o=>o.GetComponent<Terrain>()!=null).GetComponent<Terrain>();var col=terreno.GetComponent<TerrainCollider>();
        var rocas=todos.Where(t=>t.name=="Roca · abrigo y paisaje"&&Mathf.Abs(t.position.z-NivelTresChaski.CentroAgua(t.position.x))<16&&!(Mathf.Abs(t.position.x-NivelTresChaski.Centro(t.position.z))<3&&t.position.z>272&&t.position.z<291)).OrderBy(t=>t.position.x).ThenBy(t=>t.position.z).ToArray();
        var azar=new System.Random(1800281);int dentro=0,fuera=0,fallos=0;
        for(int i=0;i<rocas.Length;i++)
        {
            var p=rocas[i];bool orilla=i%9==0;float x=-100+(i+.25f+(float)azar.NextDouble()*.5f)*172/rocas.Length;
            // Keep the playable stepping-stone crossing clear.
            float cruce=NivelTresChaski.Centro(281);if(Mathf.Abs(x-cruce)<5)x+=x<cruce?-5:5;
            float ancho=NivelTresChaski.AnchoAgua(x),lado=(i%2==0?-1:1)*(orilla?ancho+.45f:ancho*(.12f+(float)azar.NextDouble()*.48f));
            var mesh=p.GetComponent<MeshFilter>().sharedMesh;var b=mesh.bounds;float w=.7f+(float)azar.NextDouble()*.7f,h=.65f+(float)azar.NextDouble()*.5f,l=.65f+(float)azar.NextDouble()*.8f;
            p.SetPositionAndRotation(new Vector3(x,0,NivelTresChaski.CentroAgua(x)+lado),Quaternion.Euler((float)azar.NextDouble()*8-4,(float)azar.NextDouble()*360,(float)azar.NextDouble()*8-4));p.localScale=new Vector3(w/b.size.x,h/b.size.y,l/b.size.z);
            float suelo=float.PositiveInfinity;for(int a=-1;a<=1;a++)for(int c=-1;c<=1;c++){var q=p.position+new Vector3(a*w*.4f,80,c*l*.4f);if(col.Raycast(new Ray(q,Vector3.down),out var hit,160))suelo=Mathf.Min(suelo,hit.point.y);}
            float baseRoca=float.PositiveInfinity;foreach(var v in mesh.vertices)baseRoca=Mathf.Min(baseRoca,p.TransformPoint(v).y);
            if(float.IsInfinity(suelo)){fallos++;continue;}p.position+=Vector3.up*(suelo-baseRoca-.16f);
            p.name=orilla?"Roca de ribera · base enterrada":"Roca del cauce · parcialmente sumergida";if(orilla)fuera++;else dentro++;
            if(p.GetComponent<Renderer>().bounds.min.y>suelo+.01f)fallos++;
        }
        var marca=new GameObject("Río · rocas sumergidas y orillas sin flotantes");SceneManager.MoveGameObjectToScene(marca,e);EditorSceneManager.SaveScene(e);AssetDatabase.SaveAssets();
        System.IO.File.WriteAllText("Temp/nivel03_rocas_asentadas.txt",$"Rocas dentro del cauce: {dentro}\nRocas en la orilla: {fuera}\nBases sin apoyo: {fallos}\nApoyos del cruce conservados: {todos.Count(t=>t.name=="Piedra del arroyo · superficie de apoyo")}\n");Debug.Log($"ROCAS_RIO_ASENTADAS: {dentro} en el cauce, {fuera} en orillas, {fallos} bases sin apoyo.");if(cerrar)EditorSceneManager.CloseScene(e,true);
    }
}
