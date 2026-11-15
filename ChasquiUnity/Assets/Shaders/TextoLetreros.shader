Shader "Chasqui/Texto de letreros" {
 Properties {_MainTex("Font",2D)="white"{} _Color("Color",Color)=(1,1,1,1)}
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Lighting Off Cull Off ZWrite Off ZTest LEqual Blend SrcAlpha OneMinusSrcAlpha
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex;float4 _MainTex_TexelSize;fixed4 _Color;
 struct a{float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};struct v{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
 v vert(a i){v o;o.pos=UnityObjectToClipPos(i.vertex);o.uv=i.uv;o.color=i.color*_Color;return o;}
 fixed4 frag(v i):SV_Target{
 float a=tex2D(_MainTex,i.uv).a;
 float2 paso=_MainTex_TexelSize.xy*2.5;
 float arriba=tex2D(_MainTex,i.uv+paso).a;
 float abajo=tex2D(_MainTex,i.uv-paso).a;
 // Dark recessed centre, shaded upper cut and a warm lower bevel.
 float borde=saturate((abajo-a)*3);
 float3 tallado=lerp(float3(.075,.033,.012),float3(.72,.47,.22),borde);
 tallado*=1-saturate((arriba-abajo)*.6)*.5;
 return fixed4(tallado,i.color.a*max(a,abajo*.85));}
 ENDCG}
 } }
