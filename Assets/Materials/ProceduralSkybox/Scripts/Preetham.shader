//Calculation Reference : Preetham, Shirley, Smits, 
//A Practical Analytic Model for Daylight, ICCGIT1999
//http://www.cs.utah.edu/~shirley/papers/sunsky/sunsky.pdf

//Need External Calculation, provided by SkyDriver, or Customize it using your own inputs.

Shader "Skybox/Preetham" {
	Properties {
		//We put some values to make it easily customizable
		[Header(Sun Position)]
		phi ("Phi (X)", Float) = 0
		theta ("Theta (Y)", Float) = 0.2
		sunS ("Sun Size", Float) = 1
		[Header(Coeffient Input Color (CIE xyY))]
		propA ("Prop A", Vector) = (0, 0, 1, 0)
		propB ("Prop B", Vector) = (0, 0, -0.1, 0)
		propC ("Prop C", Vector) = (0, 0, 4, 0)
		propD ("Prop D", Vector) = (0, 0, -3, 0)
		propE ("Prop E", Vector) = (0, 0, -1, 0)
		zenith ("Zenith (Master)", Vector) = (0.3, 0.3, 0.3, 0)
		[Header(XYZ to RGB Conversion tuning (XYZ))]
		difR ("R Channel", Vector) = (1, 0, 0, 0)
		difG ("G Channel", Vector) = (0, 1, 0, 0)
		difB ("B Channel", Vector) = (0, 0, 1, 0)
		[Header(Final Composite Color (RGBA))]
		tint ("Exposure", Vector) = (1, 1, 1, 0)
		mcol ("Night Color", Vector) = (0, 0, 0.1, 0)
		starLerp ("Star Lerp", Range(0,1.570)) = 0.2
		star ("Stars", 2D) = "black" {}
	}

	SubShader {
		Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
		Cull Off ZWrite Off

		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			#define M_PI 3.1415926
			#define M_PI_2 1.5707963
			#define M_2PI 6.2831853

			//The shader still can run without this, but error still appear at build when we comment it.
			#pragma target 3.0
			
			#pragma multi_compile __ UNITY_COLORSPACE_GAMMA
			#if defined(UNITY_COLORSPACE_GAMMA)
				#define GAMMA 2
				#define COLOR_2_GAMMA(color) color
				#define COLOR_2_LINEAR(color) color*color
				#define LINEAR_2_OUTPUT(color) sqrt(color)
			#else
				#define GAMMA 2.4
				#define COLOR_2_GAMMA(color) ((unity_ColorSpaceDouble.r>2.0) ? pow(color,1.0/GAMMA) : color)
				#define COLOR_2_LINEAR(color) color
				#define LINEAR_2_LINEAR(color) color
			#endif

			
			//Sun Pos		
			uniform half phi;		// Sun X Coordinate
			uniform half theta;		// Sun Y Coordinate
			
			uniform half3 propA; 	// The A Property
			uniform half3 propB; 	// The B Property
			uniform half3 propC; 	// The C Property
			uniform half3 propD; 	// The D Property
			uniform half3 propE; 	// The E Property
			
			uniform half3 difR; 	// R-Channel Coefficient
			uniform half3 difG; 	// G-Channel Coefficient
			uniform half3 difB; 	// B-Channel Coefficient
			
			uniform half3 zenith; 	// Sky Spectral Radiance (Master Channel for Props)
			uniform half3 tint; 	// Tint * Exposure
			uniform half sunS;		// Sun Size
			uniform half starLerp;	// Star Lerp
			uniform half4 mcol; 	// Night (Minimum) Color of the Sky
			
			uniform sampler2D star;
			float4 star_ST;
			
			struct appdata_t {
				half4 vertex : POSITION;
				half4 coord : TEXCOORD0;
			};

			struct v2f {
				half4 pos : SV_POSITION;
				half3 coord : TEXCOORD0;
				half2 starcoord : TEXCOORD1;
			}; 


			half getSunAngle(half thetav, half phiv, half theta, half phi)
			{
				half cospsi = sin(thetav) * sin(theta) * cos(phi - phiv) + cos(thetav) * cos(theta);
				if (cospsi > 1)
				return 0;
				if (cospsi < -1)
				return M_PI;
				return acos(cospsi);
			}
			
			half3 lerp3(half3 x, half3 y, half s)
			{
				return (y - x)* s + x;
			} 

			half2 getCoordinate(half3 dir)
			{
				return half2(acos(dir.y), atan2(dir.x, dir.z));
			}
			
			half3 xyY_to_xyz(half x, half y, half Y)
			{
				half X, Z;
				if (y != 0) 
				X = (x / y) * Y;
				else
				X = 0;

				if (y != 0 && Y != 0) 
				Z = ((1 - x - y) / y) * Y;
				else 
				Z = 0;

				return half3(X, Y, Z);
			}
			//Fine-tuning of colors
			half3 xyz_to_rgb(half x, half y, half z)
			{
				return half3(
				difR[0] * x + difR[1] * y + difR[2] * z,
				difG[0] * x + difG[1] * y + difG[2] * z,
				difB[0] * x + difB[1] * y + difB[2] * z);
			}

			half PerezFunction(half A,half B,half C,half D,half E, half theta, half gamma)
			{
				half cgamma = cos(gamma);
				// (1 + A*e^(B/cos(t))) * ((1 + C + e^(D*g)) + E*cos(g)^2)
				return (1 + A * exp(B / cos(theta))) *
				lerp(1,(1 + C * exp(D * gamma) + E * cgamma * cgamma),sunS); //Lerping for adding Sun Spot Coefficient
			}

			half3 computeSky(half3 dir, half sunphi, half suntheta)
			{
				// convert vector to spherical coordinates 
				half2 radSpot = getCoordinate(dir);
				half theta = radSpot[0];
				half phi = radSpot[1];

				// angle between sun direction and dir 
				half gamma = getSunAngle(theta, phi, suntheta, sunphi);

				// clamp theta to horizon 
				theta = min(theta, M_PI_2 - 0.001f);

				// compute xyY color space values 
				half x = zenith[0] * PerezFunction(propA[0], propB[0], propC[0], propD[0], propE[0], theta, gamma);
				half y = zenith[1] * PerezFunction(propA[1], propB[1], propC[1], propD[1], propE[1], theta, gamma);
				half Y = zenith[2] * PerezFunction(propA[2], propB[2], propC[2], propD[2], propE[2], theta, gamma);

				// convert to RGB 
				half3 xyz = xyY_to_xyz(x, y, Y);
				return xyz_to_rgb(xyz[0], xyz[1], xyz[2]);
				//return half3(rgb.r*rgb.r,rgb.g*rgb.g,rgb.b*rgb.b);
			}
			v2f vert (appdata_t v)
			{
				v2f OUT;

				half3 ve=v.vertex;		
				
				OUT.coord = v.vertex;	
				OUT.starcoord = TRANSFORM_TEX(half3(v.coord[0],v.coord[2],v.coord[1]), star);
				OUT.pos = mul(UNITY_MATRIX_MVP, v.vertex);			
				return OUT;
			}
			
			half4 frag (v2f IN) : SV_Target
			{
				fixed3 col=fixed3(0,0,0);
				//Make sure we get Normalized Coordinate
				half3 coord=(normalize(IN.coord));
				
				col = ( computeSky(coord, phi, theta));
				
				//Add Star Background
				if(theta > (M_PI_2 - starLerp)){
					half3 colS=half3(0,0,0);
					half2 starcoord=(IN.starcoord);
			
					colS =  lerp3(half3(0,0,0),tex2D(star,starcoord).xyz,IN.coord.y*2*mcol.w);
					
					if(theta < (M_PI_2))
						colS = lerp3(colS,half3(0,0,0),(M_PI_2-theta)/starLerp);
					col = max(col,(colS));
				}
				//Not sure why we do this, but keep in mind 
				//that we had to compute colors in linear space.
				col = col*col;
				//the end of confusion :)
				col = max( col * COLOR_2_LINEAR(tint),lerp3(COLOR_2_LINEAR(mcol).xyz,half3(0,0,0),coord.y*(mcol.w)));
				//Convert back to Gamma space if needed
				#if defined(UNITY_COLORSPACE_GAMMA)
				col = LINEAR_2_OUTPUT(col);
				#endif

				return half4( col,1);
			} 
			ENDCG
		}
	}
	Fallback "Hidden/Solid Skybox"
}