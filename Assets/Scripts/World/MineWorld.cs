using UnityEngine;
using System.Collections;

public class MineWorld : MonoBehaviour {

	private MineChunk[,] chunk;

	public GameObject chunkPrefab;

	public static int sizeX;
	public static int sizeZ;

	public MineWorld(int x, int z) {
		sizeX = x;
		sizeZ = z;
		chunk = new MineChunk[x,z];
	}

	public void Create(string seed) {
		//TODO: aqui llenamos todos los chunks de lefa, excepto si todos
		//sus blocks son solidos, entonces no se puede, no hay espacio
		// (como en un tight pussy)
		for(int i = 0; i<sizeX; ++i) {
			for(int j = 0; j<sizeZ; ++j) {
				chunk[i,j] = new MineChunk();
			}
		}
	}

	//PRECONDITION: chunks start at (0,0)
	public void addBlock(int x, int y, int z, BlockType type) {
		chunk[x/MineChunk.sizeX,z/MineChunk.sizeZ].addBlock(x%MineChunk.sizeX, y, z%MineChunk.sizeZ, type);
	}
}
