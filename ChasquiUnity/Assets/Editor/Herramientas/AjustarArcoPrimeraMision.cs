using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

static class AjustarArcoPrimeraMision
{
    const string Ruta = "Assets/Scenes/Campana/Nivel01_PrimeraMision.unity";
    const string Carpeta = "Assets/Niveles/PrimeraMision/";

    [InitializeOnLoadMethod]
    static void Preparar() => EditorApplication.delayCall += Aplicar;

    static void Aplicar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            AssetDatabase.LoadAssetAtPath<SceneAsset>(Ruta) == null) return;
        var escena = SceneManager.GetSceneByPath(Ruta);
        bool cerrar = !escena.isLoaded;
        if (cerrar) escena = EditorSceneManager.OpenScene(Ruta, OpenSceneMode.Additive);
        var pueblo = escena.GetRootGameObjects().FirstOrDefault(o => o.name.StartsWith("Pueblo"));
        var piezas = pueblo == null ? new Transform[0] : pueblo.GetComponentsInChildren<Transform>();
        var viga = piezas.FirstOrDefault(t => t.name.StartsWith("Viga · mantener"));
        if (viga != null)
        {
            var arco = new GameObject("Arco diagonal · agacharse");
            arco.transform.SetParent(pueblo.transform);
            arco.transform.position = new Vector3(0,0,116);
            viga.SetParent(arco.transform);
            foreach(var soporte in piezas.Where(t => t.name == "Soporte de viga"))
                soporte.SetParent(arco.transform);
            arco.transform.rotation = Quaternion.Euler(0,-36,0);
            viga.name = "Dintel de paso bajo";
            foreach(var texto in escena.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<TextMesh>()))
                if(texto.text.StartsWith("PASO BAJO")) texto.text="AGÁCHATE\nMANTÉN C";

            var maiz = piezas.Where(t => t.name.StartsWith("Maíz · tallo")).ToArray();
            foreach(var sector in maiz.GroupBy(t => Mathf.FloorToInt(t.position.z/24)))
                Combinar(sector.ToArray(), sector.Key, pueblo.transform);
            foreach(var render in escena.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<MeshRenderer>()))
                if(render.name.StartsWith("Ichu") || (render.transform.position.x > 16f &&
                    (render.name.StartsWith("Queñua") || render.name.StartsWith("Roca"))))
                    render.shadowCastingMode=ShadowCastingMode.Off;

            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            AssetDatabase.SaveAssets();
            Debug.Log("ARCO_Y_RENDER_CHASKI: arco diagonal y maíz agrupado por tramos.");
        }
        if (pueblo != null && pueblo.transform.Find("Paso bajo y obstáculos del sendero") == null)
        {
            var grupo = new GameObject("Paso bajo y obstáculos del sendero").transform;
            grupo.SetParent(pueblo.transform, false);
            var arco = pueblo.transform.Find("Arco diagonal · agacharse");
            foreach (var collider in arco.GetComponentsInChildren<BoxCollider>())
                collider.enabled = false;
            Obstaculo(grupo, "Colisión recta del dintel", new Vector3(0,1.87f,116), new Vector3(1.9f,.44f,.75f));

            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Personajes/Chasqui/MaterialPiedra.mat");
            float[] posiciones = { 62f, 92f, 145f, 173f, 211f };
            for (int i=0;i<posiciones.Length;i++)
            {
                float alto = .42f + .08f*(i%3);
                var malla = AssetDatabase.LoadAssetAtPath<Mesh>($"Assets/Niveles/Valle/Roca{i%4}.asset");
                Vector3 tamaño = new Vector3(1.05f,alto,1.13f);
                var roca = new GameObject("Piedra para saltar").transform;
                roca.SetParent(grupo,false);
                roca.localScale = new Vector3(tamaño.x/malla.bounds.size.x,tamaño.y/malla.bounds.size.y,tamaño.z/malla.bounds.size.z);
                roca.localPosition = new Vector3(0,alto*.5f,posiciones[i])-Vector3.Scale(malla.bounds.center,roca.localScale);
                roca.gameObject.AddComponent<MeshFilter>().sharedMesh = malla;
                roca.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
                Obstaculo(grupo, "Colisión de piedra", new Vector3(0,alto*.45f,posiciones[i]),new Vector3(.92f,alto*.90f,.88f));
            }
            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            Debug.Log("EJE_SENDERO_CHASKI: paso bajo y cinco piedras guardados.");
        }
        if (pueblo != null && pueblo.transform.Find("Obstáculos adicionales") == null)
        {
            var grupo = new GameObject("Obstáculos adicionales").transform;
            grupo.SetParent(pueblo.transform,false);
            var modeloArco = pueblo.transform.Find("Arco diagonal · agacharse");
            foreach(float z in new[]{78f,157f,224f})
            {
                var copia=Object.Instantiate(modeloArco.gameObject,grupo);
                copia.name="Arco diagonal de práctica";
                copia.transform.position=new Vector3(0,0,z);
                Obstaculo(grupo,"Paso bajo",new Vector3(0,1.87f,z),new Vector3(1.9f,.44f,.75f));
            }
            var modelo=pueblo.GetComponentsInChildren<Transform>().First(t=>t.name=="Piedra para saltar");
            foreach(float z in new[]{49f,101f,132f,197f,235f})
            {
                var copia=Object.Instantiate(modelo.gameObject,grupo);
                copia.name="Piedra del trayecto";
                var posicion=copia.transform.position;
                posicion.z=z;
                copia.transform.position=posicion;
                Obstaculo(grupo,"Colisión de piedra adicional",new Vector3(0,.19f,z),new Vector3(.92f,.38f,.88f));
            }
            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            Debug.Log("OBSTACULOS_ADICIONALES_LISTOS: tres arcos y cinco piedras sobre el eje.");
        }
        if (cerrar) EditorSceneManager.CloseScene(escena,true);
    }

    static void Obstaculo(Transform grupo, string nombre, Vector3 posicion, Vector3 tamaño)
    {
        var o = new GameObject(nombre);
        o.transform.SetParent(grupo,false);
        o.transform.localPosition = posicion;
        o.AddComponent<BoxCollider>().size = tamaño;
    }

    static void Combinar(Transform[] plantas, int sector, Transform pueblo)
    {
        var piezas = plantas.SelectMany(p=>p.GetComponentsInChildren<MeshFilter>())
            .Where(m=>m.sharedMesh!=null && m.GetComponent<MeshRenderer>().enabled).ToArray();
        if(piezas.Length==0)return;
        var grupo = new GameObject($"Maíz · tramo {sector}");
        grupo.transform.SetParent(pueblo);
        grupo.transform.position=new Vector3(0,0,sector*24);
        var aGrupo=grupo.transform.worldToLocalMatrix;
        var malla=new Mesh{name=grupo.name,indexFormat=IndexFormat.UInt32};
        malla.CombineMeshes(piezas.Select(m=>new CombineInstance
        {
            mesh=m.sharedMesh,
            transform=aGrupo*m.transform.localToWorldMatrix
        }).ToArray());
        AssetDatabase.CreateAsset(malla,Carpeta+$"MaizTramo{sector}.asset");
        grupo.AddComponent<MeshFilter>().sharedMesh=malla;
        var render=grupo.AddComponent<MeshRenderer>();
        render.sharedMaterial=piezas[0].GetComponent<MeshRenderer>().sharedMaterial;
        render.shadowCastingMode=ShadowCastingMode.Off;
        foreach(var planta in plantas) Object.DestroyImmediate(planta.gameObject);
    }
}
