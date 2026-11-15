using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class RealismoIntegralChasqui
{
    const string D="Assets/Personajes/Chasqui/";
    static RealismoIntegralChasqui(){EditorApplication.delayCall+=Auto;}
    static void Auto(){if(!EditorApplication.isPlayingOrWillChangePlaymode&&!SessionState.GetBool("ChasquiRealismoV1",false))Aplicar();}
    static Transform H(Transform t,string nombre)=>t.GetComponentsInChildren<Transform>(true).First(b=>b.name==nombre);
    static Mesh Guardar(Mesh mesh,string archivo)
    {
        string path=D+archivo;var anterior=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(anterior==null)AssetDatabase.CreateAsset(mesh,path);else{EditorUtility.CopySerialized(mesh,anterior);UnityEngine.Object.DestroyImmediate(mesh);mesh=anterior;EditorUtility.SetDirty(mesh);}return mesh;
    }
    [MenuItem("Chasqui/Mejorar personaje con tela fisica y cabello")]
    public static void Aplicar()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        var p=UnityEngine.Object.FindFirstObjectByType<ChasquiMovimiento>();if(p==null)return;
        if(p.GetComponent<CapaFisicaChasqui>()!=null){SessionState.SetBool("ChasquiRealismoV1",true);return;}
        var cuerpo=p.GetComponentsInChildren<SkinnedMeshRenderer>().First(r=>r.name=="Chasqui_Malla_Animada");
        var ropa=p.GetComponent<RopaDinamicaChasqui>();
        Materiales();
        AnatomiaYCabello(p,cuerpo);
        SepararCapa(ropa);
        CrearCapa(p,ropa);
        EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);AssetDatabase.SaveAssets();
        // Save the reusable character without references to this level's camera and obstacles.
        var camara=p.camara;var entorno=ropa.entorno;p.camara=null;ropa.entorno=new BoxCollider[0];
        try{PrefabUtility.SaveAsPrefabAsset(p.gameObject,D+"ChasquiJugadorRealista.prefab");}
        finally{p.camara=camara;ropa.entorno=entorno;}
        SessionState.SetBool("ChasquiRealismoV1",true);
        File.WriteAllText("Temp/chasqui_realismo_integral.txt","LISTO\nCapa: malla flexible con Cloth nativo\nCabello: ocho mechones con resortes\nAnatomia: volumen y transiciones de extremidades\nTela: ambientCG Fabric030 CC0\nPrefab: ChasquiJugadorRealista");
        File.WriteAllText(D+"Texturas/ambientCG/FUENTE_Y_LICENCIA.txt","Fabric030 · ambientCG · Lennart Demes\nhttps://ambientcg.com/view?id=Fabric030\nLicencia: CC0 1.0\nhttps://docs.ambientcg.com/license/\nMapas de color, normales y rugosidad usados en la ropa del Chasqui.");
        Debug.Log("REALISMO_INTEGRAL_LISTO");
    }
    static void Materiales()
    {
        string baseT=D+"Texturas/ambientCG/Fabric030_1K-JPG_";
        foreach(string sufijo in new[]{"NormalGL.jpg","Roughness.jpg","Color.jpg"})
        {
            string path=baseT+sufijo;var imp=AssetImporter.GetAtPath(path) as TextureImporter;if(imp==null)continue;
            imp.maxTextureSize=1024;imp.anisoLevel=8;imp.wrapMode=TextureWrapMode.Repeat;
            if(sufijo.StartsWith("Normal"))imp.textureType=TextureImporterType.NormalMap;
            if(sufijo.StartsWith("Rough"))imp.sRGBTexture=false;
            imp.SaveAndReimport();
        }
        var color=AssetDatabase.LoadAssetAtPath<Texture2D>(baseT+"Color.jpg");
        var normal=AssetDatabase.LoadAssetAtPath<Texture2D>(baseT+"NormalGL.jpg");
        var rough=AssetDatabase.LoadAssetAtPath<Texture2D>(baseT+"Roughness.jpg");
        foreach(string prefix in new[]{"Material_","Tela_"})foreach(int i in new[]{6,7,8,11,13})
        {
            var m=AssetDatabase.LoadAssetAtPath<Material>(D+"Materiales/"+prefix+i.ToString("D2")+".mat");if(m==null)continue;
            m.shader=Shader.Find("Chasqui/Tejido dos caras");m.mainTexture=color;m.mainTextureScale=Vector2.one*(i==11?7:4);
            m.SetTexture("_BumpMap",normal);m.SetTexture("_RoughnessMap",rough);m.SetFloat("_BumpScale",.42f);m.SetFloat("_Glossiness",.12f);m.SetFloat("_Fibra",.7f);
            if(i==6)m.color=new Color(.78f,.46f,.15f);if(i==7)m.color=new Color(.60f,.075f,.045f);if(i==8)m.color=new Color(.09f,.075f,.19f);if(i==13)m.color=new Color(.19f,.32f,.11f);
            EditorUtility.SetDirty(m);
        }
        foreach(int i in new[]{4,9})
        {
            var m=AssetDatabase.LoadAssetAtPath<Material>(D+"Materiales/Material_"+i.ToString("D2")+".mat");
            m.color=i==4?new Color(.014f,.009f,.006f):new Color(.035f,.019f,.010f);m.SetFloat("_Glossiness",i==4?.34f:.28f);EditorUtility.SetDirty(m);
        }
    }
    static void SepararCapa(RopaDinamicaChasqui ropa)
    {
        var baseOriginal=AssetDatabase.LoadAssetAtPath<Mesh>(D+"RopaBase.asset");var vistaOriginal=AssetDatabase.LoadAssetAtPath<Mesh>(D+"RopaVista.asset");
        var baseNueva=UnityEngine.Object.Instantiate(baseOriginal);var vista=UnityEngine.Object.Instantiate(vistaOriginal);
        for(int s=0;s<baseOriginal.subMeshCount;s++)
        {
            var tris=baseOriginal.GetTriangles(s);var quedan=new List<int>();
            for(int j=0;j<tris.Length;j+=3)if(!ropa.esCapa[tris[j]]&&!ropa.esCapa[tris[j+1]]&&!ropa.esCapa[tris[j+2]])quedan.AddRange(new[]{tris[j],tris[j+1],tris[j+2]});
            baseNueva.SetTriangles(quedan,s);vista.SetTriangles(quedan,s);
        }
        ropa.piel.sharedMesh=Guardar(baseNueva,"RopaSinCapaRigida.asset");ropa.salida.sharedMesh=Guardar(vista,"RopaSinCapaVista.asset");
    }
    static void AnatomiaYCabello(ChasquiMovimiento p,SkinnedMeshRenderer render)
    {
        var mesh=UnityEngine.Object.Instantiate(render.sharedMesh);mesh.name="Cuerpo con anatomia y cabello flexible";
        var vertices=mesh.vertices;var pesos=mesh.boneWeights;var originales=(BoneWeight[])pesos.Clone();var bind=mesh.bindposes;var bones=render.bones;
        var piel=new HashSet<int>(mesh.GetTriangles(0));
        foreach(int indice in piel)
        {
            var bw=pesos[indice];int b=bw.boneIndex0;if(b>=bones.Length)continue;
            string nombre=bones[b].name;string fin=null;
            if(nombre.StartsWith("upper_arm."))fin=nombre.Replace("upper_arm.","forearm.");
            if(nombre.StartsWith("forearm."))fin=nombre.Replace("forearm.","hand.");
            if(nombre.StartsWith("thigh."))fin=nombre.Replace("thigh.","shin.");
            if(nombre.StartsWith("shin."))fin=nombre.Replace("shin.","foot.");
            if(fin==null)continue;int k=Array.FindIndex(bones,x=>x.name==fin);if(k<0)continue;
            Vector3 a=bind[b].inverse.MultiplyPoint3x4(Vector3.zero),z=bind[k].inverse.MultiplyPoint3x4(Vector3.zero),eje=z-a;
            float t=Mathf.Clamp01(Vector3.Dot(vertices[indice]-a,eje)/eje.sqrMagnitude);Vector3 centro=a+eje*t,radial=vertices[indice]-centro;
            if(radial.magnitude>.18f)continue;
            float perfil=1;
            if(nombre.StartsWith("upper_arm"))perfil=1+.055f*Mathf.Sin(t*Mathf.PI)-.065f*Mathf.Exp(-Mathf.Pow((t-.95f)/.15f,2));
            if(nombre.StartsWith("forearm"))perfil=1+.065f*Mathf.Sin(t*Mathf.PI)-.06f*Mathf.SmoothStep(.65f,1,t);
            if(nombre.StartsWith("thigh"))perfil=1+.045f*Mathf.Sin(t*Mathf.PI)-.075f*Mathf.Exp(-Mathf.Pow((t-.95f)/.16f,2));
            if(nombre.StartsWith("shin"))perfil=1+.10f*Mathf.Exp(-Mathf.Pow((t-.30f)/.23f,2))-.065f*Mathf.SmoothStep(.7f,1,t);
            vertices[indice]=centro+radial*perfil;
        }
        // Eight weighted secondary bones let the existing detailed strands sway without moving the scalp.
        var cabeza=H(p.transform,"head");var nuevos=new List<Transform>(bones);var matrices=new List<Matrix4x4>(bind);var mechones=new Transform[8];
        for(int i=0;i<8;i++)
        {
            var o=new GameObject("Cabello flexible · mechon "+i);o.transform.SetParent(cabeza,false);
            o.transform.position=p.visual.TransformPoint(new Vector3(Mathf.Lerp(-.10f,.10f,i/7f),1.72f,-.035f));
            o.transform.rotation=cabeza.rotation;mechones[i]=o.transform;nuevos.Add(o.transform);matrices.Add(o.transform.worldToLocalMatrix*render.transform.localToWorldMatrix);
        }
        int headIndex=Array.FindIndex(bones,x=>x==cabeza);
        var horneada=new Mesh();render.BakeMesh(horneada);var pose=horneada.vertices;
        foreach(int indice in mesh.GetTriangles(4).Concat(mesh.GetTriangles(9)).Distinct())
        {
            Vector3 local=p.visual.InverseTransformPoint(render.transform.TransformPoint(pose[indice]));
            float suelto=Mathf.InverseLerp(1.78f,1.56f,local.y)*Mathf.InverseLerp(.08f,-.12f,local.z);
            if(suelto<.01f)continue;
            int grupo=Mathf.Clamp(Mathf.FloorToInt(Mathf.InverseLerp(-.18f,.18f,local.x)*8),0,7);
            pesos[indice]=new BoneWeight{boneIndex0=bones.Length+grupo,weight0=suelto*.9f,boneIndex1=headIndex,weight1=1-suelto*.9f};
            // Soften the upward-fanning ends into a downward fall.
            Vector3 cambio=p.visual.TransformVector(new Vector3(0,-.045f*suelto,.018f*suelto));
            var bw=originales[indice];Matrix4x4 skin=new Matrix4x4();
            int[] ids={bw.boneIndex0,bw.boneIndex1,bw.boneIndex2,bw.boneIndex3};float[] ws={bw.weight0,bw.weight1,bw.weight2,bw.weight3};
            for(int j=0;j<4;j++)if(ws[j]>0){var mat=render.transform.worldToLocalMatrix*bones[ids[j]].localToWorldMatrix*bind[ids[j]];for(int n=0;n<16;n++)skin[n]+=mat[n]*ws[j];}
            vertices[indice]+=skin.inverse.MultiplyVector(render.transform.InverseTransformVector(cambio));
        }
        UnityEngine.Object.DestroyImmediate(horneada);
        mesh.vertices=vertices;mesh.boneWeights=pesos;mesh.bindposes=matrices.ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();
        render.bones=nuevos.ToArray();render.sharedMesh=Guardar(mesh,"CuerpoRealistaFlexible.asset");render.updateWhenOffscreen=true;
        var pelo=p.gameObject.AddComponent<CabelloDinamicoChasqui>();pelo.cabeza=cabeza;pelo.marco=p.visual;pelo.mechones=mechones;
    }
    static void CrearCapa(ChasquiMovimiento p,RopaDinamicaChasqui ropa)
    {
        const int X=32,Y=42;var v=new List<Vector3>();var uv=new List<Vector2>();var w=new List<BoneWeight>();var tris=new[]{new List<int>(),new List<int>(),new List<int>()};
        var pecho=H(p.transform,"chest");var o=new GameObject("Lliclla · paño de lana con fisica");o.transform.SetParent(p.visual,false);
        for(int y=0;y<=Y;y++)for(int x=0;x<=X;x++)
        {
            float u=x/(float)X,t=y/(float)Y;float ancho=Mathf.Lerp(.59f,.72f,t);
            float pliegue=Mathf.Sin(u*Mathf.PI*8+t*1.6f)*(.012f+.019f*t);
            v.Add(new Vector3((u-.5f)*ancho,1.455f-t*.78f+.012f*Mathf.Sin(u*7),-.17f-.09f*t-pliegue));
            uv.Add(new Vector2(u,t));w.Add(new BoneWeight{boneIndex0=0,weight0=1});
        }
        for(int y=0;y<Y;y++)for(int x=0;x<X;x++)
        {
            int a=y*(X+1)+x,b=a+1,c=a+X+1,d=c+1;int mat=x<2||x>=X-2||y>=Y-2?1:x==2||x==X-3||y==Y-3?2:0;
            tris[mat].AddRange(new[]{a,c,b,b,c,d});
        }
        var mesh=new Mesh{name="Lliclla flexible · 1419 nodos"};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.boneWeights=w.ToArray();
        mesh.bindposes=new[]{pecho.worldToLocalMatrix*o.transform.localToWorldMatrix};mesh.subMeshCount=3;for(int i=0;i<3;i++)mesh.SetTriangles(tris[i],i);mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();mesh=Guardar(mesh,"LlicllaFisica.asset");
        var r=o.AddComponent<SkinnedMeshRenderer>();r.sharedMesh=mesh;r.bones=new[]{pecho};r.rootBone=pecho;r.updateWhenOffscreen=true;
        r.sharedMaterials=new[]{7,8,11}.Select(i=>AssetDatabase.LoadAssetAtPath<Material>(D+"Materiales/Tela_"+i.ToString("D2")+".mat")).ToArray();r.shadowCastingMode=ShadowCastingMode.TwoSided;
        var cloth=o.AddComponent<Cloth>();cloth.useGravity=true;cloth.bendingStiffness=.08f;cloth.stretchingStiffness=.94f;cloth.damping=.16f;cloth.friction=.35f;
        cloth.worldVelocityScale=.18f;cloth.worldAccelerationScale=.32f;cloth.clothSolverFrequency=180;cloth.enableContinuousCollision=true;cloth.collisionMassScale=.3f;cloth.useVirtualParticles=.6f;
        var coeff=cloth.coefficients;
        for(int i=0;i<coeff.Length;i++){float t=(i/(X+1))/(float)Y;coeff[i].maxDistance=t<.049f?0:Mathf.Lerp(.10f,1.25f,t);coeff[i].collisionSphereDistance=.008f;}
        cloth.coefficients=coeff;
        var colisiones=new List<CapsuleCollider>();
        foreach(var volumen in ropa.cuerpo)
        {
            var cobj=new GameObject("Volumen de contacto de lana");cobj.transform.SetParent(p.transform,false);
            var seguir=cobj.AddComponent<VolumenTelaChasqui>();seguir.inicio=volumen.inicio;seguir.fin=volumen.fin;seguir.marco=p.visual;
            seguir.desplazamientoInicio=volumen.desplazamientoInicio;seguir.desplazamientoFin=volumen.desplazamientoFin;
            var col=cobj.AddComponent<CapsuleCollider>();col.isTrigger=true;col.direction=1;col.radius=Mathf.Max(volumen.radioInicio,volumen.radioFin)+.012f;seguir.colision=col;seguir.Actualizar();colisiones.Add(col);
        }
        cloth.capsuleColliders=colisiones.ToArray();
        var fisica=p.gameObject.AddComponent<CapaFisicaChasqui>();fisica.tela=cloth;fisica.marco=p.visual;
    }
}

