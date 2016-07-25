using UnityEngine;
using System.Collections;
[RequireComponent(typeof(SkyDriver))]
[RequireComponent(typeof(Light))]
public class LightDriver : MonoBehaviour
{
	public Gradient sunColor;
	public AnimationCurve intensity;
	public float startTime;
	public float timeSpeed;
	public float starsSpeed;
	[HideInInspector]
	public float time;
		// Use this for initialization
		void Start ()
		{
		time = startTime;
		}
		// Update is called once per frame
	public void setSpeed(float v)
	{
		timeSpeed = v;
	}
		void Update ()
		{
if(timeSpeed != 0)
		{
			time += timeSpeed * Time.deltaTime;
			if(time>1)
				time -= 1;
			transform.eulerAngles = Vector3.right * (time-0.25f) * 360 ;
			GetComponent<Light>().color = sunColor.Evaluate (time);
			GetComponent<Light>().intensity = intensity.Evaluate (time);
			GetComponent<SkyDriver>().starOffset = Vector2.up * time * starsSpeed;
		}
		}
}

