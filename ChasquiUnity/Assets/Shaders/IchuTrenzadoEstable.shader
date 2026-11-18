Shader "Chasqui/Ichu trenzado estable"
{
    Properties
    {
        _Color("Ichu seco", Color) = (.64,.46,.23,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        struct Input { float3 worldPos; };
        fixed4 _Color;
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float3 p = IN.worldPos;
            float fila = floor(p.x * 6.0);
            float fase = p.z * 75.0 + sin(fila * 2.3) * 2.0;
            float filtro = 1.0 / (1.0 + fwidth(fase) * fwidth(fase));
            float fibra = sin(fase) * .10 * filtro;
            float trenza = sin(p.x * 35.0 + sin(p.z * 8.0)) * .07;
            float variacion = sin(fila * 1.73) * .09;
            o.Albedo = _Color.rgb * (.94 + fibra + trenza + variacion);
            o.Smoothness = .035;
            o.Occlusion = .94;
            o.Normal = float3(0,0,1);
        }
        ENDCG
    }
    Fallback "Diffuse"
}
