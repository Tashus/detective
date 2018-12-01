using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Character : MonoBehaviour {

	public static string charTag = "Character";

	bool tracking = false;
	float gazeAngle = 0f;

	Vector3 trackTarget = Vector3.forward;
	Vector3 trackProjection = Vector3.forward;
	float trackTime = 0f;
	float trackStart = 0f;

	Transform head;
	float headAngle = 0f;
	float headTargetAngle = 0f;
	float headTurnRate = 90f;
	float headResolution = 1f;
	bool headAtTarget = true;
	float headPauseStart = 0f;
	float headPauseTime = 0f;
	float headPauseMin = 0.5f;
	float headPauseMax = 2.5f;

	float height = 2f;
	public float visionDistance = 10f;
	float visionAngle = 60f;
	float headMaxAngle = 90f;

	NavMeshAgent agent;
	float destinationResolution = 0.5f;

	public enum ActMode {Wait, Move};
	public List<ActMode> acts = new List<ActMode>();
	public List<Vector3> actPositions = new List<Vector3>();
	public List<float> actMaxTimes = new List<float>();
	float actStartTime;

	enum BehaviorMode {Idle, Do, Wander, Patrol};
	BehaviorMode behavior = BehaviorMode.Idle;
	float[] wanderRange;
	float[] waitRange;
	List<Vector3> behaviorPositions;

	public enum CharType {Commoner, Enforcer};
	public CharType type;

	public bool chatty = false;

	public bool suspicious = false;
	int suspicion = 0;
	int suspcionThreshold = 3;
	public bool watching;
	public bool playerInRange = false;
	Player player;
	Vector3 toPlayer;
	float suspicionNotice = 0.5f;
	float noticeThreshold = 1f;
	float noticePerSecond = 2f;
	float noticeDecay = 0.2f;
	float noticeAmount = 0f;
	bool noticed = false;
	public int timesNoticed = 0;
	int highAlertSuspicion = 3;

	// Use this for initialization
	void Start () {
		agent = GetComponent<NavMeshAgent> ();
		QueueWait (0f);
		head = new GameObject ().transform;
		head.SetParent (transform);
		head.localPosition = Vector3.zero;
		head.localEulerAngles = Vector3.zero;
		transform.Find ("Sphere").transform.SetParent (head);
	}

	public void Setup (CharType t) {
		RandomHeadTurn();
	}
	
	void FixedUpdate () {

		if (tracking) {
			UpdateTracking ();
		}

		if (headAtTarget) {
			if (headPauseTime < Time.time - headPauseStart) {
				RandomHeadTurn ();
			}
		} else {
			headAngle = head.localEulerAngles.y;
			headAngle -= headAngle > 180f ? 360f : 0f;
			float angleToHeadTarget = headTargetAngle - headAngle;
			if (headResolution * headResolution < angleToHeadTarget * angleToHeadTarget) {
				head.Rotate (0f, Mathf.Clamp (angleToHeadTarget, -headTurnRate * Time.fixedDeltaTime, headTurnRate * Time.fixedDeltaTime), 0f, Space.Self);
				float yNew = head.localEulerAngles.y;
				if (headMaxAngle * headMaxAngle < yNew * yNew) {
					head.localEulerAngles.Set (head.localEulerAngles.x, headMaxAngle * Mathf.Sign (yNew), head.localEulerAngles.z);
				}
			} else {
				headAtTarget = true;
				headPauseStart = Time.time;
			}
		}

		bool lineOfSight = false;
		if (playerInRange) {
			lineOfSight = CheckVision (player.GetPosition (), Player.playerTag);
			if (lineOfSight) {
				toPlayer = player.GetPosition () - transform.position;
				// TODO Make notice checking a random event, mathed out to a comparable expected value.
				float dNotice = (noticePerSecond + suspicion * suspicionNotice) * Time.fixedDeltaTime * (1f - toPlayer.magnitude / visionDistance) * (1f - 0.5f * Vector3.Angle (head.transform.forward, toPlayer) / visionAngle);
				noticeAmount = Mathf.Clamp (noticeAmount + dNotice, 0f, noticeThreshold);
				if (!noticed && noticeAmount >= noticeThreshold) {
					noticed = true;
					if (type == CharType.Commoner) {
						if (0 < suspicion) {
							timesNoticed++;
							if (timesNoticed >= (suspcionThreshold - suspicion)) {
								Suspect ();
								timesNoticed = 0;
							}
						}
					}
					if (type == CharType.Enforcer) {
						if (0 < suspicion) {
							Debug.Log ("Confronting CHARNAME");
						}
						if (highAlertSuspicion <= suspicion) {
							Debug.Log ("Chasing CHARNAME");
							// Alert other cops
						}
					}
				}
			}
		}

		if (noticed && !lineOfSight) {
			noticeAmount = Mathf.Clamp (noticeAmount - noticeDecay * Time.fixedDeltaTime, 0f, noticeThreshold);
			if (noticeAmount <= 0f) {
				Unnotice ();
			}
		}

		switch (acts [0]) {
		case ActMode.Wait:
			{
				if (Time.time - actStartTime > actMaxTimes [0]) {
					EndAction ();
				}
			}
			break;
		case ActMode.Move:
			{
				if ((agent.destination - transform.position).sqrMagnitude < destinationResolution * destinationResolution) {
					EndAction ();
				}
			}
			break;
		}
	}

	bool CheckVision(Vector3 pos, string checkTag) {
		Vector3 toPos = pos - transform.position;
		toPos.Set (toPos.x, 0f, toPos.z);
		float a = Vector3.Angle (head.transform.forward, toPos);
		float d = toPos.magnitude;
		if (a <= visionAngle && d <= visionDistance) {
			RaycastHit hit;
			return Physics.Raycast (transform.position + Vector3.up * height * 0.75f, toPos, out hit, visionDistance) && hit.collider.CompareTag (checkTag);
		} else {
			return false;
		}
	}

	void Suspect() {
		suspicion++;
		Debug.Log ("Suspected! n: " + timesNoticed + ", s: " + suspicion);
		if (suspicion > 2) {
			Debug.Log ("GG");
		}
	}

	void Unnotice () {
		noticed = false;
		noticeAmount = 0f;
	}

	void NextAction() {
		actStartTime = Time.time;
		switch (acts [0]) {
		case ActMode.Wait:
			{
				agent.isStopped = true;
			}
			break;
		case ActMode.Move:
			{
				SetDestination (actPositions[0]);
				agent.isStopped = false;
			}
			break;
		}
	}

	void EndAction() {
		if (acts.Count > 1) {
			acts.RemoveAt (0);
			actPositions.RemoveAt (0);
			actMaxTimes.RemoveAt (0);
			NextAction ();
		} else {
			CycleBehavior ();
		}
	}

	void CycleBehavior() {
		switch (behavior) {
		case BehaviorMode.Do:
			{
				Idle ();
			}
			break;
		case BehaviorMode.Wander:
			{
				QueueMove (new Vector3 (Random.Range (wanderRange [0], wanderRange [1]), 0f, Random.Range (wanderRange [2], wanderRange [3])), 0f, Random.Range (waitRange [0], waitRange [1]));
			}
			break;
		case BehaviorMode.Patrol:
			{
				foreach (Vector3 p in behaviorPositions) {
					QueueMove (p, 0f, Random.Range (waitRange [0], waitRange [1]));
				}
			}
			break;
		}
		if (behavior != BehaviorMode.Idle) {
			NextAction ();
		}
	}

	public void Idle() {
		behavior = BehaviorMode.Idle;
	}

	public void Do(List<ActMode> aList, List<Vector3> posList, List<float> tList) {
		behavior = BehaviorMode.Do;
		acts = aList;
		actPositions = posList;
		actMaxTimes = tList;
	}

	public void Wander (float[] rWander, float[] rWait) {
		behavior = BehaviorMode.Wander;
		wanderRange = rWander;
		waitRange = rWait;
	}

	public void Patrol (List<Vector3> posList, float[] rWait) {
		behavior = BehaviorMode.Patrol;
		waitRange = rWait;
		behaviorPositions = posList;
	}

	void QueueAct (ActMode mode, Vector3 pos, float tMax) {
		QueueAct (mode, pos, tMax, false);
	}

	void QueueAct (ActMode mode, Vector3 pos, float tMax, bool skipLine) {
		if (skipLine) {
			acts.Insert (0, mode);
			actPositions.Insert (0, pos);
			actMaxTimes.Insert (0, tMax);
			NextAction ();
		} else {
			acts.Add (mode);
			actPositions.Add (pos);
			actMaxTimes.Add (tMax);
		}

	}

	public void QueueWait (float tMax) {
		QueueWait (tMax, false);
	}

	public void QueueWait (float tMax, bool skipLine) {
		QueueAct (ActMode.Wait, Vector3.zero, tMax, false);
	}

	public void QueueMove (Vector3 pos) {
		QueueMove (pos, false);
	}

	public void QueueMove (Vector3 pos, bool skipLine) {
		QueueMove (pos, 0f, 0f, skipLine);
	}

	public void QueueMove (Vector3 pos, float tDelay, float tLinger) {
		QueueMove (pos, tDelay, tLinger, false);
	}

	public void QueueMove (Vector3 pos, float tDelay, float tLinger, bool skipLine) {
		if (tDelay > 0f) {
			QueueWait (tDelay);
		}
		QueueAct (ActMode.Move, pos, 0f);
		if (tLinger > 0f) {
			QueueWait (tLinger);
		}
	}

	void SetDestination (Vector3 dest) {
		NavMeshHit hit;
		NavMesh.SamplePosition (dest, out hit, height, 1);
		agent.SetDestination (hit.position);
	}

	public void AddPlayer (Player p) {
		player = p;
	}

	void TurnHeadTo (float a, float t) {
		headTargetAngle = Mathf.Clamp(a, -headMaxAngle, headMaxAngle) ;
		headPauseTime = t;
		headAtTarget = false;
	}

	void RandomHeadTurn () {
		float turnAngle = Random.Range (-headMaxAngle, headMaxAngle);
		turnAngle *= suspicious ? 1f : 0.75f;
		float pause = Random.Range (headPauseMin, headPauseMax) / (suspicion + 1);
		TurnHeadTo (turnAngle, pause);
	}

	public void TrackPoint (Vector3 p, float t) {
		trackTarget = p;
		trackTime = t;
		trackStart = Time.time;
		tracking = true;
	}

	void UpdateTracking () {
		trackProjection = head.InverseTransformPoint (trackTarget.x, head.position.y, trackTarget.z);
		float trackAngle = Vector3.Angle (Vector3.forward, trackProjection) * (0 < trackProjection.x ? 1f : -1f);
		float remainingTrackTime = trackTime - (Time.time - trackStart);
		if (0f < remainingTrackTime) {
			TurnHeadTo (trackAngle, remainingTrackTime);
			if (acts [0] == ActMode.Wait) {
				transform.Rotate (0f, Mathf.Clamp (trackAngle, -120f * Time.fixedDeltaTime, 120f * Time.fixedDeltaTime), 0f);
			}
		} else {
			tracking = false;
		}
	}

	public void DrawAttention(Vector3 p, float trackLength, float waitTime) {
		DrawAttention (p, trackLength, waitTime, 1f);
	}

	public void DrawAttention(Vector3 p, float trackLength, float waitTime, float chance) {
		if (Random.Range (0f, 1f) <= chance) {
			TrackPoint (p, trackLength);
			QueueAct (ActMode.Wait, p, waitTime, true);
		}
	}
}
