using UnityEngine;
using System.Collections;

public class Intersection {
	
	Vector3 position;						// position of intersection center
	RoadSegment[] incident;					// incident edges (North, South, ...)

											// getters/setters
	public Vector3 Position { get { return position; } set { position = value; } }
	public RoadSegment[] Incident { get { return incident; } set { incident = value; } }


	public Intersection(Vector3 pos) {		// construct given position
		position = pos;
		incident = new RoadSegment[(int) RelDirection.NRelDir];
		for (int i = 0; i < 4; i++) {
			incident[i] = null;
		}
	}
}

