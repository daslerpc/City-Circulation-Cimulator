Shader "Skybox/Debug" {
	Properties {
		[Range(0,1)]m("m",Float)=0
	}

	SubShader {
		Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
		Cull Off ZWrite Off

		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			struct appdata_t {
				half4 vertex : POSITION;
				half4 coord : TEXCOORD0;
			};

			struct v2f {
				half4 pos : SV_POSITION;
				half3 coord : TEXCOORD0;
			};
			
			fixed m;
			v2f vert (appdata_t v)
			{
				v2f OUT;

				half3 ve=v.vertex;		
				OUT.coord = v.vertex;
				OUT.pos = mul(UNITY_MATRIX_MVP, ve);			
				return OUT;
			}
			#define M_PI 3.1415926
		
			half4 frag (v2f IN) : SV_Target
			{
			fixed3 n=pow((normalize(IN.coord)),3);
			
				return half4(((n.r))*m%1,((n.g))*m%1,((n.b))*m%1,1);
			} 
			ENDCG
		}
	}
}

