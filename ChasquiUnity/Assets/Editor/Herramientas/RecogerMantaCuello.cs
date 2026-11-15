using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class RecogerMantaCuello
{
    static RecogerMantaCuello(){EditorApplication.delayCall+=Aplicar;}
    static void Aplicar()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        var p=Object.FindFirstObjectByType<ChasquiMovimiento>();
        if(p==null)return;
        var capa=p.GetComponent<CapaFisicaChasqui>();
        if(capa==null||p.visual.Find("Amarre de la lliclla")!=null)return;
        var r=capa.tela.GetComponent<SkinnedMeshRenderer>();
        var m=Object.Instantiate(r.sharedMesh);m.name="Lliclla recogida en el cuello";
        var vertices=m.vertices;var uv=m.uv;
        for(int i=0;i<vertices.Length;i++)
        {
            float u=uv[i].x,t=uv[i].y;
            float abrir=Mathf.SmoothStep(0,1,Mathf.Clamp01(t/.25f));
            float ancho=Mathf.Lerp(.245f,Mathf.Lerp(.59f,.72f,t),abrir);
            float pliegue=Mathf.Sin(u*Mathf.PI*12+t*2)*Mathf.Lerp(.006f,.022f,abrir);
            vertices[i]=new Vector3((u-.5f)*ancho,1.50f-t*.825f-.018f*Mathf.Pow(Mathf.Abs(u-.5f)*2,2)*(1-abrir),Mathf.Lerp(-.105f,-.17f-.09f*t,abrir)-pliegue);
        }
        m.vertices=vertices;m.RecalculateNormals();m.RecalculateTangents();m.RecalculateBounds();
        AssetDatabase.CreateAsset(m,"Assets/Personajes/Chasqui/LlicllaAmarrada.asset");
        var coef=capa.tela.coefficients;capa.tela.enabled=false;r.sharedMesh=m;capa.tela.enabled=true;
        for(int i=0;i<coef.Length;i++){float t=uv[i].y;coef[i].maxDistance=t<.075f?0:Mathf.Lerp(.025f,1.15f,Mathf.InverseLerp(.075f,1,t));}
        capa.tela.coefficients=coef;
        var root=new GameObject("Amarre de la lliclla");root.transform.SetParent(p.visual,false);
        var chest=r.bones[0];var material=r.sharedMaterials[0];
        // A soft, broad fabric roll wraps behind the neck and meets in front.
        const int pasos=64,lados=10;var v=new Vector3[(pasos+1)*lados];var tex=new Vector2[v.Length];var tri=new int[pasos*lados*6];
        for(int a=0;a<=pasos;a++)
        {
            float ang=a/(float)pasos*Mathf.PI*2;
            var centro=new Vector3(Mathf.Sin(ang)*.13f,1.487f-.04f*Mathf.Max(0,Mathf.Cos(ang)),.035f+Mathf.Cos(ang)*.155f);
            var radial=new Vector3(Mathf.Sin(ang),0,Mathf.Cos(ang));
            for(int b=0;b<lados;b++){float f=b/(float)lados*Mathf.PI*2;int i=a*lados+b;v[i]=centro+Vector3.up*(Mathf.Sin(f)*.028f)+radial*(Mathf.Cos(f)*.018f);tex[i]=new Vector2(a/(float)pasos*3,b/(float)lados);}
        }
        int n=0;for(int a=0;a<pasos;a++)for(int b=0;b<lados;b++){int i=a*lados+b,j=a*lados+(b+1)%lados;foreach(int k in new[]{i,j,i+lados,j,j+lados,i+lados})tri[n++]=k;}
        var collar=new Mesh{name="Lana recogida alrededor del cuello"};collar.vertices=v;collar.uv=tex;collar.triangles=tri;collar.RecalculateNormals();collar.RecalculateTangents();
        AssetDatabase.CreateAsset(collar,"Assets/Personajes/Chasqui/AmarreCuello.asset");root.AddComponent<MeshFilter>().sharedMesh=collar;root.AddComponent<MeshRenderer>().sharedMaterial=material;
        var knot=GameObject.CreatePrimitive(PrimitiveType.Sphere);knot.name="Nudo de tela";Object.DestroyImmediate(knot.GetComponent<Collider>());knot.transform.SetParent(root.transform,false);knot.transform.localPosition=new Vector3(0,1.445f,.199f);knot.transform.localScale=new Vector3(.071f,.055f,.042f);knot.GetComponent<Renderer>().sharedMaterial=material;
        root.transform.SetParent(chest,true);
        // Keep the construction marker under Visual while the actual cloth follows the chest.
        var marker=new GameObject("Amarre de la lliclla");marker.transform.SetParent(p.visual,false);
        EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);AssetDatabase.SaveAssets();
        var cam=p.camara;var ropa=p.GetComponent<RopaDinamicaChasqui>();var entorno=ropa.entorno;p.camara=null;ropa.entorno=new BoxCollider[0];
        try{PrefabUtility.SaveAsPrefabAsset(p.gameObject,"Assets/Personajes/Chasqui/ChasquiJugadorRealista.prefab");}finally{p.camara=cam;ropa.entorno=entorno;}
        Debug.Log("MANTA_CUELLO: borde recogido, cuello envolvente y nudo frontal guardados.");
    }
}
