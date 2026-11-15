Shader "Chasqui/Cabello natural" {
 Properties {_Color("Cabello",Color)=(.018,.011,.007,1) _MainTex("Hebras",2D)="white"{}}
 SubShader {Tags{"RenderType"="Opaque"} Cull Off
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows addshadow
 #pragma target 3.0
 fixed4 _Color;
 struct Input{float2 uv_MainTex;};
 void surf(Input IN,inout SurfaceOutputStandard o){
 float fase=IN.uv_MainTex.x*350+sin(IN.uv_MainTex.y*20)*.5;
 float fade=1/(1+abs(fwidth(fase))*2);
 float hebra=sin(fase)*fade;
 o.Albedo=_Color.rgb*(.9+hebra*.12);
 o.Normal=normalize(float3(hebra*.025,0,1));o.Smoothness=.22;o.Metallic=0;o.Occlusion=.95;o.Alpha=1;
 }
 ENDCG
 }Fallback "Standard"
}
