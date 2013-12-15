using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;

public class World : MonoBehaviour {

	// Use this for initialization
	public GameObject chunkPrefab;
	public Material[] mats;
	public string seed;
	public static int sizex = 1;
	public static int sizez = 1;
	public const int numMaxMaterials = 256;

	private static int[][] indexsBlocks;
	private Chunk[,] chunks;
	private GameObject collNorth,collSouth,collWest,collEast;

	public GameObject steve;
	public GameObject zombie;

	private GameObject _steve;
	private ArrayList _zombies;

	private const int maxZombies = 100;

//	private int xa,ya,za;
//	private int xd,yd,zd;


	private void setMaterialIndexs(){
		BlockType[] types = (BlockType[])Enum.GetValues (typeof(BlockType));
		string [] noms = Enum.GetNames (typeof(BlockType));
		indexsBlocks = new int[numMaxMaterials][];
		for(int i = 0;i<noms.Length;i++) {
			string typeName = noms[i];
			int indexBase = 0;
			while(indexBase<mats.Length && mats[indexBase].name.CompareTo(typeName) != 0)indexBase++;
			if(indexBase>=mats.Length) throw new Exception("material no dins del sistema");
			indexsBlocks[(int)types[i]] = new int[6];
			for(int j = 0;j<6;j++){
				indexsBlocks[(int)types[i]][j] = indexBase;
			}
			string [] especials = null;
			switch(noms[i]){
			case "Grass":
				especials = new string[6];
				especials[4] = "Grass_top";
				especials[5] = "Dirt";
				break;
			}
			if (especials != null){
				for(int j = 0;j<6;j++){
					if(especials[j] != null){
						indexBase = 0;
						while(indexBase<mats.Length && mats[indexBase].name.CompareTo(especials[j]) != 0)indexBase++;
						if(indexBase>=mats.Length) throw new Exception("material no dins del sistema");
						indexsBlocks[(int)types[i]][j] = indexBase;
					}
				}
			}
		}
	}

	private void init() {
		DateTime tiempo1 = DateTime.Now;
		
		setMaterialIndexs();

		GameObject GO;
		Chunk c;
		chunks = new Chunk[sizex,sizez];
		//creo chunks

		byte[] heightmap;
		byte[] data;
		for (int x=0;x<sizex;x++){
			for(int z=0;z<sizez;z++){
				GO = (GameObject)Instantiate(chunkPrefab);
				GO.name = "Chunk[" + x + ","+z+"]";
				GO.transform.parent = transform;
				c = GO.GetComponent<Chunk>();
				c.init(new Vector3(x*Chunk.sizex,0,z*Chunk.sizez),this,mats);
				heightmap = File.ReadAllBytes("World/h" + x + "," + z);
				data = File.ReadAllBytes("World/d" + x + "," + z);
				c.fillColums(heightmap,data);
				chunks[x,z] = c;
			}
		}
		//pinto chunks
		for (int x=0;x<sizez;x++){
			for(int z=0;z<sizez;z++){
				chunks[x,z].ompleMesh();
			}
		}
		Debug.Log("creacion " + sizex + " * " + sizez + " Chunks -> tiempoTotal = " + new TimeSpan(DateTime.Now.Ticks - tiempo1.Ticks).ToString());
	}

	void Start () {
		init();
		collSouth = new GameObject("Colider der sur");
		collNorth = new GameObject("ahi va collider!");
		collEast = new GameObject("col·lisionador de l'est");
		collWest = new GameObject("collider de la vieira");
		BoxCollider bc = collSouth.AddComponent<BoxCollider>();
		bc.size = new Vector3(sizex*Chunk.sizex,Chunk.sizey,1f);
		bc.center = new Vector3(sizex*Chunk.sizex/2f,Chunk.sizey/2f,-0.5f);

		bc = collNorth.AddComponent<BoxCollider>();
		bc.size = new Vector3(sizex*Chunk.sizex,Chunk.sizey,1f);
		bc.center = new Vector3(sizex*Chunk.sizex/2f,Chunk.sizey/2f,sizez*Chunk.sizez+0.5f);

		bc = collEast.AddComponent<BoxCollider>();
		bc.size = new Vector3(1f,Chunk.sizey,sizez*Chunk.sizez);
		bc.center = new Vector3(sizex*Chunk.sizex+0.5f,Chunk.sizey/2f,sizez*Chunk.sizez/2.0f);
		
		bc = collWest.AddComponent<BoxCollider>();
		bc.size = new Vector3(1f,Chunk.sizey,sizez*Chunk.sizez);
		bc.center = new Vector3(-0.5f,Chunk.sizey/2f,sizez*Chunk.sizez/2.0f);
//		xa=xd=za=zd=0;
//		yd=0;
//		ya=70;

		_steve = (GameObject) Instantiate (steve);
		_steve.GetComponent<MouseClick>().world = this;
		this.gameObject.GetComponent<CountingOfTime>().sky = _steve.GetComponent<MovementPlayer>().steveEyes;

		_zombies = new ArrayList();
	}
	
	// Update is called once per frame
	void Update () {
//		Chunk c = chunks[0,0];
//		if(Input.GetKey(KeyCode.A)){
//			if(za < Chunk.sizez && c.newCube(xa,ya,za,BlockType.Bedrock)){
////				Debug.Log("success");
//				xa++;
//				if(xa >=Chunk.sizex){
//					xa=0;
//					za++;
//				}
//			}
////			else Debug.Log("failuer");
//		}

		if(this.gameObject.GetComponent<CountingOfTime>().ThisIsNight()) {
			if(!enoughZombiesPlease()) {
				spawnZombie();
			}
		} else {
			foreach(GameObject z in _zombies) {
				z.GetComponent<IAZombie>().damage(10.0f, new Vector3(0f,0f,0f), new Vector3(0f,0f,0f));
			}
		}
	}

	private bool enoughZombiesPlease() {
		return _zombies.Count > maxZombies;
	}

	private void spawnZombie() {
		GameObject z = (GameObject) Instantiate(zombie);
		z.GetComponent<IAZombie>().steve = _steve.transform;
		Vector3 pos;
		pos.x = UnityEngine.Random.Range (1f, Chunk.sizex * sizex);
		pos.y = 128.0f;
		pos.z = UnityEngine.Random.Range (1f, Chunk.sizex * sizex);
		z.transform.position = pos;
		_zombies.Add (z);
	}

	public bool removeCube(int x,int y,int z){
		Vector3 pos = new Vector3(x,y,z);
		if(!posValida(pos))return false;
		pos = relativePos(pos);
		return getChunk(pos).removeCube((int)pos.x,(int)pos.y,(int)pos.z);
	}

	public bool addCube(int x,int y,int z,BlockType type){
		Vector3 pos = new Vector3(x,y,z);
		if(!posValida(pos))return false;
		pos = relativePos(pos);
		return getChunk(pos).newCube((int)pos.x,(int)pos.y,(int)pos.z,type);
	}

	public BlockType getBlockType(int x,int y,int z){
		Vector3 pos = new Vector3(x,y,z);
		if(!posValida(pos))return BlockType.Bedrock;//apaño pa' k no peteh(etxo por dni el reshulonako
		pos = relativePos(pos);
		return getChunk(pos).getBlockType((int)pos.x,(int)pos.y,(int)pos.z);
	}
	
	private Vector3 relativePos(Vector3 pos){
		return new Vector3(((int)pos.x)%Chunk.sizex,(int)pos.y,((int)pos.z)%Chunk.sizez);
	}

	private Chunk getChunk(Vector3 p){
		return chunks[(int)(p.x/Chunk.sizex),(int)(p.z/Chunk.sizez)];
	}

	public bool existsCube(Vector3 pos){
		//limites absolutos
		if(pos.x < 0 ||pos.y < 0 || pos.z < 0)return true;
		if(pos.x >= sizex*Chunk.sizex || pos.y >= Chunk.sizey ||pos.z >= sizez*Chunk.sizez)return true;
		return getChunk(pos).existsCube(relativePos(pos));
	}

	private bool posValida(Vector3 pos){
		if(pos.x < 0 ||pos.y < 0 || pos.z < 0)return false;
		if(pos.x >= sizex*Chunk.sizex || pos.y >= Chunk.sizey ||pos.z >= sizez*Chunk.sizez)return false;
		return true;
	}

	public void delFace(Vector3 pos,faceType face){
		if(posValida(pos))
			getChunk(pos).delFace(relativePos(pos),face);
	}

	public bool addFace(Vector3 pos,faceType face){
		if(posValida(pos))return getChunk(pos).addFace(relativePos(pos),face);
		return false;
	}
	
	public static int[] getMatPointerArray(BlockType type){
		if(indexsBlocks[(int)type] == null) throw new Exception("material " + type + " no dins del sistema");
		return indexsBlocks[(int)type];
	}
}
