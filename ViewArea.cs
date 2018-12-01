using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewArea : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter (Collider other) {
		if (other.CompareTag(Character.charTag)) {
			other.GetComponent<Character>().playerInRange = true;
		}
	}

	void OnTriggerExit (Collider other) {
		if (other.CompareTag(Character.charTag)) {
			other.GetComponent<Character>().playerInRange = false;
		}
	}
}
