using UnityEngine;
using System.Collections;

public class MovementPlayer : MonoBehaviour {
	private float threshold = 0.1f;
	public float speed = 5.0f;
	public float jumpForce = 3000.0f;

	private bool isGrounded;

	void Start() {
		isGrounded = true;
	}

	// Update is called once per frame
	void Update () {
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
		if(Input.GetKeyDown(KeyCode.Space) && isGrounded) {
			isGrounded = false;
			rigidbody.AddForce (Vector3.up * jumpForce, ForceMode.Impulse);
		}
		if(Input.GetKey(KeyCode.LeftShift)) {
			transform.Translate(Vector3.down * speed * Time.deltaTime);
		}
	}

	void OnCollisionEnter(Collision other) {
		for(int i = 0;i<other.contacts.Length && !isGrounded;i++){
			if(Vector3.Cross(other.contacts[i].normal, Vector3.up).magnitude < threshold) {
				isGrounded = true;
			}
		}
		Debug.Log(isGrounded);
	}

}
