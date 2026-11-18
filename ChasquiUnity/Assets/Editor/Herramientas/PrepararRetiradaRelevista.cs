using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

static class PrepararRetiradaRelevista
{
    [InitializeOnLoadMethod]
    static void Iniciar()
    {
        EditorApplication.delayCall+=Aplicar;
        EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredEditMode)EditorApplication.delayCall+=Aplicar;};
    }
    [MenuItem("Chasqui/Preparar regreso del relevista a casa")]
    public static void Aplicar()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        var escena=SceneManager.GetSceneByPath(ConstruirSegundoRecorrido.Ruta);bool cerrar=!escena.isLoaded;
        if(cerrar)escena=EditorSceneManager.OpenScene(ConstruirSegundoRecorrido.Ruta,OpenSceneMode.Additive);
        var intro=escena.GetRootGameObjects().Select(o=>o.GetComponent<EntregaInicialChaski>()).FirstOrDefault(i=>i!=null);
        if(intro==null || intro.puerta!=null){if(cerrar)EditorSceneManager.CloseScene(escena,true);return;}
        var casa=escena.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Transform>()).First(t=>t.name=="Chaskiwasi de salida");
        var p=casa.position;
        var bisagra=new GameObject("Puerta de la casa · bisagra animada").transform;bisagra.SetParent(casa,false);
        bisagra.position=p+new Vector3(-.82f,.3f,-3.32f);
        var hoja=GameObject.CreatePrimitive(PrimitiveType.Cube);hoja.name="Puerta de madera · hoja con colisión";hoja.transform.SetParent(bisagra,false);
        hoja.transform.localPosition=new Vector3(.82f,1.06f,0);hoja.transform.localScale=new Vector3(1.64f,2.12f,.14f);
        hoja.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Niveles/Valle/Madera.mat");
        foreach(float y in new[]{.4f,1.6f})
        {
            var travesaño=GameObject.CreatePrimitive(PrimitiveType.Cube);travesaño.name="Travesaño de madera";travesaño.transform.SetParent(bisagra,false);
            travesaño.transform.localPosition=new Vector3(.82f,y,-.1f);travesaño.transform.localScale=new Vector3(1.5f,.12f,.09f);
            travesaño.GetComponent<Renderer>().sharedMaterial=hoja.GetComponent<Renderer>().sharedMaterial;Object.DestroyImmediate(travesaño.GetComponent<Collider>());
        }
        var acceso=new GameObject("Entrada de la casa · punto exterior").transform;acceso.SetParent(casa,false);acceso.position=p+new Vector3(0,.03f,-3.9f);
        var interior=new GameObject("Interior de la casa · fin del recorrido").transform;interior.SetParent(casa,false);interior.position=p+new Vector3(.3f,.33f,-2.5f);
        intro.puerta=bisagra;intro.accesoCasa=acceso;intro.interiorCasa=interior;
        intro.crujidoPuerta=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Nivel01/Ambiente/Madera_cruje_01.ogg");
        intro.cierrePuerta=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Nivel01/Pasos/Madera/Paso_01.ogg");
        EditorSceneManager.MarkSceneDirty(escena);EditorSceneManager.SaveScene(escena);
        Debug.Log("RELEVISTA_REGRESO_PREPARADO: puerta con bisagra, entrada despejada, retirada caminando y cierre conectados.");
        if(cerrar)EditorSceneManager.CloseScene(escena,true);
    }
}
