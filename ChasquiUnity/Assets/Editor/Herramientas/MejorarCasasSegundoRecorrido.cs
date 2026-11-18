using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

static class MejorarCasasSegundoRecorrido
{
    [InitializeOnLoadMethod]
    static void Iniciar()
    {
        EditorApplication.delayCall+=Aplicar;
        EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredEditMode)EditorApplication.delayCall+=Aplicar;};
    }
    [MenuItem("Chasqui/Mejorar casas de piedra del nivel 2")]
    static void Aplicar()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        var escena=SceneManager.GetSceneByPath(ConstruirSegundoRecorrido.Ruta);bool cerrar=!escena.isLoaded;
        if(cerrar)escena=EditorSceneManager.OpenScene(ConstruirSegundoRecorrido.Ruta,OpenSceneMode.Additive);
        var mundo=escena.GetRootGameObjects().First(o=>o.name.StartsWith("Nivel 02"));
        if(mundo.transform.Find("Viviendas de mampostería andina")!=null){if(cerrar)EditorSceneManager.CloseScene(escena,true);return;}
        const string dir="Assets/Niveles/SegundoRecorrido/";
        var roca=new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/Personajes/Chasqui/MaterialPiedra.mat"));
        roca.name="Piedra gris · mampostería";roca.SetColor("_ColorBase",new Color(.62f,.63f,.59f));roca.SetColor("_ColorOscuro",new Color(.31f,.33f,.31f));
        roca.SetFloat("_Musgo",.10f);roca.SetFloat("_Escala",4);roca.SetFloat("_Rugosidad",.95f);
        AssetDatabase.CreateAsset(roca,dir+"PiedraViviendas.mat");
        var junta=new Material(Shader.Find("Standard")){color=new Color(.24f,.25f,.22f)};junta.SetFloat("_Glossiness",.01f);
        AssetDatabase.CreateAsset(junta,dir+"JuntasPiedra.mat");
        int numero=0;
        foreach(var casa in mundo.GetComponentsInChildren<Transform>().Where(t=>new[]{"Chaskiwasi de salida","Vivienda de los cultivos","Refugio junto al arroyo","Posta sin relevista"}.Contains(t.name)))
        {
            var vertices=new List<Vector3>();var triangulos=new List<int>();
            foreach(var r in casa.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name=="Muro de piedra" || r.name=="Muro posterior" || r.name=="Fachada · entrada despejada" || r.name=="Cimiento de piedra").ToArray())
            {
                r.sharedMaterial=junta;
                Vector3 escala=r.transform.lossyScale;
                bool ejeX=escala.x>=escala.z;
                float largo=ejeX?escala.x:escala.z;
                int filas=Mathf.Max(1,Mathf.RoundToInt(escala.y/.44f));float alto=escala.y/filas;
                for(int fila=0;fila<filas;fila++)
                {
                    float cursor=-largo*.5f;
                    int bloque=0;
                    while(cursor<largo*.5f-.035f)
                    {
                        float ancho=Mathf.Min(largo*.5f-cursor,bloque==0&&fila%2==1?.44f:.72f+Mathf.PerlinNoise(fila*2.7f+numero,bloque*3.1f)*.35f);
                        Vector3 centro=r.transform.position;
                        centro.y+=-escala.y*.5f+alto*(fila+.5f);
                        if(ejeX)centro.x+=cursor+ancho*.5f;else centro.z+=cursor+ancho*.5f;
                        var medida=ejeX?new Vector3(ancho-.018f,alto-.018f,escala.z+.12f):new Vector3(escala.x+.12f,alto-.018f,ancho-.018f);
                        AgregarPiedra(vertices,triangulos,casa.InverseTransformPoint(centro),medida,.045f);
                        cursor+=ancho;bloque++;
                    }
                }
            }
            var mesh=new Mesh{name="Muros de piedra con juntas y biseles"};mesh.SetVertices(vertices);mesh.SetTriangles(triangulos,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh,dir+"MamposteriaCasa"+(numero++)+".asset");
            var o=new GameObject("Mampostería · bloques de piedra en relieve");o.transform.SetParent(casa,false);
            o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=roca;
        }
        new GameObject("Viviendas de mampostería andina").transform.SetParent(mundo.transform,false);
        EditorSceneManager.MarkSceneDirty(escena);EditorSceneManager.SaveScene(escena);AssetDatabase.SaveAssets();
        Debug.Log("CASAS_PIEDRA_NIVEL02: cuatro viviendas con bloques biselados, juntas y material pétreo; colisiones conservadas.");
        if(cerrar)EditorSceneManager.CloseScene(escena,true);
    }
    static void AgregarPiedra(List<Vector3> v,List<int> t,Vector3 centro,Vector3 size,float bisel)
    {
        var h=size*.5f;float b=Mathf.Min(bisel,Mathf.Min(h.x,Mathf.Min(h.y,h.z))*.35f);
        // Six inset faces, twelve bevel strips and eight corner triangles form a closed stone.
        for(int axis=0;axis<3;axis++)foreach(int signo in new[]{-1,1})
        {
            int u=(axis+1)%3,w=(axis+2)%3;var p=new Vector3[4];int k=0;
            foreach(var s in new[]{new Vector2(-1,-1),new Vector2(1,-1),new Vector2(1,1),new Vector2(-1,1)})
            {var q=Vector3.zero;q[axis]=signo*h[axis];q[u]=s.x*(h[u]-b);q[w]=s.y*(h[w]-b);p[k++]=q;}
            Cara(v,t,centro,p);
        }
        for(int axis=0;axis<3;axis++)foreach(int su in new[]{-1,1})foreach(int sw in new[]{-1,1})
        {
            int u=(axis+1)%3,w=(axis+2)%3;var p=new Vector3[4];
            for(int i=0;i<4;i++)
            {var q=Vector3.zero;q[axis]=(i<2?-1:1)*(h[axis]-b);bool junto=i==0||i==3;q[u]=su*(h[u]-(junto?0:b));q[w]=sw*(h[w]-(junto?b:0));p[i]=q;}
            Cara(v,t,centro,p);
        }
        foreach(int sx in new[]{-1,1})foreach(int sy in new[]{-1,1})foreach(int sz in new[]{-1,1})
        {
            Vector3 c=new Vector3(sx*(h.x-b),sy*(h.y-b),sz*(h.z-b));
            Cara(v,t,centro,new[]{c+Vector3.right*sx*b,c+Vector3.up*sy*b,c+Vector3.forward*sz*b});
        }
    }
    static void Cara(List<Vector3> v,List<int> t,Vector3 centro,Vector3[] p)
    {
        int inicio=v.Count;Vector3 normal=Vector3.Cross(p[1]-p[0],p[2]-p[0]);Vector3 medio=Vector3.zero;
        foreach(var q in p)medio+=q;
        bool invertir=Vector3.Dot(normal,medio)<0;
        foreach(var q in p)v.Add(q+centro);
        for(int i=1;i<p.Length-1;i++)t.AddRange(invertir?new[]{inicio,inicio+i+1,inicio+i}:new[]{inicio,inicio+i,inicio+i+1});
    }
}
