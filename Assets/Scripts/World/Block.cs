using UnityEngine;
using System.Collections;

public enum BlockType {
	Grass=0,
	Dirt,
	Stone,
	Bedrock
};

public class Block {
	public static int numTypesCubes = 4;
	private int[] matPointer;
	private face[] faces;
	private BlockType type;

	public Block(BlockType type, World w){
		faces = new face[]{null,null,null,null,null,null};
		if(type == BlockType.Grass)
			matPointer = w.getMatPointerArray(type,new string[]{null,null,null,null,"Grass_top","Dirt"});
		else
			matPointer = w.getMatPointerArray(type);
		this.type = type;
	}

	public void addFace(face faceAdd,faceType f){
		faces[(int)f]=faceAdd;
	}

	public face getFace(faceType f){
		return faces[(int)f];
	}

	public void delFace(faceType f){
		faces[(int)f] = null;
	}

	public int getMatIndex(faceType f){
		return matPointer[(int)f];
	}
}
