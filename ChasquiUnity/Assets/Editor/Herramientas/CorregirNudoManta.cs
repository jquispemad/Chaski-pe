using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class CorregirNudoManta
{
    static CorregirNudoManta(){EditorApplication.delayCall+=Aplicar;}
    static void Aplicar()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        var p=Object.FindFirstObjectByType<ChasquiMovimiento>();if(p==null)return;
        var amarre=p.GetComponentsInChildren<MeshFilter>().FirstOrDefault(f=>f.sharedMesh!=null&&AssetDatabase.GetAssetPath(f.sharedMesh).EndsWith("/AmarreCuello.asset"));
        if(amarre==null)return;
        // Replace the round roll with a flat fabric fold tapering towards the knot.
        const int N=64;var v=new Vector3[(N+1)*3];var uv=new Vector2[v.Length];var triangles=new List<int>();
        for(int a=0;a<=N;a++)
        {
            float ang=a/(float)N*Mathf.PI*2;
            float frente=(Mathf.Cos(ang)+1)*.5f;
            float ancho=Mathf.Lerp(.055f,.012f,frente*frente);
            for(int j=0;j<3;j++)
            {
                float b=j/2f;int k=a*3+j;
                Vector3 centro=new Vector3(Mathf.Sin(ang)*.105f,1.493f-.047f*frente,.035f+Mathf.Cos(ang)*.15f);
                Vector3 radial=new Vector3(Mathf.Sin(ang),0,Mathf.Cos(ang));
                Vector3 point=centro+Vector3.up*((b-.5f)*ancho)+radial*(Mathf.Sin(b*Mathf.PI)*.003f);
                v[k]=amarre.transform.InverseTransformPoint(p.visual.TransformPoint(point));uv[k]=new Vector2(a/(float)N*2,b);
            }
        }
        for(int a=0;a<N;a++)for(int j=0;j<2;j++){int k=a*3+j;triangles.AddRange(new[]{k,k+3,k+1,k+1,k+3,k+4});}
        var m=new Mesh{name="Pliegue plano hacia nudo pequeno"};m.vertices=v;m.uv=uv;m.SetTriangles(triangles,0);m.RecalculateNormals();m.RecalculateTangents();m.RecalculateBounds();
        AssetDatabase.CreateAsset(m,"Assets/Personajes/Chasqui/AmarrePlanoCuello.asset");amarre.sharedMesh=m;
        var nudo=amarre.transform.Find("Nudo de tela");
        if(nudo!=null){nudo.localScale=new Vector3(.047f,.035f,.028f);nudo.position=p.visual.TransformPoint(new Vector3(0,1.446f,.194f));}
        var ropa=p.GetComponent<RopaDinamicaChasqui>();
        Limpiar(p,ropa.piel,true);ropa.salida.sharedMesh=ropa.piel.sharedMesh;
        var body=p.GetComponentsInChildren<SkinnedMeshRenderer>().First(r=>r.name=="Chasqui_Malla_Animada");Limpiar(p,body,false);
        EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);AssetDatabase.SaveAssets();
        var cam=p.camara;var entorno=ropa.entorno;p.camara=null;ropa.entorno=new BoxCollider[0];
        try{PrefabUtility.SaveAsPrefabAsset(p.gameObject,"Assets/Personajes/Chasqui/ChasquiJugadorRealista.prefab");}finally{p.camara=cam;ropa.entorno=entorno;}
        Debug.Log("NUDO_MANTA_CORREGIDO: aro eliminado, tiras antiguas de hombros retiradas, nudo pequeno guardado.");
    }
    static void Limpiar(ChasquiMovimiento p,SkinnedMeshRenderer r,bool ropa)
    {
        var bake=new Mesh();r.BakeMesh(bake);var pose=bake.vertices;var m=Object.Instantiate(r.sharedMesh);m.name=ropa?"Ropa sin tiras antiguas de hombros":"Cuerpo sin aro de hombros";
        foreach(int sub in ropa?new[]{7,8,11}:new[]{7})
        {
            var indices=m.GetTriangles(sub);var keep=new List<int>();
            for(int i=0;i<indices.Length;i+=3)
            {
                Vector3 c=p.visual.InverseTransformPoint(r.transform.TransformPoint((pose[indices[i]]+pose[indices[i+1]]+pose[indices[i+2]])/3));
                if(!ropa||c.y>1.375f)continue;
                keep.AddRange(new[]{indices[i],indices[i+1],indices[i+2]});
            }
            m.SetTriangles(keep,sub);
        }
        m.RecalculateBounds();AssetDatabase.CreateAsset(m,"Assets/Personajes/Chasqui/"+(ropa?"RopaSinTirasHombro.asset":"CuerpoSinTirasHombro.asset"));r.sharedMesh=m;Object.DestroyImmediate(bake);
    }
}

