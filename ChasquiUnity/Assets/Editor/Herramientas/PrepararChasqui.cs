using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class PrepararChasqui
{
    [Serializable] public class MaterialInfo { public string name; public float[] color; public float roughness; public float metallic; }
    [Serializable] public class MaterialList { public MaterialInfo[] materials; }
    static string Normalizar(string text)
    {
        var sb = new StringBuilder();
        foreach (char c in text.Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(c)) sb.Append(char.ToLowerInvariant(c));
        return sb.ToString();
    }
    [MenuItem("Chasqui/Preparar escena del personaje")]
    public static void Crear()
    {
        const string folder = "Assets/Personajes/Chasqui";
        const string modelPath = folder + "/Chasqui.fbx";
        Directory.CreateDirectory(folder + "/Materiales");
        Directory.CreateDirectory("Assets/Scenes");
        AssetDatabase.Refresh();
        var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
        if (importer == null) throw new Exception("No se encontró Chasqui.fbx");
        importer.importAnimation = false;
        importer.animationType = ModelImporterAnimationType.None;
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        importer.SaveAndReimport();
        var json = File.ReadAllText(folder + "/ChasquiMateriales.json");
        var info = JsonUtility.FromJson<MaterialList>(json);
        var materials = new Dictionary<string, Material>();
        var shader = Shader.Find("Standard");
        if (shader == null) throw new Exception("No se encontró el shader Standard");
        for (int i = 0; i < info.materials.Length; i++)
        {
            var data = info.materials[i];
            var matPath = folder + "/Materiales/Material_" + i.ToString("D2") + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, matPath); }
            mat.name = data.name;
            mat.color = new Color(data.color[0], data.color[1], data.color[2], data.color[3]);
            mat.SetFloat("_Metallic", data.metallic);
            mat.SetFloat("_Glossiness", 1f - data.roughness);
            materials[Normalizar(data.name)] = mat;
            EditorUtility.SetDirty(mat);
        }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (model == null) throw new Exception("No se pudo importar el personaje");
        var root = (GameObject)PrefabUtility.InstantiatePrefab(model);
        root.name = "Chasqui";
        int assigned = 0, missing = 0, triangles = 0;
        foreach (var renderer in root.GetComponentsInChildren<Renderer>())
        {
            var slots = renderer.sharedMaterials;
            for (int j = 0; j < slots.Length; j++)
            {
                var sourceName = slots[j] != null ? slots[j].name : "";
                Material replacement;
                if (materials.TryGetValue(Normalizar(sourceName), out replacement)) { slots[j] = replacement; assigned++; }
                else { missing++; Debug.LogWarning("Material sin correspondencia: " + sourceName); }
            }
            renderer.sharedMaterials = slots;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }
        foreach (var mf in root.GetComponentsInChildren<MeshFilter>()) triangles += mf.sharedMesh.triangles.Length / 3;
        var renderers = root.GetComponentsInChildren<Renderer>();
        var bounds = renderers[0].bounds;
        foreach (var r in renderers) bounds.Encapsulate(r.bounds);
        root.transform.position += Vector3.up * -bounds.min.y;
        PrefabUtility.SaveAsPrefabAssetAndConnect(root, folder + "/Chasqui.prefab", InteractionMode.AutomatedAction);
        bounds = renderers[0].bounds;
        foreach (var r in renderers) bounds.Encapsulate(r.bounds);
        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane); floor.name = "Suelo";
        var groundMat = new Material(shader); groundMat.color = new Color(.13f,.15f,.17f); groundMat.SetFloat("_Glossiness",.12f);
        var groundPath = "Assets/Personajes/Chasqui/Materiales/Suelo.mat";
        if (AssetDatabase.LoadAssetAtPath<Material>(groundPath) == null) AssetDatabase.CreateAsset(groundMat,groundPath);
        floor.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(groundPath);
        var lightObj = new GameObject("Luz principal"); var light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional; light.intensity = 1.3f; light.color = new Color(1f,.93f,.83f); light.shadows = LightShadows.Soft;
        lightObj.transform.rotation = Quaternion.Euler(45f,145f,0);
        RenderSettings.skybox = null; RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(.55f,.58f,.64f);
        var cameraObj = new GameObject("Camara principal"); cameraObj.tag = "MainCamera";
        var cam = cameraObj.AddComponent<Camera>(); cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.075f,.085f,.10f);
        cameraObj.transform.position = new Vector3(2.6f,1.55f,3.8f); cameraObj.transform.LookAt(bounds.center);
        cam.fieldOfView = 35f; cam.nearClipPlane = .05f; cam.farClipPlane = 100f;
        QualitySettings.antiAliasing = 4;
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/Chasqui.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Chasqui.unity",true) };
        Selection.activeGameObject = root;
        if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.FrameSelected();
        AssetDatabase.SaveAssets();
        Debug.Log("CHASQUI_LISTO materiales=" + assigned + " faltantes=" + missing + " triangulos=" + triangles + " altura=" + bounds.size.y);
        if (missing != 0) throw new Exception("Revisar materiales sin correspondencia");
    }
}

