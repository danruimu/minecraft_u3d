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
	public Transform weapon;
	
	private Vector3 _selectedBlockPosition;
	private Vector3 _colisionPoint;

	void Start() {
		attack = false;
		originalRotation = weapon.localRotation;
		originalPosition = weapon.localPosition;
	}

	// Update is called once per frame
	void Update () {

		if(!attack){
			if(Input.GetMouseButton(0)) {
				attack = true;
				time = 0.0f;
				dirRot = Vector3.right;
				dirForw = Vector3.forward;
				dirHori = Vector3.left;
				dirVert = Vector3.up;

				hitWhatever();
			}
			if(Input.GetMouseButton (1)) {
				putCube();
			}
		}

		#region attack_animation
		if(attack && time <= 1.0f) { //pickaxe forward
			weapon.Rotate (dirRot, attackSpeedRot * Time.deltaTime * attackAngleRot);
			weapon.Translate(dirForw * attackSpeed * Time.deltaTime * 2.0f);
			weapon.Translate(dirHori * attackSpeed * Time.deltaTime * 3.0f);
			weapon.Translate(dirVert * attackSpeed * Time.deltaTime * 4.0f);
			time += Time.deltaTime * attackSpeedRot;
		} else if(attack && time > 1.0f && dirRot.Equals(Vector3.right)) { //pickaxe back
			weapon.Rotate (dirRot, attackSpeedRot * Time.deltaTime);
			time = 0.0f;
			dirRot = Vector3.left;
			dirForw = Vector3.back;
			dirHori = Vector3.right;
			dirVert = Vector3.down;
		} else if(attack && time > 1.0f && dirRot.Equals(Vector3.left)) {
			attack = false;
			weapon.localRotation = originalRotation;
			weapon.localPosition = originalPosition;
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

//				if(!World.newCube(x, y, z, BlockType.GoldOre)) {
//					Debug.LogError("Cannot add Cube at "+x+","+y+","+z);
//				}
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

					z.damage(1.0f, rhit.normal, rhit.point);
				}
			} else {
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

//				if(!World.removeCube(x, y, z)) {
//					Debug.LogError("Cannot remove Cube at "+x+","+y+","+z);
//				}
			}
		}
	}

}
