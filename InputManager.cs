using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {

	string hAxis;
	string vAxis;
	string runAxis;
	string sneakAxis;

	Vector2 moveInput = Vector2.zero;
	float leftIndex = 0f;
	float leftPalm = 0f;


	// Use this for initialization
	void Start () {
		hAxis = "Horizontal";
		vAxis = "Vertical";
		runAxis = "Run";
		sneakAxis = "Sneak";
	}
	
	// Update is called once per frame
	void Update () {
		float x = Input.GetAxis(hAxis);
		float y = Input.GetAxis(vAxis);
		moveInput.Set(Input.GetAxis(hAxis), Input.GetAxis(vAxis));
		if (x != 0f & y != 0f){
			float f = x * x > y * y ? (y * y) / (x * x) : (x * x) / (y * y);
			moveInput = moveInput.normalized * moveInput.magnitude / Mathf.Sqrt(1f + f);
		}
		leftIndex = Input.GetAxis(runAxis);
		leftPalm = Input.GetAxis(sneakAxis);
	}

	void FixedUpdate () {

	}

	public Vector2 GetMoveInput() {
		return new Vector2(moveInput.x, moveInput.y);
	}

	public float GetRun() {
		return leftIndex;
	}

	public float GetSneak() {
		return leftPalm;
	}

}
