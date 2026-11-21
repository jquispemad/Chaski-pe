using UnityEngine;
public static class FuenteCastellanaChaski
{
 static Font fuente;
 public static Font Obtener(){if(fuente==null)fuente=Font.CreateDynamicFontFromOSFont(new[]{"Georgia","Times New Roman","DejaVu Serif"},24);return fuente;}
}
