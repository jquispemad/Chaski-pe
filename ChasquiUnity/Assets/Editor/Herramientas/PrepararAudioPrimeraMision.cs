using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

static class PrepararAudioPrimeraMision
{
    const string Base = "Assets/Audio/Nivel01/";
    [InitializeOnLoadMethod]
    static void Iniciar() => EditorApplication.delayCall += Aplicar;

    [MenuItem("Chasqui/Preparar audio del nivel 1")]
    public static void Aplicar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        const string ruta = "Assets/Scenes/Campana/Nivel01_PrimeraMision.unity";
        var escena = SceneManager.GetSceneByPath(ruta);
        bool cerrar = !escena.isLoaded;
        if (cerrar) escena = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Additive);
        var mision = escena.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<PrimeraMisionChaski>(true)).FirstOrDefault();
        if (mision == null) { if (cerrar) EditorSceneManager.CloseScene(escena, true); return; }
        if (mision.GetComponent<AudioPrimeraMision>() != null) { if (cerrar) EditorSceneManager.CloseScene(escena, true); return; }
        foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { Base.TrimEnd('/') }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null) continue;
            var ajustes = importer.defaultSampleSettings;
            ajustes.loadType = path.Contains("/Musica/") || path.EndsWith("Viento-arboles-aves.mp3")
                ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            ajustes.compressionFormat = AudioCompressionFormat.Vorbis;
            ajustes.quality = .7f;
            importer.defaultSampleSettings = ajustes;
            importer.SaveAndReimport();
        }
        var audio = mision.gameObject.AddComponent<AudioPrimeraMision>();
        audio.musica = Clip("Musica/Pan-mart.ogg");
        audio.ambiente = Clip("Ambiente/Viento-arboles-aves.mp3");
        audio.pututu = Clip("Pututu/Caracola-RoofDog.mp3");
        audio.confirmar = Clip("Interfaz/Confirmar.ogg");
        audio.abrirPanel = Clip("Interfaz/Abrir_panel.ogg");
        audio.pasosTierra = Serie("Pasos/Tierra/Paso_", 5);
        audio.pasosPiedra = Serie("Pasos/Piedra/Paso_", 5);
        audio.pasosMadera = Serie("Pasos/Madera/Paso_", 5);
        audio.tela = Serie("Ropa/Tela_", 4);
        audio.impulso = Serie("Movimiento/Impulso_salto_", 2);
        audio.aterrizaje = Serie("Movimiento/Aterrizaje_suave_", 3);
        audio.caida = Serie("Movimiento/Caida_fuerte_", 2);
        audio.quipu = Serie("Quipu/Entrega_quipu_", 2);
        if (mision.vista != null)
        {
            var receptor = mision.vista.GetComponent<AudioListener>();
            if (receptor == null) receptor = mision.vista.gameObject.AddComponent<AudioListener>();
            receptor.enabled = true;
        }
        EditorUtility.audioMasterMute = false;
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        if (cerrar) EditorSceneManager.CloseScene(escena, true);
        Debug.Log("AUDIO_NIVEL01_GUARDADO: clips conectados, receptor activo y editor sin silenciar.");
    }

    static AudioClip Clip(string archivo) => AssetDatabase.LoadAssetAtPath<AudioClip>(Base + archivo);
    [MenuItem("Chasqui/Comprobar audio del nivel 1")]
    static void Comprobar()
    {
        var audio = Object.FindFirstObjectByType<AudioPrimeraMision>();
        if (!EditorApplication.isPlaying || audio == null)
        { Debug.LogWarning("AUDIO: inicia Play en el nivel 1 para comprobar la salida."); return; }
        var fuentes = audio.GetComponentsInChildren<AudioSource>();
        int activas = fuentes.Count(f => f.isPlaying && !f.mute && f.volume > 0);
        var muestras = new float[1024];
        AudioListener.GetOutputData(muestras, 0);
        float pico = muestras.Max(v => Mathf.Abs(v));
        Debug.Log($"AUDIO_COMPROBADO: fuentes activas={activas}, señal={pico:F5}, volumen={AudioListener.volume}, pausa={AudioListener.pause}, silencio editor={EditorUtility.audioMasterMute}, receptores={Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(l => l.isActiveAndEnabled)}");
    }
    static AudioClip[] Serie(string prefijo, int cantidad) => Enumerable.Range(1, cantidad)
        .Select(i => Clip(prefijo + i.ToString("00") + ".ogg")).Where(c => c != null).ToArray();
}
