using UnityEngine;
using System.Collections;
using System;
using TreeEditor;

public class World : MonoBehaviour {

	// Use this for initialization
	public GameObject chunkPrefab;
	public Material[] mats;
	public string seed;
	public static int sizex = 16;
	public static int sizez = 16;
	private Chunk[,] chunks;
	private Perlin p;
	private enum a{
		alfa,beta,gamma
	}

	void init() {
		p = new Perlin();
		p.SetSeed(seed.GetHashCode());
		GameObject GO;
		Chunk c;
		chunks = new Chunk[sizex,sizez];
		DateTime tiempo1 = DateTime.Now;
		//creo chunks
		for (int x=0;x<sizez;x++){
			for(int z=0;z<sizez;z++){
				GO = (GameObject)Instantiate(chunkPrefab);
				GO.name = "Chunk[" + x + ","+z+"]";
				GO.transform.parent = transform;
				c = GO.GetComponent<Chunk>();
				c.init(new Vector3(x*Chunk.sizex,0,z*Chunk.sizez),this,mats);
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
//		string alfa = "alfa";
//		if(alfa.Equals("alfa"))
		Debug.Log("iguals");
//		init();
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
		if(pos.x < 0 ||pos.y < 0 || pos.z < 0)return true;// es true per a no pintar els bordes
		if(pos.x >= sizex*Chunk.sizex || pos.y >= Chunk.sizey ||pos.z >= sizez*Chunk.sizez)return true;// es true per a no pintar els bordes
		return getChunk(pos).existsCube(relativePos(pos));
	}

	public void delFace(Vector3 pos,faceType face){
		getChunk(pos).delFace(relativePos(pos),face);
	}

	public void addFace(Vector3 pos,faceType face){
		getChunk(pos).addFace(relativePos(pos),face);
	}

	//TODO hacer estructura para hacer esto rapido (array de tamaño BlockType.Length de (arrays de tamaño 6 de punteros a materiales))
	public int[] getMatPointerArray(BlockType type){
		string typeName = type.ToString();
		int[] array = new int[6];
		int i=0;
		//TODO: posar un per defecte per a no tenir accidents

		while(i<mats.Length && mats[i].name.CompareTo(typeName) != 0)i++;

		return array;
	}
//	void CreateCollum(int x, int y, int z)
//	{
//		//		Debug.Log(y);
//		_world[x,0,z] = new Block(BlockType.Bedrock);//Final del mon
//		_world[x,1,z] = new Block(BlockType.Stone);//Base de pedra
//		int dirtHeight = UnityEngine.Random.Range(2, y-2);
//		for(int i=2;i<y-1;i++){
//			float density = _simplexNoise3D.GetDensity( new Vector3(x, i, z) );
//			if (density > 0){
//				if(y < dirtHeight){
//					_world[x,i,z] = new Block(BlockType.Stone, x,i,z);
//				}
//				else{
//					_world[x,i,z] = new Block(BlockType.Dirt, x,i,z);
//				}
//			}
//			else{
//				_world[x,i,z] = new Block(BlockType.Air, x,i,z);
//			}
//		}
//		_world[x,y-1,z] = new Block(BlockType.Grass, x,y-1,z);
//		_world[x,y,z] = new Block(BlockType.Height, x,y,z);
//		for(int i=y;i<_world.WorldVisibleSizeY;i++){
//			_world[x,i,z] = new Block(BlockType.Air, x,i,z);
//			_world[x,y,z].Light = Block.MaxLight;
//		}
//	}
}
