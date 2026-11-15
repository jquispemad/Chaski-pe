Shader "Chasqui/Rio valle" {
 Properties {_Color("Agua",Color)=(.055,.25,.25,1)}
 SubShader {Tags{"RenderType"="Opaque"} CGPROGRAM
 #pragma surface surf Standard fullforwardshadows
 #pragma target 3.0
 fixed4 _Color;struct Input{float3 worldPos;float3 viewDir;};
 void surf(Input IN,inout SurfaceOutputStandard o){float2 p=IN.worldPos.xz;float ondas=sin(p.x*3+_Time.y*2+p.y*.7)*sin(p.y*4-_Time.y);o.Normal=normalize(float3(ondas*.035,cos(p.x*5+_Time.y*3)*.025,1));o.Albedo=_Color.rgb+ondas*.018;o.Metallic=.22;o.Smoothness=.72;o.Alpha=1;}
 ENDCG
 }Fallback "Standard"
}

