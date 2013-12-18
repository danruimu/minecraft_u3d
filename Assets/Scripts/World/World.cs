using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;

public enum ItemType{
	Stick=0,
	Torch=-1
}

public class World : MonoBehaviour {

	// Use this for initialization
	public GameObject chunkPrefab;
	public Material[] mats;
	public Texture[] texturesItem;
	public static Texture[] texts;
	public string seed;
	public static int sizex = 2;
	public static int sizez = 2;
	public const int numMaxMaterials = 256;

	private static int[][] indexsBlocks;
	private Chunk[,] chunks;
	private GameObject collNorth,collSouth,collWest,collEast;

	public GameObject steve;
	public GameObject zombie;

	private GameObject _steve;
	private ArrayList _zombies;

	private const int maxZombies = 10;

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
		texts = new Texture[mats.Length]; 
		for(int i=0;i<mats.Length;i++){
			texts[i] = mats[i].mainTexture;
			Debug.Log(texturesItem[0].name);
		}
	}

	private byte[] convert(byte[] data){
		byte[] res = new byte[data.Length/2];
		for(int i=0;i<data.Length;i+=2){
			byte alt = asciiToHex(data[i]);
			byte baix = asciiToHex(data[i+1]);
			res[i/2] = (byte)((alt<<4 | baix));
		}
		return res;
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
				heightmap = convert(File.ReadAllBytes("World/" + x + "_" + z + ".hm.milf"));
				data = convert(File.ReadAllBytes("World/" + x + "_" + z + ".b.milf"));
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

	public int getSizeX() {
		return sizex;
	}

	public int getSizeZ() {
		return sizez;
	}

	public byte getHeight(int x, int z){
		return chunks[x/Chunk.sizex,z/Chunk.sizez].getHeight(x%Chunk.sizex,z%Chunk.sizez);
	}

	private byte asciiToHex(byte input){
		if(input >= 'A' && input <= 'F') return (byte)(input - 'A' + 10);
		if(input >= 'a' && input <= 'f') return (byte)(input - 'a' + 10);
		if(input >= '0' && input <= '9') return (byte)(input - '0');
		throw new Exception("numero no hexadecimal");
	}

	void Start () {
		init();
		collSouth = new GameObject("Colider der sur");
		collSouth.transform.parent = transform;
		collNorth = new GameObject("ahi va collider!");
		collNorth.transform.parent = transform;
		collEast = new GameObject("col·lisionador de l'est");
		collEast.transform.parent = transform;
		collWest = new GameObject("collider de la vieira");
		collWest.transform.parent = transform;
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

		_steve = (GameObject) Instantiate (steve);
		_steve.GetComponent<MouseClick>().world = this;
		this.gameObject.GetComponent<CountingOfTime>().sky = _steve.GetComponent<MovementPlayer>().steveEyes;
		Vector3 pos;
		pos.x = UnityEngine.Random.Range (1f, Chunk.sizex * sizex);
		pos.z = UnityEngine.Random.Range (1f, Chunk.sizez * sizez);
		pos.y = getHeight(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.z));
		pos.y += 2.0f;
		_steve.transform.position = pos;
		InventoryManagment.texturesItem = texturesItem;

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

//		if(gameObject.GetComponent<CountingOfTime>().ThisIsNight()) {
//			if(!enoughZombiesPlease()) {
//				spawnZombie();
//			}
//		} else {
//			foreach(GameObject z in _zombies) {
//				if(z != null) {
//					//kill the zombie
//					z.GetComponent<IAZombie>().damage(10.0f, new Vector3(0f,0f,0f), new Vector3(0f,0f,0f));
//				}
//			}
//			if(_zombies.Count > 0) {
//				_zombies.Clear();
//			}
//		}
	}

	//Returns true if there are enough zombies at the scene (so another zombie cannot be spawn)
	private bool enoughZombiesPlease() {
		int zombiesNear = 0;
		foreach(GameObject z in _zombies) {
			if(z != null) {
				if(Vector3.Distance(z.transform.position, _steve.transform.position) < 20.0f) ++zombiesNear;
			}
		}
		if(zombiesNear >= maxZombies) return true; 	//maximum of 10 zombies near steve

		return _zombies.Count*sizex*sizez > maxZombies; //maximum of 10 zombies per chunk
	}

	private void spawnZombie() {
		float desX = UnityEngine.Random.Range (10.0f, 50.0f);
		float desZ = UnityEngine.Random.Range (10.0f, 50.0f);
		Vector3 pos;
		pos.x = _steve.transform.position.x + desX;
		pos.z = _steve.transform.position.z + desZ;
		if(pos.x <= 0.0f || pos.z <= 0.0f || pos.x >= (sizex * Chunk.sizex) || pos.z >= (sizez * Chunk.sizez)) return;
		pos.y = getHeight(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.z));
		pos.y += 2.0f;
		//only to be sure, but it's not needed
		if(Vector3.Distance(_steve.transform.position, pos) < 10.0f) return;	//zombie cannot be spawned too close to Steve
		foreach(GameObject torch in _steve.GetComponent<MouseClick>().torchs) {
			if(Vector3.Distance(torch.transform.position, pos) < 10.0f) return;	//zombie cannot be spawned near a Torch
		}
		
		GameObject z = (GameObject) Instantiate(zombie);
		z.GetComponent<IAZombie>().steve = _steve.transform;
		z.transform.position = pos;
		z.GetComponent<IAZombie>().setWorld(this);
		_zombies.Add (z);
	}

	public bool removeCube(int x,int y,int z){
		Vector3 pos = new Vector3(x,y,z);
		if(!posValida(pos))return false;
		Chunk c = getChunk(pos);
		pos = relativePos(pos);
		return c.removeCube((int)pos.x,(int)pos.y,(int)pos.z);
	}

	public bool addCube(int x,int y,int z,BlockType type){
		Vector3 pos = new Vector3(x,y,z);
		if(!posValida(pos))return false;
		Chunk c = getChunk(pos);
		pos = relativePos(pos);
		return c.newCube((int)pos.x,(int)pos.y,(int)pos.z,type);
	}

	public BlockType getBlockType(int x,int y,int z){
		Vector3 pos = new Vector3(x,y,z);
		if(!posValida(pos))return BlockType.Bedrock;//apaño pa' k no peteh(etxo por dni el reshulonako
		Chunk c = getChunk(pos);
		pos = relativePos(pos);
		return c.getBlockType((int)pos.x,(int)pos.y,(int)pos.z);
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

	public static Texture getTextureBlock(BlockType type){
		int []indexs = indexsBlocks[(int)type];
		if(indexs == null) throw new Exception("material " + type + " no dins del sistema");
		return texts[indexs[0]];
	}
}
