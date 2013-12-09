using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;
using Ionic.Zlib;

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


	void setMaterialIndexs(){
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

	void init() {
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
		DateTime tiempo2 = DateTime.Now;
		TimeSpan total = new TimeSpan(tiempo2.Ticks - tiempo1.Ticks);
		Debug.Log("creacion " + sizex + " * " + sizez + " Chunks -> tiempoTotal = " + total.ToString());
	}

	void Start () {
		init();
	}
	
	// Update is called once per frame
	void Update () {

	}
	
	private Vector3 relativePos(Vector3 pos){
		return new Vector3(((int)pos.x)%Chunk.sizex,(int)pos.y,((int)pos.z)%Chunk.sizez);
	}

	private Chunk getChunk(Vector3 p){
		return chunks[(int)(p.x/Chunk.sizex),(int)(p.z/Chunk.sizez)];
	}

	public bool existsCube(Vector3 pos){
		//limites absolutos
		if(pos.x < 0 ||pos.y < 0 || pos.z < 0)return false;// es true per a no pintar els bordes
		if(pos.x >= sizex*Chunk.sizex || pos.y >= Chunk.sizey ||pos.z >= sizez*Chunk.sizez)return false;// es true per a no pintar els bordes
		return getChunk(pos).existsCube(relativePos(pos));
	}

	public void delFace(Vector3 pos,faceType face){
		getChunk(pos).delFace(relativePos(pos),face);
	}

	public void addFace(Vector3 pos,faceType face){
		getChunk(pos).addFace(relativePos(pos),face);
	}

	//TODO hacer estructura para hacer esto rapido (array de tamaño BlockType.Length de (arrays de tamaño 6 de punteros a materiales))
	public static int[] getMatPointerArray(BlockType type){
		if(indexsBlocks[(int)type] == null) throw new Exception("material " + type + " no dins del sistema");
		return indexsBlocks[(int)type];
	}
}
