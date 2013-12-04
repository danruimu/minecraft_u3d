using UnityEngine;
using System.Collections;

public class Cube {
	public face[] faces;
	public int material;

	public Cube(int material){
		faces = new face[]{null,null,null,null,null,null};
		this.material = material;
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
}
