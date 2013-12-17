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

	public AudioClip destroyGrass;
	public AudioClip destroyCloth;
	public AudioClip destroyGravel;
	public AudioClip destroySand;
	public AudioClip destroySnow;
	public AudioClip destroyStone;
	public AudioClip destroyWood;

	private AudioSource _destroyGrass;
	private AudioSource _destroyCloth;
	private AudioSource _destroyGravel;
	private AudioSource _destroySand;
	private AudioSource _destroySnow;
	private AudioSource _destroyStone;
	private AudioSource _destroyWood;

	private bool damagingBlock;
	private int damageDoneToBlock;

	public GameObject destroyPlane;
	private GameObject _destroyPlane;

	public Material[] destroyStages;

	private int lastX;
	private int lastY;
	private int lastZ;

	//public 

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
		//sound
		_destroyGrass = gameObject.AddComponent<AudioSource>();
		_destroyGrass.playOnAwake = false;
		_destroyGrass.loop = false;
		_destroyGrass.clip = destroyGrass;

		_destroyCloth = gameObject.AddComponent<AudioSource>();
		_destroyCloth.playOnAwake = false;
		_destroyCloth.loop = false;
		_destroyCloth.clip = destroyCloth;

		_destroyGravel = gameObject.AddComponent<AudioSource>();
		_destroyGravel.playOnAwake = false;
		_destroyGravel.loop = false;
		_destroyGravel.clip = destroyGravel;

		_destroySand = gameObject.AddComponent<AudioSource>();
		_destroySand.playOnAwake = false;
		_destroySand.loop = false;
		_destroySand.clip = destroySand;

		_destroySnow = gameObject.AddComponent<AudioSource>();
		_destroySnow.playOnAwake = false;
		_destroySnow.loop = false;
		_destroySnow.clip = destroySnow;

		_destroyStone = gameObject.AddComponent<AudioSource>();
		_destroyStone.playOnAwake = false;
		_destroyStone.loop = false;
		_destroyStone.clip = destroyStone;

		_destroyWood = gameObject.AddComponent<AudioSource>();
		_destroyWood.playOnAwake = false;
		_destroyWood.loop = false;
		_destroyWood.clip = destroyWood;
		//endsound

		attack = false;

		if(!changeWeapon(0)) {
			Debug.LogError("Cannot change to Weapon "+weapons[0]);
		}

		damagingBlock = false;

	}

	// Update is called once per frame
	void Update () {
		if(!attack){
			if(Input.GetMouseButton(0) && currentWeapon != null && !world.GetComponent<PauseMenu>().isPaused()) {
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
				if(currentWeapon.CompareTag("Sword") )changeWeapon(0);
				else changeWeapon(1);
			}
		}
		if(Input.GetMouseButtonUp(0) && damagingBlock) {
			damagingBlock = false;
			Destroy (_destroyPlane);
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

				Vector3 posPlane = new Vector3(Mathf.Floor (cubePos.x), Mathf.Floor (cubePos.y), Mathf.Floor (cubePos.z));
				Vector3 eulerRotPlane = new Vector3(0.0f,0.0f,0.0f);

				if(normal.y >= 1.0f || normal.y <= -1.0f) {
					posPlane.x += 0.5f;
					posPlane.z += 0.5f;
					if(normal.y <= -1.0f) {
						eulerRotPlane.x = 180.0f;
						posPlane.y -= 0.01f;
					} else {
						posPlane.y += 0.01f;
					}

				} else if(normal.x >= 1.0f || normal.x <= -1.0f) {
					posPlane.y += 0.5f;
					posPlane.z += 0.5f;
					eulerRotPlane.z = 90.0f;
					if(normal.x >= 1.0f) {
						eulerRotPlane.z = 270.0f;
						posPlane.x += 0.01f;
					} else {
						posPlane.x -= 0.01f;
					}
				} else if(normal.z >= 1.0f || normal.z <= -1.0f) {
					posPlane.y += 0.5f;
					posPlane.x += 0.5f;
					eulerRotPlane.x = 90.0f;
					if(normal.z <= -1.0f) {
						eulerRotPlane.x = 270.0f;
						posPlane.z -= 0.01f;
					} else {
						posPlane.z += 0.01f;
					}
				}

				if(normal.y >= 1.0f) {
					y -= 1;
				} else if(normal.x >= 1.0f) {
					x -= 1;
				} else if(normal.z >= 1.0f) {
					z -= 1;
				}

				BlockType bt = world.getBlockType(x,y,z);

				if(lastX != x || lastY != y || lastZ != z) {
					damagingBlock = false;
				}

				if(bt != BlockType.Bedrock) {
					if(!damagingBlock) {
						damagingBlock  = true;
						damageDoneToBlock = 0;
						if(_destroyPlane != null) Destroy (_destroyPlane);
						_destroyPlane = (GameObject) Instantiate(destroyPlane);
						_destroyPlane.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
						_destroyPlane.transform.position = posPlane;
						_destroyPlane.transform.eulerAngles = eulerRotPlane;
						_destroyPlane.renderer.material = destroyStages[damageDoneToBlock];

						lastX = x;
						lastY = y;
						lastZ = z;
					} else {
						_destroyPlane.renderer.material = destroyStages[damageDoneToBlock];
						++damageDoneToBlock;
					}

					switch(bt) {
					case BlockType.Clay:
						_destroyGrass.Play ();
						break;
					case BlockType.CoalOre:
						_destroyStone.Play ();
						break;
					case BlockType.DiamondOre:
						_destroyStone.Play ();
						break;
					case BlockType.Dirt:
						_destroyGrass.Play ();
						break;
					case BlockType.GoldOre:
						_destroyStone.Play ();
						break;
					case BlockType.Grass:
						_destroyGrass.Play ();
						break;
					case BlockType.Gravel:
						_destroyGravel.Play ();
						break;
					case BlockType.IronOre:
						_destroyStone.Play ();
						break;
					case BlockType.RedstoneOre:
						_destroyStone.Play ();
						break;
					case BlockType.Sand:
						_destroySand.Play();
						break;
					case BlockType.LapisOre:
						_destroyStone.Play ();
						break;
					case BlockType.Stone:
						_destroyStone.Play ();
						break;
					}
				}

				if(damageDoneToBlock > 3) {
					if(!world.removeCube(x, y, z)) {
						Debug.LogError("Cannot remove Cube at "+x+","+y+","+z);
					} else {
						damagingBlock = false;
						damageDoneToBlock = 0;

						Destroy(_destroyPlane);
					}
				}
			}
		}
	}

	public World getWorld() {
		return world;
	}

}
