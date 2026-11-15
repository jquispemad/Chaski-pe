using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;

public static class ConfigurarAnimacionesChasqui
{
    const string Folder = "Assets/Personajes/Chasqui";
    [MenuItem("Chasqui/Activar animaciones del personaje")]
    public static void Aplicar()
    {
        if (EditorApplication.isPlaying) throw new Exception("Sal de Play para instalar las animaciones.");
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/Chasqui.unity")
            throw new Exception("Abre la escena Chasqui.");
        if (UnityEngine.Object.FindFirstObjectByType<ChasquiMovimiento>() == null) ConfigurarControlesChasqui.Aplicar();
        string modelPath = Folder + "/ChasquiAnimado.fbx";
        AssetDatabase.ImportAsset(modelPath, ImportAssetOptions.ForceSynchronousImport);
        var importer = (ModelImporter)AssetImporter.GetAtPath(modelPath);
        importer.animationType = ModelImporterAnimationType.Generic;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.motionNodeName = "root";
        importer.importAnimation = true;
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        importer.SaveAndReimport();
        var defaults = importer.defaultClipAnimations;
        foreach (var clip in defaults)
        {
            foreach (var kind in new[] { "Caminar", "Correr", "Saltar", "Reposo" })
                if (clip.name.IndexOf(kind,StringComparison.OrdinalIgnoreCase) >= 0) clip.name = kind;
            clip.loopTime = clip.name != "Saltar";
            clip.loopPose = clip.name != "Saltar";
        }
        importer.clipAnimations = defaults; importer.SaveAndReimport();
        var clips = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview")).ToArray();
        Debug.Log("CLIPS_CHASQUI: " + string.Join(", ",clips.Select(c => c.name + "=" + c.length)));
        Func<string,AnimationClip> buscar = kind => clips.FirstOrDefault(c => c.name.IndexOf(kind,StringComparison.OrdinalIgnoreCase) >= 0)
            ?? throw new Exception("Falta clip " + kind);
        var idle = buscar("Reposo"); var walk = buscar("Caminar"); var run = buscar("Correr"); var jump = buscar("Saltar");
        string controllerPath = Folder + "/Chasqui.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter("Movimiento",AnimatorControllerParameterType.Float);
            controller.AddParameter("EnSuelo",AnimatorControllerParameterType.Bool);
            controller.AddParameter("Saltar",AnimatorControllerParameterType.Trigger);
            var sm = controller.layers[0].stateMachine;
            var locomotion = sm.AddState("Caminar y correr",new Vector3(250,60,0));
            var blend = new BlendTree { name = "Reposo - caminar - correr", blendType = BlendTreeType.Simple1D, blendParameter = "Movimiento", useAutomaticThresholds = false };
            AssetDatabase.AddObjectToAsset(blend,controller);
            blend.AddChild(idle,0); blend.AddChild(walk,.52f); blend.AddChild(run,1);
            locomotion.motion = blend; sm.defaultState = locomotion;
            var jumping = sm.AddState("Salto",new Vector3(250,190,0)); jumping.motion = jump;
            jumping.speed = jump.length / .70f;
            var enter = sm.AddAnyStateTransition(jumping); enter.hasExitTime = false; enter.duration = .06f; enter.canTransitionToSelf = false;
            enter.AddCondition(AnimatorConditionMode.If,0,"Saltar");
            var land = jumping.AddTransition(locomotion); land.hasExitTime = false; land.duration = .10f;
            land.AddCondition(AnimatorConditionMode.If,0,"EnSuelo");
        }
        var movement = UnityEngine.Object.FindFirstObjectByType<ChasquiMovimiento>();
        var visual = movement.visual;
        // Sustituir la instancia visual estática; conservar controlador, cámara y posición.
        foreach (Transform child in visual.Cast<Transform>().ToArray()) UnityEngine.Object.DestroyImmediate(child.gameObject);
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(model,visual);
        instance.name = "Chasqui Animado";
        instance.transform.localPosition = Vector3.zero;
        var animator = instance.GetComponent<Animator>(); if (animator == null) animator = instance.AddComponent<Animator>();
        animator.avatar = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<Avatar>().FirstOrDefault();
        animator.runtimeAnimatorController = controller; animator.applyRootMotion = false; animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        movement.animador = animator;
        var serialized = new SerializedObject(movement);
        serialized.FindProperty("velocidad").floatValue = 2.5f; serialized.FindProperty("velocidadCorrer").floatValue = 4.8f; serialized.ApplyModifiedPropertiesWithoutUndo();
        var materials = AssetDatabase.FindAssets("t:Material",new[] { Folder + "/Materiales" })
            .Select(g => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
        int applied = 0, missing = 0;
        foreach (var r in instance.GetComponentsInChildren<Renderer>())
        {
            var slots = r.sharedMaterials;
            for (int i=0;i<slots.Length;i++)
            {
                string n = slots[i] != null ? slots[i].name : "";
                var replacement = materials.FirstOrDefault(m => Normalizar(m.name) == Normalizar(n));
                if (replacement != null) { slots[i] = replacement; applied++; } else { missing++; Debug.LogWarning("Material animado sin asignar: " + n); }
            }
            r.sharedMaterials = slots;
        }
        PrefabUtility.SaveAsPrefabAsset(movement.gameObject,Folder + "/ChasquiJugador.prefab");
        EditorUtility.SetDirty(controller); EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes(); AssetDatabase.SaveAssets();
        Selection.activeGameObject = movement.gameObject;
        File.WriteAllText(Path.Combine(Path.GetTempPath(),"chasqui_animaciones_instaladas.txt"),"OK clips=" + string.Join(",",clips.Select(c=>c.name)) + " materiales=" + applied + " faltantes=" + missing + " avatar=" + (animator.avatar != null));
        Debug.Log("ANIMACIONES_CHASQUI_LISTAS: reposo, caminar, correr, saltar.");
        if (missing != 0) throw new Exception("Revisar materiales del personaje animado.");
    }
    static string Normalizar(string s) { return new string(s.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray()); }
}
