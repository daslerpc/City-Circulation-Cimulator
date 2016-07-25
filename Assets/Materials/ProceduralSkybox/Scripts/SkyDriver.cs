//The Preetham Skybox Shader need to be drived by this script, since the Computation is really high.
using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SkyDriver : MonoBehaviour
{
	[SerializeField]
	Material
		m_skybox;
	[SerializeField]
	[Range(0,7)]
	int
		m_customizationMode;
	[SerializeField]
	[Range(1.7f,10)]
	float
		m_turbidity = 2.5f;
	[SerializeField]
	[Range(0,5)]
	float
		m_sunSize = 1f;
	[SerializeField]
	[Range(0,5)]
	float
		m_exposure = 1f;
	[SerializeField]
	Color
		m_tint = Color.white;
	[SerializeField]
	[Range(0,2)]
	float
		m_nightStrength = 0.4f;
	[SerializeField]
	Color
		m_nightColor = Color.blue * 0.05f;
	[SerializeField]
	Vector2
		m_starOffset = Vector2.zero;
	[SerializeField]
	[Range(0,1.570f)]
	float
		m_starLerp = 0.2f;
	[SerializeField]
	[Range(0,1f)]
	float
		m_sunLerp = 0.2f;
	[SerializeField]
	bool m_overrideZenith=false;
	[SerializeField]
	[Range(0.14f,1.57f)]
	float m_zenithClamp=0.14f;

	Quaternion lastRot;
	Vector3 direction;
	float phi; //Sun Direction on X
	float theta; //Sun Direction on Y
	public Vector3[]
		datas;

	#region Properties
	public Material skybox { 
		get { 
			return m_skybox; 
		} 
		set { 
			if (SetStruct (ref m_skybox, value))
				Start (); 
		} 
	}

	public int customizationMode { 
		get { 
			return m_customizationMode; 
		} 
		set { 
			if (SetStruct (ref m_customizationMode, value))
				Compute ();
			SaveToMaterial(false);
		} 
	}

	public float turbidity {
		get { 
			return m_turbidity;
		}
		set { 
			if (SetStruct (ref m_turbidity, value)) {
				Compute ();
				SaveToMaterial(false);
			} 
		}
	}

	public float sunSize {
		get { 
			return m_sunSize;
		}
		set { 
			if (SetStruct (ref m_sunSize, value)) {
				Compute ();
				SaveToMaterial(false);
			} 
		}
	}

	public float exposure {
		get { 
			return m_exposure; 
		} 
		set { 
			if (SetStruct (ref m_exposure, value))
				m_skybox.SetVector ("tint", m_tint * m_exposure); 
		} 
	}

	public Color tint { 
		get { 
			return m_tint; 
		} 
		set { 
			if (SetStruct (ref m_tint, value))
				m_skybox.SetVector ("tint", m_tint * m_exposure); 
		} 
	}
	
	public float nightStrength { 
		get { 
			return m_nightStrength; 
		} 
		set { 
			if (SetStruct (ref m_nightStrength, value))
				m_skybox.SetVector ("mcol", new Vector4 (m_nightColor [0], m_nightColor [1], m_nightColor [2],
				                                      m_nightStrength)); 
		}
	}
	
	public Color nightColor{ get { return m_nightColor; } set { 
			if (SetStruct (ref m_nightColor, value))
				m_skybox.SetVector ("mcol", new Vector4 (m_nightColor [0], m_nightColor [1], m_nightColor [2],
				                                         m_nightStrength)); 
		} 
	}

	public Vector2 starOffset {
		get { 
			return m_starOffset; 
		} 
		set { 
			if (SetStruct (ref m_starOffset, value))
				m_skybox.SetTextureOffset ("star", value); 
		} 
	}

	public float starLerp {
		get { 
			return m_starLerp; 
		} 
		set { 
			if (SetStruct (ref m_starLerp, value))
				m_skybox.SetFloat("starLerp", value); 
		} 
	}
	public float sunLerp {
		get { 
			return m_sunLerp; 
		} 
		set { 
			if (SetStruct (ref m_sunLerp, value))
				Compute(); 
			SaveToMaterial(true);

		} 
	}

	public bool overrideZenith {
		get { 
			return m_overrideZenith;
		}
		set { 
			if (SetStruct (ref m_overrideZenith, value)) {
				Compute ();
				SaveToMaterial(true);
			} 
		}
	}

	public float zenithClamp {
		get { 
			return m_zenithClamp;
		}
		set { 
			if (SetStruct (ref m_zenithClamp, value)) {
				Compute ();
				SaveToMaterial(true);
			} 
		}
	}

	#endregion 


	void Start ()
	{
		//	datas=SkyDriverHelper.defaultDatas;
		UpdateCoordinate ();
		Compute ();
		SaveToMaterial (false);
	}

	void Update ()
	{
		if ((m_skybox != null) && transform.rotation != lastRot) {
			UpdateCoordinate ();
			Compute ();
			SaveToMaterial (true);
		}
	}


	#region "Calculations"
	void Compute ()
	{
		Vector2 spherical = SkyDriverHelper.GetSunCoordinate (direction);
		theta = spherical.x;
		phi = spherical.y;

		if (datas == null)
			datas = SkyDriverHelper.defaultDatas;

		int m = (int)m_customizationMode;

		if (m < 4)
			SkyDriverHelper.CalculateTurbidity (ref datas, turbidity);
		else
			m -= 4;

		if (m < 2 )
		{
			SkyDriverHelper.CalculateZenithCoefficient (ref datas);
			overrideZenith=false;
		}
		else
			m -= 2;
		if(!overrideZenith)
			SkyDriverHelper.CalculateZenith (ref datas, turbidity, theta,zenithClamp);

		
		if (m < 1)
			SkyDriverHelper.CalculateChannels (ref datas);
	}
	const float M_PI_2 = 1.57079f;
	void SaveToMaterial (bool quick)
	{
		if(overrideZenith)
		{
			m_skybox.SetVector ("zenith", datas[6]);
		}
		else
		{
			m_skybox.SetVector ("zenith",SkyDriverHelper.linearMult(datas[5], datas[6]));
		}
		m_skybox.SetFloat ("phi", phi);
		m_skybox.SetFloat ("theta", theta);
		m_skybox.SetFloat ("sunS", theta > M_PI_2? m_sunSize * Mathf.Max(1-((theta/M_PI_2-1)/(m_sunLerp+1e-4f)) ,0f) : m_sunSize);
		if (quick)
			return;
		m_skybox.SetVector ("propA", datas [0]);
		m_skybox.SetVector ("propB", datas [1]);
		m_skybox.SetVector ("propC", datas [2]);
		m_skybox.SetVector ("propD", datas [3]);
		m_skybox.SetVector ("propE", datas [4]);
		m_skybox.SetVector ("difR", datas [7]);
		m_skybox.SetVector ("difG", datas [8]);
		m_skybox.SetVector ("difB", datas [9]);
		m_skybox.SetVector ("tint", m_tint * m_exposure);
		m_skybox.SetFloat ("starLerp", m_starLerp);
		m_skybox.SetTextureOffset ("star", m_starOffset);
		m_skybox.SetVector ("mcol", new Vector4 (m_nightColor [0], m_nightColor [1], m_nightColor [2],
		                                         m_nightStrength));
	}

	void UpdateCoordinate ()
	{
		lastRot = transform.rotation;
		direction = (lastRot * Quaternion.Euler (-90, 0, 0)) * Vector3.up;
		direction = new Vector3 (direction [0], direction [2], direction [1]);
	}

	public static bool SetStruct <T> (ref T currentValue, T newValue)
	{
		if (currentValue.Equals (newValue))
			return false; 
		
		currentValue = newValue;
		return true;
	}
	#endregion
#if UNITY_EDITOR

	void Reset ()
	{
		m_skybox = RenderSettings.skybox;
		
		datas = SkyDriverHelper.defaultDatas;
		for (int i = 0; i < 9; i++) {
			datas [i] = Vector3.zero;
		}
		OnValidate ();
	}
	
	public	void OnValidate ()
	{
		if (m_skybox) {
			Compute ();
			SaveToMaterial (false);
		}
	}

	const string bufferFormat="0.0000";
	[ContextMenu("Export to Clipboard")]
	public void ExportData()
	{
		string r;
		r=customizationMode.ToString("0") + del + turbidity.ToString(bufferFormat) + del +
			sunSize.ToString(bufferFormat) + delL;
		r+=exposure.ToString(bufferFormat) + del + tint.r.ToString(bufferFormat) + del +
			tint.g.ToString(bufferFormat) + del + tint.b.ToString(bufferFormat) + delL;
		r+=nightStrength.ToString(bufferFormat) + del + nightColor.r.ToString(bufferFormat) + del +
			nightColor.g.ToString(bufferFormat) + del + nightColor.b.ToString(bufferFormat) + delL;
		r+=m_starOffset.x.ToString(bufferFormat) + del + m_starOffset.y.ToString(bufferFormat) + del +
			m_starLerp.ToString(bufferFormat) + del + m_sunLerp.ToString(bufferFormat) + delL;
		r+= (overrideZenith?"1":"0") + del + zenithClamp + delL;
		for (int i = 0; i < datas.Length; i++) {
			r+=datas[i].x.ToString(bufferFormat) + del +
				datas[i].y.ToString(bufferFormat) + del +
					datas[i].z.ToString(bufferFormat) + delL;
		}
		UnityEditor.EditorGUIUtility.systemCopyBuffer=r.Substring(0,r.Length-2);
		// Debug.Log("Successfully exported to Clipboard");
	}

	const string bufferShort="0.0";
	const string del=",";
	const string delL=",\n";
	[ContextMenu("Export (Clean Format)")]
	public void ExportDataClean()
	{
		string r;
		r=customizationMode.ToString("0") + del + turbidity.ToString(bufferShort) + del +
			sunSize.ToString(bufferShort) + delL;
		r+=exposure.ToString(bufferShort) + del + tint.r.ToString(bufferShort) + del +
			tint.g.ToString(bufferShort) + del + tint.b.ToString(bufferShort) + delL;
		r+=nightStrength.ToString(bufferShort) + del + nightColor.r.ToString(bufferShort) + del +
			nightColor.g.ToString(bufferShort) + del + nightColor.b.ToString(bufferShort) + delL;
		r+=m_starOffset.x.ToString(bufferShort) + del + m_starOffset.y.ToString(bufferShort) + del +
			m_starLerp.ToString(bufferShort) + del + m_sunLerp.ToString(bufferShort) + delL;
		r+= (overrideZenith?"1":"0") + del + zenithClamp + delL;
		for (int i = 0; i < datas.Length; i++) {
			r+=datas[i].x.ToString(bufferShort) + del +
				datas[i].y.ToString(bufferShort) + del +
					datas[i].z.ToString(bufferShort) + delL;
		}
		UnityEditor.EditorGUIUtility.systemCopyBuffer=r.Substring(0,r.Length-2);
		// Debug.Log("Successfully exported (Clean Format) to Clipboard");
	}
	
	[ContextMenu("Import from Clipboard")]
	public void ImportData()
	{
		string[] parsed= (UnityEditor.EditorGUIUtility.systemCopyBuffer.Replace("\n",System.String.Empty)).Split(',');
		if(parsed.Length != 47)
		{
			if(parsed.Length<5)
				Debug.LogError("Failed to Import: Invalid System Buffer");
			else
				Debug.LogError("Failed to Import: The resulting float count are " + parsed.Length+", the expected count is 47");
			return;
		}
		float n;
		UnityEditor.Undo.RecordObject(this,"Sky Import");
		for (int i = 0; i < 47; i++) {
			try
			{
				n=float.Parse(parsed[i]);
			}
			catch(System.Exception)
			{
				Debug.LogError("Failed to Import: Error at Parsing \"" + parsed[i] + "\" from the Clipboard");
				return;
			}
			if(i<17)
			{
				switch(i)
				{
				case 0:m_customizationMode=(int)n;break;
				case 1:m_turbidity=n;break;
				case 2:m_sunSize=n;break;
				case 3:m_exposure=n;break;
				case 4:m_tint.r=n;break;
				case 5:m_tint.g=n;break;
				case 6:m_tint.b=n;break;
				case 7:m_nightStrength=n;break;
				case 8:m_nightColor.r=n;break;
				case 9:m_nightColor.g=n;break;
				case 10:m_nightColor.b=n;break;
				case 11:m_starOffset.x=n;break;
				case 12:m_starOffset.y=n;break;
				case 13:m_starLerp=n;break;
				case 14:m_sunLerp=n;break;
				case 15:m_overrideZenith=n>0;break;
				case 16:m_zenithClamp=n;break;
				}
			}
			else
			{
				datas[(i-17)/3][(i-17)%3]=n;
			}
		}
		// Debug.Log("Successfully importing data from Clipboard");
		OnValidate();
	}
	[ContextMenu("Import (Exclude Basic)")]
	public void ImportExcludeBasic()
	{
		string[] parsed= (UnityEditor.EditorGUIUtility.systemCopyBuffer.Replace("\n",System.String.Empty)).Split(',');
		if(parsed.Length != 47)
		{
			if(parsed.Length<5)
				Debug.LogError("Failed to Import: Invalid System Buffer");
			else
				Debug.LogError("Failed to Import: The resulting float count are " + parsed.Length+", the expected count is 47");
			return;
		}
		float n;
		UnityEditor.Undo.RecordObject(this,"Sky Import");
		for (int i = 15; i < 47; i++) {
			try
			{
				n=float.Parse(parsed[i]);
			}
			catch(System.Exception)
			{
				Debug.LogError("Failed to Import: Error at Parsing \"" + parsed[i] + "\" from the Clipboard");
				return;
			}
			if(i<17)
			{
				switch(i)
				{
				case 15:m_overrideZenith=n>0;break;
				case 16:m_zenithClamp=n;break;
				}
			}
			else
				datas[(i-17)/3][(i-17)%3]=n;
		}
		// Debug.Log("Successfully importing (Excluding Basic) data from Clipboard");
		OnValidate();
	}
#endif
}
