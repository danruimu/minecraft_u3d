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
	
	private Vector3 _selectedBlockPosition;
	private Vector3 _colisionPoint;

	void Start() {
		attack = false;
		originalRotation = transform.localRotation;
		originalPosition = transform.localPosition;
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
			transform.Rotate (dirRot, attackSpeedRot * Time.deltaTime * attackAngleRot);
			transform.Translate(dirForw * attackSpeed * Time.deltaTime * 2.0f);
			transform.Translate(dirHori * attackSpeed * Time.deltaTime * 3.0f);
			transform.Translate(dirVert * attackSpeed * Time.deltaTime * 4.0f);
			time += Time.deltaTime * attackSpeedRot;
		} else if(attack && time > 1.0f && dirRot.Equals(Vector3.right)) { //pickaxe back
			transform.Rotate (dirRot, attackSpeedRot * Time.deltaTime);
			time = 0.0f;
			dirRot = Vector3.left;
			dirForw = Vector3.back;
			dirHori = Vector3.right;
			dirVert = Vector3.down;
		} else if(attack && time > 1.0f && dirRot.Equals(Vector3.left)) {
			attack = false;
			transform.localRotation = originalRotation;
			transform.localPosition = originalPosition;
		}
		#endregion
	}

	private void putCube() {
		Ray ray = new Ray(center.position, transform.forward);
		RaycastHit rhit = new RaycastHit();

		if(Physics.Raycast (ray, out rhit, 5.0f)) {
			if(!rhit.collider.CompareTag("MOB")) {
				Chunk c = rhit.collider.GetComponent<Chunk>();
				Vector3 cubePos = rhit.point;
				int x = Mathf.FloorToInt(cubePos.x);
				int y = Mathf.FloorToInt(cubePos.y);
				int z = Mathf.FloorToInt(cubePos.z);
				x %= Chunk.sizex;
				z %= Chunk.sizez;

				if(!c.newCube(x, y, z, BlockType.Dirt)) {
					Debug.LogError("Cannot remove Cube at "+x+","+y+","+z);
				}
			}
		}
	}

	private void hitWhatever() {
		Ray ray = new Ray(center.position, transform.forward);
		RaycastHit rhit = new RaycastHit();

		if(Physics.Raycast (ray, out rhit, 5.0f)) {
			if(rhit.collider.CompareTag("MOB")) {
				if(rhit.distance < 2.0f) {
					IAZombie z = rhit.collider.GetComponent<IAZombie>();

					z.damage(1.0f, rhit.normal);
				}
			} else {
				Chunk c = rhit.collider.GetComponent<Chunk>();
				Vector3 cubePos = rhit.point;
				int x = Mathf.FloorToInt(cubePos.x);
				int y = Mathf.FloorToInt(cubePos.y);
				int z = Mathf.FloorToInt(cubePos.z);
				x %= Chunk.sizex;
				--y;
				z %= Chunk.sizez;
				if(!c.removeCube(x, y, z)) {
					Debug.LogError("Cannot remove Cube at "+x+","+y+","+z);
				}

			}
		}
	}

}
