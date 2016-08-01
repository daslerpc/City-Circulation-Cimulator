using UnityEngine;
using System.Collections;
//using UnityEditor;

public class TrafficController : MonoBehaviour {

//	static public readonly float spanSpeed = 50;	// speed at which spans move
//	static public readonly float laneOffset = 2.5f; // offset of two lanes relative to road center

											// controls (probably overridden in editor)
	public bool generateNetwork = true;		// generate new network?
	public bool saveAsPrefab = false;		// save the generated network as Unity prefab

	// network structure (may be overridden in editor)
	// '-' = road segment
	// '+' = intersection (two way)
	// '^''v' = intersection (one way)
	public string verticalPattern   = "--+---+----^----v-----+---+--+--"; // pattern of vertical roads
	public string horizontalPattern = "--+---+--+-----v------^---+---+---"; // pattern of horizontal roads


	bool runtimeInitialized = false;		// has network been initialized?
	RoadNetwork roadNetwork;				// the road network
	GameObject roadNetObject;				// RoadNetwork game object
	GameObject runtimeGenerated;			// Portion of RoadNetwork generated at runtime
	GameObject spansFolder;					// folder where spans are stored

	public RoadNetwork getRoadNetwork() {	// get the associated network
		return roadNetwork;
	}
											// Use this for initialization
	void Awake () {
		roadNetObject = GameObject.Find ("RoadNetwork");
		runtimeGenerated = GameObject.Find ("RuntimeGeneratedNetwork");
		spansFolder = GameObject.Find ("Spans");
		roadNetwork = GenerateNetwork (verticalPattern, horizontalPattern);
	}

	void Start() {							// Use this for initialization
		SpanController.CreateInitialSpans (roadNetwork, spansFolder);
	}

	RoadNetwork GenerateNetwork(string verticalPattern, string horizontalPattern) {
		PseudoNetwork baseNet;				// partial (undirected) road network
		PseudoNetwork refinedNet;			// partial (directed) road network
		GraphicalNetwork graphicalNetwork;	// ...and its GraphicalNetwork component

											// get the RoadNetwork object
		if (runtimeGenerated != null) {		// destroy any existing RuntimeGenerated object
			GameObject.Destroy (runtimeGenerated);
		}
											// create a new one
		runtimeGenerated = new GameObject ("RuntimeGeneratedNetwork");
		runtimeGenerated.transform.parent = roadNetObject.transform; // child of RoadNetwork
											// save the graphical component
		graphicalNetwork = roadNetObject.GetComponent<GraphicalNetwork> ();
											// create the base road network
		baseNet = new PseudoNetwork (verticalPattern, horizontalPattern);
											// generated refined network
		refinedNet = baseNet.CreateRefinedNetwork (GraphicalNetwork.laneOffset); 

		if (generateNetwork) {				// generate graphical elements?
			graphicalNetwork.InstantiateGameObjects (verticalPattern, horizontalPattern, baseNet, runtimeGenerated); 
			runtimeInitialized = true;
		}

		if (saveAsPrefab) {					// save it as a prefab?
			SaveNetworkAsPrefab ();
		}
											// generate RoadNetwork from pseudo-network
		RoadNetwork theNetwork = new RoadNetwork (
			                         refinedNet.horzOffsets, 
			                         refinedNet.vertOffsets, 
			                         refinedNet.horzDirections, 
			                         refinedNet.vertDirections, 
			                         refinedNet.height,
			                         refinedNet.width);
		return theNetwork;
	}
	
											// update is called once per frame
	void Update () {
		if (Input.GetKey("escape"))
			Application.Quit();
	}

	void SaveNetworkAsPrefab() {			// save the network (for use outside of game)
		if (runtimeInitialized) {			// network must already be generated
//			GameObject prefab = PrefabUtility.CreatePrefab("Assets/Prefabs/saved-network.prefab", runtimeGenerated, ReplacePrefabOptions.ReplaceNameBased);
//			if (prefab == null) {
//				Debug.Log("Failed to create network prefab. Sorry!");
//			}
		}
	}

}
