using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityTracker : MonoBehaviour {

	List<GameObject> objects = new List<GameObject> ();
	string objectTag;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Configure (string objTag, float r, Transform parentTo) {
		objectTag = objTag;
		Rigidbody rb = gameObject.AddComponent<Rigidbody> ();
		rb.isKinematic = true;
		SphereCollider sph = gameObject.AddComponent<SphereCollider> ();
		sph.radius = r;
		sph.isTrigger = true;
		transform.SetParent (parentTo);
		transform.localPosition = Vector3.zero;
	}

	void OnTriggerEnter (Collider other) {
		if (other.CompareTag(objectTag)) {
			objects.Add (other.gameObject);
		}
	}

	void OnTriggerExit (Collider other) {
		if (other.CompareTag(objectTag)) {
			objects.Remove (other.gameObject);
		}
	}

	public List<GameObject> GetObjectsInRange() {
		return objects;
	}
}
