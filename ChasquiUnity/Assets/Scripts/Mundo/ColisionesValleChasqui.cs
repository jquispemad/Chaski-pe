using UnityEngine;
using UnityEngine.SceneManagement;

public static class ColisionesValleChasqui
{
    public static bool EsSolido(MeshFilter filtro)
    {
        string n=filtro.name;
        var visible=filtro.GetComponent<Renderer>();if(visible!=null&&!visible.enabled)return false;
        if(filtro.GetComponent<TextMesh>()!=null || filtro.GetComponentInParent<ChasquiMovimiento>()!=null)return false;
        return n.StartsWith("Roca") || n=="Tronco" || n.StartsWith("Salto") ||
            n=="Letrero de ruta" || n=="Tablilla" || n=="Poste de ruta" ||
            n=="Muros" || n=="Techo de ichu" || n=="Dintel" || n=="Puerta" ||
            n=="Ventana" || n=="Hito de piedra" || n.StartsWith("Anden") ||
            n=="Cerco de madera" || n=="Travesano" || n=="Mojon" ||
            n=="Poste del puente" || n.StartsWith("Puente · tablones") || n=="Baranda · colision";
    }

    public static int Aplicar(Scene escena)
    {
        int nuevos=0;
        foreach(var raiz in escena.GetRootGameObjects())
        foreach(var filtro in raiz.GetComponentsInChildren<MeshFilter>(true))
        {
            if(filtro.sharedMesh==null || !EsSolido(filtro))continue;
            if(filtro.name=="Tronco")
            {
                // Branch triangles formed narrow wedges against hillsides and trapped the controller.
                // A smooth trunk collider blocks the solid stem without enclosing the canopy.
                foreach(var c in filtro.GetComponents<Collider>())c.enabled=false;
                var tronco=filtro.GetComponent<CapsuleCollider>();
                if(tronco==null){tronco=filtro.gameObject.AddComponent<CapsuleCollider>();nuevos++;}
                tronco.direction=1;tronco.center=new Vector3(.09f,1.6f,.05f);
                tronco.height=3.2f;tronco.radius=.23f;tronco.isTrigger=false;tronco.enabled=true;
                continue;
            }
            // Use actual rock geometry: tops remain available to land on.
            bool malla=filtro.name.StartsWith("Roca");
            var existente=filtro.GetComponent<Collider>();
            if(existente==null)
            {
                if(malla)filtro.gameObject.AddComponent<MeshCollider>().sharedMesh=filtro.sharedMesh;
                else
                {
                    var caja=filtro.gameObject.AddComponent<BoxCollider>();
                    caja.center=filtro.sharedMesh.bounds.center;caja.size=filtro.sharedMesh.bounds.size;
                }
                nuevos++;
            }
            foreach(var c in filtro.GetComponents<Collider>()){c.enabled=true;c.isTrigger=false;}
        }
        // Disable the old duplicate colliders on the tree roots.
        foreach(var raiz in escena.GetRootGameObjects())
        foreach(var c in raiz.GetComponentsInChildren<CapsuleCollider>(true))
            if(c.name.StartsWith("Arbol"))c.enabled=false;
        foreach(var raiz in escena.GetRootGameObjects())
        foreach(var c in raiz.GetComponentsInChildren<CharacterController>(true))
        {
            c.detectCollisions=true;c.enableOverlapRecovery=true;
            c.stepOffset=.22f;c.slopeLimit=48f;c.skinWidth=.025f;c.minMoveDistance=0;
        }
        Physics.SyncTransforms();
        return nuevos;
    }
}
