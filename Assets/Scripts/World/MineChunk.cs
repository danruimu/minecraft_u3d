using UnityEngine;
using System.Collections;

public class MineChunk : MonoBehaviour {

	public static int sizeX = 16;
	public static int sizeY = 128;
	public static int sizeZ = 16;

	private MineBlock[,,] block;

	public GameObject chunkPrefab;

	public MineChunk() {
		block = new MineBlock[sizeX,sizeY,sizeZ];
	}

	public bool addBlock(int x, int y, int z, BlockType type) {
		return SetBlock (x,y,z,type);
	}

	public bool SetBlock(int x, int y, int z, BlockType type) {
		if(x >= sizeX || x < 0 || y >= sizeY || y < 0 || z >= sizeZ || z < 0) 
			return false;

		block[x, y, z] = new MineBlock(type);
		return true;
	}
}
