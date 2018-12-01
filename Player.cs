using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour {

	public static string playerTag = "Player";

	//CharacterController charCon;
	Transform uiAnchor;
	ProximityTracker prox;

	NavMeshAgent nav;

	Vector3 movement = Vector3.zero;
	float maxSpeed = 2f;
	float playerHeight = 2f;

	float crashMaxDistance = 10f;
	float crashBaseAttentionDraw = 1f;

	// Use this for initialization
	void Start () {
		uiAnchor = GameObject.Find ("UI Anchor").transform;
		//charCon = GetComponent<CharacterController> ();
		//GetComponent<NavMeshAgent> ().SetDestination (new Vector3 (30f, 0f, 10f));
		prox = new GameObject().AddComponent<ProximityTracker>();
		prox.Configure(Character.charTag, 10f, transform);
		gameObject.AddComponent<Rigidbody> ().isKinematic = true;
		nav = GetComponent<NavMeshAgent> ();
	}
	
	// Update is called once per frame
	void Update () {
		movement.Set (0f, 0f, 0f);
		if (Input.GetKey (KeyCode.A)) {
			movement += Vector3.left;
		}
		if (Input.GetKey (KeyCode.D)) {
			movement += Vector3.right;
		}
		if (Input.GetKey (KeyCode.W)) {
			movement += Vector3.forward;
		}
		if (Input.GetKey (KeyCode.S)) {
			movement += Vector3.back;
		}
		if (Input.GetKeyDown (KeyCode.Space)) {
			
		}
	}

	void FixedUpdate() {
		//charCon.SimpleMove (movement.normalized * maxSpeed);
		nav.velocity = movement.normalized * maxSpeed;
		uiAnchor.Translate (transform.position - uiAnchor.position);
	}

	public float GetHeight() {
		return playerHeight;
	}

	public Vector3 GetPosition() {
		return new Vector3 (transform.position.x, 0f, transform.position.z);
	}

	void OnTriggerEnter (Collider other) {
		if (other.gameObject.CompareTag (Character.charTag)) {
			if (Vector3.Angle(transform.forward, other.transform.forward) > 90f && nav.velocity.magnitude > maxSpeed * 0.25f) {
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
