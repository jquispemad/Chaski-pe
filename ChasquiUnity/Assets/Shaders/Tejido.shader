Shader "Chasqui/Tejido dos caras" {
 Properties {_Color("Tinte natural",Color)=(.5,.3,.1,1) _MainTex("Albedo",2D)="white"{} _BumpMap("Relieve del tejido",2D)="bump"{} _BumpScale("Relieve",Range(0,2))=.5 _RoughnessMap("Rugosidad",2D)="white"{} _Glossiness("Suavidad",Range(0,1))=.16 _Metallic("Metal",Range(0,1))=0 _Fibra("Intensidad del tejido",Range(0,1))=.8}
 SubShader {Tags{"RenderType"="Opaque"} Cull Off LOD 300
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows vertex:vert
 #pragma target 3.0
 sampler2D _MainTex,_BumpMap,_RoughnessMap;fixed4 _Color;half _Glossiness,_Metallic,_Fibra,_BumpScale;
 struct Input{float2 uv_MainTex;float3 localPos;float3 worldPos;float3 worldNormal;float facing:VFACE;INTERNAL_DATA};
 void vert(inout appdata_full v,out Input o){UNITY_INITIALIZE_OUTPUT(Input,o);o.localPos=v.vertex.xyz;}
 float hash(float3 p){p=frac(p*.1031);p+=dot(p,p.yzx+33.33);return frac((p.x+p.y)*p.z);}
 float ruido(float3 p){float3 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(lerp(hash(a),hash(a+float3(1,0,0)),f.x),lerp(hash(a+float3(0,1,0)),hash(a+float3(1,1,0)),f.x),f.y),lerp(lerp(hash(a+float3(0,0,1)),hash(a+float3(1,0,1)),f.x),lerp(hash(a+float3(0,1,1)),hash(a+float3(1,1,1)),f.x),f.y),f.z);}
 void surf(Input IN,inout SurfaceOutputStandard o){
 float3 p=IN.localPos;float macro=ruido(p*18),grano=ruido(p*230);
 float3 hilo=p*1600;float fade=1/(1+dot(fwidth(hilo),float3(1,1,1))*.3);
 float trama=(sin(hilo.x)*sin(hilo.y)+sin(hilo.y)*sin(hilo.z)+sin(hilo.z)*sin(hilo.x))*.333;
 float tinte=1+_Fibra*((macro-.5)*.14+(grano-.5)*.10+trama*.06*fade);
 o.Albedo=tex2D(_MainTex,IN.uv_MainTex).rgb*_Color.rgb*tinte;
 float h=_Fibra*((grano-.5)*.0003+trama*.00015*fade);
 float lado=IN.facing>=0?1:-1;float3 N=normalize(WorldNormalVector(IN,float3(0,0,lado)));
 float3 dx=ddx(IN.worldPos),dy=ddy(IN.worldPos),r1=cross(dy,N),r2=cross(N,dx);float det=dot(dx,r1);float3 normal=normalize(abs(det)*N-sign(det)*(ddx(h)*r1+ddy(h)*r2));
 float3 T=WorldNormalVector(IN,float3(1,0,0)),B=WorldNormalVector(IN,float3(0,1,0)),N0=WorldNormalVector(IN,float3(0,0,1));
 float3 texturaNormal=UnpackNormal(tex2D(_BumpMap,IN.uv_MainTex));
 o.Normal=normalize(float3(dot(normal,T)+texturaNormal.x*_BumpScale,dot(normal,B)+texturaNormal.y*_BumpScale,dot(normal,N0)));o.Metallic=_Metallic;o.Smoothness=saturate(_Glossiness*.6+(1-tex2D(_RoughnessMap,IN.uv_MainTex).r)*.25+(grano-.5)*.04);o.Occlusion=.97;o.Alpha=1;
 }
 ENDCG
 }Fallback "Diffuse"
}
