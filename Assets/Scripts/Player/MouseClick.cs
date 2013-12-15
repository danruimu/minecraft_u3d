using UnityEngine;
using System.Collections;

public class MouseClick : MonoBehaviour {

	private static bool attack;
	public static bool Attack { get { return attack; } }
	private float time;

	private const float attackSpeedRot = 10.0f;
	private const float attackAngleRot = 60.0f;
	private const float attackSpeed = 1.0f;

	private Vector3 dirRot;
	private Vector3 dirForw;
	private Vector3 dirHori;
	private Vector3 dirVert;

	private Quaternion originalRotation;
	private Vector3 originalPosition;

	public Transform center;
	public GameObject[] weapons;

	private GameObject currentWeapon;
	
	private Vector3 _selectedBlockPosition;
	private Vector3 _colisionPoint;

	public World world;

	public AudioClip destroyBlock;
	private AudioSource _destroyBlock;

	public void shitTheWeapon() {
		if(currentWeapon != null) {
			Destroy (currentWeapon);
		}
		currentWeapon = null;
	}

	public bool changeWeapon(int id) {
		if(id >= weapons.Length || id < 0) return false;
		if(currentWeapon != null) {
			Destroy (currentWeapon);
		}
		currentWeapon = (GameObject) Instantiate(weapons[id]);
		currentWeapon.transform.parent = center;
		switch(id) {
		case 0:
			currentWeapon.transform.localPosition = new Vector3(0.445755f, -0.2155447f, 0.4476013f);
			originalRotation.eulerAngles = new Vector3(15f, 0f, 345f);
			currentWeapon .transform.localRotation = originalRotation;
			break;
		case 1:
			currentWeapon.transform.localPosition = new Vector3(0.445755f, -0.2155447f, 0.4476013f);
			originalRotation.eulerAngles = new Vector3(15f, 0f, 345f);
			currentWeapon .transform.localRotation = originalRotation;
			break;
		}

		originalRotation = currentWeapon.transform.localRotation;
		originalPosition = currentWeapon.transform.localPosition;
		return true;

	}

	void Start() {
		_destroyBlock = gameObject.AddComponent<AudioSource>();
		_destroyBlock.playOnAwake = false;
		_destroyBlock.loop = false;
		_destroyBlock.clip = destroyBlock;

		attack = false;

		if(!changeWeapon(0)) {
			Debug.LogError("Cannot change to Weapon "+weapons[0]);
		}
	}

	// Update is called once per frame
	void Update () {

		if(!attack){
			if(Input.GetMouseButton(0) && currentWeapon != null) {
				attack = true;
				time = 0.0f;
				dirRot = Vector3.right;
				dirForw = Vector3.forward;
				dirHori = Vector3.left;
				dirVert = Vector3.up;

				hitWhatever();
			}
			if(Input.GetMouseButtonDown (1)) {
				putCube();
			}

			if(Input.GetKeyDown(KeyCode.Q)) {
				changeWeapon(0);
			}
			if(Input.GetKeyDown(KeyCode.Z)) {
				changeWeapon(1);
			}

			if(Input.GetKeyDown (KeyCode.F)) {
				shitTheWeapon();
			}
		}

		#region attack_animation
		if(currentWeapon != null) {
			if(attack && time <= 1.0f) { //pickaxe forward
				currentWeapon.transform.Rotate (dirRot, attackSpeedRot * Time.deltaTime * attackAngleRot);
				currentWeapon.transform.Translate(dirForw * attackSpeed * Time.deltaTime * 2.0f);
				currentWeapon.transform.Translate(dirHori * attackSpeed * Time.deltaTime * 3.0f);
				currentWeapon.transform.Translate(dirVert * attackSpeed * Time.deltaTime * 4.0f);
				time += Time.deltaTime * attackSpeedRot;
			} else if(attack && time > 1.0f && dirRot.Equals(Vector3.right)) { //pickaxe back
				currentWeapon.transform.Rotate (dirRot, attackSpeedRot * Time.deltaTime);
				time = 0.0f;
				dirRot = Vector3.left;
				dirForw = Vector3.back;
				dirHori = Vector3.right;
				dirVert = Vector3.down;
			} else if(attack && time > 1.0f && dirRot.Equals(Vector3.left)) {
				attack = false;
				currentWeapon.transform.localRotation = originalRotation;
				currentWeapon.transform.localPosition = originalPosition;
			}
		}
		#endregion
	}

	private void putCube() {
		Ray ray = new Ray(center.position, center.forward);
		RaycastHit rhit = new RaycastHit();

		if(Physics.Raycast (ray, out rhit, 5.0f)) {
			if(!rhit.collider.CompareTag("MOB")) {
				Vector3 cubePos = rhit.point;
				Vector3 normal = rhit.normal;

				//x,y,z depend on the normal of the face
				int x = Mathf.FloorToInt(cubePos.x);
				int y = Mathf.FloorToInt(cubePos.y);
				int z = Mathf.FloorToInt(cubePos.z);

				if(normal.y <= -1.0f) {
					y -= 1;
				} else if(normal.x <= -1.0f) {
					x -= 1;
				} else if(normal.z <= -1.0f) {
					z -= 1	;
				}

				BlockType bt;
				if(gameObject.GetComponent<InventoryManagment>().getItem(out bt)) {
					if(!world.addCube(x, y, z, bt)) {
						Debug.LogError("Cannot add Cube at "+x+","+y+","+z);
					}
				}
			}
		}
	}

	private void hitWhatever() {
		Ray ray = new Ray(center.position, center.forward);
		RaycastHit rhit = new RaycastHit();

		if(Physics.Raycast (ray, out rhit, 5.0f)) {
			if(rhit.collider.CompareTag("MOB")) {
				if(rhit.distance < 2.0f) {
					IAZombie z = rhit.collider.GetComponent<IAZombie>();

					float damage;
					if(currentWeapon.CompareTag("Sword")) {
						damage = 1.0f;
					} else {
						damage = 0.25f;
					}

					z.damage(damage, rhit.normal, rhit.point);
				}
			} else if(currentWeapon.CompareTag("PickAxe")) {
				Vector3 cubePos = rhit.point;
				Vector3 normal = rhit.normal;
				int x, y, z;

				x = Mathf.FloorToInt(cubePos.x);
				y = Mathf.FloorToInt(cubePos.y);
				z = Mathf.FloorToInt(cubePos.z);

				if(normal.y >= 1.0f) {
					y -= 1;
				} else if(normal.x >= 1.0f) {
					x -= 1;
				} else if(normal.z >= 1.0f) {
					z -= 1;
				}

				if(!world.removeCube(x, y, z)) {
					Debug.LogError("Cannot remove Cube at "+x+","+y+","+z);
				} else {

				}
			}
		}
	}

}
