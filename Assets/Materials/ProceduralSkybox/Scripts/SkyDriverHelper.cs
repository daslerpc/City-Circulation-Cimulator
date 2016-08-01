using UnityEngine;
using System.Collections;

static public class SkyDriverHelper
{
	static public readonly Vector3[] defaultDatas=new Vector3[]{
		Vector3.zero,
		Vector3.zero,
		Vector3.zero,
		Vector3.zero,
		Vector3.zero,
		Vector3.one * 0.3f,
		Vector3.one,
		Vector3.right,
		Vector3.up,
		Vector3.forward,
	};
	
	static public void CalculateTurbidity (ref Vector3[] datas, float T)
	{
		datas [0].x = (-0.0193f * T - 0.2592f); //X
		datas [1].x = (-0.0665f * T + 0.0008f);
		datas [2].x = (-0.0004f * T + 0.2125f);
		datas [3].x = (-0.0641f * T - 0.8989f);
		datas [4].x = (-0.0033f * T + 0.0452f);
		
		datas [0].y = (-0.0167f * T - 0.2608f); //Y
		datas [1].y = (-0.0950f * T + 0.0092f);
		datas [2].y = (-0.0079f * T + 0.2102f);
		datas [3].y = (-0.0441f * T - 1.6537f);
		datas [4].y = (-0.0109f * T + 0.0529f);
		
		datas [0].z = (0.1787f * T - 1.4630f); //Luminance
		datas [1].z = (-0.3554f * T + 0.4275f);
		datas [2].z = (-0.0227f * T + 5.3251f);
		datas [3].z = (0.1206f * T - 2.5771f);
		datas [4].z = (-0.0670f * T + 0.3703f);
	}
	
	static public void CalculateZenith(ref Vector3[] datas, float T, float theta, float ZClamp)
	{
		//THE GLITCH: We don't know why the Y formula get glitching when theta < 0.14, put ZClamp > 0.14 to avoid it.
		Vector3 zenith;
		float thetaZ = Mathf.Max(theta,ZClamp);
		float theta2 = theta * theta;
		float theta3 = theta2 * theta;
		float T2 = T * T;
		
		float chi = (4.0f / 9.0f - T / 120.0f) * (Mathf.PI - 2.0f * thetaZ);
		zenith.z = Mathf.Max ((4.0453f * T - 4.9710f) * Mathf.Tan (chi) - 0.2155f * T + 2.4192f, 0f);
		zenith.z *= 0.06f;
		
		zenith.x =
			(0.00166f * theta3 - 0.00375f * theta2 + 0.00209f * theta) * T2 +
				(-0.02903f * theta3 + 0.06377f * theta2 - 0.03202f * theta + 0.00394f) * T +
				(0.11693f * theta3 - 0.21196f * theta2 + 0.06052f * theta + 0.25886f);
		
		zenith.y =
			(0.00275f * theta3 - 0.00610f * theta2 + 0.00317f * theta) * T2 +
				(-0.04214f * theta3 + 0.08970f * theta2 - 0.04153f * theta + 0.00516f) * T +
				(0.15346f * theta3 - 0.26756f * theta2 + 0.06670f * theta + 0.26688f);
	
		zenith.x /=PerezFunction(datas[0].x, datas[1].x, datas[2].x, datas[3].x, datas[4].x, 0, theta);
		zenith.y /=PerezFunction(datas[0].y, datas[1].y, datas[2].y, datas[3].y, datas[4].y, 0, theta);
		zenith.z /=PerezFunction(datas[0].z, datas[1].z, datas[2].z, datas[3].z, datas[4].z, 0, thetaZ);

		datas[5]=zenith;
	}

	//The original value of XYZ Conversion
	static public void CalculateChannels(ref Vector3[] datas)
	{
		//picked from sRGB color space, more information see
		//http://www.brucelindbloom.com/index.html?Eqn_XYZ_to_RGB.html
		
		datas [7] = new Vector3 (3.2404f, -1.5372f, -0.4985f);
		datas [8] = new Vector3 (-0.9692f, 1.8760f, 0.0415f);
		datas [9] = new Vector3 (0.0556f, -0.2040f, 1.0573f);
	}
	//The original Zenith Coefficient (nothing, just to make it look good at main script)
	static public void CalculateZenithCoefficient(ref Vector3[] datas)
	{
		datas [6] = Vector3.one;
	}
	static public float PerezFunction (float A, float B, float C, float D, float E, float theta, float gamma)
	{
		float cgamma=Mathf.Cos(gamma);
		return (1.0f + A * Mathf.Exp (B / Mathf.Cos (theta)))
			* (1.0f + C * Mathf.Exp (D * gamma) + E * cgamma * cgamma);
	}

	static public Vector2 GetSunCoordinate (Vector3 dir)
	{
		return new Vector2 (Mathf.Acos (dir.z), Mathf.Atan2 (dir.x, dir.y));
	}

	static public Vector3 linearMult(Vector3 a, Vector3 b)
	{
		return new Vector3(a.x*b.x,a.y*b.y,a.z*b.z);
	}

}

