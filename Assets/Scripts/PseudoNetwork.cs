using UnityEngine;
using System.Collections;

public class PseudoNetwork  {
	// The following objects are derived from the segment information above

	public int nVertRoads;						// numbers of roads
	public int nHorzRoads;

	public float[] vertOffsets;					// x-offsets of vertical roads
	public float[] horzOffsets; 				// y-offsets of horizontal roads

	public int[] vertDirections;				// directions of vert/horz roads
	public int[] horzDirections;

	public float width;							// geometric size (Unity units)
	public float height;

	public PseudoNetwork() {					// empty constructor
		nVertRoads = nHorzRoads = 0;
		width = height = 0;
	}
												// construct from fragment descriptors
	public PseudoNetwork(string horizontalFragments, string verticalFragments) {
		GenerateNetworkStructure (horizontalFragments, verticalFragments);
	}

												// generate from fragment descriptors
	public void  GenerateNetworkStructure(string verticalPattern, string horizontalPattern) {

		// get network dimensions
		width = GetTotalWidth (verticalPattern);
		height = GetTotalWidth (horizontalPattern);

		// horizontal fragments define vertical roads
		GenerateOffsetsAndDirections (verticalPattern, width, out nVertRoads, out vertOffsets, out vertDirections);
		// vertical fragments define horizontal roads
		GenerateOffsetsAndDirections (horizontalPattern, height, out nHorzRoads, out horzOffsets, out horzDirections); 
	}

												// generate the road offsets and directions
	void GenerateOffsetsAndDirections (string fragments, float width, out int nRoads, out float[] offsets, out int[] directions) {

		nRoads = 0;							// number of roads
		// "+^v" - signify intersections
		for (int i = 0; i < fragments.Length; i++) {
			if (fragments[i] == '+' || fragments [i] == '^' || fragments [i] == 'v') {
				nRoads++;
			}
		}

		offsets = new float[nRoads];		// allocate storage for offsets and directions
		directions = new int[nRoads];

		float currentOffset = -width / 2;	// current offset w.r.t. origin
		int m = 0;							// current road index

		for (int i = 0; i < fragments.Length; i++) {
			// intersection
			if (fragments[i] == '+' || fragments [i] == '^' || fragments [i] == 'v') {
				directions [m] = (fragments[i] == '+' ? 0 : (fragments [i] == '^' ? +1 : -1));
				offsets [m] = currentOffset + RoadNetwork.intersectionWidth/2;
				m++;
				currentOffset += RoadNetwork.intersectionWidth;
			} else {						// road segment
				currentOffset += RoadNetwork.roadPieceLength;
			}
		}
	}

	// compute total network width
	float GetTotalWidth(string fragments) {
		float totalWidth = 0;
		for (int i = 0; i < fragments.Length; i++) {
			if (fragments[i] == '+' || fragments [i] == '^' || fragments [i] == 'v') {
				totalWidth += RoadNetwork.intersectionWidth;
			} else {
				totalWidth += RoadNetwork.roadPieceLength;
			}
		}
		return totalWidth;
	}

	// build refined network by splitting roads
	public PseudoNetwork CreateRefinedNetwork(float laneOffset) {
		PseudoNetwork rn = new PseudoNetwork (); // the refined network
		rn.nVertRoads = 2 * nVertRoads;		// each road is doubled
		rn.nHorzRoads = 2 * nHorzRoads;

		rn.width = width;					// geometric size is unchanged
		rn.height = height;
		// allocate storage for offsets and directions
		rn.vertOffsets = new float [rn.nVertRoads];
		rn.horzOffsets = new float [rn.nHorzRoads];
		rn.vertDirections = new int [rn.nVertRoads];
		rn.horzDirections = new int [rn.nHorzRoads];

		splitRoads (laneOffset, nHorzRoads, horzOffsets, rn.horzOffsets, horzDirections, rn.horzDirections, true);
		splitRoads (laneOffset, nVertRoads, vertOffsets, rn.vertOffsets, vertDirections, rn.vertDirections, false);

		return rn;
	}

	static void splitRoads (float laneOffset, int nRoads, float[] offsets, float[] rnOffsets, int[] directions, int[] rnDirections, bool isHorizontal) {
		for (int i = 0; i < nRoads; i++) {
			rnOffsets [2 * i] = offsets [i] - laneOffset;
			rnOffsets [2 * i + 1] = offsets [i] + laneOffset;
			if (directions [i] == 0) {
				if (isHorizontal) {				// road orientation USA (drive on right)
					rnDirections [2 * i] = +1;
					rnDirections [2 * i + 1] = -1;
				} else {
					rnDirections [2 * i] = -1;
					rnDirections [2 * i + 1] = +1;
				}
			}
			else {
				rnDirections [2 * i] = rnDirections [2 * i + 1] = directions [i];
			}
		}
	}

}
