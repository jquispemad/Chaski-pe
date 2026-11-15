using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class RefinarCabelloYCapa
{
    const string D="Assets/Personajes/Chasqui/";
    static RefinarCabelloYCapa(){EditorApplication.delayCall+=Auto;}
    static void Auto()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||SessionState.GetBool("CabelloNaturalV2",false))return;
        var p=UnityEngine.Object.FindFirstObjectByType<ChasquiMovimiento>();if(p==null||p.GetComponent<CapaFisicaChasqui>()==null)return;
        if(p.visual.Find("Cabello · mechones naturales")==null)Aplicar(p);SessionState.SetBool("CabelloNaturalV2",true);
    }
    static Mesh Guardar(Mesh m,string n){string path=D+n;var previo=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(previo==null)AssetDatabase.CreateAsset(m,path);else{EditorUtility.CopySerialized(m,previo);UnityEngine.Object.DestroyImmediate(m);m=previo;}return m;}
    static void Aplicar(ChasquiMovimiento p)
    {
        var fisica=p.GetComponent<CapaFisicaChasqui>();var tela=fisica.tela;
        tela.selfCollisionDistance=.012f;tela.selfCollisionStiffness=.85f;tela.SetSelfAndInterCollisionIndices(Enumerable.Range(0,tela.vertices.Length).Select(i=>(uint)i).ToList());
        tela.bendingStiffness=.18f;tela.useTethers=true;tela.worldVelocityScale=.08f;tela.worldAccelerationScale=.18f;
        var pelvis=p.GetComponentsInChildren<Transform>().First(t=>t.name=="pelvis");
        var volumen=new GameObject("Contacto de capa contra tunica");volumen.transform.SetParent(pelvis,false);
        volumen.transform.position=p.visual.TransformPoint(new Vector3(0,.89f,-.025f));volumen.transform.rotation=p.visual.rotation;
        var capsula=volumen.AddComponent<CapsuleCollider>();capsula.isTrigger=true;capsula.radius=.29f;capsula.height=.67f;
        tela.capsuleColliders=tela.capsuleColliders.Concat(new[]{capsula}).ToArray();
        var cuerpo=p.GetComponentsInChildren<SkinnedMeshRenderer>().First(r=>r.name=="Chasqui_Malla_Animada");
        var baseCuerpo=UnityEngine.Object.Instantiate(cuerpo.sharedMesh);var bake=new Mesh();cuerpo.BakeMesh(bake);var pose=bake.vertices;
        foreach(int sub in new[]{4,9})
        {
            var quedan=new List<int>();var tri=baseCuerpo.GetTriangles(sub);
            for(int i=0;i<tri.Length;i+=3)
            {
                Vector3 centro=p.visual.InverseTransformPoint(cuerpo.transform.TransformPoint((pose[tri[i]]+pose[tri[i+1]]+pose[tri[i+2]])/3));
                // Keep eyebrows at the front of the face; replace the old scalp rods and stiff ends.
                if(centro.z>.13f&&centro.y<1.79f)quedan.AddRange(new[]{tri[i],tri[i+1],tri[i+2]});
            }
            baseCuerpo.SetTriangles(quedan,sub);
        }
        UnityEngine.Object.DestroyImmediate(bake);cuerpo.sharedMesh=Guardar(baseCuerpo,"CuerpoSinCabelloRigido.asset");
        var dinamica=p.GetComponent<CabelloDinamicoChasqui>();var head=dinamica.cabeza;
        var obj=new GameObject("Cabello · mechones naturales");obj.transform.SetParent(p.visual,false);
        var puntos=new List<Vector3>();var uv=new List<Vector2>();var pesos=new List<BoneWeight>();var triangulos=new List<int>();
        // A continuous scalp prevents skin showing through the hair strands.
        const int A=64,B=18;
        for(int j=0;j<=B;j++)for(int i=0;i<=A;i++)
        {
            float theta=j/(float)B*1.55f,phi=i/(float)A*Mathf.PI*2;
            puntos.Add(new Vector3(Mathf.Sin(theta)*Mathf.Sin(phi)*.135f,1.755f+Mathf.Cos(theta)*.17f,.055f+Mathf.Sin(theta)*Mathf.Cos(phi)*.14f));uv.Add(new Vector2(i/(float)A,j/(float)B));pesos.Add(new BoneWeight{boneIndex0=0,weight0=1});
        }
        for(int j=0;j<B;j++)for(int i=0;i<A;i++){int a=j*(A+1)+i;triangulos.AddRange(new[]{a,a+1,a+A+1,a+1,a+A+2,a+A+1});}
        var azar=new System.Random(831);
        for(int mechon=0;mechon<140;mechon++)
        {
            float phi=Mathf.Lerp(.72f,Mathf.PI*2-.72f,mechon/139f);float longitud=.36f+(float)azar.NextDouble()*.07f;
            int grupo=Mathf.Clamp(Mathf.FloorToInt(Mathf.InverseLerp(-.18f,.18f,Mathf.Sin(phi)*.15f)*8),0,7);int inicio=puntos.Count;
            for(int j=0;j<=14;j++)
            {
                float t=j/14f;float radio=.035f+.12f*Mathf.Sin(t*Mathf.PI*.5f);
                Vector3 centro=new Vector3(Mathf.Sin(phi)*radio,1.918f-t*longitud,.055f+Mathf.Cos(phi)*radio-.02f*t*t);
                centro+=new Vector3(Mathf.Sin(t*7+phi*3)*.004f*t,0,Mathf.Sin(t*8+phi*4)*.004f*t);
                Vector3 lado=new Vector3(Mathf.Cos(phi),0,-Mathf.Sin(phi));float ancho=(.009f+(float)azar.NextDouble()*.002f)*Mathf.Lerp(1,.12f,Mathf.Pow(t,5));
                for(int k=0;k<2;k++)
                {
                    puntos.Add(centro+lado*ancho*(k==0?-.5f:.5f));uv.Add(new Vector2(k,t));float flexible=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.40f,1,t))*.90f;
                    pesos.Add(new BoneWeight{boneIndex0=0,weight0=1-flexible,boneIndex1=grupo+1,weight1=flexible});
                }
                if(j<14){int a=inicio+j*2;triangulos.AddRange(new[]{a,a+2,a+1,a+1,a+2,a+3});}
            }
        }
        var mesh=new Mesh{name="Cabello continuo con 140 mechones"};mesh.SetVertices(puntos);mesh.SetUVs(0,uv);mesh.SetTriangles(triangulos,0);mesh.boneWeights=pesos.ToArray();
        Transform[] bones=new[]{head}.Concat(dinamica.mechones).ToArray();mesh.bindposes=bones.Select(b=>b.worldToLocalMatrix*obj.transform.localToWorldMatrix).ToArray();mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();mesh=Guardar(mesh,"CabelloNaturalFlexible.asset");
        var r=obj.AddComponent<SkinnedMeshRenderer>();r.sharedMesh=mesh;r.bones=bones;r.rootBone=head;r.updateWhenOffscreen=true;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.TwoSided;
        string matPath=D+"Materiales/CabelloNatural.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(matPath);if(mat==null){mat=new Material(Shader.Find("Chasqui/Cabello natural"));AssetDatabase.CreateAsset(mat,matPath);}mat.color=new Color(.018f,.011f,.007f);r.sharedMaterial=mat;
        foreach(int i in new[]{0,5,16}){var m=AssetDatabase.LoadAssetAtPath<Material>(D+"Materiales/Material_"+i.ToString("D2")+".mat");if(m!=null){m.SetFloat("_Glossiness",i==5?.27f:.20f);EditorUtility.SetDirty(m);}}
        EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);AssetDatabase.SaveAssets();
        var camara=p.camara;var ropa=p.GetComponent<RopaDinamicaChasqui>();var entorno=ropa.entorno;p.camara=null;ropa.entorno=new BoxCollider[0];
        try{PrefabUtility.SaveAsPrefabAsset(p.gameObject,D+"ChasquiJugadorRealista.prefab");}finally{p.camara=camara;ropa.entorno=entorno;}
        Debug.Log("CABELLO_NATURAL_Y_CONTACTO_DE_CAPA_LISTOS");
    }
}
