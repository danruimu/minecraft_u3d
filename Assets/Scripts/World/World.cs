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
//		xa=xd=za=zd=0;
//		yd=0;
//		ya=70;
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
	}
	
	private Vector3 relativePos(Vector3 pos){
		return new Vector3(((int)pos.x)%Chunk.sizex,(int)pos.y,((int)pos.z)%Chunk.sizez);
	}

	private Chunk getChunk(Vector3 p){
		return chunks[(int)(p.x/Chunk.sizex),(int)(p.z/Chunk.sizez)];
	}

	public bool existsCube(Vector3 pos){
		//limites absolutos
		if(pos.x < 0 ||pos.y < 0 || pos.z < 0)return true;// es true per a no pintar els bordes
		if(pos.x >= sizex*Chunk.sizex || pos.y >= Chunk.sizey ||pos.z >= sizez*Chunk.sizez)return true;// es true per a no pintar els bordes
		return getChunk(pos).existsCube(relativePos(pos));
	}

	public void delFace(Vector3 pos,faceType face){
		getChunk(pos).delFace(relativePos(pos),face);
	}

	public bool addFace(Vector3 pos,faceType face){
		return getChunk(pos).addFace(relativePos(pos),face);
	}
	
	public static int[] getMatPointerArray(BlockType type){
		if(indexsBlocks[(int)type] == null) throw new Exception("material " + type + " no dins del sistema");
		return indexsBlocks[(int)type];
	}
}
