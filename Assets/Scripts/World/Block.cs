using UnityEngine;
using System.Collections;

public enum BlockType {
	Grass=2,
	Dirt=3,
	Stone=1,
	Bedrock=7,
	Gravel=13,
	CoalOre=16,
	IronOre=15,
	RedstoneOre=73,
	DiamondOre=56,
	GoldOre=14,
	LapisOre=21,
	Sand=12,
	Clay=82
};

public class Block {
	private int[] matPointer;
	private face[] faces;
	private BlockType type;

	public Block(BlockType type){
		faces = new face[]{null,null,null,null,null,null};
		matPointer = World.getMatPointerArray(type);
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

	public BlockType getType(){
		return type;
	}
}
