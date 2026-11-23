Shader "Chasqui/Piel natural" {
 Properties {_Color("Piel",Color)=(.43,.225,.125,1) _Glossiness("Suavidad",Range(0,1))=.32 _Metallic("Metal",Range(0,1))=0}
 SubShader {Tags{"RenderType"="Opaque"} LOD 300
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows vertex:vert
 #pragma target 3.0
 fixed4 _Color;half _Glossiness;
 struct Input{float3 localPos;float3 worldPos;float3 worldNormal;INTERNAL_DATA};
 void vert(inout appdata_full v,out Input o){UNITY_INITIALIZE_OUTPUT(Input,o);o.localPos=v.vertex.xyz;}
 float hash(float3 p){p=frac(p*.1031);p+=dot(p,p.yzx+33.33);return frac((p.x+p.y)*p.z);}
 void surf(Input IN,inout SurfaceOutputStandard o){float3 p=IN.localPos;float poro=hash(floor(p*950));float variacion=sin(p.x*39+p.y*27)*sin(p.z*33)*.025;float fade=1/(1+length(fwidth(p*950)));o.Albedo=_Color.rgb*(1+variacion+(poro-.5)*.045*fade);o.Metallic=0;o.Smoothness=_Glossiness+(poro-.5)*.05*fade;float h=(poro-.5)*.000045*fade;float3 N=normalize(WorldNormalVector(IN,float3(0,0,1))),dx=ddx(IN.worldPos),dy=ddy(IN.worldPos),r1=cross(dy,N),r2=cross(N,dx);float det=dot(dx,r1);float3 n=normalize(abs(det)*N-sign(det)*(ddx(h)*r1+ddy(h)*r2));o.Normal=normalize(float3(dot(n,WorldNormalVector(IN,float3(1,0,0))),dot(n,WorldNormalVector(IN,float3(0,1,0))),dot(n,N)));o.Alpha=1;}
 ENDCG
 }Fallback "Standard"
}
