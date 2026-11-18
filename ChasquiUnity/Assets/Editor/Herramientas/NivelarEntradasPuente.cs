using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

static class NivelarEntradasPuente
{
    [InitializeOnLoadMethod]
    static void Programar()
    {
        EditorApplication.delayCall+=Aplicar;
        EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredEditMode)EditorApplication.delayCall+=Aplicar;};
    }
    [MenuItem("Chasqui/Nivelar accesos del puente")]
    public static void Aplicar()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        var escena=SceneManager.GetSceneByPath(ConstruirSegundoRecorrido.Ruta);bool cerrar=!escena.isLoaded;
        if(cerrar)escena=EditorSceneManager.OpenScene(ConstruirSegundoRecorrido.Ruta,OpenSceneMode.Additive);
        var mundo=escena.GetRootGameObjects().First(o=>o.name.StartsWith("Nivel 02"));
        if(mundo.transform.Find("Accesos del puente · suelo y colisión alineados")==null)
        {
            var grupo=new GameObject("Accesos del puente · suelo y colisión alineados").transform;grupo.SetParent(mundo.transform,false);
            var terreno=mundo.GetComponentInChildren<Terrain>();var d=terreno.terrainData;var origen=terreno.transform.position;
            int n=d.heightmapResolution;var alturas=d.GetHeights(0,0,n,n);
            for(int j=0;j<n;j++)for(int i=0;i<n;i++)
            {
                float z=origen.z+j*d.size.z/(n-1);
                if(!((z>=262&&z<=267)||(z>=293&&z<=298)))continue;
                float x=origen.x+i*d.size.x/(n-1),dist=Mathf.Abs(x-ConstruirSegundoRecorrido.Centro(z));
                float peso=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(2.9f,4,dist));
                float y=alturas[j,i]*d.size.y+origen.y;
                alturas[j,i]=(Mathf.Lerp(y,1f,peso)-origen.y)/d.size.y;
            }
            d.SetHeights(0,0,alturas);EditorUtility.SetDirty(d);
            var sendero=mundo.GetComponentsInChildren<MeshFilter>().First(m=>m.name=="Sendero · tierra compactada visible");
            var malla=sendero.sharedMesh;var v=malla.vertices;
            for(int i=0;i<v.Length;i++)
                if((v[i].z>=262&&v[i].z<=267)||(v[i].z>=293&&v[i].z<=298))v[i].y=1.01f;
            malla.vertices=v;malla.RecalculateNormals();malla.RecalculateBounds();EditorUtility.SetDirty(malla);
            foreach(bool salida in new[]{false,true})
            {
                var vertices=new List<Vector3>();var indices=new List<int>();
                const int pasos=48;
                for(int i=0;i<=pasos;i++)
                {
                    float z=(salida?291f:262f)+i*7f/pasos;
                    float dentro=salida?Mathf.Clamp01((293-z)/2):Mathf.Clamp01((z-267)/2);
                    float y=z>=267&&z<=293?1-.65f*Mathf.Sin((z-267)/26*Mathf.PI):1.01f;
                    float ancho=z<267?Mathf.Lerp(2.9f,1.68f,Mathf.InverseLerp(264,267,z)):
                        z>293?Mathf.Lerp(1.68f,2.9f,Mathf.InverseLerp(293,296,z)):1.68f;
                    float x=z>=267&&z<=293?ConstruirSegundoRecorrido.Centro(280):ConstruirSegundoRecorrido.Centro(z);
                    vertices.Add(new Vector3(x-ancho,y,z));vertices.Add(new Vector3(x+ancho,y,z));
                    if(i>0){int a=(i-1)*2,b=i*2;indices.AddRange(new[]{a,b,a+1,a+1,b,b+1});}
                }
                var mesh=new Mesh{name="Acceso continuo al puente"};mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
                AssetDatabase.CreateAsset(mesh,"Assets/Niveles/SegundoRecorrido/AccesoPuente"+(salida?"Salida":"Entrada")+".asset");
                var o=new GameObject(salida?"Puente · transición nivelada de salida":"Puente · transición nivelada de entrada");o.transform.SetParent(grupo,false);o.AddComponent<MeshCollider>().sharedMesh=mesh;
            }
            Physics.SyncTransforms();int muestras=0;float diferencia=0;
            foreach(bool salida in new[]{false,true})for(int i=0;i<=48;i++)
            {
                float z=(salida?291f:262f)+i*7f/48;
                float esperado=z>=267&&z<=293?1-.65f*Mathf.Sin((z-267)/26*Mathf.PI):1.01f;
                float x=z>=267&&z<=293?ConstruirSegundoRecorrido.Centro(280):ConstruirSegundoRecorrido.Centro(z);
                if(Physics.Raycast(new Vector3(x,5,z),Vector3.down,out var h,8))
                {muestras++;diferencia=Mathf.Max(diferencia,Mathf.Abs(h.point.y-esperado));}
            }
            System.IO.File.WriteAllText("Temp/accesos_puente_revision.txt",$"Apoyos: {muestras}/98\nDiferencia máxima del piso: {diferencia:F4} m\n");
            EditorSceneManager.MarkSceneDirty(escena);EditorSceneManager.SaveScene(escena);AssetDatabase.SaveAssets();
            Debug.Log($"ACCESOS_PUENTE_NIVELADOS: {muestras}/98 apoyos; diferencia entre apoyo y superficie {diferencia:F4} m.");
        }
        if(cerrar)EditorSceneManager.CloseScene(escena,true);
    }
}
