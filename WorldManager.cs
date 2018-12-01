using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WorldManager : MonoBehaviour {

	Player player;
	//GameObject obstructions;
	GameObject baseNPC;
	public GameObject obstructionPlane;
	float visionDistance = 10f;

	public Material npcMat;
	public Material copMat;
	public Material targetMat;

	// Use this for initialization
	void Start () {

		player = GameObject.Find ("Player").GetComponent<Player> ();
		// Build environment:
		//obstructions = transform.Find ("Obstructions").gameObject;

		float tileSize = 16f;
		float border = 1f;
		float cubeWidth = 2f;
		float pathWidth = 2f;
		int numCubes = 4;

		GameObject floor = transform.Find ("Floor").gameObject;
		GameObject station = transform.Find ("Station").gameObject;
		station.transform.SetParent (floor.transform);

		GameObject blocker = transform.Find("Blocker").gameObject;
		GameObject nonBlocker = transform.Find("NonBlocker").gameObject;
		float percentBlockers = 0.75f;
		List<GameObject> obs = new List<GameObject> ();

		for (int r = 0; r < 3; r++) {
			for (int c = 0; c < 3; c++) {
				if (r == 1 && c == 1) {
					
				} else {
					int gapStart = r == 1 ? 1 : numCubes;
					int gapEnd = r == 1 ? numCubes - 1 : numCubes;
					for (int m = 0; m < numCubes; m++) {
						for (int n = 0; n < numCubes; n++) {
							if (n < gapStart || gapEnd <= n) {
								bool isBlocker = Random.Range (0f, 1f) < percentBlockers;
								GameObject addition = Instantiate (isBlocker ? blocker : nonBlocker, Vector3.zero, new Quaternion ());
								if (isBlocker) {
									obs.Add (addition);
								}
								addition.transform.SetParent (floor.transform);
								addition.transform.Translate (border + cubeWidth / 2f + m * (cubeWidth + pathWidth) + tileSize * (c - 1.5f), 0f, border + cubeWidth / 2f + n * (cubeWidth + pathWidth) + tileSize * (r - 1.5f));
							}
						}
					}
				}
			}
		}

		//station.SetActive (false);
		floor.GetComponent<NavMeshSurface> ().BuildNavMesh ();
		station.AddComponent<Obstruction> ().ConfigureRect (6f, 2f, 2f, player, obstructionPlane.transform);
		foreach (GameObject ob in obs) {
			ob.AddComponent<Obstruction> ().ConfigureRect (2f, 2f, 2f, player, obstructionPlane.transform);
		}

		blocker.SetActive (false);
		nonBlocker.SetActive(false);
		obstructionPlane.SetActive (false);

		// Populate environment:
		player.GetComponent<NavMeshAgent>().Warp(Vector3.forward * 3f);

		baseNPC = GameObject.Find("NPC").gameObject;
		int numCommon = 94;
		int numEnforcer = 4;
		float[] wanderRange = new float[4] { -tileSize * 1.5f, tileSize * 1.5f, -tileSize * 1.5f, tileSize * 1.5f};
		float[] waitRange = new float[2] { 1f, 5f };
		{
			Character target = MakeNPC (1.75f, Character.CharType.Commoner);
			target.Setup (Character.CharType.Commoner);
			Vector3 pos = new Vector3 (0f, 0f, 6f);
			//Vector3 pos = new Vector3 (Random.Range (wanderRange [0], wanderRange [1]), 0f, Random.Range (wanderRange [2], wanderRange [3]));
			NavMeshHit hit;
			while (!NavMesh.SamplePosition (pos, out hit, 2f, 1)) {
			}
			target.GetComponent<NavMeshAgent> ().Warp (hit.position);
			target.Wander (wanderRange, waitRange);
			target.GetComponent<Character> ().QueueWait (10f, true);
			foreach (Transform part in target.transform) {
				part.GetComponent<MeshRenderer> ().material = targetMat;
			}
			target.suspicious = true;
		}
		for (int i = 0; i < numCommon; i++) {
			Character newChar = MakeNPC (1.5f, Character.CharType.Commoner);
			Vector3 pos = new Vector3 (Random.Range (wanderRange [0], wanderRange [1]), 0f, Random.Range (wanderRange [2], wanderRange [3]));
			NavMeshHit hit;
			while (!NavMesh.SamplePosition(pos, out hit, 2f, 1)) {
				pos = new Vector3 (Random.Range (wanderRange [0], wanderRange [1]), 0f, Random.Range (wanderRange [2], wanderRange [3]));
			}
			newChar.GetComponent<NavMeshAgent> ().Warp (hit.position);
			newChar.Wander (wanderRange, waitRange);
			foreach (Transform part in newChar.transform) {
				part.GetComponent<MeshRenderer> ().material = npcMat;
			}
		}
		for (int i = 0; i < numEnforcer; i++) {
			Character newChar = MakeNPC (1.75f, Character.CharType.Enforcer);
			Vector3 pos = new Vector3 (Random.Range (wanderRange [0], wanderRange [1]), 0f, Random.Range (wanderRange [2], wanderRange [3]));
			NavMeshHit hit;
			while (!NavMesh.SamplePosition(pos, out hit, 2f, 1)) {
				pos = new Vector3 (Random.Range (wanderRange [0], wanderRange [1]), 0f, Random.Range (wanderRange [2], wanderRange [3]));
			}
			//newChar.transform.position = hit.position;
			newChar.GetComponent<NavMeshAgent> ().Warp (hit.position);
			List<Vector3> patrolRoute = new List<Vector3> ();
			patrolRoute.Add (new Vector3 (wanderRange [0], 0f, wanderRange [2]));
			patrolRoute.Add (new Vector3 (wanderRange [0], 0f, wanderRange [3]));
			patrolRoute.Add (new Vector3 (wanderRange [1], 0f, wanderRange [2]));
			patrolRoute.Add (new Vector3 (wanderRange [1], 0f, wanderRange [3]));
			newChar.Patrol (patrolRoute, waitRange);
			for (int j = i; j < patrolRoute.Count; j++) {
				newChar.QueueMove (patrolRoute [j]);
			}
			foreach (Transform part in newChar.transform) {
				part.GetComponent<MeshRenderer> ().material = copMat;
			}
		}

		baseNPC.SetActive (false);
	}

	// Update is called once per frame
	void Update () {

	}

	Character MakeNPC (float speed, Character.CharType type) {
		GameObject n = Instantiate (baseNPC);
		n.AddComponent<Rigidbody> ().isKinematic = true;
		CapsuleCollider cap = n.AddComponent<CapsuleCollider> ();
		//cap.isTrigger = true;
		cap.center = Vector3.up;
		cap.radius = 0.35f;
		cap.height = 2f;
		NavMeshAgent nav = n.AddComponent<NavMeshAgent> ();
		nav.radius = 0.35f;
		nav.height = 2f;
		nav.speed = speed * Random.Range(0.8f, 1.2f);
		Character charScript = n.AddComponent<Character> ();
		charScript.type = type;
		charScript.AddPlayer (player);
		return charScript;
	}
}