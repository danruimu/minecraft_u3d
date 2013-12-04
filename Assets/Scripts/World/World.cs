using UnityEngine;
using System.Collections;
using System;
using TreeEditor;

public class World : MonoBehaviour {

	// Use this for initialization
	public GameObject chunkPrefab;
	public Material[] mats;
	public static int sizex = 16;
	public static int sizez = 16;
	private Chunk[,] chunks;
	private Perlin p;
	public string seed;
	void Start () {
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
}
