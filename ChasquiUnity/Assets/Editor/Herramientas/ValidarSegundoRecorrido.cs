using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

static class ValidarSegundoRecorrido
{
    [InitializeOnLoadMethod]
    static void Preparar() => EditorApplication.delayCall += AsentarRocas;
    static void AsentarRocas()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode || AssetDatabase.LoadAssetAtPath<SceneAsset>(ConstruirSegundoRecorrido.Ruta)==null)return;
        var escena=SceneManager.GetSceneByPath(ConstruirSegundoRecorrido.Ruta);bool cerrar=!escena.isLoaded;
        if(cerrar)escena=EditorSceneManager.OpenScene(ConstruirSegundoRecorrido.Ruta,OpenSceneMode.Additive);
        var mundo=escena.GetRootGameObjects().First(o=>o.name.StartsWith("Nivel 02"));
        if(mundo.transform.Find("Rocas asentadas en ladera")==null)
        {
            var suelo=mundo.GetComponentInChildren<Terrain>();
            foreach(var r in mundo.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name=="Roca erosionada" && r.GetComponent<BoxCollider>()!=null))
            {
                var b=r.bounds;float y=float.PositiveInfinity;
                foreach(float dx in new[]{-.35f,.35f})foreach(float dz in new[]{-.35f,.35f})
                {
                    var p=b.center+new Vector3(b.size.x*dx,0,b.size.z*dz);
                    y=Mathf.Min(y,suelo.SampleHeight(p)+suelo.transform.position.y);
                }
                r.transform.position+=Vector3.up*(y-b.min.y-.08f);
            }
            new GameObject("Rocas asentadas en ladera").transform.SetParent(mundo.transform,false);
            // Store a useful camera view even when Play is stopped.
            var m=escena.GetRootGameObjects().Select(o=>o.GetComponent<SegundaMisionChaski>()).First(x=>x!=null);
            m.vista.transform.position=m.jugador.transform.position+new Vector3(0,2.3f,-4.5f);
            m.vista.transform.LookAt(m.jugador.transform.position+Vector3.up*1.1f);
            EditorSceneManager.MarkSceneDirty(escena);EditorSceneManager.SaveScene(escena);
        }
        if(cerrar)EditorSceneManager.CloseScene(escena,true);
    }

    [MenuItem("Chasqui/Comprobar segundo recorrido")]
    static void Comprobar()
    {
        if(!EditorApplication.isPlaying)AsentarRocas();
        var escena=SceneManager.GetSceneByPath(ConstruirSegundoRecorrido.Ruta);
        if(!escena.isLoaded){Debug.LogWarning("Abre el segundo recorrido para comprobarlo.");return;}
        var fallos=new List<string>();
        var m=escena.GetRootGameObjects().Select(o=>o.GetComponent<SegundaMisionChaski>()).FirstOrDefault(x=>x!=null);
        if(m==null){Debug.LogError("NIVEL02: falta la misión");return;}
        if(m.jugador==null || m.vista==null || m.responsable==null || m.entrega==null || m.mirador==null)fallos.Add("referencias de misión");
        if(m.jugador.recorridoLateral)fallos.Add("movimiento bloqueado a 2.5D");
        var audio=m.GetComponent<AudioPrimeraMision>();
        if(audio==null || audio.musica==null || audio.ambiente==null || audio.pututu==null || audio.pasosTierra.Length!=5)fallos.Add("audio incompleto");
        if(m.arroyo==null || m.arroyo.clip==null)fallos.Add("sonido del arroyo");
        if(!EditorBuildSettings.scenes.Any(s=>s.enabled&&s.path==ConstruirSegundoRecorrido.Ruta))fallos.Add("escena fuera de campaña");
        Physics.SyncTransforms();
        int apoyos=0;
        for(float z=10;z<460;z+=2)
        {
            var p=new Vector3(ConstruirSegundoRecorrido.Centro(z),ConstruirSegundoRecorrido.AlturaCamino(z)+20,z);
            if(Physics.Raycast(p,Vector3.down,out var hit,30,~0,QueryTriggerInteraction.Ignore))
            {
                if(z>=270&&z<=290&&!hit.collider.name.ToLowerInvariant().Contains("puente"))fallos.Add("pasarela sin apoyo en z="+z);
                apoyos++;
            }
            else fallos.Add("sin suelo en z="+z);
        }
        float x=ConstruirSegundoRecorrido.Centro(199),y=ConstruirSegundoRecorrido.AlturaCamino(199);
        if(!Physics.Raycast(new Vector3(x,y+1.9f,196),Vector3.forward,6))fallos.Add("viga no bloquea de pie");
        if(Physics.Raycast(new Vector3(x,y+1.5f,196),Vector3.forward,6))fallos.Add("paso bajo sin margen");
        var mundo=escena.GetRootGameObjects().First(o=>o.name.StartsWith("Nivel 02"));
        int hojas=mundo.GetComponentsInChildren<MeshRenderer>().Count(r=>r.name=="Queñua · hojas" && r.shadowCastingMode!=UnityEngine.Rendering.ShadowCastingMode.Off);
        if(hojas==0)fallos.Add("sombras de hojas desactivadas");
        if(m.GetComponent<EntregaInicialChaski>()==null)fallos.Add("sin entrega inicial");
        if(mundo.transform.Find("Sendero · tierra compactada visible")==null)fallos.Add("sin sendero visible");
        for(int z=0;z<470;z+=10)foreach(int lado in new[]{-1,1})
        {
            var p=new Vector3(ConstruirSegundoRecorrido.Centro(z),50,z);
            if(!Physics.Raycast(p,Vector3.right*lado,out var h,40,~0,QueryTriggerInteraction.Ignore) || !h.collider.name.StartsWith("Barrera lateral") || Mathf.Abs(h.distance-32.8f)>1.5f)
                fallos.Add("barrera lateral fuera de posición en z="+z+" lado="+lado);
        }
        foreach(var f in fallos)Debug.LogError("NIVEL02_VALIDACION: "+f);
        Debug.Log($"NIVEL02_VALIDADO: apoyos={apoyos}/225, fallos={fallos.Count}, modo3D={!m.jugador.recorridoLateral}, escena y audio revisados.");
        System.IO.File.WriteAllText("Temp/nivel02_validacion.txt",$"Apoyos: {apoyos}/225\nCopas con sombra: {hojas}\nErrores: {fallos.Count}\n"+string.Join("\n",fallos));
        var camObj=new GameObject("Vista de comprobación del puente");var cam=camObj.AddComponent<Camera>();
        float bx=ConstruirSegundoRecorrido.Centro(280);cam.transform.position=new Vector3(bx+10,6,260);cam.transform.LookAt(new Vector3(bx,.6f,280));cam.fieldOfView=55;
        var rt=RenderTexture.GetTemporary(1200,800,24);var previo=RenderTexture.active;var tex=new Texture2D(1200,800,TextureFormat.RGB24,false);
        try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1200,800),0,0);tex.Apply();System.IO.File.WriteAllBytes("Temp/nivel02_puente.png",tex.EncodeToPNG());}
        finally{RenderTexture.active=previo;RenderTexture.ReleaseTemporary(rt);Object.DestroyImmediate(tex);Object.DestroyImmediate(camObj);}
    }
}
