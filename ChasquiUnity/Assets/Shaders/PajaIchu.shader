Shader "Chasqui/Paja de ichu" {
 Properties {_Color("Paja seca",Color)=(.63,.43,.18,1)}
 SubShader {Tags{"RenderType"="Opaque"} Cull Off
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows addshadow vertex:vert
 #pragma target 3.0
 fixed4 _Color;
 struct Input{float3 localPos;float4 color:COLOR;};
 void vert(inout appdata_full v,out Input o){UNITY_INITIALIZE_OUTPUT(Input,o);o.localPos=v.vertex.xyz;o.color=v.color;}
 float hash(float n){return frac(sin(n*127.1)*43758.5453);}
 void surf(Input IN,inout SurfaceOutputStandard o){
 float3 p=IN.localPos;float hebra=hash(floor(p.z*760));
 float estria=sin(p.z*3800+sin(p.x*53)*.9);float fade=1/(1+abs(fwidth(p.z*3800)));
 float tramo=hash(floor(p.x*18)+floor(p.z*180)*37);
 o.Albedo=_Color.rgb*(.73+hebra*.40+tramo*.16+estria*.12*fade)*max(IN.color.rgb,float3(.65,.65,.65));
 o.Normal=normalize(float3(0,estria*.16*fade,1));o.Smoothness=.07;o.Occlusion=.87+hebra*.12;o.Alpha=1;
 }
 ENDCG
 }Fallback "Diffuse"
}
