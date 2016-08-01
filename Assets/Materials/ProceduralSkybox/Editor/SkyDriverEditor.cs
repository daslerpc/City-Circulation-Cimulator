using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

[CustomEditor(typeof(SkyDriver))]
public class SkyDriverEditor : Editor
{

	SerializedProperty skybox;
	SerializedProperty customizationMode;
	SerializedProperty turbidity;
	SerializedProperty sunSize;
	SerializedProperty exposure;
	SerializedProperty tint;
	SerializedProperty nightStrength;
	SerializedProperty nightColor;
	SerializedProperty starOffset;
	SerializedProperty starLerp;
	SerializedProperty sunLerp;
	SerializedProperty overrideZenith;
	SerializedProperty zenithClamp;
	SerializedProperty datas;

	protected void OnEnable ()
	{
		skybox = serializedObject.FindProperty ("m_skybox");
		customizationMode = serializedObject.FindProperty ("m_customizationMode");
		turbidity = serializedObject.FindProperty ("m_turbidity");
		sunSize = serializedObject.FindProperty ("m_sunSize");
		exposure = serializedObject.FindProperty ("m_exposure");
		tint = serializedObject.FindProperty ("m_tint");
		nightStrength = serializedObject.FindProperty ("m_nightStrength");
		nightColor = serializedObject.FindProperty ("m_nightColor");
		starOffset = serializedObject.FindProperty ("m_starOffset");
		starLerp = serializedObject.FindProperty ("m_starLerp");
		sunLerp = serializedObject.FindProperty ("m_sunLerp");
		datas = serializedObject.FindProperty ("datas");
		overrideZenith = serializedObject.FindProperty ("m_overrideZenith");
		zenithClamp = serializedObject.FindProperty ("m_zenithClamp");
	}

	public override void OnInspectorGUI ()
	{
		serializedObject.Update ();
		EditorGUILayout.PropertyField (skybox);
		if (skybox.objectReferenceValue != null) {
			int c=DetermineCustomization();
			DoCoreCustomization(c<6 || !overrideZenith.boolValue);
			if(c >= 4)
			{
				DoCoefficientsCustomization();
				c-=4;
			}
			if(c >= 2)
			{
				DoZenithCustomization();
				c-=2;
			}
			if(c >= 1)
				DoCompositeCustomization();
		}
		serializedObject.ApplyModifiedProperties ();
	}

	int DetermineCustomization ()
	{
		int n = customizationMode.intValue;
		EditorGUI.BeginChangeCheck ();
		bool[] b = new bool[3];
		EditorGUILayout.LabelField ("Customize", EditorStyles.boldLabel);
		Rect r = EditorGUILayout.GetControlRect();
		r.width /= 3;
		b [2] = EditorGUI.ToggleLeft (r,"Coefficients", (n & 4)==4);
		r.x += r.width;
		b [1] = EditorGUI.ToggleLeft (r,"Zenith", (n & 2)==2);
		r.x += r.width;
		b [0] = EditorGUI.ToggleLeft (r,"Composition", (n & 1)==1);
		if (EditorGUI.EndChangeCheck ())
		{
			int x=(b [2] ? 4 : 0) + (b [1] ? 2 : 0) + (b [0] ? 1 : 0);
			customizationMode.intValue = x; 
			return x;
		}
		return n;
	}

	void DoCoreCustomization(bool showTurbidity)
	{
		EditorGUILayout.LabelField ("Basic", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		if(showTurbidity)
			EditorGUILayout.PropertyField(turbidity);
		else
		{
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.PropertyField(turbidity);
			EditorGUI.EndDisabledGroup();
		}
		EditorGUILayout.PropertyField(sunSize);
		EditorGUILayout.PropertyField(exposure);
		EditorGUILayout.PropertyField(tint);
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(nightStrength);
		EditorGUILayout.PropertyField(nightColor);
		EditorGUILayout.PropertyField(starOffset);
		EditorGUILayout.PropertyField(starLerp);
		EditorGUILayout.PropertyField(sunLerp);
		EditorGUI.indentLevel--;
	}

	void DoCompositeCustomization()
	{
		EditorGUILayout.LabelField ("Composite", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		DoTri(datas.GetArrayElementAtIndex(7),"Red Channel",XYZs);
		DoTri(datas.GetArrayElementAtIndex(8),"Green Channel",XYZs);
		DoTri(datas.GetArrayElementAtIndex(9),"Blue Channel",XYZs);
		EditorGUI.indentLevel--;
	}

	void DoZenithCustomization()
	{
		EditorGUILayout.LabelField ("Zenith", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(overrideZenith,new GUIContent("Override Value"));
		if(overrideZenith.boolValue)
		{
			DoTri(datas.GetArrayElementAtIndex(6),"Value",CIEs);
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.PropertyField(zenithClamp);
			EditorGUI.EndDisabledGroup();
		}
		else
		{
			DoTri(datas.GetArrayElementAtIndex(6),"Multiplier",CIEs);
			EditorGUILayout.PropertyField(zenithClamp);
		}
		EditorGUI.indentLevel--;
	}
	void DoCoefficientsCustomization()
	{
		EditorGUILayout.LabelField ("Coefficients", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		DoTri(datas.GetArrayElementAtIndex(0),"Prop A",CIEs);
		DoTri(datas.GetArrayElementAtIndex(1),"Prop B",CIEs);
		DoTri(datas.GetArrayElementAtIndex(2),"Prop C",CIEs);
		DoTri(datas.GetArrayElementAtIndex(3),"Prop D",CIEs);
		DoTri(datas.GetArrayElementAtIndex(4),"Prop E",CIEs);
		EditorGUI.indentLevel--;
	}
	static readonly GUIContent[] XYZs=new GUIContent[]{
		new GUIContent("X"),
		new GUIContent("Y"),
		new GUIContent("Z"),
	};
	static readonly GUIContent[] CIEs=new GUIContent[]{
		new GUIContent("x"),
		new GUIContent("y"),
		new GUIContent("Y"),
	};
	Vector3 Arr2Vec(float[] v)
	{
		return new Vector3(v[0],v[1],v[2]);
	}
	float[] Vec2Arr(Vector3 v)
	{
		return new float[]{v[0],v[1],v[2]};
	}
	void DoTri(SerializedProperty value, string name, GUIContent[] labels)
	{
		Rect r;
		float[] n=Vec2Arr(value.vector3Value);
		EditorGUI.BeginChangeCheck();
		r=EditorGUILayout.GetControlRect();
		if(Screen.width < 333)
		{
			EditorGUI.LabelField(r,name);
			r = EditorGUILayout.GetControlRect();
			EditorGUI.MultiFloatField(r,labels,n);
		}
		else
		{
			EditorGUI.MultiFloatField(r,new GUIContent(name),labels,n);
		}
		if(EditorGUI.EndChangeCheck())
			value.vector3Value=Arr2Vec(n);
	}
}

