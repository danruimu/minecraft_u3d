using UnityEngine;
using System.Collections;

public class MovementPlayer : MonoBehaviour {

	#region constant variables
	private const float durationDamaged = 1.0f;
	private const float timeToRecover = 5.0f;
	private const float deadDuration = 10.0f;
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

	public GUITexture blood;

	public AudioClip hurt;
	public AudioClip death;
	public AudioClip step;
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
	private bool delayedRecovering;

	private GUITexture bloodInst;
	private bool dead;
	private float timerDead;
	private Quaternion origRot;
	private Vector3 origPos;
	private Vector3 steveOrigPos;
	private Quaternion steveOrigRot;

	private float soundTimer;
	private AudioSource _hurt;
	private AudioSource _death;
	private AudioSource _step;
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
		delayedRecovering = false;

		bloodInst = (GUITexture) Instantiate (blood);
		Rect px = new Rect(0.0f, 0.0f, Screen.width, Screen.height);
		bloodInst.pixelInset = px;
		bloodInst.color = new Color(1.0f, 0.0f, 0.0f, 0.0f);

		origRot = steveEyes.transform.rotation;
		origPos = steveEyes.transform.position;

		steveOrigPos = transform.position;
		steveOrigRot = transform.rotation;

		dead = false;

		//sound
		_death = gameObject.AddComponent<AudioSource>();
		_death.playOnAwake = false;
		_death.clip = death;
		_death.loop = false;
		_death.Stop();
		
		_hurt = gameObject.AddComponent<AudioSource>();
		_hurt.playOnAwake = false;
		_hurt.clip = hurt;
		_hurt.loop = false;
		_hurt.Stop();
		
		_step = gameObject.AddComponent<AudioSource>();
		_step.playOnAwake = false;
		_step.clip = step;
		_step.loop = true;
		_step.Stop();
	}

	// Update is called once per frame
	void Update () {
		if(!dead && !gameObject.GetComponent<MouseClick>().getWorld().gameObject.GetComponent<PauseMenu>().isPaused()) {
		#region movement
			//Movement with WASD
			if(Input.GetKey(KeyCode.W) && !dir[(int)directions.UP]) {
				transform.Translate(Vector3.forward * speed * Time.deltaTime);
				if(!_step.isPlaying) _step.Play();
			}
			if(Input.GetKey(KeyCode.S) && !dir[(int)directions.DOWN]) {
				transform.Translate(Vector3.back * speed * Time.deltaTime);
				if(!_step.isPlaying) _step.Play();
			}
			if(Input.GetKey(KeyCode.A) && !dir[(int)directions.LEFT]) {
				transform.Translate(Vector3.left * speed * Time.deltaTime);
				if(!_step.isPlaying) _step.Play();
			}
			if(Input.GetKey(KeyCode.D) && !dir[(int)directions.RIGHT]) {
				transform.Translate(Vector3.right * speed * Time.deltaTime);
				if(!_step.isPlaying) _step.Play();
			}

			if(Input.GetKeyUp(KeyCode.W)) {
				if(_step.isPlaying) _step.Stop();
			}
			if(Input.GetKeyUp(KeyCode.A)) {
				if(_step.isPlaying) _step.Stop();
			}
			if(Input.GetKeyUp(KeyCode.S)) {
				if(_step.isPlaying) _step.Stop();
			}
			if(Input.GetKeyUp(KeyCode.D)) {
				if(_step.isPlaying) _step.Stop();
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
		} else if(gameObject.GetComponent<MouseClick>().getWorld().gameObject.GetComponent<PauseMenu>().isPaused() && _step.isPlaying){
			_step.Stop ();
		}

		#region life control
		if((damaged || recovering || delayedRecovering) && !dead) treatDamage();
		if(dead) {
			timerDead += Time.deltaTime;
			if(timerDead <= 1.0f) {
				steveEyes.transform.Rotate (steveEyes.transform.forward, 90.0f*Time.deltaTime);
				steveEyes.transform.Translate(1.5f*Vector3.down*Time.deltaTime);
			} else if(timerDead >= deadDuration) {
				respawn();
			} else {
				if(_death.isPlaying) _death.Stop();
			}

		}
		#endregion
	}

	void OnCollisionEnter(Collision other) {
		//detecting if we are colliding with the floor or not
		bool otherIsFloor = false;
		bool otherIsMOB = false;
		Vector3 normalMOB = new Vector3();
		bool otherIsWall = false;
		for (int i = 0; i<other.contacts.Length; ++i) {
			otherIsFloor = otherIsFloor || other.contacts[i].otherCollider.CompareTag("Chunk");
			otherIsMOB = otherIsMOB || other.contacts[i].otherCollider.CompareTag ("MOB");
			if(otherIsMOB) normalMOB = other.contacts[i].normal;
			otherIsWall = other.contacts[i].otherCollider.CompareTag("Chunk") && (otherIsWall || (other.contacts[i].normal.y >= -0.5f && other.contacts[i].normal.y <= 0.5f));
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
		if(otherIsMOB && !damaged) {
			transform.rigidbody.AddForce((Vector3.up + normalMOB) * 250.0f, ForceMode.Impulse);
			damaged = true;
			recovering = false;
			delayedRecovering = false;
			damageTimer = 0.0f;
			steveLife -= 1.0f;
			if(steveLife <= 0.0f) {
				dead = true;
				if(!_death.isPlaying) _death.Play ();
			} else {
				if(!_hurt.isPlaying) _hurt.Play();
			}
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
		if(damageTimer >= durationDamaged && damaged) {
			damaged = false;
			if(_hurt.isPlaying) _hurt.Stop();
			delayedRecovering = true;
			damageTimer = 0.0f;
		}
		if(damageTimer >= timeToRecover) {
			delayedRecovering = false;
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
			}
		}
		bloodInst.color = new Color(1.0f, 0.0f, 0.0f, 0.4f * (1.0f - steveLife/10.0f));
	}

	private void respawn() {
		transform.position = steveOrigPos;
		transform.rotation = steveOrigRot;
		steveEyes.transform.position = origPos;
		steveEyes.transform.rotation = origRot;

		dead = false;
		isGrounded = false;
		godMode = false;
		objectCollision = false;
		dir = new bool[4];
		dir[(int)directions.UP] = dir[(int)directions.LEFT] = dir[(int)directions.DOWN] = dir[(int)directions.RIGHT] = false;
		steveLife = 10.0f;
		damaged = false;
		recovering = false;
		delayedRecovering = false;
		bloodInst.color = new Color(1.0f, 0.0f, 0.0f, 0.4f * (1.0f - steveLife/10.0f));
	}
}