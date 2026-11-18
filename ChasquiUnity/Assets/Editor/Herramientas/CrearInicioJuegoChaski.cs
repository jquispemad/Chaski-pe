using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

static class CrearInicioJuegoChaski
{
    public const string Ruta="Assets/Scenes/Inicio/InicioChaski.unity";
    [InitializeOnLoadMethod]
    static void Preparar()=>EditorApplication.delayCall+=Aplicar;
    [MenuItem("Chasqui/Abrir portada y menú principal")]
    static void Abrir()
    {
        if(EditorApplication.isPlaying)return;
        Aplicar();EditorSceneManager.OpenScene(Ruta);EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(Ruta);
    }
    static void Aplicar()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        const string carpeta="Assets/Interfaz/Inicio/";
        foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{carpeta.TrimEnd('/')}))
        {
            var imp=AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as TextureImporter;
            if(imp==null || imp.maxTextureSize==2048&&!imp.mipmapEnabled)continue;
            imp.textureType=TextureImporterType.Default;imp.maxTextureSize=2048;imp.mipmapEnabled=false;imp.textureCompression=TextureImporterCompression.CompressedHQ;imp.SaveAndReimport();
        }
        if(!AssetDatabase.IsValidFolder("Assets/Scenes/Inicio"))AssetDatabase.CreateFolder("Assets/Scenes","Inicio");
        var escena=SceneManager.GetSceneByPath(Ruta);bool cerrar=!escena.isLoaded;
        if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Ruta)==null)
        {
            var anterior=SceneManager.GetActiveScene();escena=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(escena);
            var cam=new GameObject("Cámara · portada").AddComponent<Camera>();cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.black;cam.cullingMask=0;cam.gameObject.AddComponent<AudioListener>();
            new GameObject("Inicio · portada, menú e historieta").AddComponent<InicioJuegoChaski>();
            EditorSceneManager.SaveScene(escena,Ruta);if(anterior.IsValid())SceneManager.SetActiveScene(anterior);
        }
        else if(cerrar)escena=EditorSceneManager.OpenScene(Ruta,OpenSceneMode.Additive);
        var inicio=escena.GetRootGameObjects().Select(o=>o.GetComponent<InicioJuegoChaski>()).First(c=>c!=null);
        inicio.portada=AssetDatabase.LoadAssetAtPath<Texture2D>(carpeta+"Portada.png");
        inicio.menu=AssetDatabase.LoadAssetAtPath<Texture2D>(carpeta+"MenuGenerado.png");
        inicio.historieta=Enumerable.Range(1,3).Select(i=>AssetDatabase.LoadAssetAtPath<Texture2D>(carpeta+"Historieta0"+i+".png")).ToArray();
        inicio.musica=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Nivel01/Musica/Pan-mart.ogg");inicio.click=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Nivel01/Interfaz/Confirmar.ogg");
        EditorSceneManager.MarkSceneDirty(escena);EditorSceneManager.SaveScene(escena);
        if(cerrar)EditorSceneManager.CloseScene(escena,true);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Ruta,true)}.Concat(EditorBuildSettings.scenes.Where(s=>s.path!=Ruta)).ToArray();
        EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(Ruta);
        Debug.Log("INICIO_CHASKI_LISTO: portada, menú, historia, tres páginas de historieta, opciones y niveles conectados.");
    }
}
