using UnityEngine;
using System.Collections;

public class IAZombie : MonoBehaviour {

	#region constant variables
	private const float minTimeBetweenActions = 3.0f;
	private const float actionDuration = 1.0f;
	private const float detectionDistance = 10.0f;
	private const float damageAnimationDuration = 1.0f;
	#endregion

	#region public variables
	public float timeBetweenActions;
	public float speedMovement;
	public float speedRotation;
	public float speedJump;
	public Transform steve;
	public Animator legLeft;
	public Animator legRight;

	public ParticleSystem blood;
	#endregion

	#region private variables
	private float time;
	private bool isMoving;
	private bool isRotating;
	private int rotationDir; //1 = left, 0 = right
	private float actionTimer;
	private bool isSteveNear;
	private float angleToSteve;
	private bool isJumping;
	private bool isGrounded;
	private bool jumpEnough;
	private float heightZombie;

	private float life;
	private float damageTimer;
	private bool damaged;
	private bool died;
	#endregion

	// Use this for initialization
	void Start () {
		time = 0.0f;
		timeBetweenActions = Mathf.Min(minTimeBetweenActions, timeBetweenActions);
		isMoving = false;
		isRotating = false;
		isSteveNear = false;
		isJumping = true;
		jumpEnough = false;

		legLeft.enabled = false;
		legRight.enabled = false;

		life = 5.0f;
		damaged = false;
		died = false;
	}
	
	// Update is called once per frame
	void Update () {
		time += Time.deltaTime;

		isSteveNear = detectSteve();

		//we don't want to do anything when we are damaged
		if(time >= timeBetweenActions && !damaged) {
			time = 0.0f;
			int action;
			action = Random.Range(0, 2);
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

		if(!damaged) {
			if(isSteveNear) goForSteve();
			else doAction();
		
			if(isJumping && !jumpEnough) doJump();
		} else doAnimationDamaged ();
	}

	private void doAction() {
		actionTimer += Time.deltaTime;
		if(isMoving) {
			transform.Translate(Vector3.left * speedMovement * Time.deltaTime);
			legLeft.enabled = true;
			legRight.enabled = true;
			if(actionTimer >= actionDuration) {
				isMoving = false;
				actionTimer = 0.0f;
			}
		}
		if(isRotating) {
			float rotation;
			rotation = rotationDir==0 ? speedRotation : -speedRotation;
			transform.Rotate (Vector3.up, rotation * Time.deltaTime);
			if(actionTimer >= actionDuration) {
				isRotating = false;
				actionTimer = 0.0f;
				isMoving = true;
			}
		}
	}

	private void goForSteve() {
		//0 <= dirZtoS <= 180
		Vector2 dirZtoS = new Vector2(steve.position.x - transform.position.x, steve.position.z - transform.position.z);
		angleToSteve = Vector2.Angle(dirZtoS, new Vector2(1.0f, 0.0f));
		//Convert anglToSteve in a 0-360 vector in the same axis of zombieAngleY
		if(dirZtoS.y >= 0) {
			angleToSteve = 360.0f - angleToSteve;
		}
		angleToSteve += 180.0f;
		if(angleToSteve >= 360.0f) {
			angleToSteve -= 360.0f;
		}

		//0 <= zombieAngleY <= 360
		float zombieAngleY = transform.rotation.eulerAngles.y;

		float angle = angleToSteve - zombieAngleY;
		if( angle >= 5.0f || angle <= -5.0f) {	//ROT
			legLeft.enabled = false;
			legRight.enabled = false;
			float rotation = angle<0.0f ? -speedRotation : speedRotation;
			transform.Rotate (Vector3.up, rotation * Time.deltaTime);
			
		} else {
			transform.Translate(Vector3.left * speedMovement * Time.deltaTime);
			legLeft.enabled = true;
			legRight.enabled = true;
		}
	}

	private bool detectSteve() {
		if(Vector3.Distance(steve.position, transform.position) <= detectionDistance) {
			return true;
		}
		legLeft.enabled = false;
		legRight.enabled = false;
		return false;
	}

	private void doJump() {
		transform.Translate(Vector3.up * speedJump * Time.deltaTime);
		transform.Translate(Vector3.left * speedMovement * Time.deltaTime);
		if (transform.position.y - heightZombie > 1.5f) {
			jumpEnough = true;
		}
	}

	void OnCollisionEnter(Collision other) {
//		bool otherIsChunk = false;
//		foreach(ContactPoint cp in other.contacts) {
//			otherIsChunk = otherIsChunk || cp.otherCollider.CompareTag("Chunk");
//		}
	}

	void OnCollisionStay(Collision other) {
		bool otherIsWall = false;
		foreach (ContactPoint cp in other.contacts) {
			Debug.Log (cp.normal.y);
			otherIsWall = otherIsWall || (cp.normal.y <= 0.5f && cp.normal.y >= -0.5f && cp.otherCollider.CompareTag("Chunk"));
		}
		if(!isJumping && otherIsWall) {
			isJumping = true;
			jumpEnough = false;
			heightZombie = transform.position.y;
		} else {
			isJumping = false;
			jumpEnough = false;
		}
	}

	//return the current life
	public void damage(float damage, Vector3 normalImpact) {
		if(!damaged) {
			life -= damage;
			rigidbody.AddForce(-normalImpact * 200.0f, ForceMode.Impulse);
			blood.enableEmission = true;
			blood.Play ();
			if(life <= 0.0f) died = true;
			damaged = true;
			damageTimer = 0.0f;
		}
	}

	private void die() {
		GameObject go = this.gameObject;
		Destroy (go);
	}

	private void doAnimationDamaged() {
		damageTimer += Time.deltaTime;
		if(damageTimer <= damageAnimationDuration && died) {
			transform.Rotate(Vector3.left, 90.0f*Time.deltaTime);
		} else {
			damaged = false;
			blood.enableEmission = false;
			blood.Stop();
			if(died) die ();
		}
	}
}
