using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IntersectionController : MonoBehaviour {
	public bool usingAvailabilityMarker = false;
	private bool free = true;
	private MeshRenderer availabilityMarker;

	Queue waitingSpans;

	void Start() {
		availabilityMarker = GetComponentInChildren<MeshRenderer> ();
		waitingSpans = new Queue ();
	}

	void OnTriggerEnter(Collider other) {
		if (free) {
			blockIntersection ();
		} else {
			SpanController thisSpan = other.GetComponentInParent<SpanController> ();
			waitingSpans.Enqueue (thisSpan);
			thisSpan.stopSpan ();
		}
	}

	void OnTriggerExit(Collider other) {
		if (waitingSpans.Count == 0)
			freeIntersection ();
		else {
			(waitingSpans.Dequeue () as SpanController).startSpan ();
		}
	}

	void freeIntersection() {
		free = true;
		availabilityMarker.enabled = false;
	}

	void blockIntersection() {
		free = false;
		availabilityMarker.enabled = usingAvailabilityMarker;
	}
}
