//using UnityEngine;
//using System.Collections;
//
//public class MouseClick : MonoBehaviour {
//
//	private static bool attack;
//	public static bool Attack { get { return attack; } }
//	private float time;
//
//	private const float attackSpeedRot = 10.0f;
//	private const float attackAngleRot = 60.0f;
//	private const float attackSpeed = 1.0f;
//
//	private Vector3 dirRot;
//	private Vector3 dirForw;
//	private Vector3 dirHori;
//	private Vector3 dirVert;
//
//	private Quaternion originalRotation;
//	private Vector3 originalPosition;
//
//	public ParticleSystem _grassDestroy;
//	public ParticleSystem _dirtyDestroy;
//	public ParticleSystem _stoneDestroy;
//	
//	private World _world;
//	
//	private MineBlock _selectedBlock;
//	private Vector3 _selectedBlockPosition;
//	private Vector3 _colisionPoint;
//	private BlockType _selectedBlockType;
//
//	MineBlock GetBlock(bool getNearestNeighbor = false)
//	{
//		Ray ray = Camera.main.ScreenPointToRay( new Vector2(Screen.width/2f, Screen.height/2f) );
//		RaycastHit hit = new RaycastHit();
//		
//		MineBlock block = new Block(BlockType.Unknown);
//		if (Physics.Raycast(ray, out hit, 10.0f) == true)
//		{
//			_colisionPoint = hit.point;
//			Vector3 hp = _colisionPoint + 0.0001f * ray.direction;
//			
//			int x = Mathf.CeilToInt(hp.x) - 1;
//			int y = Mathf.CeilToInt(hp.y) - 1;
//			int z = Mathf.CeilToInt(hp.z) - 1;
//			
//			_selectedBlockPosition = new Vector3(x,y,z);
//			
//			if (getNearestNeighbor == true)
//			{
//				#region GetNearestNeighbor
//				Vector3 nearestBlock = _colisionPoint - _selectedBlockPosition;
//				
//				if (nearestBlock.x == 1.0f)
//				{
//					x++;
//				}
//				else if (nearestBlock.x == 0.0f)
//				{
//					x--;
//				}
//				
//				if (nearestBlock.y == 1.0f)
//				{
//					y++;
//				}
//				else if (nearestBlock.y == 0.0f)
//				{
//					y--;
//				}
//				
//				if (nearestBlock.z == 1.0f)
//				{
//					z++;
//				}
//				else if (nearestBlock.z == 0.0f)
//				{
//					z--;
//				}
//				
//				_selectedBlockPosition.x = x;
//				_selectedBlockPosition.y = y;
//				_selectedBlockPosition.z = z;
//				
//				block = _world[x, y, z];
//				#endregion
//			}
//			else
//			{
//				block = _world[x, y, z];	
//			}
//		}		
//		
//		return block;
//	}
//
//	void Start() {
//		attack = false;
//		originalRotation = transform.localRotation;
//		originalPosition = transform.localPosition;
//	}
//
//	// Update is called once per frame
//	void Update () {
//		if(!attack){
//			if(Input.GetMouseButton(0)) {
//				attack = true;
//				time = 0.0f;
//				dirRot = Vector3.right;
//				dirForw = Vector3.forward;
//				dirHori = Vector3.left;
//				dirVert = Vector3.up;
//			}
//		}
//		if(attack && time <= 1.0f) {
//			transform.Rotate (dirRot, attackSpeedRot * Time.deltaTime * attackAngleRot);
//			transform.Translate(dirForw * attackSpeed * Time.deltaTime * 1.5f);
//			transform.Translate(dirHori * attackSpeed * Time.deltaTime * 2.0f);
//			transform.Translate(dirVert * attackSpeed * Time.deltaTime * 2.0f);
//			time += Time.deltaTime * attackSpeedRot;
//		} else if(attack && time > 1.0f && dirRot.Equals(Vector3.right)) {
//			transform.Rotate (dirRot, attackSpeedRot * Time.deltaTime);
//			time = 0.0f;
//			dirRot = Vector3.left;
//			dirForw = Vector3.back;
//			dirHori = Vector3.right;
//			dirVert = Vector3.down;
//		} else if(attack && time > 1.0f && dirRot.Equals(Vector3.left)) {
//			attack = false;
//			transform.localRotation = originalRotation;
//			transform.localPosition = originalPosition;
//		}
//
//		if(_world != null) {
//			_selectedBlock = GetBlock();
//		} else {
//			_world = WorldController.getWorld();
//		}
//
//		if(Input.GetMouseButton(0)) {
//			DestroyBlock(_selectedBlock);
//		}
//
//
//	}
//
//	void DestroyBlock(MineBlock selectedBlock)
//	{
//		if ( selectedBlock.IsSolid() && selectedBlock.Type != BlockType.Lava)
//		{
//			_selectedBlock = selectedBlock; //needed? _selectedBlock is the block passed as argument actually
//			
//			#region DestroyFx
//			ParticleSystem destroyParticle = null;
//
//			switch (_selectedBlock.Type)
//			{
//				case BlockType.Grass:
//					destroyParticle = _grassDestroy;
//					break;
//				case BlockType.Dirt:
//					destroyParticle = _dirtyDestroy;
//					break;
//				case BlockType.Stone:
//					destroyParticle = _stoneDestroy;
//					break;
//			}
//			
//			if (destroyParticle != null)
//			{
//				ParticleSystem particle = GameObject.Instantiate(destroyParticle) as ParticleSystem;
//
//				// Apply the same light of the block that will be destroyed into the particles
//				int x = Mathf.RoundToInt(_selectedBlockPosition.x);
//				int y = Mathf.RoundToInt(_selectedBlockPosition.y);
//				int z = Mathf.RoundToInt(_selectedBlockPosition.z);
//				MineBlock topBlock = _world[x, y+1, z];
//				float particleColor = ((float)topBlock.Light)/255.0f;
//				particle.renderer.material.color = new Color(particleColor, particleColor, particleColor);
//
//				Vector3 particlePos = _selectedBlockPosition;
//				particlePos.y += 0.75f;
//				particle.transform.position = particlePos;
//			}
//			#endregion
//			
//			_selectedBlock.Destroy();
//
//			StartCoroutine( _world.RefreshChunkMesh( new Vector3i(_selectedBlockPosition), false  ) );
//		}			
//	}
//
//
//}
