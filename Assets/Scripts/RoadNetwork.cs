using UnityEngine;
using System.Collections;

public enum Direction {West, East, South, North, NDir}; // road directions
public enum RelDirection {HPrev, HNext, VPrev, VNext, NRelDir}; // relative road directions

public class RoadNetwork {

											// sizes (Unity units) of network elements
	static public readonly float intersectionWidth = 33; // intersection side length (square)
	static public readonly float roadPieceLength = 44; // length of road piece (width is intersectionWidth)

	int nHorzRoads;							// number of horizontal roads
	int nVertRoads;							// number of vertical roads
	Intersection[,] isects;					// intersections

	float height;							// network height
	float width;							// network width

											// getters
	public int NHorzRoads { get { return nHorzRoads; } }
	public int NVertRoads { get { return nVertRoads; } }
	public Intersection[,] Isects { get { return isects; } set { isects = value; } }
	public float Height { get { return height; } }
	public float Width { get { return width; } }

											// horizontal edge into
	public RoadSegment SegmentOn(int i, int j, RelDirection dir) {
		return isects [i, j].Incident [(int) dir];
	}

	public void Wrap(ref Vector3 pos) {		// utility to wrap a vector around edge of network
		float maxX = width / 2;
		float maxZ = height / 2;
		if (pos.x < -maxX) pos.x += width;
		else if (pos.x > maxX) pos.x -= width;
		if (pos.z < -maxZ) pos.z += height;
		else if (pos.z > maxZ) pos.z -= height;
	}

	int Mod(int i, int n) {					// i mod n (positive result, even if i < 0)
		return (i + n) % n;
	}

	public RoadNetwork(						// standard constructor
		float[] horzOffsets, 
		float[] vertOffsets, 
		int[] horzDirections, 
		int[] vertDirections, 
		float ht,
		float wd)
	{
		nHorzRoads = horzOffsets.Length;
		nVertRoads = vertOffsets.Length;
		height = ht;
		width = wd;
											// create intersections
		isects = new Intersection[nHorzRoads, nVertRoads];
		for (int i = 0; i < nHorzRoads; i++) {
			for (int j = 0; j < nVertRoads; j++) {
				isects [i, j] = new Intersection (new Vector3(vertOffsets[j], 0, horzOffsets[i]));
			}
		}
											// create road segments and link in
		for (int i = 0; i < nHorzRoads; i++) {
			for (int j = 0; j < nVertRoads; j++) {
				int iNext = Mod (i + 1, nHorzRoads);
				int jNext = Mod (j + 1, nVertRoads);

				if (horzDirections [i] > 0) {
					RoadSegment seg = new RoadSegment (isects [i, j], isects [i, jNext], true, jNext == 0, width);
					isects [i, j].Incident[(int) RelDirection.HNext] = seg;
					isects [i, jNext].Incident[(int) RelDirection.HPrev] = seg;

				} else if (horzDirections [i] < 0) {
					RoadSegment seg = new RoadSegment (isects [i, jNext], isects [i, j], true, jNext == 0, width);
					isects [i, jNext].Incident[(int) RelDirection.HNext] = seg;
					isects [i, j].Incident[(int) RelDirection.HPrev] = seg;
				} else {
					Debug.Log ("Road direction must be nonzero");
				}
				if (vertDirections [j] > 0) {
					RoadSegment seg = new RoadSegment (isects [i, j], isects [iNext, j], false, iNext == 0, height);
					isects [i, j].Incident[(int) RelDirection.VNext] = seg;
					isects [iNext, j].Incident[(int) RelDirection.VPrev] = seg;

				} else if (vertDirections [j] < 0) {
					RoadSegment seg = new RoadSegment (isects [iNext, j], isects [i, j], false, iNext == 0, height);
					isects [iNext, j].Incident[(int) RelDirection.VNext] = seg;
					isects [i, j].Incident[(int) RelDirection.VPrev] = seg;
				} else {
					Debug.Log ("Road direction must be nonzero");
				}
			}
		}
	}

}
