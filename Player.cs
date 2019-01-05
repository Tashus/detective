using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour {

	public static string playerTag = "Player";


	//CharacterController charCon;
	Transform uiAnchor;
	InputManager im;
	ProximityTracker prox;

	NavMeshAgent nav;
	
	float playerHeight = 2f;

	Vector3 movement = Vector3.zero;
	float walkSpeed = 1.5f; // 3.3 mph
	bool isRunning = false;
	float runPace = 0f;
	float minRunPace = 0.25f;
	float runBoost = 2f; // 7.8 mph
	bool isSneaking = false;
	float sneakPace = 0f;
	float minSneakPace = 0.2f;
	float sneakLimit = 0.5f;

	float crashMaxDistance = 10f;
	float crashBaseAttentionDraw = 1f;

	// Use this for initialization
	void Start () {
		uiAnchor = GameObject.Find ("UI Anchor").transform;
		im = uiAnchor.GetComponent<InputManager>();
		//charCon = GetComponent<CharacterController> ();
		//GetComponent<NavMeshAgent> ().SetDestination (new Vector3 (30f, 0f, 10f));
		prox = new GameObject().AddComponent<ProximityTracker>();
		prox.Configure(Character.charTag, 10f, transform);
		gameObject.AddComponent<Rigidbody> ().isKinematic = true;
		nav = GetComponent<NavMeshAgent> ();
	}
	
	// Update is called once per frame
	void Update () {
		uiAnchor.Translate (transform.position - uiAnchor.position);
	}

	void FixedUpdate() {
		Vector2 moveInput = im.GetMoveInput();
		movement.Set(moveInput.x, 0f, moveInput.y);
		runPace = im.GetRun();
		isRunning = runPace > minRunPace;
		runPace = isRunning ? (runPace - minRunPace) / (1f - minRunPace) : 0f;
		//charCon.SimpleMove (movement.normalized * maxSpeed);
		sneakPace = im.GetSneak();
		isSneaking = sneakPace > minSneakPace;
		sneakPace = isSneaking ? (sneakPace - minSneakPace) / (1f - minSneakPace) : 0f;
		float moveSpeed = Mathf.Clamp(movement.magnitude, 0f, 1f - sneakLimit * sneakPace) * (walkSpeed + runBoost * runPace);
		nav.velocity = movement.normalized * moveSpeed;
	}

	public float GetHeight() {
		return playerHeight;
	}

	public Vector3 GetPosition() {
		return new Vector3 (transform.position.x, 0f, transform.position.z);
	}

	void OnTriggerEnter (Collider other) {
		if (other.gameObject.CompareTag (Character.charTag)) {
			if (Vector3.Angle(transform.forward, other.transform.forward) > 90f && nav.velocity.magnitude > walkSpeed * 0.25f) {
				Crash ();
			} else {
				other.GetComponent<Character> ().DrawAttention (transform.position, 30f, 5f);
			}
		}
	}

	public void Crash() {
		foreach (GameObject go in prox.GetObjectsInRange()) {
			float d = (go.transform.position - transform.position).magnitude;
			if (Random.Range (0f, 1f) <= crashBaseAttentionDraw * (1 - d / crashMaxDistance)) {
				go.GetComponent<Character> ().DrawAttention (transform.position, 10f, 1.5f);
			}
		}
	}

}
