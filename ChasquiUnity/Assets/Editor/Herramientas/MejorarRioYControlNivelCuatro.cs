using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MejorarRioYControlNivelCuatro
{
    const string Marca = "Nivel 4 · río elevado y orillas naturales";
    [InitializeOnLoadMethod] static void Preparar() => EditorApplication.delayCall += Aplicar;
    static void Aplicar()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode || AssetDatabase.LoadAssetAtPath<SceneAsset>(ConstruirPuenteAbismo.Ruta)==null)return;
        var e=SceneManager.GetSceneByPath(ConstruirPuenteAbismo.Ruta);bool cerrar=!e.isLoaded;
        if(cerrar)e=EditorSceneManager.OpenScene(ConstruirPuenteAbismo.Ruta,OpenSceneMode.Additive);
        if(e.GetRootGameObjects().Any(o=>o.name==Marca)){if(cerrar)EditorSceneManager.CloseScene(e,true);return;}
        var m=e.GetRootGameObjects().Select(o=>o.GetComponent<NivelCuatroChaski>()).First(o=>o!=null);
        var mundo=e.GetRootGameObjects().First(o=>o.name.StartsWith("Nivel 04 ·")).transform;
        var terreno=mundo.GetComponentInChildren<Terrain>();float Agua=-18.5f;
        var filtro=mundo.GetComponentsInChildren<MeshFilter>().First(o=>o.name=="Río · fondo de la quebrada");
        var vertices=new List<Vector3>();var uv=new List<Vector2>();var indices=new List<int>();var colores=new List<Color>();
        for(int i=0;i<=480;i++)
        {
            float x=-120+i*.5f,centro=NivelCuatroChaski.CentroRio(x);float[] borde=new float[2];
            for(int lado=0;lado<2;lado++)
            {
                float signo=lado==0?-1:1,anterior=0;
                for(float d=.1f;d<=32;d+=.1f)
                {
                    var p=new Vector3(x,0,centro+signo*d);
                    if(terreno.SampleHeight(p)+terreno.transform.position.y>=Agua)
                    {
                        float bajo=anterior,alto=d;
                        for(int k=0;k<12;k++){float medio=(bajo+alto)*.5f;p.z=centro+signo*medio;if(terreno.SampleHeight(p)+terreno.transform.position.y<Agua)bajo=medio;else alto=medio;}
                        borde[lado]=centro+signo*(bajo+alto)*.5f;break;
                    }
                    anterior=d;
                }
            }
            for(int j=0;j<11;j++)
            {
                float u=j/10f;vertices.Add(new Vector3(x,Agua,Mathf.Lerp(borde[0],borde[1],u)));uv.Add(new Vector2(u,x*.23f));colores.Add(new Color(Mathf.Sin(u*Mathf.PI),0,0,1));
                if(i>0&&j<10){int a=(i-1)*11+j,b=i*11+j;indices.AddRange(new[]{a,a+1,b,a+1,b+1,b});}
            }
        }
        var mesh=filtro.sharedMesh;mesh.Clear();mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.SetUVs(0,uv);mesh.SetColors(colores);mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
        // Quitar las dos filas repetidas de grandes bloques que bordeaban el río.
        int quitadas=0;foreach(var roca in mundo.GetComponentsInChildren<MeshFilter>().Where(o=>o.name=="Roca asentada · ladera").ToArray())
            if(Mathf.Abs(roca.transform.position.z-146)<.1f||Mathf.Abs(roca.transform.position.z-191)<.1f){Object.DestroyImmediate(roca.gameObject);quitadas++;}
        var rocaMesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Niveles/Valle/Roca0.asset");var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Personajes/Chasqui/MaterialPiedra.mat");var azar=new System.Random(4407);
        for(int i=0;i<12;i++)
        {
            float x=-105+i*18+(float)azar.NextDouble()*8,z=NivelCuatroChaski.CentroRio(x)+((float)azar.NextDouble()-.5f)*4;
            var p=new Vector3(x,0,z);float suelo=terreno.SampleHeight(p)+terreno.transform.position.y;
            var o=new GameObject("Roca de cauce · parcialmente sumergida");o.transform.SetParent(mundo);o.transform.rotation=Quaternion.Euler(0,(float)azar.NextDouble()*360,0);
            float ancho=1.1f+(float)azar.NextDouble()*1.5f,alto=Mathf.Max(.6f,Agua-suelo+(i%4==0?.25f:-.3f));var b=rocaMesh.bounds;o.transform.localScale=new Vector3(ancho/b.size.x,alto/b.size.y,ancho*.8f/b.size.z);
            o.AddComponent<MeshFilter>().sharedMesh=rocaMesh;var r=o.AddComponent<MeshRenderer>();r.sharedMaterial=material;o.transform.position=p-r.bounds.center+Vector3.up*r.bounds.center.y;o.transform.position+=Vector3.up*(suelo-r.bounds.min.y-.15f);
        }
        if(m.controles.Length==3)
        {
            var c=new GameObject("Control · antes de la ladera del abismo");SceneManager.MoveGameObjectToScene(c,e);var p=new Vector3(NivelCuatroChaski.Centro(68),0,68);p.y=terreno.SampleHeight(p)+terreno.transform.position.y+.12f;c.transform.position=p;
            m.controles=new[]{m.controles[0],c.transform,m.controles[1],m.controles[2]};
            int previo=PlayerPrefs.GetInt("Chaski.Nivel04.Control",0);if(previo>0)PlayerPrefs.SetInt("Chaski.Nivel04.Control",previo+1);PlayerPrefs.Save();
        }
        var marca=new GameObject(Marca);SceneManager.MoveGameObjectToScene(marca,e);EditorSceneManager.SaveScene(e);AssetDatabase.SaveAssets();
        System.IO.File.WriteAllText("Temp/nivel04_rio_mejora.txt",$"Agua: {Agua} m (antes -21,15). Filas de rocas retiradas: {quitadas}. Rocas de cauce: 12. Controles: {m.controles.Length}.\n");
        if(cerrar)EditorSceneManager.CloseScene(e,true);
    }
}
