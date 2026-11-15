Shader "Chasqui/Follaje valle" {
 Properties {_MainTex("Hojas",2D)="white"{} _Color("Color",Color)=(.6,.7,.4,1) _Cutoff("Recorte",Range(0,1))=.4}
 SubShader {Tags{"Queue"="AlphaTest" "RenderType"="TransparentCutout"} Cull Off
 CGPROGRAM
 #pragma surface surf Standard alphatest:_Cutoff addshadow vertex:vert
 #pragma target 3.0
 sampler2D _MainTex;fixed4 _Color;struct Input{float2 uv_MainTex;float facing:VFACE;};
 void vert(inout appdata_full v){float3 w=mul(unity_ObjectToWorld,v.vertex).xyz;v.vertex.x+=sin(_Time.y*1.8+w.x*.7+w.z*.6)*.025*saturate(v.vertex.y/3);}
 void surf(Input IN,inout SurfaceOutputStandard o){fixed4 c=tex2D(_MainTex,IN.uv_MainTex)*_Color;o.Albedo=c.rgb;o.Alpha=c.a;o.Normal=float3(0,0,IN.facing>=0?1:-1);o.Smoothness=.14;}
 ENDCG
 }Fallback "Transparent/Cutout/Diffuse"
}
