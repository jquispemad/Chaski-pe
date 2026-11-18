Shader "Chasqui/Paja agrupada de cubierta"
{
 Properties { _Color("Ichu seco",Color)=(.59,.46,.27,1) }
 SubShader {
 Tags {"RenderType"="Opaque"} Cull Off
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows addshadow
 #pragma target 3.0
 fixed4 _Color;
 struct Input {float3 worldPos;float3 worldNormal;};
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 void surf(Input IN,inout SurfaceOutputStandard o)
 {
   float3 p=IN.worldPos;
   float grupo=hash(floor(p.xz*float2(4,7)));
   float vertical=saturate(1-abs(IN.worldNormal.y)*2);
   float fase=lerp(p.z*155,p.x*155,vertical)+sin(p.y*8)*.3;
   float filtro=1/(1+pow(fwidth(fase)*1.5,2));
   float fibras=sin(fase)*.065*filtro;
   float variacion=.81+grupo*.29+fibras+sin(p.y*45)*.06*vertical;
   o.Albedo=_Color.rgb*variacion;o.Smoothness=.01;o.Occlusion=.90;
 }
 ENDCG
 } Fallback "Diffuse"
}
