﻿using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;


public class World : MonoBehaviour {

	// Use this for initialization
	public GameObject chunkPrefab;
	public Material[] mats;
	public Texture[] texturesItem;
	public static Texture[] texts;
	public string seed;
	public static int sizex = 12;
	public static int sizez = 12;
	public const int numMaxMaterials = 256;

	private static int[][] indexsBlocks;
	private Chunk[,] chunks;
	private GameObject collNorth,collSouth,collWest,collEast;

	public GameObject steve;
	public GameObject zombie;

	private GameObject _steve;
	private ArrayList _zombies;

	private int maxZombies;
	private int zombiesDead;

	private Rect infoPlayer;
	private GUI.WindowFunction theInfoPlayer;

	private int day,month,year;

	public Texture textureForeground;
	private float progress;
	private bool started = false;
	private DateTime tiempo1;
	
	public void addZombies(int cant) {
		maxZombies += cant;
	}

	private void setMaterialIndexs(){
		BlockType[] types = (BlockType[])Enum.GetValues (typeof(BlockType));
		string [] noms = Enum.GetNames (typeof(BlockType));
		indexsBlocks = new int[numMaxMaterials][];
		for(int i = 0;i<noms.Length;i++) {
			string typeName = noms[i];
			int indexBase = 0;
			while(indexBase<mats.Length && mats[indexBase].name.CompareTo(typeName) != 0)indexBase++;
			if(indexBase>=mats.Length) {
				throw new Exception("material no dins del sistema");
			}
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
			
			case "Wood":
				especials = new string[6];
				especials[4] = "Wood_top";
				especials[5] = "Wood_top";
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

	void OnGUI() {
		if(started){
			Time.timeScale = 0f;
			GUI.DrawTexture(new Rect(0,0,((float)Screen.width)*progress,20),textureForeground);
			GUI.Label(new Rect(Screen.width/2f,0,150,20),"Loading Level... "+((int)(progress*100f))+"%");
		} else{
			Time.timeScale = 1f;
			GUI.Window(2, infoPlayer, theInfo, "SCORE");
		}
	}

	private IEnumerator init() {

		
		setMaterialIndexs();

		GameObject GO;
		Chunk c;
		chunks = new Chunk[sizex,sizez];
		//creo chunks

		byte[] heightmap;
		byte[] data;
		for (int z=0;z<sizex;z++){
			for(int x=0;x<sizez;x++){
				GO = (GameObject)Instantiate(chunkPrefab);
				GO.name = "Chunk[" + x + ","+z+"]";
				GO.transform.parent = transform;
				c = GO.GetComponent<Chunk>();
				c.init(new Vector3(x*Chunk.sizex,0,z*Chunk.sizez),this,mats);
				heightmap = convert(File.ReadAllBytes("milf_final/" + x + "_" + z + ".hm.milf"));
				data = convert(File.ReadAllBytes("milf_final/" + x + "_" + z + ".b.milf"));
				c.fillColums(heightmap,data);
				chunks[x,z] = c;

			}
		}
		yield return new WaitForEndOfFrame();
		_steve.GetComponent<MovementPlayer>().enabled = false;
		_steve.GetComponent<MouseClick>().enabled = false;
		
		
		//pinto chunks
		for (int x=0;x<sizez;x++){
			for(int z=0;z<sizez;z++){
				chunks[x,z].ompleMesh(50);
				progress = ((float)(x*sizex + z))/((float)(sizez*sizex));
				started = true;
				yield return new WaitForEndOfFrame();
			}
		}
		_steve.GetComponent<MovementPlayer>().enabled = true;
		_steve.GetComponent<MouseClick>().enabled = true;
		
		started = false;
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

	public void addZombiesDead() {
		++zombiesDead;
	}

	void Start () {
		tiempo1 = DateTime.Now;
		progress = 0;
		StartCoroutine(init());
		//QUITAR
		maxZombies = 0;
//		maxZombies = 5;
		zombiesDead = 0;
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

		Camera[] auxCams = _steve.GetComponent<MovementPlayer>().steveEyes.GetComponentsInChildren<Camera>();
		this.gameObject.GetComponent<CountingOfTime>().sky = auxCams[1/* <- Because of yes*/];

		Vector3 pos;
		pos.x = UnityEngine.Random.Range (1f, Chunk.sizex * sizex);
		pos.z = UnityEngine.Random.Range (1f, Chunk.sizez * sizez);
		pos.y = getHeight(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.z));
		pos.y += 7.5f;
		_steve.transform.position = pos;
		InventoryManagment.texturesItem = texturesItem;

		_zombies = new ArrayList();

		infoPlayer = new Rect(10f, 10f, 150.0f, 75.0f);
		infoPlayer.center = new Vector2(Screen.width-80.0f, 47.5f);
		theInfoPlayer = theInfo;
	}

	void theInfo(int windowID) {
		gameObject.GetComponent<CountingOfTime>().getDate (out day, out month, out year);
		int score = 0;
		score += 100*zombiesDead;
		int diffYear = year - 1;	//1 = initYear
		int diffMonth = month - 1;	//1 = initMonth
		int totalDays = diffYear*365;
		if(diffMonth >= 1) totalDays += 31;
		if(diffMonth >= 2) totalDays += 28;
		if(diffMonth >= 3) totalDays += 31;
		if(diffMonth >= 4) totalDays += 30;
		if(diffMonth >= 5) totalDays += 31;
		if(diffMonth >= 6) totalDays += 30;
		if(diffMonth >= 7) totalDays += 31;
		if(diffMonth >= 8) totalDays += 31;
		if(diffMonth >= 9) totalDays += 30;
		if(diffMonth >= 10) totalDays += 31;
		if(diffMonth == 11) totalDays += 30;
		totalDays += day - 1;

		score += 50*totalDays;

		GUI.skin.textField.alignment = TextAnchor.MiddleCenter;
		GUILayout.TextField("Killed Zombies: "+zombiesDead+"\n" +
							"Date: "+day+"/"+month+"/"+year+"\n" +
		                    "Score: "+score);
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

		gameObject.GetComponent<CountingOfTime>().getDate (out day, out month, out year);

		if(gameObject.GetComponent<CountingOfTime>().ThisIsNight()) {
			if(!enoughZombiesPlease()) {
				spawnZombie();
			}
		} else {
			foreach(GameObject z in _zombies) {
				if(z != null) {
					//kill the zombie
					z.GetComponent<IAZombie>().damage(10.0f, new Vector3(0f,0f,0f), new Vector3(0f,0f,0f));
				}
			}
			if(_zombies.Count > 0) {
				_zombies.Clear();
				//QUITAR
//				addZombies(5);
			}
		}
	}

	//Returns true if there are enough zombies at the scene (so another zombie cannot be spawn)
	private bool enoughZombiesPlease() {
		int zombiesNear = 0;
		foreach(GameObject z in _zombies) {
			if(z != null) {
				if(Vector3.Distance(z.transform.position, _steve.transform.position) < 20.0f) ++zombiesNear;
			}
		}
		if(zombiesNear >= maxZombies) return true; 	//maximum of maxZombies zombies near steve

		return _zombies.Count > maxZombies; //maximum of 10 zombies per chunk
	}

	private void spawnZombie() {
		float desX = UnityEngine.Random.Range (10.0f, 20.0f);
		float desZ = UnityEngine.Random.Range (10.0f, 20.0f);
		int xRand = UnityEngine.Random.Range (1, 3); if(xRand == 2) xRand = -1; else xRand = 1;
		int zRand = UnityEngine.Random.Range (1, 3); if(zRand == 2) zRand = -1; else zRand = 1;
		desX *= xRand;
		desZ *= zRand;
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
		int x = Mathf.FloorToInt(pos.x)%Chunk.sizex;
		int y = Mathf.FloorToInt(pos.y);
		int z = Mathf.FloorToInt(pos.z)%Chunk.sizez;
		return new Vector3(x,y,z);
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
		if(posValida(pos)){
			Chunk c = getChunk(pos);
			Vector3 rpos = relativePos(pos);
			bool res = c.addFace(rpos,face);
			c.meshLoad();
			return res;

		}
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
