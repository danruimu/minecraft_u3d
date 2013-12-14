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
	private GameObject coll;
	private int[] matPointer;
	private face[] faces;
	private BlockType type;

	public Block(BlockType type){
		faces = new face[]{null,null,null,null,null,null};
		matPointer = World.getMatPointerArray(type);
		this.type = type;
		coll = null;
	}

	public bool addFace(face faceAdd,faceType f){
		faces[(int)f]=faceAdd;
		return coll == null;
	}

	public void initCollider(Vector3 pos){
		coll = new GameObject();
		coll.transform.position = pos;
		coll.tag = "Chunk";
		coll.name = "Collider["+pos.x+","+pos.y+","+pos.z+"]";
		BoxCollider bc = coll.AddComponent<BoxCollider>();
		bc.center = new Vector3(0.5f,0.5f,0.5f);
	}

	public face getFace(faceType f){
		return faces[(int)f];
	}

	public void delFace(faceType f){
		faces[(int)f] = null;
		if(coll!=null){
			foreach(face fa in faces){
				if(fa!=null)return;
			}
			GameObject.Destroy(coll);
			coll=null;
		}
	}

	public int getMatIndex(faceType f){
		return matPointer[(int)f];
	}

	public BlockType getType(){
		return type;
	}
}
