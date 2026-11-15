using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;

public static class CorregirZancadaChasqui
{
    const string Folder = "Assets/Personajes/Chasqui";
    static bool EsPierna(string path)
    {
        string n = path.Split('/').Last().Replace('.','_').ToLowerInvariant();
        return new[]{"thigh_l","thigh_r","shin_l","shin_r","foot_l","foot_r"}.Contains(n);
    }
    static AnimationClip Adelante(AnimationClip source,string name)
    {
        var copy = UnityEngine.Object.Instantiate(source); copy.name = name;
        int changed = 0;
        foreach (var binding in AnimationUtility.GetCurveBindings(source))
        {
            if (!EsPierna(binding.path)) continue;
            var original = AnimationUtility.GetEditorCurve(source,binding);
            var keys = original.keys.Select(k =>
            {
                var reversed = new Keyframe(source.length-k.time,k.value,-k.outTangent,-k.inTangent,k.outWeight,k.inWeight);
                var mode = WeightedMode.None;
                if ((k.weightedMode & WeightedMode.In) != 0) mode |= WeightedMode.Out;
                if ((k.weightedMode & WeightedMode.Out) != 0) mode |= WeightedMode.In;
                reversed.weightedMode = mode;
                return reversed;
            }).OrderBy(k=>k.time).ToArray();
            var curve = new AnimationCurve(keys) { preWrapMode = original.preWrapMode, postWrapMode = original.postWrapMode };
            AnimationUtility.SetEditorCurve(copy,binding,curve); changed++;
        }
        if (changed == 0) throw new Exception("No se encontraron curvas de piernas en " + source.name);
        copy.EnsureQuaternionContinuity();
        var settings = AnimationUtility.GetAnimationClipSettings(copy); settings.loopTime = true; settings.loopBlend = true;
        AnimationUtility.SetAnimationClipSettings(copy,settings);
        string path = Folder + "/" + name + ".anim";
        var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (existing == null) { AssetDatabase.CreateAsset(copy,path); existing = copy; }
        else { EditorUtility.CopySerialized(copy,existing); UnityEngine.Object.DestroyImmediate(copy); EditorUtility.SetDirty(existing); }
        Debug.Log("ZANCADA_CORREGIDA " + name + " curvas=" + changed);
        return existing;
    }
    static float Apoyo(AnimationClip clip,GameObject model)
    {
        var feet = model.GetComponentsInChildren<Transform>().Where(t=>t.name.Replace('.','_').StartsWith("foot_",StringComparison.OrdinalIgnoreCase)).ToArray();
        if (feet.Length < 2) throw new Exception("Faltan huesos de pie para comprobar el apoyo.");
        const int samples=96;
        var positions = new Vector3[feet.Length,samples+1];
        for (int i=0;i<=samples;i++)
        {
            clip.SampleAnimation(model,clip.length*i/samples);
            for (int f=0;f<feet.Length;f++) positions[f,i]=feet[f].position;
        }
        float sum=0; int count=0;
        for(int f=0;f<feet.Length;f++)
        {
            float min=Enumerable.Range(0,samples+1).Min(i=>positions[f,i].y);
            for(int i=1;i<samples;i++)
                if(positions[f,i].y<min+.035f && positions[f,i-1].y<min+.035f)
                { sum += (positions[f,i].z-positions[f,i-1].z)/(clip.length/samples); count++; }
        }
        if(count==0) throw new Exception("No se pudo medir la fase de apoyo.");
        return sum/count;
    }
    [MenuItem("Chasqui/Corregir zancada y carrera automatica")]
    public static void Aplicar()
    {
        if(EditorApplication.isPlaying) throw new Exception("Sal de Play antes de corregir los clips.");
        var clips=AssetDatabase.LoadAllAssetsAtPath(Folder+"/ChasquiAnimado.fbx").OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview")).ToArray();
        var walkSource=clips.First(c=>c.name=="Caminar"); var runSource=clips.First(c=>c.name=="Correr");
        var walk=Adelante(walkSource,"CaminarAdelante"); var run=Adelante(runSource,"CorrerAdelante");
        var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(Folder+"/Chasqui.controller");
        foreach(var child in controller.layers[0].stateMachine.states)
        {
            var blend=child.state.motion as BlendTree; if(blend==null)continue;
            var motions=blend.children;
            for(int i=0;i<motions.Length;i++)
            {
                if(motions[i].motion.name.IndexOf("Caminar",StringComparison.OrdinalIgnoreCase)>=0) motions[i].motion=walk;
                if(motions[i].motion.name.IndexOf("Correr",StringComparison.OrdinalIgnoreCase)>=0) motions[i].motion=run;
            }
            blend.children=motions;EditorUtility.SetDirty(blend);
        }
        var movement=UnityEngine.Object.FindFirstObjectByType<ChasquiMovimiento>();
        if(movement==null)throw new Exception("No se encuentra el jugador.");
        var properties=new SerializedObject(movement);properties.FindProperty("duracionPasoBreve").floatValue=.18f;properties.ApplyModifiedPropertiesWithoutUndo();
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"/ChasquiAnimado.fbx");
        var sample=UnityEngine.Object.Instantiate(prefab);sample.hideFlags=HideFlags.HideAndDontSave;
        var animator=sample.GetComponent<Animator>();if(animator!=null)animator.enabled=false;
        float oldWalk=Apoyo(walkSource,sample),newWalk=Apoyo(walk,sample),oldRun=Apoyo(runSource,sample),newRun=Apoyo(run,sample);
        UnityEngine.Object.DestroyImmediate(sample);
        string report="Caminar antes="+oldWalk.ToString("F3")+" despues="+newWalk.ToString("F3")+"\nCorrer antes="+oldRun.ToString("F3")+" despues="+newRun.ToString("F3");
        File.WriteAllText(Path.Combine(Path.GetTempPath(),"chasqui_comprobar_zancada.txt"),report);
        Debug.Log("APOYO_PIES "+report);
        if(newWalk>=0 || newRun>=0)throw new Exception("El pie debe retroceder respecto al cuerpo durante su apoyo al avanzar.");
        // Sincronizar la fase de apoyo con el desplazamiento real del controlador.
        float velocidadPaso=properties.FindProperty("velocidad").floatValue;
        float velocidadCarrera=properties.FindProperty("velocidadCorrer").floatValue;
        foreach(var state in controller.layers[0].stateMachine.states)
        {
            var tree=state.state.motion as BlendTree;if(tree==null)continue;
            var children=tree.children;
            for(int i=0;i<children.Length;i++)
            {
                if(children[i].motion.name.Contains("Caminar"))children[i].timeScale=Mathf.Clamp(velocidadPaso/Mathf.Abs(newWalk),.5f,5f);
                if(children[i].motion.name.Contains("Correr"))children[i].timeScale=Mathf.Clamp(velocidadCarrera/Mathf.Abs(newRun),.5f,5f);
            }
            tree.children=children;EditorUtility.SetDirty(tree);
        }

        EditorUtility.SetDirty(controller);EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
        PrefabUtility.SaveAsPrefabAsset(movement.gameObject,Folder+"/ChasquiJugador.prefab");
        Debug.Log("CARRERA_AUTOMATICA_LISTA: toque breve = paso; mantener = correr.");
    }
}
