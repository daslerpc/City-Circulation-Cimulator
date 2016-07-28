using UnityEngine;
using System.Collections;

// This is a totally innocuous change

public class RoadSegment {

	Intersection tail;
	Intersection head;
	int direction;
	float length; 
	Vector3 vect;

	public Intersection Tail { get { return tail; } }
	public Intersection Head { get { return head; } }
	public int Direction { get { return direction; } }
	public float Length { get { return length; } }

										// standard constructor
	public RoadSegment(Intersection t, Intersection h, bool isHorizontal, bool isWrapped, float width) {
		tail = t;
		head = h;
		vect = head.Position - tail.Position;
		if (isWrapped) {
			if (isHorizontal) {
				if (head.Position.x < tail.Position.x) {
					vect.x += width;
				} else {
					vect.x -= width;
				}
			} else {
				if (head.Position.z < tail.Position.z) {
					vect.z += width;
				} else {
					vect.z -= width;
				}
			}
		}
		if (isHorizontal) {
			direction = Sign (vect.x);
		} else {
			direction = Sign (vect.z);
		}
		length = Vector3.Magnitude (vect);
	}

	public Vector3 PointAt(float dist) {
		return tail.Position  + dist * vect.normalized;
	}

	public Vector3 PointAtRelative(float ratio) {
		return tail.Position + ratio * vect;
	}

	static int Sign(float x) {
		return (x >= 0 ? +1 : -1);
	}
}

