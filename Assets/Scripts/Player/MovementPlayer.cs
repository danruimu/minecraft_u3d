using UnityEngine;
using System.Collections;

public class MovementPlayer : MonoBehaviour {

	#region constant variables
	private const float durationDamaged = 1.0f;
	private const float timeToRecover = 5.0f;
	#endregion

	#region public variables
	public float speed = 5.0f;
	public float jumpForce = 5.0f;

	public Camera steveEyes;

	public enum directions {
		UP = 0,
		LEFT,
		DOWN,
		RIGHT,
	}
	#endregion

	#region private variables
	private bool isGrounded;
	private bool jumpEnough;
	private bool godMode;
	private bool objectCollision;

	private bool[] dir;
	private float height;

	private float steveLife;
	private bool damaged;
	private float damageTimer;
	private bool recovering;
	#endregion

	void Start() {
		isGrounded = false;
		godMode = false;
		objectCollision = false;
		dir = new bool[4];
		dir[(int)directions.UP] = dir[(int)directions.LEFT] = dir[(int)directions.DOWN] = dir[(int)directions.RIGHT] = false;
		steveLife = 10.0f;
		damaged = false;
		recovering = false;
	}

	// Update is called once per frame
	void Update () {
		#region movement
		//Movement with WASD
		if(Input.GetKey(KeyCode.W) && !dir[(int)directions.UP]) {
			transform.Translate(Vector3.forward * speed * Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.S) && !dir[(int)directions.DOWN]) {
			transform.Translate(Vector3.back * speed * Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.A) && !dir[(int)directions.LEFT]) {
			transform.Translate(Vector3.left * speed * Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.D) && !dir[(int)directions.RIGHT]) {
			transform.Translate(Vector3.right * speed * Time.deltaTime);
		}
		#endregion

		#region jump
		if(Input.GetKeyDown(KeyCode.Space) && isGrounded) {
			isGrounded = false;
			height = transform.position.y;
			transform.Translate(Vector3.up * jumpForce * Time.deltaTime);
			jumpEnough = false;
		}
		if(Input.GetKey(KeyCode.LeftShift) && godMode) {
			transform.Translate(Vector3.down * speed * Time.deltaTime);
		}


		if(!isGrounded && !jumpEnough) {
			transform.Translate(Vector3.up * jumpForce  * Time.deltaTime);
			if(transform.position.y - height > 1.2f ) {
				jumpEnough = true;
			}
		}
		#endregion

		if(damaged || recovering) treatDamage();
	}

	void OnCollisionEnter(Collision other) {
		//detecting if we are colliding with the floor or not
		bool otherIsFloor = false;
		bool otherIsMOB = false;
		Vector3 normalMOB = new Vector3();
		//bool otherIsWall = false;
		for (int i = 0; i<other.contacts.Length; ++i) {
			otherIsFloor = otherIsFloor || other.contacts[i].otherCollider.CompareTag("Chunk");
			otherIsMOB = otherIsMOB || other.contacts[i].otherCollider.CompareTag ("MOB");
			if(otherIsMOB) normalMOB = other.contacts[i].normal;
			//otherIsWall = otherIsFloor && (otherIsWall || Vector3.Cross(other.contacts[i].normal, Vector3.up).magnitude > threshold);
		}

		#region collision floor
		if(otherIsFloor) {
			for(int i = 0;i<other.contacts.Length && !isGrounded;i++){
				isGrounded = true;

				dir[(int)directions.UP] = false;
				dir[(int)directions.LEFT] = false;
				dir[(int)directions.DOWN] = false;
				dir[(int)directions.RIGHT] = false;
				objectCollision = false;
			}
		}
		#endregion

		#region collision wall
		//if(otherIsWall) {	TODO: return to the otherIsWall
		if(!otherIsFloor) {
			objectCollision = true;
			if(Input.GetKey(KeyCode.W)) {
				dir[(int)directions.UP] = true;
			}
			if(Input.GetKey(KeyCode.A)) {
				dir[(int)directions.LEFT] = true;
			}
			if(Input.GetKey(KeyCode.S)) {
				dir[(int)directions.DOWN] = true;
			}
			if(Input.GetKey(KeyCode.D)) {
				dir[(int)directions.RIGHT] = true;
			}
		}
		#endregion

		#region collision MOB
		if(otherIsMOB && !damaged) {
			transform.rigidbody.AddForce(normalMOB * 500.0f, ForceMode.Impulse);
			damaged = true;
			recovering = false;
			damageTimer = 0.0f;
			steveLife -= 1.0f;
		}
		#endregion
	}

	void OnCollisionExit(Collision other) {
		if(objectCollision) {
			dir[(int)directions.UP] = false;
			dir[(int)directions.LEFT] = false;
			dir[(int)directions.DOWN] = false;
			dir[(int)directions.RIGHT] = false;
			objectCollision = false;
		}
	}

	private void treatDamage() {
		damageTimer += Time.deltaTime;
		steveEyes.backgroundColor = Color.white * Color.red * (255.5f - (10.0f - steveLife));
		if(damageTimer >= durationDamaged && damaged) {
			damaged = false;
			damageTimer = 0.0f;
		}
		if(damageTimer >= timeToRecover) {
			recovering = true;
			damageTimer = 0.0f;
		}
		if(recovering) {
			if(damageTimer >= 1.0f) {
				steveLife += 0.5f;
				damageTimer = 0.0f;
			}
			if(steveLife >= 10.0f) {
				recovering = false;
				steveEyes.backgroundColor = Color.white;
			}
		}
	}
}