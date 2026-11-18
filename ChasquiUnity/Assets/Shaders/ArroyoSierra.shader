Shader "Chasqui/Arroyo de sierra"
{
 Properties {_MainTex("Base",2D)="white"{}}
 SubShader {Tags{"RenderType"="Opaque"} Cull Off
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows
 #pragma target 3.0
 struct Input {float2 uv_MainTex;float3 worldPos;float4 color:COLOR;};
 void surf(Input IN,inout SurfaceOutputStandard o)
 {
   float2 p=IN.uv_MainTex;
   float fase=p.y*7+_Time.y*2.3;
   float onda=sin(fase+sin(p.x*16))*sin(p.x*31+fase*.4);
   float centro=saturate(IN.color.r);
   float espuma=(1-smoothstep(.02,.25,centro))*smoothstep(.55,.9,sin(fase*1.3+p.x*25)*.5+.5);
   o.Albedo=lerp(float3(.26,.32,.23),float3(.055,.19,.18),centro)+onda*.015+espuma*.2;
   o.Normal=normalize(float3(onda*.045,cos(fase*.7+p.x*22)*.035,1));
   o.Smoothness=.76;o.Metallic=.12;o.Occlusion=.97;
 }
 ENDCG
 }Fallback "Standard"
}
