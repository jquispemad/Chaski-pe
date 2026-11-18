using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

static class OrdenarPaisajeSegundoRecorrido
{
    const string Dir="Assets/Niveles/SegundoRecorrido/";
    [InitializeOnLoadMethod]
    static void Iniciar()
    {
        EditorApplication.delayCall+=Aplicar;
        EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredEditMode)EditorApplication.delayCall+=Aplicar;};
    }
    [MenuItem("Chasqui/Ordenar letreros, cultivos y arroyo")]
    public static void Aplicar()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        var escena=SceneManager.GetSceneByPath(ConstruirSegundoRecorrido.Ruta);bool cerrar=!escena.isLoaded;
        if(cerrar)escena=EditorSceneManager.OpenScene(ConstruirSegundoRecorrido.Ruta,OpenSceneMode.Additive);
        var mundo=escena.GetRootGameObjects().First(o=>o.name.StartsWith("Nivel 02"));
        var mision=escena.GetRootGameObjects().Select(o=>o.GetComponent<SegundaMisionChaski>()).First(m=>m!=null);
        if(mision.arroyo!=null)
        {
            mision.arroyo.volume=.16f;mision.arroyo.minDistance=4;mision.arroyo.maxDistance=26;
            mision.arroyo.spatialBlend=1;mision.arroyo.dopplerLevel=0;
            EditorUtility.SetDirty(mision.arroyo);
        }
        if(mundo.transform.Find("Paisaje revisado · naciente y señalización")==null)
        {
            var terreno=mundo.GetComponentInChildren<Terrain>();
            var cultivos=mundo.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name=="Cultivo · hojas"||r.name=="Tierra de cultivo").Select(r=>{var b=r.bounds;b.Expand(new Vector3(3,12,3));return b;}).ToArray();
            var troncos=mundo.GetComponentsInChildren<Transform>().Where(t=>t.name=="Queñua · tronco").ToArray();
            int quitados=0;
            foreach(var arbol in troncos)
            {
                if(!cultivos.Any(b=>b.Contains(new Vector3(arbol.position.x,b.center.y,arbol.position.z))))continue;
                QuitarArbol(arbol,mundo.transform);quitados++;
            }
            foreach(var letrero in mundo.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Letrero · ")).ToArray())
            {
                float z=letrero.position.z;
                // Leave room for the full width of the wooden board, not only its post.
                float x=ConstruirSegundoRecorrido.Centro(z)-5.0f;
                letrero.position=new Vector3(x,Altura(terreno,x,z),z);
                float pendiente=ConstruirSegundoRecorrido.Centro(z+1)-ConstruirSegundoRecorrido.Centro(z-1);
                letrero.rotation=Quaternion.LookRotation(new Vector3(pendiente,0,2));
                if(letrero.name.Contains("LOS RELEVOS"))mision.mirador.position=letrero.position-Vector3.forward*1.5f;
                var area=new Bounds(letrero.position+Vector3.up*2,new Vector3(10,10,9));
                foreach(var arbol in troncos.Where(t=>t!=null && t.gameObject.activeSelf))
                {
                    var copa=mundo.GetComponentsInChildren<MeshRenderer>().FirstOrDefault(r=>r.name=="Queñua · hojas" && (r.transform.position-arbol.position).sqrMagnitude<.01f);
                    if(area.Contains(arbol.position+Vector3.up*2) || copa!=null&&area.Intersects(copa.bounds))
                    {QuitarArbol(arbol,mundo.transform);quitados++;}
                }
                foreach(var r in mundo.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name=="Roca erosionada"))
                    if(r.bounds.SqrDistance(letrero.position+Vector3.up*.7f)<3.24f)r.gameObject.SetActive(false);
            }
            CrearArroyo(terreno,mundo.transform);
            new GameObject("Paisaje revisado · naciente y señalización").transform.SetParent(mundo.transform,false);
            EditorSceneManager.MarkSceneDirty(escena);EditorSceneManager.SaveScene(escena);AssetDatabase.SaveAssets();
            Debug.Log("PAISAJE_NIVEL02: letreros despejados junto al sendero, "+quitados+" árboles retirados; arroyo tallado desde la naciente del cerro.");
        }
        if(mundo.transform.Find("Apoyo del puente y ribera revisados")==null)
        {
            var terreno=mundo.GetComponentInChildren<Terrain>();var d=terreno.terrainData;var p=terreno.transform.position;
            int n=d.heightmapResolution;var alturas=d.GetHeights(0,0,n,n);float centro=ConstruirSegundoRecorrido.Centro(280);
            for(int j=0;j<n;j++)for(int i=0;i<n;i++)
            {
                float x=p.x+i*d.size.x/(n-1),z=p.z+j*d.size.z/(n-1);
                if(z<266.4f || z>293.6f || Mathf.Abs(x-centro)>3.3f)continue;
                float piso=1-.65f*Mathf.Sin(Mathf.Clamp01((z-267)/26)*Mathf.PI);
                float peso=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(2.1f,3.3f,Mathf.Abs(x-centro)));
                float y=alturas[j,i]*d.size.y+p.y;
                alturas[j,i]=(Mathf.Lerp(y,Mathf.Min(y,piso-.34f),peso)-p.y)/d.size.y;
            }
            d.SetHeights(0,0,alturas);EditorUtility.SetDirty(d);
            foreach(var r in mundo.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name.StartsWith("Ribera ·")||r.name.StartsWith("Naciente ·")))
            {
                float gap=float.PositiveInfinity;
                foreach(var v in r.GetComponent<MeshFilter>().sharedMesh.vertices)
                {var q=r.transform.TransformPoint(v);gap=Mathf.Min(gap,q.y-Altura(terreno,q.x,q.z));}
                r.transform.position-=Vector3.up*(gap+.12f);
            }
            new GameObject("Apoyo del puente y ribera revisados").transform.SetParent(mundo.transform,false);
        }
        EditorSceneManager.MarkSceneDirty(escena);EditorSceneManager.SaveScene(escena);AssetDatabase.SaveAssets();
        var corredorPuente=new Bounds(new Vector3(ConstruirSegundoRecorrido.Centro(280),1,280),new Vector3(3.5f,5,26));
        foreach(var r in mundo.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name=="Roca erosionada" && r.GetComponent<BoxCollider>()!=null))
            if(corredorPuente.Intersects(r.bounds))r.gameObject.SetActive(false);
        EditorSceneManager.MarkSceneDirty(escena);EditorSceneManager.SaveScene(escena);
        if(!SessionState.GetBool("ArroyoSierraRevisadoV3",false))
        {
            Comprobar(escena,mundo);
            SessionState.SetBool("ArroyoSierraRevisadoV3",true);
        }
        if(cerrar)EditorSceneManager.CloseScene(escena,true);
    }
    static void Comprobar(Scene escena,GameObject mundo)
    {
        Physics.SyncTransforms();var terreno=mundo.GetComponentInChildren<Terrain>();int apoyos=0,lecho=0;
        for(float z=267.25f;z<293;z+=.25f)
            if(Physics.Raycast(new Vector3(ConstruirSegundoRecorrido.Centro(280),5,z),Vector3.down,out var h,10) && h.collider.name.ToLowerInvariant().Contains("puente"))apoyos++;
        for(int x=-70;x<=80;x+=5)if(Altura(terreno,x,CentroRio(x))<Agua(x)-.45f)lecho++;
        System.IO.File.WriteAllText("Temp/arroyo_sierra_revision.txt",$"Puente: {apoyos}/103 apoyos\nLecho bajo agua: {lecho}/31 muestras\n");
        var o=new GameObject("Cámara temporal de revisión del arroyo");SceneManager.MoveGameObjectToScene(o,escena);
        var cam=o.AddComponent<Camera>();cam.transform.position=new Vector3(42,28,245);cam.transform.LookAt(new Vector3(32,7,282));cam.fieldOfView=63;cam.farClipPlane=500;
        var rt=RenderTexture.GetTemporary(1400,900,24);var anterior=RenderTexture.active;var tex=new Texture2D(1400,900,TextureFormat.RGB24,false);
        try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1400,900),0,0);tex.Apply();System.IO.File.WriteAllBytes("Temp/arroyo_sierra_revision.png",tex.EncodeToPNG());}
        finally{RenderTexture.active=anterior;RenderTexture.ReleaseTemporary(rt);Object.DestroyImmediate(tex);Object.DestroyImmediate(o);}
        Debug.Log($"ARROYO_REVISADO: puente {apoyos}/103, cauce {lecho}/31; sonido a volumen 0.16 y distancia máxima 26 m.");
    }
    static void QuitarArbol(Transform tronco,Transform mundo)
    {
        foreach(var t in mundo.GetComponentsInChildren<Transform>().Where(t=>(t.name=="Queñua · hojas"||t.name=="Queñua · tronco") && (t.position-tronco.position).sqrMagnitude<.01f).ToArray())t.gameObject.SetActive(false);
    }
    static float Altura(Terrain t,float x,float z)
    {
        var d=t.terrainData;var p=t.transform.position;
        return d.GetInterpolatedHeight(Mathf.Clamp01((x-p.x)/d.size.x),Mathf.Clamp01((z-p.z)/d.size.z))+p.y;
    }
    static float CentroRio(float x)=>280+(6*Mathf.Sin(x*.055f)+3*Mathf.Sin(x*.13f))*Mathf.SmoothStep(0,1,Mathf.InverseLerp(10,24,Mathf.Abs(x)));
    static float Agua(float x)=> x>=0?-.7f+18*Mathf.SmoothStep(0,1,Mathf.Clamp01((x-14)/74)):-.7f-3*Mathf.Pow(Mathf.Abs(x)/110,1.3f);
    static float Ancho(float x)=> (2.25f+Mathf.PerlinNoise(x*.07f+15,8)*1.4f)*Mathf.SmoothStep(0,1,Mathf.Clamp01((88-x)/7));
    static void CrearArroyo(Terrain t,Transform mundo)
    {
        foreach(var r in mundo.GetComponentsInChildren<Renderer>().Where(r=>r.name=="Arroyo · superficie de agua").ToArray())Object.DestroyImmediate(r.gameObject);
        var d=t.terrainData;int n=d.heightmapResolution;var alturas=d.GetHeights(0,0,n,n);var origen=t.transform.position;
        for(int j=0;j<n;j++)for(int i=0;i<n;i++)
        {
            float x=origen.x+i*d.size.x/(n-1),z=origen.z+j*d.size.z/(n-1);
            if(z<249||z>311)continue;
            float lateral=Mathf.Abs(x-ConstruirSegundoRecorrido.Centro(z));
            float colina=Mathf.SmoothStep(0,1,Mathf.InverseLerp(12,75,lateral));
            float baseSuelo=ConstruirSegundoRecorrido.AlturaCamino(z)+colina*(8+23*Mathf.PerlinNoise(x*.032f+17,z*.017f));
            if(x<=88)
            {
                float dist=Mathf.Abs(z-CentroRio(x)),ancho=Ancho(x);
                float margen=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(ancho*.7f,ancho+4,dist));
                baseSuelo=Mathf.Lerp(baseSuelo,Agua(x)-1.1f,margen);
            }
            alturas[j,i]=(baseSuelo-origen.y)/d.size.y;
        }
        d.SetHeights(0,0,alturas);
        int a=d.alphamapResolution;var pintura=d.GetAlphamaps(0,0,a,a);
        for(int j=0;j<a;j++)for(int i=0;i<a;i++)
        {
            float x=origen.x+i*d.size.x/(a-1),z=origen.z+j*d.size.z/(a-1);
            if(x>88 || Mathf.Abs(z-CentroRio(x))>Ancho(x)+3)continue;
            for(int k=0;k<d.alphamapLayers;k++)pintura[j,i,k]=0;
            pintura[j,i,2]=.72f;pintura[j,i,1]=.28f;
        }
        d.SetAlphamaps(0,0,pintura);EditorUtility.SetDirty(d);
        var v=new List<Vector3>();var uv=new List<Vector2>();var colores=new List<Color>();var tri=new List<int>();
        const int filas=397,cols=9;
        for(int i=0;i<filas;i++)
        {
            float x=-110+i*.5f,ancho=Ancho(x);
            for(int k=0;k<cols;k++)
            {
                float u=k/(cols-1f),z=CentroRio(x)+(u*2-1)*ancho;
                v.Add(new Vector3(x,Agua(x)+Mathf.Sin(x*2.1f+u*5)*.018f,z));uv.Add(new Vector2(u,x*.2f));
                colores.Add(new Color(Mathf.Sin(u*Mathf.PI),0,0,1));
                if(i==0||k==cols-1)continue;
                int b=i*cols+k,a0=b-cols;tri.AddRange(new[]{a0,a0+1,b,a0+1,b+1,b});
            }
        }
        var mesh=new Mesh{name="Arroyo irregular · cerro al valle"};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetColors(colores);mesh.SetTriangles(tri,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,Dir+"CauceArroyo.asset");
        var mat=new Material(Shader.Find("Chasqui/Arroyo de sierra"));AssetDatabase.CreateAsset(mat,Dir+"AguaSierra.mat");
        var o=new GameObject("Arroyo · naciente del cerro y cauce sinuoso");o.transform.SetParent(mundo,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=mat;o.AddComponent<ArroyoSierraChaski>();
        // Banks and spring rocks hide the origin of the water strip in a natural crevice.
        for(int i=0;i<65;i++)
        {
            float x=-95+i*2.75f;if(Mathf.Abs(x+1.58f)<4)continue;
            float lado=i%2==0?-1:1,z=CentroRio(x)+lado*(Ancho(x)+.9f);
            var m=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Niveles/Valle/Roca"+i%4+".asset");
            var roca=new GameObject(i>62?"Naciente · roca del cerro":"Ribera · piedra erosionada");roca.transform.SetParent(mundo,false);
            float s=i>62?2.2f:.35f+(i%4)*.18f;roca.transform.localScale=Vector3.one*s;
            roca.transform.position=new Vector3(x,Altura(t,x,z)-m.bounds.min.y*s-.18f,z);
            roca.AddComponent<MeshFilter>().sharedMesh=m;roca.AddComponent<MeshRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Personajes/Chasqui/MaterialPiedra.mat");
            var c=roca.AddComponent<BoxCollider>();c.center=m.bounds.center;c.size=m.bounds.size*.8f;
        }
        // Settle existing stones again after carving the bank beneath them.
        foreach(var r in mundo.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name=="Roca erosionada"&&Mathf.Abs(r.bounds.center.z-280)<24 && r.GetComponent<BoxCollider>()!=null))
        {
            var b=r.bounds;float y=Altura(t,b.center.x,b.center.z);
            r.transform.position+=Vector3.up*(y-b.min.y-.2f);
        }
    }
}
