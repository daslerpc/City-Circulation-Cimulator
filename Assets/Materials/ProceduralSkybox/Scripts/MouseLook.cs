using UnityEngine;
using System.Collections;

public class MouseLook : MonoBehaviour
{
	public float distance=5;
	public float speedAngle=10;
	Vector2 curAxis;

	// Use this for initialization
	void Start () {
		curAxis = Vector2.right * 180;
		curAxis += new Vector2 ( Input.GetAxis("Mouse X")*2,-Input.GetAxis("Mouse Y"))  * speedAngle;
	
		transform.rotation = Quaternion.Euler(curAxis[1], curAxis[0], 0);

	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButton(0) && Input.mousePosition.y > Screen.height /5){
			curAxis += new Vector2 ( Input.GetAxis("Mouse X")*2,-Input.GetAxis("Mouse Y"))  * speedAngle;
			
				transform.rotation = Quaternion.Euler(curAxis[1], curAxis[0], 0);
		}
	}
}

