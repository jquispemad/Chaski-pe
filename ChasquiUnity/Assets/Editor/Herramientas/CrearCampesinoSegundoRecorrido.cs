using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

static class CrearCampesinoSegundoRecorrido
{
    [InitializeOnLoadMethod]
    static void Programar()
    {
        EditorApplication.delayCall+=Aplicar;
        EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredEditMode)EditorApplication.delayCall+=Aplicar;};
    }
    [MenuItem("Chasqui/Convertir agricultor en campesino")]
    public static void Aplicar()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        var escena=SceneManager.GetSceneByPath(ConstruirSegundoRecorrido.Ruta);bool cerrar=!escena.isLoaded;
        if(cerrar)escena=EditorSceneManager.OpenScene(ConstruirSegundoRecorrido.Ruta,OpenSceneMode.Additive);
        var campesino=escena.GetRootGameObjects().FirstOrDefault(o=>o.name=="Agricultor junto al cultivo");
        if(campesino==null)
        {
            campesino=escena.GetRootGameObjects().FirstOrDefault(o=>o.name=="Campesino · ropa de trabajo junto a los cultivos");
            if(campesino!=null && campesino.transform.Find("Apoyo del campesino verificado")==null)
            {Asentar(campesino,escena);EditorSceneManager.MarkSceneDirty(escena);EditorSceneManager.SaveScene(escena);}
            if(cerrar)EditorSceneManager.CloseScene(escena,true);return;
        }
        // Only this instance changes. The player and the other messengers keep their original outfit.
        foreach(var t in campesino.GetComponentsInChildren<Transform>(true))
        {
            string n=t.name.ToLowerInvariant();
            if(n.Contains("lliclla")||n.Contains("nudo de tela")||n.Contains("contacto de capa"))t.gameObject.SetActive(false);
        }
        const string dir="Assets/Niveles/SegundoRecorrido/";
        var materiales=new Dictionary<Material,Material>();int contador=0;
        foreach(var r in campesino.GetComponentsInChildren<Renderer>(true))
        {
            if(!r.enabled || !r.gameObject.activeInHierarchy)continue;
            var mats=r.sharedMaterials;
            Mesh original=null;
            var sk=r as SkinnedMeshRenderer;var mf=r.GetComponent<MeshFilter>();
            if(sk!=null)original=sk.sharedMesh;else if(mf!=null)original=mf.sharedMesh;
            if(original==null || original.subMeshCount<15)continue;
            var mesh=Object.Instantiate(original);mesh.name="Campesino · sin emblemas ni equipo de mensajero";
            foreach(int s in new[]{12,13,14})if(s<mesh.subMeshCount)mesh.SetTriangles(new int[0],s);
            // Remove the remaining decorative feather segments above the headband.
            var vertices=mesh.vertices;
            foreach(int s in new[]{7,8,11})if(s<mesh.subMeshCount)
            {
                var tris=mesh.GetTriangles(s);var conservar=new List<int>();
                for(int i=0;i<tris.Length;i+=3)
                {
                    float alto=(r.transform.TransformPoint(vertices[tris[i]]).y+r.transform.TransformPoint(vertices[tris[i+1]]).y+r.transform.TransformPoint(vertices[tris[i+2]]).y)/3-campesino.transform.position.y;
                    if(alto<1.87f)conservar.AddRange(new[]{tris[i],tris[i+1],tris[i+2]});
                }
                mesh.SetTriangles(conservar,s);
            }
            AssetDatabase.CreateAsset(mesh,dir+"CampesinoMalla"+(contador++)+".asset");
            if(sk!=null)sk.sharedMesh=mesh;else mf.sharedMesh=mesh;
            foreach(int s in new[]{6,7,8,11})
            {
                if(s>=mats.Length || mats[s]==null)continue;
                Material copia;
                if(!materiales.TryGetValue(mats[s],out copia))
                {
                    copia=new Material(mats[s]);copia.name="Campesino · tejido de trabajo "+s;
                    copia.color=s==6?new Color(.42f,.31f,.19f):s==11?new Color(.57f,.49f,.34f):new Color(.22f,.20f,.15f);
                    if(copia.HasProperty("_Glossiness"))copia.SetFloat("_Glossiness",.03f);
                    AssetDatabase.CreateAsset(copia,dir+"CampesinoTela"+materiales.Count+".mat");materiales.Add(mats[s],copia);
                }
                mats[s]=copia;
            }
            r.sharedMaterials=mats;
        }
        var terreno=escena.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Terrain>()).First();
        float z=53,x=ConstruirSegundoRecorrido.Centro(z)+5.6f;
        var td=terreno.terrainData;var origen=terreno.transform.position;
        float y=td.GetInterpolatedHeight((x-origen.x)/td.size.x,(z-origen.z)/td.size.z)+origen.y;
        campesino.transform.position=new Vector3(x,y+.04f,z);
        campesino.transform.rotation=Quaternion.Euler(0,100,0);
        Physics.SyncTransforms();
        // Clear a small standing area around him, including rocks whose wide shapes extend into it.
        int retiradas=0;
        foreach(var r in escena.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<MeshRenderer>()).Where(r=>r.name=="Roca erosionada"))
        {
            var b=r.bounds;
            var p=new Vector3(x,b.center.y,z);
            if(b.SqrDistance(p)<2.25f){r.gameObject.SetActive(false);retiradas++;}
        }
        campesino.name="Campesino · ropa de trabajo junto a los cultivos";
        var contacto=campesino.AddComponent<CapsuleCollider>();contacto.center=Vector3.up;contacto.height=1.9f;contacto.radius=.28f;
        Asentar(campesino,escena);
        EditorSceneManager.MarkSceneDirty(escena);EditorSceneManager.SaveScene(escena);AssetDatabase.SaveAssets();
        Debug.Log("CAMPESINO_LISTO: capa y equipo de mensajero retirados, ropa de trabajo marrón, suelo libre y "+retiradas+" rocas apartadas.");
        if(cerrar)EditorSceneManager.CloseScene(escena,true);
    }
    static void Asentar(GameObject o,Scene escena)
    {
        var piel=o.GetComponentsInChildren<SkinnedMeshRenderer>().FirstOrDefault(r=>r.enabled && r.sharedMesh!=null && r.sharedMesh.subMeshCount>=15);
        if(piel==null)return;
        var bake=new Mesh();piel.BakeMesh(bake);var vertices=bake.vertices;var usados=bake.triangles;
        var b=new Bounds(piel.transform.TransformPoint(vertices[usados[0]]),Vector3.zero);
        foreach(int i in usados)b.Encapsulate(piel.transform.TransformPoint(vertices[i]));
        Object.DestroyImmediate(bake);
        var terreno=escena.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Terrain>()).First();
        float z=53,x=ConstruirSegundoRecorrido.Centro(z)+5.6f;
        var d=terreno.terrainData;var p=terreno.transform.position;
        float y=d.GetInterpolatedHeight((x-p.x)/d.size.x,(z-p.z)/d.size.z)+p.y;
        o.transform.position+=new Vector3(x-b.center.x,y+.03f-b.min.y,z-b.center.z);
        var col=o.GetComponent<CapsuleCollider>();if(col!=null)col.center=o.transform.InverseTransformPoint(new Vector3(x,y+.98f,z));
        foreach(var r in escena.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<MeshRenderer>()).Where(r=>r.name=="Roca erosionada"))
            if(r.bounds.SqrDistance(new Vector3(x,r.bounds.center.y,z))<2.25f)r.gameObject.SetActive(false);
        new GameObject("Apoyo del campesino verificado").transform.SetParent(o.transform,false);
        Debug.Log("CAMPESINO_APOYO: pies sobre terreno real y colisión alineada con el cuerpo, sin rocas en 1.5 m.");
    }
}
