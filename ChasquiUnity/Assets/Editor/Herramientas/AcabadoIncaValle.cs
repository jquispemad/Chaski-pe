using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class AcabadoIncaValle
{
    const string D="Assets/Niveles/Valle/";
    static AcabadoIncaValle(){EditorApplication.delayCall+=Auto;EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredEditMode)EditorApplication.delayCall+=Auto;};}
    static void Auto(){if(!EditorApplication.isPlayingOrWillChangePlaymode&&!SessionState.GetBool("AcabadoIncaV2",false))Aplicar();}
    [MenuItem("Chasqui/Aplicar letreros tallados, manta y paja")]
    public static void Aplicar()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        var nivel=UnityEngine.Object.FindFirstObjectByType<NivelValleChasqui>();if(nivel==null)return;
        Font fuente=Resources.Load<Font>("EstiloInca/TituloTradicional");if(fuente==null)return;
        var material=AssetDatabase.LoadAssetAtPath<Material>(D+"LetraCarteles.mat");
        material.shader=Shader.Find("Chasqui/Texto de letreros");material.mainTexture=fuente.material.mainTexture;
        var tablillas=UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f=>f.name=="Tablilla"||f.name=="Letrero de ruta").ToArray();
        int letras=0;
        foreach(var t in UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsSortMode.None))
        {
            t.font=fuente;t.fontSize=144;t.fontStyle=FontStyle.Normal;t.characterSize=.1f;t.lineSpacing=1.18f;
            t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=Color.white;
            fuente.RequestCharactersInTexture(t.text,144,FontStyle.Normal);
            t.transform.localScale=Vector3.one;var r=t.GetComponent<MeshRenderer>();r.sharedMaterial=material;
            bool grande=t.text.Contains("QHAPAQ")||t.text.Contains("TAMBO DEL");
            var bounds=r.bounds;float ancho=grande?5.8f:1.76f,alto=grande?.43f:.46f;
            if(bounds.size.x>.001f&&bounds.size.y>.001f)t.transform.localScale=Vector3.one*Mathf.Min(ancho/bounds.size.x,alto/bounds.size.y);
            if(t.GetComponent<TextoCartelChasqui>()==null)t.gameObject.AddComponent<TextoCartelChasqui>();
            letras++;
        }
        material.mainTexture=fuente.material.mainTexture;EditorUtility.SetDirty(material);
        // Warm, lighter timber makes dark incisions readable at a distance.
        var madera=Material("MaderaLetrerosTallados",Shader.Find("Standard"));madera.color=new Color(.61f,.39f,.18f);madera.SetFloat("_Glossiness",.12f);
        madera.mainTexture=TexturaMadera();madera.mainTextureScale=new Vector2(2,1);EditorUtility.SetDirty(madera);
        foreach(var f in tablillas)f.GetComponent<MeshRenderer>().sharedMaterial=madera;
        foreach(var f in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f=>f.name=="Poste de ruta"))
        {
            var tabla=tablillas.Where(t=>t.name=="Tablilla").OrderBy(t=>Vector3.Distance(t.transform.position,f.transform.position)).FirstOrDefault();
            if(tabla!=null){var p=f.transform.position;p.z=tabla.transform.position.z+.18f;f.transform.position=p;}
        }
        var paja=Material("PajaIchuReal",Shader.Find("Chasqui/Paja de ichu"));paja.color=new Color(.66f,.46f,.21f);EditorUtility.SetDirty(paja);
        int techos=0;
        foreach(var f in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f=>f.name=="Techo de ichu"))
        {
            f.GetComponent<MeshRenderer>().sharedMaterial=paja;
            if(f.transform.Find("Hebras y alero de ichu")==null)
            {
                var hebras=new GameObject("Hebras y alero de ichu");hebras.transform.SetParent(f.transform,false);
                var mesh=CrearPaja(techos);string path=D+"HebrasTecho"+techos+".asset";
                var anterior=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(anterior==null)AssetDatabase.CreateAsset(mesh,path);else{EditorUtility.CopySerialized(mesh,anterior);UnityEngine.Object.DestroyImmediate(mesh);mesh=anterior;}
                hebras.AddComponent<MeshFilter>().sharedMesh=mesh;hebras.AddComponent<MeshRenderer>().sharedMaterial=paja;hebras.isStatic=true;
            }
            techos++;
        }
        if(nivel.GetComponent<InterfazIncaChasqui>()==null)nivel.gameObject.AddComponent<InterfazIncaChasqui>();
        EditorSceneManager.MarkSceneDirty(nivel.gameObject.scene);EditorSceneManager.SaveScene(nivel.gameObject.scene);AssetDatabase.SaveAssets();
        SessionState.SetBool("AcabadoIncaV1",true);SessionState.SetBool("AcabadoIncaV2",true);
        File.WriteAllText("Temp/chasqui_acabado_inca.txt","LISTO\nLetreros tallados: "+letras+"\nPaneles de paja con hebras: "+techos+"\nFuente de avisos y manta: Georgia\nBotones finales: solo visuales");
        Debug.Log("ACABADO_INCA_LISTO: "+letras+" letreros y "+techos+" techos de ichu");
        Vistas(nivel);
    }
    static Material Material(string nombre,Shader shader)
    {string p=D+nombre+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(m==null){m=new Material(shader);AssetDatabase.CreateAsset(m,p);}m.shader=shader;return m;}
    static Texture2D TexturaMadera()
    {
        const int W=512,H=128;var t=new Texture2D(W,H,TextureFormat.RGB24,true);var c=new Color[W*H];
        for(int y=0;y<H;y++)for(int x=0;x<W;x++)
        {
            float veta=Mathf.Sin(y*.55f+Mathf.PerlinNoise(x*.006f,y*.02f)*5)*.11f;
            float n=Mathf.PerlinNoise(x*.02f,y*.28f)*.20f+Mathf.PerlinNoise(x*.1f,y*.8f)*.08f;
            float v=.75f+veta+n;c[y*W+x]=new Color(v,v*.89f,v*.68f);
        }
        t.SetPixels(c);t.Apply();string path=D+"MaderaTallada.png";File.WriteAllBytes(path,t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);AssetDatabase.ImportAsset(path);return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    static Mesh CrearPaja(int semilla)
    {
        var rng=new System.Random(7319+semilla);var v=new List<Vector3>();var tri=new List<int>();var color=new List<Color>();
        for(int capa=0;capa<4;capa++)for(int i=0;i<170;i++)
        {
            float z=-.515f+i/169f*1.03f+(float)rng.NextDouble()*.003f;
            float x=-.54f+capa*.27f;float y=.58f+(3-capa)*.045f+(float)rng.NextDouble()*.14f;
            float largo=.26f+(float)rng.NextDouble()*.11f;
            var a=new Vector3(x,y,z);var b=new Vector3(x+largo,y+.03f,z+(float)(rng.NextDouble()-.5)*.004f);
            int k=v.Count;float rz=.0011f+(float)rng.NextDouble()*.0011f,ry=.022f+(float)rng.NextDouble()*.035f;
            Color c=Color.white*(.72f+(float)rng.NextDouble()*.35f);
            for(int extremo=0;extremo<2;extremo++)for(int j=0;j<4;j++)
            {float ang=j*Mathf.PI*.5f;v.Add((extremo==0?a:b)+new Vector3(0,Mathf.Sin(ang)*ry,Mathf.Cos(ang)*rz));color.Add(c);}
            for(int j=0;j<4;j++){int n=(j+1)%4;tri.AddRange(new[]{k+j,k+4+j,k+n,k+n,k+4+j,k+4+n});}
        }
        var mesh=new Mesh{name="Ichu seco en haces",indexFormat=IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetTriangles(tri,0);mesh.SetColors(color);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
    }
    public static void Vistas(NivelValleChasqui nivel)
    {
        var cartel=UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsSortMode.None).First(t=>t.text.Contains("QHAPAQ"));
        Foto("chasqui_letrero_tallado",cartel.transform.position+new Vector3(.5f,.25f,-8),cartel.transform.position,48);
        var pequeno=UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsSortMode.None).First(t=>t.text.Contains("ESPACIO"));
        Foto("chasqui_letrero_pequeno",pequeno.transform.position+new Vector3(.6f,.1f,-3.2f),pequeno.transform.position,48);
        var techo=UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).First(f=>f.name=="Techo de ichu");
        Foto("chasqui_techo_ichu",techo.transform.position+new Vector3(4,3,-5),techo.transform.position,52);
    }
    static void Foto(string nombre,Vector3 pos,Vector3 mirar,float fov)
    {
        var o=new GameObject("Vista temporal");var c=o.AddComponent<Camera>();c.transform.position=pos;c.transform.LookAt(mirar);c.fieldOfView=fov;c.farClipPlane=600;
        var rt=RenderTexture.GetTemporary(1200,800,24);var anterior=RenderTexture.active;var t=new Texture2D(1200,800,TextureFormat.RGB24,false);
        try{c.targetTexture=rt;c.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1200,800),0,0);t.Apply();File.WriteAllBytes("Temp/"+nombre+".png",t.EncodeToPNG());}
        finally{RenderTexture.active=anterior;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(o);}
    }
}
