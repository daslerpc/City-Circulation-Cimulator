using UnityEngine;
using System.Collections;

public class GraphicalNetwork : MonoBehaviour {

	static public readonly float laneOffset = 2.5f; // offset of two lanes relative to road center

	// instantiate the road network game objects from given base network
	public void InstantiateGameObjects(string verticalPattern, string horizontalPattern, PseudoNetwork baseNet, GameObject runtimeGenerated) {
		GameObject intersectionPrefab = Resources.Load ("Prefabs/Intersection") as GameObject;
		GameObject roadPieceXPrefab = Resources.Load ("Prefabs/Road-Piece-X") as GameObject;
		GameObject roadPieceZPrefab = Resources.Load ("Prefabs/Road-Piece-Z") as GameObject;
		GameObject groundPrefab = Resources.Load ("Prefabs/Ground") as GameObject;

		if (intersectionPrefab == null) {
			Debug.Log ("Cannot find intersection prefab");
		}
		GameObject go;						// temporary game object for prefabs
											// additional offsets for centering
		float intersectionBias = RoadNetwork.intersectionWidth / 2;
		float roadPieceBias = RoadNetwork.roadPieceLength / 2;

											// fill in from lower left to upper right
		float currentHorzOffset = -baseNet.height / 2;
		for (int i = 0; i < horizontalPattern.Length; i++) {

			float currentVertOffset = -baseNet.width / 2;
			for (int j = 0; j < verticalPattern.Length; j++) {
				if (horizontalPattern[i] == '+' || horizontalPattern [i] == '^' || horizontalPattern [i] == 'v') {
					if (verticalPattern [j] == '+' || verticalPattern [j] == '^' || verticalPattern [j] == 'v') {
											// instantiate intersection
						Vector3 center = new Vector3(currentVertOffset + intersectionBias, 0, currentHorzOffset + intersectionBias);
						go = Instantiate (intersectionPrefab, center, Quaternion.identity) as GameObject;
						go.transform.parent = runtimeGenerated.transform;
						currentVertOffset += RoadNetwork.intersectionWidth;
					} 
					else {					// must be (verticalPattern [j] == '-')
											// instantiate horizontal road piece
						Vector3 center = new Vector3(currentVertOffset + roadPieceBias, 0, currentHorzOffset + intersectionBias);
						go = Instantiate (roadPieceXPrefab, center, Quaternion.identity) as GameObject;
						go.transform.parent = runtimeGenerated.transform;
						currentVertOffset += RoadNetwork.roadPieceLength;
					}
				} 
				else { 						// must be (horizontalPattern [i] == '-')
					if (verticalPattern [j] == '+' || verticalPattern [j] == '^' || verticalPattern [j] == 'v') {
						// instantiate vertical road piece
						Vector3 center = new Vector3(currentVertOffset + intersectionBias, 0, currentHorzOffset + roadPieceBias);
						go = Instantiate (roadPieceZPrefab, center, Quaternion.identity) as GameObject;
						go.transform.parent = runtimeGenerated.transform;
						currentVertOffset += RoadNetwork.intersectionWidth;
					} 
					else {					// must be (verticalPattern [j] == '-')
						// Empty space - nothing to instantiate
						currentVertOffset += RoadNetwork.roadPieceLength;
					}
				}
			}

			if (horizontalPattern [i] == '+' || horizontalPattern [i] == '^' || horizontalPattern [i] == 'v') {
				currentHorzOffset += RoadNetwork.intersectionWidth;
			} else {
				currentHorzOffset += RoadNetwork.roadPieceLength;
			}

		}
		// instantiate ground object
		GameObject ground = Instantiate (groundPrefab);
		ground.transform.localScale = new Vector3(baseNet.width / 10, 1, baseNet.height / 10);
		ground.transform.parent = runtimeGenerated.transform;
	}

}
