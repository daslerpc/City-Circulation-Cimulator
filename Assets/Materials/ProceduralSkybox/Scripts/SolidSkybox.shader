// Deprecated Package, but still available for compatibility purposes
Shader "Hidden/Solid Skybox" {
	Properties {
		mcol ("Solid Color", Color) = (0, 0, 0.2, 0)
		tint ("Tint", Color)= (1,1,1,1)
	}

	SubShader {
		Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
		Cull Off ZWrite Off
		Pass{
		CGPROGRAM
			#pragma target 2.0
			#pragma vertex vert
			#pragma fragment frag
	
		#include "UnityCG.cginc"
		
		uniform half4 mcol; 	// Night Config
	uniform half4 tint; 	// Night Config
	
		struct appdata_t {
			half4 vertex : POSITION;
			half4 coord : TEXCOORD0;
		};

		struct v2f {
			half4 pos : SV_POSITION;
			half3 coord : TEXCOORD0;
		}; 

		v2f vert (appdata_t v)
		{
			v2f OUT;

			OUT.coord = half3(v.coord[0],v.coord[2], v.coord[1]);	
			
			OUT.pos =  mul(UNITY_MATRIX_MVP, v.vertex);			
			return OUT;
		}
			
			half4 frag (v2f IN) : SV_Target
			{
				return half4(mcol.xyz * tint.xyz,1.0);
			} 
			ENDCG
	}
	}
	Fallback Off
}

