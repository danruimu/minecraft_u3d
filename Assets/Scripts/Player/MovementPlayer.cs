using UnityEngine;
using System.Collections;

public class MovementPlayer : MonoBehaviour {
	private float threshold = 0.2f;
	public float speed = 5.0f;
	public float jumpForce = 5.0f;

	private bool isGrounded;
	private bool jumpEnough;
	private bool godMode;
	private bool objectCollision;

	private bool[] dir;

	public enum directions {
		UP = 0,
		LEFT,
		DOWN,
		RIGHT,
	}

	private float height;

	void Start() {
		isGrounded = false;
		godMode = false;
		objectCollision = false;
		dir = new bool[4];
		dir[(int)directions.UP] = dir[(int)directions.LEFT] = dir[(int)directions.DOWN] = dir[(int)directions.RIGHT] = false;
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

		//TODO: revisar
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
	}

	void OnCollisionEnter(Collision other) {
		//detecting if we are colliding with the floor or not
		bool otherIsFloor = false;
		bool otherIsMOB = false;
		bool otherIsWall = false;
		for (int i = 0; i<other.contacts.Length; ++i) {
			otherIsFloor = otherIsFloor || other.contacts[i].otherCollider.CompareTag("Chunk");
			otherIsMOB = otherIsMOB || other.contacts[i].otherCollider.CompareTag ("MOB");
			otherIsWall = otherIsFloor && (otherIsWall || Vector3.Cross(other.contacts[i].normal, Vector3.up).magnitude > threshold);
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
		if(otherIsWall) {
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
		if(otherIsMOB) {
			transform.rigidbody.AddForce(Vector3.left * 500.0f, ForceMode.Impulse);
			//TODO: vidas steve
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
}