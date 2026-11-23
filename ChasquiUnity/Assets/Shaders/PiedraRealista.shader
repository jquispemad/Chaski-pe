Shader "Chasqui/Piedra realista"
{
 Properties {
 _ColorBase("Piedra clara",Color)=(.48,.43,.38,1)
 _ColorOscuro("Piedra oscura",Color)=(.22,.18,.15,1)
 _ColorMusgo("Liquen",Color)=(.3,.38,.22,1)
 _Escala("Escala",Range(1,20))=7
 _Detalle("Grano",Range(0,1))=.65
 _Rugosidad("Rugosidad",Range(0,1))=.85
 _Metalico("Metalico",Range(0,1))=0
 _Musgo("Liquen",Range(0,1))=.4
 _Relieve("Relieve",Range(0,2))=.9
 }
 SubShader {
 Tags {"RenderType"="Opaque"}
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows
 #pragma target 3.0
 fixed4 _ColorBase,_ColorOscuro,_ColorMusgo;
 float _Escala,_Detalle,_Rugosidad,_Metalico,_Musgo,_Relieve;
 struct Input {float3 worldPos;float3 worldNormal;INTERNAL_DATA};
 float hash(float3 p){p=frac(p*.1031);p+=dot(p,p.yzx+33.33);return frac((p.x+p.y)*p.z);}
 float noise3(float3 p){float3 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(lerp(hash(i),hash(i+float3(1,0,0)),f.x),lerp(hash(i+float3(0,1,0)),hash(i+float3(1,1,0)),f.x),f.y),lerp(lerp(hash(i+float3(0,0,1)),hash(i+float3(1,0,1)),f.x),lerp(hash(i+float3(0,1,1)),hash(i+float3(1,1,1)),f.x),f.y),f.z);}
 float fbm2(float3 p){return noise3(p)*.67+noise3(p*2.13)*.33;}
 void surf(Input IN,inout SurfaceOutputStandard o){
 float3 p=IN.worldPos*_Escala;
 float macro=fbm2(p*.38),medio=fbm2(p*2.1),grano=noise3(p*23);
 float veta=abs(sin((p.y*.7+p.x*.22+(macro*1.6+medio*.8))*5));
 float fisura=1-smoothstep(.025,.10,veta);
 float mezcla=saturate((macro-.23)*1.9+(medio-.45)*.35);
 float3 color=lerp(_ColorOscuro.rgb,_ColorBase.rgb,mezcla);
 color*=lerp(.80,1.15,grano);color*=1-fisura*.45;
 float3 N=normalize(WorldNormalVector(IN,float3(0,0,1)));
 if(_Musgo>.001 && N.y>.05){
  float liquen=smoothstep(.48,.68,noise3(p*1.7))*saturate(N.y)*_Musgo;
  color=lerp(color,_ColorMusgo.rgb,liquen);
 }
 float altura=(medio*.65+grano*.075-fisura*.09)*.025*_Relieve;
 float3 dx=ddx(IN.worldPos),dy=ddy(IN.worldPos),r1=cross(dy,N),r2=cross(N,dx);
 float det=dot(dx,r1);float3 grad=sign(det)*(ddx(altura)*r1+ddy(altura)*r2);
 float3 normal=normalize(abs(det)*N-grad);
 float3 T=WorldNormalVector(IN,float3(1,0,0)),B=WorldNormalVector(IN,float3(0,1,0));
 o.Normal=normalize(float3(dot(normal,T),dot(normal,B),dot(normal,N)));
 o.Albedo=color;o.Metallic=_Metalico;o.Smoothness=saturate(1-_Rugosidad+(grano-.5)*.12);o.Occlusion=1-fisura*.25;o.Alpha=1;
 }
 ENDCG
 }
 Fallback "Standard"
}
