using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {

	string hAxis;
	string vAxis;

	Vector2 moveInput = new Vector2();


	// Use this for initialization
	void Start () {
		hAxis = "Horizontal";
		vAxis = "Vertical";
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void FixedUpdate () {
		moveInput.Set(Input.GetAxis(hAxis), Input.GetAxis(vAxis));
	}

	public Vector2 GetMoveInput() {
		return new Vector2(moveInput.x, moveInput.y);
	}
}
