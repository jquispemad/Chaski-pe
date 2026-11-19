using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AbastecerRutaChaski
{
    const string Marca="Provisiones de ruta · sin recuperación gratuita";
    [InitializeOnLoadMethod]static void Preparar()=>EditorApplication.delayCall+=Aplicar;
    static void Aplicar()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        Abastecer(ConstruirValleVientos.Ruta,true);Abastecer(ConstruirPuenteAbismo.Ruta,false);
    }
    static void Abastecer(string ruta,bool tres)
    {
        if(AssetDatabase.LoadAssetAtPath<SceneAsset>(ruta)==null)return;
        var e=SceneManager.GetSceneByPath(ruta);bool cerrar=!e.isLoaded;if(cerrar)e=EditorSceneManager.OpenScene(ruta,OpenSceneMode.Additive);
        if(e.GetRootGameObjects().Any(o=>o.name==Marca)){if(cerrar)EditorSceneManager.CloseScene(e,true);return;}
        var m3=e.GetRootGameObjects().Select(o=>o.GetComponent<NivelTresChaski>()).FirstOrDefault(o=>o!=null);
        var m4=e.GetRootGameObjects().Select(o=>o.GetComponent<NivelCuatroChaski>()).FirstOrDefault(o=>o!=null);
        var lista=(tres?m3.recursos:m4.recursos).ToList();var terreno=e.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Terrain>()).First();
        float Centro(float z)=>tres?NivelTresChaski.Centro(z):NivelCuatroChaski.Centro(z);
        void Situar(RecursoValleVientos r,float z,float lado,bool moverBase=true)
        {
            var antes=r.transform.position;var p=new Vector3(Centro(z)+lado,0,z);p.y=terreno.SampleHeight(p)+terreno.transform.position.y+.45f;
            var pedestal=e.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Transform>()).FirstOrDefault(o=>o.name=="Piedra de abastecimiento"&&Vector2.Distance(new Vector2(o.position.x,o.position.z),new Vector2(antes.x,antes.z))<.3f);
            if(moverBase&&pedestal!=null)pedestal.position+=p-antes;r.transform.position=p;
        }
        void Agregar(int tipo,float z,float lado,string id)
        {
            var original=lista.FirstOrDefault(o=>o.tipo==tipo)??lista.First();var o=Object.Instantiate(original.gameObject,original.transform.parent);o.name="Provisión en el sendero · "+id;
            var r=o.GetComponent<RecursoValleVientos>();r.identificador=id;r.tipo=tipo;
            // Los nuevos tipos siempre se clonan de una ración del mismo alimento.
            Situar(r,z,lado,false);lista.Add(r);
            var baseRacion=GameObject.CreatePrimitive(PrimitiveType.Cube);baseRacion.name="Piedra de abastecimiento";baseRacion.transform.SetParent(original.transform.parent);baseRacion.transform.position=r.transform.position-Vector3.up*.25f;baseRacion.transform.localScale=new Vector3(.7f,.4f,.65f);baseRacion.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Personajes/Chasqui/MaterialPiedra.mat");
        }
        if(tres)
        {
            Situar(lista.First(o=>o.identificador=="cancha-ladera"),80,-2.1f);
            Situar(lista.First(o=>o.identificador=="coca-ladera"),125,2.1f);
            Situar(lista.First(o=>o.identificador=="charki-desvio"),185,-2.1f);
            Situar(lista.First(o=>o.identificador=="agua-refugio"),245,2.1f);
            Situar(lista.First(o=>o.identificador=="cancha-refugio"),310,-2.1f);
            Agregar(3,365,2.1f,"agua-tramo-medio");Agregar(2,420,-2.1f,"charki-sendero");Agregar(1,465,2.1f,"cancha-tramo-final");m3.recursos=lista.ToArray();
        }
        else
        {
            Situar(lista.First(o=>o.identificador=="cancha-mirador"),64,-2.1f);
            Agregar(3,280,2.1f,"agua-montana");Agregar(2,330,-2.1f,"charki-montana");m4.recursos=lista.ToArray();
        }
        var marca=new GameObject(Marca);SceneManager.MoveGameObjectToScene(marca,e);EditorSceneManager.SaveScene(e);
        System.IO.File.WriteAllText("Temp/abastecimiento-nivel"+(tres?3:4)+".txt",string.Join("\n",lista.OrderBy(o=>o.transform.position.z).Select(o=>o.Nombre+" · "+o.identificador+" · "+o.transform.position.ToString("F2"))));
        if(cerrar)EditorSceneManager.CloseScene(e,true);
    }
}
