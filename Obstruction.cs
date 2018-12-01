using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstruction : MonoBehaviour {

	List<Transform> vertices = new List<Transform>();

	float height;
	float zFightPad = 0.001f;
	float playerHeight;
	Player player;

	Transform leftWall;
	Transform rightWall;
	List<GameObject> topQuads = new List<GameObject> ();

	Vector3 pToOb;

	float visionDistance = 10f;

	public int d;

	// Use this for initialization
	void Start () {
		
	}

	public void ConfigureVertices (List<Vector3> verts, float h, Player p, Transform plane) {
		foreach (Vector3 vert in verts) {
			Transform vertTrans = new GameObject ().transform;
			vertTrans.position = transform.position + vert;
			vertTrans.SetParent (transform);
			vertTrans.RotateAround (transform.position, Vector3.up, transform.eulerAngles.y);
			vertices.Add (vertTrans);
		}

		height = h;

		player = p;
		playerHeight = player.GetHeight ();

		leftWall = Instantiate (plane.gameObject).transform;
		leftWall.SetParent (transform);
		rightWall = Instantiate (plane.gameObject).transform;
		rightWall.SetParent (transform);

		for (int i = 0; i < 3; i++) {
			GameObject q = new GameObject();
			q.name = "Quad_" + i;
			q.transform.SetParent (transform);
			q.transform.localPosition = Vector3.zero;
			q.transform.localRotation = Quaternion.identity;
			q.AddComponent<MeshFilter> ();
			q.AddComponent<MeshRenderer> ();
			Mesh m = new Mesh ();
			Vector3[] vs = new Vector3[4];//{Vector3.zero, Vector3.forward, Vector3.forward + Vector3.right, Vector3.right};
			Vector3[] ns = new Vector3[4];
			for (int j = 0; j < 4; j++) {
				vs [j] = Vector3.zero;
				ns [j] = Vector3.up;

			}
			m.vertices = vs;
			m.triangles = new int[6] {0, 2, 1, 2, 3, 1};
			m.normals = ns;
			q.GetComponent<MeshFilter> ().mesh = m;
			q.GetComponent<MeshRenderer> ().material = plane.GetComponent<MeshRenderer> ().material;
			q.GetComponent<MeshRenderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			q.GetComponent<MeshRenderer> ().receiveShadows = false;
			topQuads.Add (q);
		}
	}

	public void ConfigureRect (float w, float d, float h, Player p, Transform plane) {
		List<Vector3> verts = new List<Vector3> ();
		verts.Add (new Vector3(-w / 2f, 0f, -d / 2f));
		verts.Add (new Vector3 (-w / 2f, 0f, d / 2f));
		verts.Add (new Vector3 (w / 2f, 0f, d / 2f));
		verts.Add (new Vector3 (w / 2f, 0f, -d / 2f));
		ConfigureVertices (verts, h, p, plane);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void FixedUpdate () {
		
		pToOb = transform.position - player.GetPosition ();
		Vector3 perp = Quaternion.AngleAxis (90f, Vector3.up) * pToOb;
		List<Vector3> pToVerts = new List<Vector3>();
		int vs = vertices.Count;
		float[] dots = new float[vs];
		float[] angles = new float[vs];
		int iMax = 0;
		int iMin = 0;
		for (int i = 0; i < vs; i++) {
			pToVerts.Add (vertices [i].position - player.GetPosition ());
			dots [i] = Vector3.Dot (pToVerts[i], perp);
			angles [i] = Vector3.Angle (pToOb, pToVerts [i]);
			iMax = Mathf.Sign(dots[i]) * angles [i] > Mathf.Sign(dots[iMax]) * angles [iMax] ? i : iMax;
			iMin = Mathf.Sign(dots[i]) * angles [i] < Mathf.Sign(dots[iMin]) * angles [iMin] ? i : iMin;
		}

		rightWall.position = vertices [iMax].position + pToVerts [iMax].normalized * visionDistance / 2f + Vector3.up * zFightPad;
		rightWall.forward = Quaternion.AngleAxis (-90f, Vector3.up) * pToVerts [iMax].normalized;
		leftWall.position = vertices [iMin].position + pToVerts [iMin].normalized * visionDistance / 2f + Vector3.up * zFightPad;
		leftWall.forward = Quaternion.AngleAxis (90f, Vector3.up) * pToVerts [iMin].normalized;

		topQuads [2].SetActive ((iMax - iMin + vs) % vs == 3);
		d = (iMax - iMin + vs) % vs;

		for (int i = 0; i < (iMax - iMin + vs) % vs; i++) {
			Quaternion eQuat = Quaternion.Euler (0f, -transform.eulerAngles.y, 0f);
			Vector3[] qVerts = new Vector3[4];
			qVerts [0] = vertices [(iMin + i) % vs].localPosition + Vector3.up * (height + zFightPad);
			qVerts [1] = vertices [(iMin + i + 1) % vs].localPosition + Vector3.up * (height + zFightPad);
			qVerts [2] = vertices [(iMin + i) % vs].localPosition + eQuat * pToVerts [(iMin + i) % vs].normalized * visionDistance * 2f + Vector3.up * (height + zFightPad);
			qVerts [3] = vertices [(iMin + i + 1) % vs].localPosition + eQuat * pToVerts [(iMin + i + 1) % vs].normalized * visionDistance * 2f + Vector3.up * (height + zFightPad);
			topQuads [i].GetComponent<MeshFilter> ().mesh.vertices = qVerts;
		}
	}
}
