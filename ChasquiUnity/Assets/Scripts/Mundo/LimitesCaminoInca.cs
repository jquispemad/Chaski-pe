using UnityEngine;

public static class LimitesCaminoInca
{
    public const float DistanciaLateral = 50f;

    // The same centre line used to build the road, including its straight bridge.
    public static float Centro(float z)
    {
        float x = 11 * Mathf.Sin((z - 20) * .026f) + 5 * Mathf.Sin((z - 20) * .057f);
        float puente = 11 * Mathf.Sin(144 * .026f) + 5 * Mathf.Sin(144 * .057f);
        return Mathf.Lerp(x, puente, 1 - Mathf.SmoothStep(0, 1,
            Mathf.Clamp01((Mathf.Abs(z - 164) - 13) / 13)));
    }

    public static void Crear(Transform nivel)
    {
        const string nombre = "Barreras invisibles · 50 m a cada lado";
        if (nivel.Find(nombre) != null) return;
        var grupo = new GameObject(nombre).transform;
        grupo.SetParent(nivel, false);
        for (int lado = -1; lado <= 1; lado += 2)
        {
            var borde = new GameObject(lado < 0 ? "Limite izquierdo" : "Limite derecho").transform;
            borde.SetParent(grupo, false);
            for (int z = 0; z < 350; z += 2)
            {
                Vector3 a = new Vector3(Centro(z) + lado * DistanciaLateral, 70, z);
                Vector3 b = new Vector3(Centro(z + 2) + lado * DistanciaLateral, 70, z + 2);
                var tramo = new GameObject("Barrera " + z);
                tramo.transform.SetParent(borde, false);
                // Place the inner face approximately 50 m from the road centre.
                tramo.transform.position = (a + b) * .5f + Vector3.right * lado * .3f;
                tramo.transform.rotation = Quaternion.LookRotation(b - a);
                var colision = tramo.AddComponent<BoxCollider>();
                colision.size = new Vector3(.6f, 200, Vector3.Distance(a, b) + .15f);
                colision.isTrigger = false;
                tramo.isStatic = true;
            }
        }
    }
}
