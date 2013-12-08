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
	public static int sizex = 4;
	public static int sizez = 4;
	public const int numMaxMaterials = 256;

	private static int[][] indexsBlocks;
	private Chunk[,] chunks;
	private float[,] heightmap;


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

		#region terrain_generation
//		UnityEngine.Random.seed = seed.GetHashCode();
//		heightmap = new float[sizex*Chunk.sizex + 1,sizez*Chunk.sizez + 1];//[N + 1,N + 1] --> sqrt(N) must be int
//		heightmap[0,0] = UnityEngine.Random.Range(1,Chunk.sizey);
//		heightmap[0,sizez*Chunk.sizez] = UnityEngine.Random.Range(1,Chunk.sizey);
//		heightmap[sizex*Chunk.sizex,0] = UnityEngine.Random.Range(1,Chunk.sizey);
//		heightmap[sizex*Chunk.sizex,sizez*Chunk.sizez] = UnityEngine.Random.Range(1,Chunk.sizey);
//		DiamondSquare(heightmap);
		#endregion

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

//	void DiamondSquare(float[,] map){
//		float h = Chunk.sizey; 	//the range (-h -> h) for the average offset
//		int DATA_SIZE = sizex*Chunk.sizex + 1;
//		//side length is the distance of a single square side
//		//or distance of diagonal in diamond
//		//each iteration we are looking at smaller squares and diamonds, we decrease the variation of the offset
//		for (int sideLength = DATA_SIZE-1; sideLength >= 2; sideLength /= 2, h /= 2.0f)	{
//			
//			int halfSide = sideLength/2;
//			
//			//generate new square values
//			for(int x=0; x<DATA_SIZE-1;x+=sideLength) {
//				for(int y=0; y<DATA_SIZE-1; y+=sideLength) {
//					
//					//x,y is upper left corner of the square
//					//calculate average of existing corners
//					float avg = map[x,y] + 				//top left
//						map[x+sideLength,y]   +				//top right
//							map[x,y+sideLength]   + 				//lower left
//							map[x+sideLength,y+sideLength]; 	//lower right
//					
//					avg /= 4.0f;
//					
//					//center is average plus random offset in the range (-h, h)
//					map[x+halfSide,y+halfSide] = avg + UnityEngine.Random.Range(-h,h);
//					
//				} //for y
//			} // for x
//			
//			
//			//Generate the diamond values
//			//Since diamonds are staggered, we only move x by half side
//			//NOTE: if the data shouldn't wrap the x < DATA_SIZE and y < DATA_SIZE
//			for (int x=0; x<DATA_SIZE-1; x+=halfSide) {
//				for (int y=(x+halfSide)%sideLength; y<DATA_SIZE-1; y+=sideLength) {
//					
//					//x,y is center of diamond
//					//we must use mod and add DATA_SIZE for subtraction 
//					//so that we can wrap around the array to find the corners
//					
//					float avg = 
//						map[(x-halfSide+DATA_SIZE)%DATA_SIZE,y] +	//left of center
//							map[(x+halfSide)%DATA_SIZE,y]				+	//right of center
//							map[x,(y+halfSide)%DATA_SIZE]				+	//below center
//							map[x,(y-halfSide+DATA_SIZE)%DATA_SIZE];	//above center
//					
//					avg /= 4.0f;
//					
//					//new value = average plus random offset
//					//calc random value in the range (-h,+h)
//					avg = avg + UnityEngine.Random.Range(-h,h);
//					
//					//update value for center of diamond
//					map[x,y] = avg;
//					
//					//wrap values on the edges
//					//remove this and adjust loop condition above
//					//for non-wrapping values
//					if (x == 0) map[DATA_SIZE-1,y] = avg;
//					if (y == 0) map[x,DATA_SIZE-1] = avg;
//				} //for y
//			} //for x
//		} //for sideLength
//
//		float minY = map[0,0];
//		float maxY = map[0,0];
//		//Calculate minY and maxY values
//		for (int i = 1; i<DATA_SIZE-1; i++)
//			for(int j=1; j<DATA_SIZE-1; j++) {
//				if (map[i,j] > maxY)
//					maxY = map[i,j];
//				if (map[i,j] < minY)
//					minY = map[i,j];
//			}
//		if(minY < 0){
//			for (int i = 0; i<DATA_SIZE-1; i++)
//				for(int j=0; j<DATA_SIZE-1; j++) {
//					map[i,j] = Math.Min (Chunk.sizey,map[i,j]+(-minY));
//				}
//		}
//	}

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
