using UnityEngine;
using System.Collections;

public class IAZombie : MonoBehaviour {

	#region constant variables
	private const float minTimeBetweenActions = 3.0f;
	private const float actionDuration = 1.0f;
	#endregion

	#region public variables
	public float timeBetweenActions;
	public float speedMovement;
	public float speedRotation;
	public GameObject steve;	//TODO: research best option to obtain the Steve's position
	#endregion

	#region private variables
	private float time;
	private bool isMoving;
	private bool isRotating;
	private int rotationDir; //1 = left, 0 = right
	private float actionTimer;
	#endregion

	// Use this for initialization
	void Start () {
		time = 0.0f;
		timeBetweenActions = Mathf.Min(minTimeBetweenActions, timeBetweenActions);
		isMoving = false;
		isRotating = false;
	}
	
	// Update is called once per frame
	void Update () {
		time += Time.deltaTime;

		if(time >= timeBetweenActions) {
			time = 0.0f;
			int action = Random.Range(0, 2);
			switch(action) {
			case 0:	//MOV forward
				isMoving = true;
				actionTimer = 0.0f;
				break;
			case 1:	//ROT and MOV
				isRotating = true;
				rotationDir = Random.Range(0, 2);
				actionTimer = 0.0f;
				break;
			}
		}

		doAction();
	}

	private void doAction() {
		actionTimer += Time.deltaTime;
		if(isMoving) {
			transform.Translate(Vector3.forward * speedMovement * Time.deltaTime);
			if(actionTimer == actionDuration) {
				isMoving = false;
			}
		}
		if(isRotating) {
			float rotation = rotationDir==0 ? speedRotation : -speedRotation;
			transform.Rotate (Vector3.up, rotation * Time.deltaTime);
			if(actionTimer == actionDuration) {
				isRotating = false;
				actionTimer = 0.0f;
				isMoving = true;
			}
		}
	}

	void OnCollisionEnter(Collision other) {
		//TODO: not working
		if(other.gameObject.ToString() == "Steve") {
			Debug.Log ("Steve");
		} else {
			Debug.Log ("World");
		}
	}
}
