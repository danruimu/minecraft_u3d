using UnityEngine;
using System.Collections;

public class MovementPlayer : MonoBehaviour {
	private float threshold = 0.1f;
	public float speed = 5.0f;
	public float jumpForce = 5.0f;

	private bool isGrounded;
	private bool jumpEnough;
	private bool godMode;

	private float height;

	void Start() {
		isGrounded = true;
		godMode = false;
	}

	// Update is called once per frame
	void Update () {
		#region movement
		//Movement with WASD
		if(Input.GetKey(KeyCode.W)) {
			transform.Translate(Vector3.forward * speed * Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.S)) {
			transform.Translate(Vector3.back * speed * Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.A)) {
			transform.Translate(Vector3.left * speed * Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.D)) {
			transform.Translate(Vector3.right * speed * Time.deltaTime);
		}
		#endregion

		#region jump
		if(Input.GetKeyDown(KeyCode.Space) && isGrounded) {
			isGrounded = false;
			//rigidbody.AddForce (Vector3.up * jumpForce, ForceMode.Impulse);
			height = transform.position.y;
			transform.Translate(Vector3.up * jumpForce * Time.deltaTime);
			jumpEnough = false;
		}
		if(Input.GetKey(KeyCode.LeftShift) && godMode) {
			transform.Translate(Vector3.down * speed * Time.deltaTime);
		}
		if(!isGrounded && !jumpEnough) {
			transform.Translate(Vector3.up * jumpForce  * Time.deltaTime);
			if(transform.position.y - height > 1.0f ) {
				jumpEnough = true;
			}
		}
		#endregion
	}

	void OnCollisionEnter(Collision other) {
		for(int i = 0;i<other.contacts.Length && !isGrounded;i++){
			if(Vector3.Cross(other.contacts[i].normal, Vector3.up).magnitude < threshold) {
				isGrounded = true;
				Debug.Log ("Grounded!");
			}
		}
	}
}
