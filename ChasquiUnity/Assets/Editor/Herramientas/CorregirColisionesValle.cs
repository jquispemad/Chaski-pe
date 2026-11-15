using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CorregirColisionesValle
{
    static CorregirColisionesValle(){EditorApplication.delayCall+=Instalar;}
    [MenuItem("Chasqui/Corregir colisiones del valle")]
    public static void Instalar()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        var nivel=Object.FindFirstObjectByType<NivelValleChasqui>();if(nivel==null)return;
        if(SessionState.GetBool("ColisionesValleV1",false))return;
        int nuevos=ColisionesValleChasqui.Aplicar(nivel.gameObject.scene);
        var solidos=nivel.gameObject.scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<MeshFilter>(true))
            .Where(f=>f.sharedMesh!=null&&ColisionesValleChasqui.EsSolido(f)).ToArray();
        int faltan=solidos.Count(f=>!f.GetComponents<Collider>().Any(c=>c.enabled&&!c.isTrigger));
        int rocas=solidos.Count(f=>f.name.StartsWith("Roca")),arboles=solidos.Count(f=>f.name=="Tronco");
        // Validate a real CharacterController against representative scene geometry.
        int bloqueados=0,apoyos=0;
        var prueba=new GameObject("Prueba temporal de colisiones");prueba.hideFlags=HideFlags.HideAndDontSave;
        var jugador=new GameObject("Controlador de prueba");jugador.hideFlags=HideFlags.HideAndDontSave;
        var cc=jugador.AddComponent<CharacterController>();cc.height=2;cc.radius=.23f;cc.center=Vector3.up;
        cc.stepOffset=.22f;cc.skinWidth=.025f;cc.minMoveDistance=0;
        try
        {
            foreach(string tipo in new[]{"Roca","Tronco","Tablilla","Letrero de ruta","Muros"})
            {
                var f=solidos.FirstOrDefault(x=>x.name.StartsWith(tipo));if(f==null)continue;
                prueba.transform.SetPositionAndRotation(new Vector3(0,150,0),f.transform.rotation);
                prueba.transform.localScale=f.transform.lossyScale;
                var col=prueba.AddComponent<MeshCollider>();col.sharedMesh=f.sharedMesh;
                Physics.SyncTransforms();Bounds b=col.bounds;
                // Find the actual centre-facing surface rather than relying on irregular mesh bounds.
                var rayo=new Ray(new Vector3(b.center.x,b.center.y,b.min.z-3),Vector3.forward);
                if(!col.Raycast(rayo,out var frente,b.size.z+6))
                {Object.DestroyImmediate(col);continue;}
                cc.enabled=false;jugador.transform.position=frente.point+Vector3.back-Vector3.up;cc.enabled=true;
                Physics.SyncTransforms();for(int i=0;i<100;i++)cc.Move(Vector3.forward*.04f);
                if(jugador.transform.position.z<frente.point.z+.08f)bloqueados++;
                if(tipo=="Roca")
                {
                    var arriba=new Ray(new Vector3(b.center.x,b.max.y+3,b.center.z),Vector3.down);
                    if(col.Raycast(arriba,out var cima,b.size.y+6))
                    {
                        cc.enabled=false;jugador.transform.position=cima.point+Vector3.up*2;cc.enabled=true;
                        for(int i=0;i<180;i++)cc.Move(Vector3.down*.025f);
                        if(cc.isGrounded&&jugador.transform.position.y>cima.point.y-.12f)apoyos++;
                    }
                }
                Object.DestroyImmediate(col);
            }
        }
        finally{Object.DestroyImmediate(jugador);Object.DestroyImmediate(prueba);}
        EditorSceneManager.MarkSceneDirty(nivel.gameObject.scene);EditorSceneManager.SaveScene(nivel.gameObject.scene);
        string informe=(faltan==0&&bloqueados==5&&apoyos==1?"CORRECTO":"REVISAR")+
            "\nColliders agregados: "+nuevos+"\nRocas solidas: "+rocas+"\nArboles solidos: "+arboles+
            "\nObjetos solidos sin colision: "+faltan+"\nControlador bloqueado por roca, arbol, tablilla, letrero y muro: "+bloqueados+
            " / 5\nPuede aterrizar sobre roca: "+(apoyos==1);
        File.WriteAllText("Temp/chasqui_colisiones_valle.txt",informe);Debug.Log(informe);
        SessionState.SetBool("ColisionesValleV1",true);
    }
}
