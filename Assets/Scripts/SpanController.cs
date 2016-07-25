using UnityEngine;
using System.Collections;

public class SpanController : MonoBehaviour {

	static readonly float maxSpeed = 50;	// maximum speed
	static readonly float spanHeightAboveGround = 2; // how high to place above ground level
	static readonly float spanLength = 10;	// length of a single span
//	static readonly float maxAccel = 10;	// maximum acceleration
//	static readonly float minGapSize = 10;	// min gap between spans

	RoadNetwork roadNetwork;			// the RoadNetwork object
	SpanController followingSpan;

	private bool moving = true;

//	RoadSegment frontSeg;				// road segment of front
//	RoadSegment rearSeg;				// road segment of rear
//	float relativePos;					// relative position along front segment
//	float speed;						// current speed
//	SpanController prevSpan;			// the span ahead of this (if any)

	// Use this for initialization
	void Start () {
										// access the global road network
		GameObject trafficControllerObject = GameObject.Find("TrafficController");
		if (trafficControllerObject == null) {
			Debug.Log ("Can't find TrafficController object");
		}
		TrafficController trafficController = trafficControllerObject.GetComponent<TrafficController> ();
		if (trafficController == null) {
			Debug.Log ("Can't find TrafficController");
		}
		roadNetwork = trafficController.getRoadNetwork ();
		if (roadNetwork == null) {
			Debug.Log ("Cannot access RoadNetwork component of TrafficController");
		}

//		speed = -1;						// speed will be set in Update()
//		prevSpan = null;				// also set in Update
	}

										// assign front segment (should be done on initialization)
//	public void Initialize(RoadSegment fs) {
//		frontSeg = fs;
//	}

	//------------------------------------------------------------------
	// Here is the high-level overview. 
	//
	// If there is a span ahead of us in the current road segment, then
	// compute the difference between its speed and ours. If we are
	// gaining on it, then we decelerate to match its speed. (If the gap
	// size falls to small, we match its speed exactly to maintain the
	// minimum gap.)
	//
	// Otherwise, we request the intersection ahead of us to grant us
	// access. If the permission is granted, we are given an estimate of
	// when the intersection will be free. Based on this, we adjust our
	// speed (according to acceleration limits) to arrive at this time.
	// If not, we determine our distance to the end of the road segment.
	// If we are sufficiently far away, we continue at full speed.
	// Otherwise, we begin to decelerate.
	//------------------------------------------------------------------

	void Update ()
	{
		if (roadNetwork != null && moving) {
			transform.Translate (maxSpeed * Time.deltaTime * Vector3.forward);
			Vector3 pos = transform.position;
			roadNetwork.Wrap (ref pos);
			transform.position = pos;

			if (followingSpan != null) {
				followingSpan.startSpan ();
				followingSpan = null;
			}
		}
	}

	void OnTriggerEnter(Collider other) {
		Vector3 targetDir = (other.GetComponentInParent<Transform>() as Transform).position - transform.position;
		float angle = Vector3.Angle (targetDir, transform.forward);

		// a span has hit our back
		if (angle == 180) {
			followingSpan = other.GetComponentInParent<SpanController> ();
			followingSpan.stopSpan ();
		}
	}

	public void stopSpan() {
		moving = false;
	}

	public void startSpan() {
		moving = true;
	}

	static public void CreateInitialSpans(RoadNetwork roadNetwork, GameObject spansFolder) {
		GameObject spanPrefab = Resources.Load ("Prefabs/Span") as GameObject;
		for (int i = 0; i < roadNetwork.NHorzRoads; i++) {
			for (int j = 0; j < roadNetwork.NVertRoads; j++) {
				GameObject span ;
				RoadSegment hseg = roadNetwork.Isects [i, j].Incident [(int)RelDirection.HNext];
				if (hseg.Length > 2 * spanLength) {
					Vector3 startPt = hseg.PointAtRelative (Random.Range(0.25f, 0.75f));
					startPt.y = spanHeightAboveGround;
					if (hseg.Direction > 0) {
						span = Instantiate (spanPrefab, startPt, Quaternion.Euler (0, 90, 0)) as GameObject;
					} else {
						span = Instantiate (spanPrefab, startPt, Quaternion.Euler (0, -90, 0)) as GameObject;
					}
					span.transform.parent = spansFolder.transform;
				}
				RoadSegment vseg = roadNetwork.Isects [i, j].Incident [(int)RelDirection.VNext];
				if (vseg.Length > 2 * spanLength) {
					Vector3 startPt = vseg.PointAtRelative (Random.Range(0.25f, 0.75f));
					startPt.y = spanHeightAboveGround;
					if (vseg.Direction > 0) {
						span = Instantiate (spanPrefab, startPt, Quaternion.Euler (0, 0, 0)) as GameObject;
					} else {
						span = Instantiate (spanPrefab, startPt, Quaternion.Euler (0, 180, 0)) as GameObject;
					}
					span.transform.parent = spansFolder.transform;
				}
			}
		}
	}
}
