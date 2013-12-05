using UnityEngine;
using System;
using System.Collections;
#region structures
public struct cubeProperties{
	public Vector3[,] vertexList;
	public Vector2[] uvList;
	public Vector4[] tangent;
	public int[] faceList;
}

public class face{
	public Block cub;
	public Vector3[] vertexs;
	public Vector2[] uv;
	public Vector4[] tangents;
	public int pos;
	public bool added;
	//	public Vector3[] normals
}

public enum faceType{
	front=0,
	rear,
	left,
	right,
	top,
	bottom
}
#endregion
[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
[RequireComponent (typeof (MeshCollider))]

public class Chunk : MonoBehaviour {
	#region constants
	public const int numFaces = 6;
	public const int numVertexsFace = 4;
	public const int numPointsPerFace = 6;
	public const int numMaxFaces = 16384;
	#endregion
	#region sizes
	public static int sizex = 16;
	public static int sizey = 128;
	public static int sizez = 16;
	#endregion
	private Vector3 chunkPosition;
	private World father;
	private int[,] heightmap;

	#region stuff_mesh
	private cubeProperties props;
	private Block[,,] cubes;
	private int numMaterials;
	private int numFacesAdded;
	private Mesh mesh;
	private ArrayList[] mats;
	private ArrayList[] add;
	private ArrayList[] remove;

	private Vector3[] vertexs;
	private Vector2[] uv;
	private Vector4[] tangents;
	private int[][] faces;
	
	Vector3[] posiciones = {new Vector3(0,0,-1),new Vector3(0,0,1),new Vector3(-1,0,0),new Vector3(1,0,0),new Vector3(0,1,0),new Vector3(0,-1,0)};
	#endregion

	public bool newCube(Vector3 pos, BlockType type){
		if(insideChunk(pos))return false;
		Block c = new Block(type);
		if(!addCube(c,pos,true))return false;
		cubes[(int)pos.x,(int)pos.y,(int)pos.z] = c;
		return true;
	}

	public Vector3 getPosition(){
		return chunkPosition;
	}

	//TODO: tener en cuenta que afectamos a varios chunks
	public bool removeCube(Vector3 position){
		if(!existsCube(position))return false;
//		int added = 0;
//
//		for(int face=0;face<numFaces;face++){
//			Vector3 colliding = posiciones[face] + position;
//			if(existsCube(colliding)){
//				added++;
//			}
//			else{
//				added--;
//			}
//		}
//
//		if(added + numFacesAdded >= numMaxFaces)return false;

		for(int face=0;face<numFaces;face++){
			Vector3 colliding = posiciones[face] + position;
			if(existsCube(colliding)){
				if(insideChunk(colliding))
					addFace(colliding,(faceType)(face^1));
				else{
					father.addFace(chunkPosition + colliding,(faceType)(face^1));
				}
			}
			else{
				delFace(position,(faceType)face);
			}
		}
		cubes[(int)position.x,(int)position.y,(int)position.z] = null;
		return true;
	}
	
	public bool existsCube(Vector3 position){
		if(!insideChunk(position)){
			return father.existsCube(position+chunkPosition);
		}
		return (cubes[(int)position.x,(int)position.y,(int)position.z] != null);
	}
	
	public void init(Vector3 position,World padre,Material[] materials){
		transform.position = position;
		chunkPosition = position;
		father = padre;
		numMaterials = materials.Length;
		fillProperties();
		heightmap= new int[sizex,sizez];
		numFacesAdded = 0;
		mats = new ArrayList[numMaterials];
		add = new ArrayList[numMaterials];
		remove = new ArrayList[numMaterials];
		faces = new int[numMaterials][];
		for(int i=0;i<numMaterials;i++){
			mats[i] = new ArrayList();
			add[i] = new ArrayList();
			remove[i] = new ArrayList();
		}
		cubes = new Block[sizex,sizey,sizez];
		mesh = new Mesh();
		GetComponent<MeshFilter> ().sharedMesh = mesh;
		GetComponent<MeshCollider>().sharedMesh = mesh;
		GetComponent<MeshRenderer>().sharedMaterials = materials;

		//TODO:barra progres
		//TODO: properly done terraingeneration
		//TODO: provar eliminar y afegir block edge chunk
//		for(int x = 0; x < sizex; x++){
//			for (int z=0; z < sizez; z++){
//				cubes[x,0,z] = new Block(0);
//			}
//		}
//		
//		
//		for(int x = 0; x < sizex; x++){
//			for (int z=0; z < sizez; z++){
//				int y;
//				for(y = 1;y < 20; y++){
//					cubes[x,y,z] = new Block(UnityEngine.Random.Range(0,Block.numTypesCubes));//stubCreacioMon
//				}
//				heightmap[x,z] = y;
//			}
//		}
	}

	public void ompleMesh(){
		//TODO:barra progres
		for(int x = 0; x < sizex; x++){
			for (int z=0; z < sizez; z++){
				for(int y = 0;y < heightmap[x,z] && y <sizey; y++){
					if(cubes[x,y,z]!=null)
						addCube(cubes[x,y,z],new Vector3(x,y,z));
				}
			}
		}
		meshLoad(false);
	}

	private bool insideChunk(Vector3 pos){
		return (pos.x >=0 && pos.y>=0 && pos.z>=0 && pos.x < sizex && pos.y < sizey && pos.z < sizez);
	}
	
	//TODO:no recalcular normals sino pasarli, ja que no cal calcularles, son straightforward
	
	public void addFace(Vector3 pos,faceType face){
		Block cub = cubes[(int)pos.x,(int)pos.y,(int)pos.z];
		if(cub.getFace(face)==null){
			face f = new face();
			f.cub=cub;
			f.added = true;
			f.uv = new Vector2[numVertexsFace];
			f.tangents = new Vector4[numVertexsFace];
			f.vertexs = new Vector3[numVertexsFace];
			for(int vertex=0;vertex<numVertexsFace;vertex++){
				f.vertexs[vertex] = props.vertexList[(int)face,vertex]+pos;
				f.uv[vertex] = props.uvList[vertex];
				f.tangents[vertex] = props.tangent[(int)face];
			}
			f.added = true;
			f.pos = -1;
			add[cub.getMatIndex(face)].Add(f);
			cub.addFace(f,face);
			numFacesAdded++;
		}
		else{
			Debug.LogError("cara a afegir ja afegida");
		}
	}
	
	public void delFace(Vector3 pos,faceType face){
		Block cub = cubes[(int)pos.x,(int)pos.y,(int)pos.z];
		int material = cub.getMatIndex(face);
		face removeFace = cub.getFace(face);

		
		if(removeFace!=null){
			remove[material].Add(mats[material].IndexOf(removeFace));
			cub.delFace(face);
			numFacesAdded--;
		}
		else{
			Debug.LogError("cara a borrar encara no pintada");
		}
	}

	//TODO: tener en cuenta que afectamos a varios chunks
	private bool addCube (Block cub,Vector3 position,bool update = false) {
		Vector3 p;
//		int added = 0;
//		for(int face=0;face<numFaces;face++){
//			p = posiciones[face] + position;
//			if(!existsCube(p))
//				added++;
//			else{
//				added--;
//			}
//		}
//		if (added + numFacesAdded >= numMaxFaces)return false;

		for(int face=0;face<numFaces;face++){
			p = posiciones[face] + position;
			if(!existsCube(p))
				addFace(position,(faceType)face);
			else{
				if(update){
					if(insideChunk(p)){
						delFace(p,(faceType)(face^1));
					}
					else{
						father.delFace(p+chunkPosition,(faceType)(face^1));
					}
				}
			}
		}

		return true;
	}

	//TODO: no fer el recalculate normals
	private void meshLoad(bool update = true){
		if(update){
			mesh.Clear();
			Array.Resize(ref vertexs,numFacesAdded*numVertexsFace);
			Array.Resize(ref uv,numFacesAdded*numVertexsFace);
			Array.Resize(ref tangents,numFacesAdded*numVertexsFace);
		}
		else{
			vertexs = new Vector3[numFacesAdded*numVertexsFace];
			uv = new Vector2[numFacesAdded*numVertexsFace];
			tangents = new Vector4[numFacesAdded*numVertexsFace];
		}
		
		mesh.subMeshCount = numMaterials;
		
		int pos=0;
		for(int i=0;i<numMaterials;i++){
			if(remove[i] != null && remove[i].Count > 0)remove[i].Sort();
			
			if(faces[i] == null)faces[i] = new int[(mats[i].Count + add[i].Count - remove[i].Count)*numPointsPerFace];
			else Array.Resize(ref faces[i],(mats[i].Count + add[i].Count - remove[i].Count)*numPointsPerFace);
			
			for(int j=0;j<Math.Min(add[i].Count,remove[i].Count);j++){
				((face)add[i][j]).added = false;
				mats[i][(int)remove[i][j]] = (face)add[i][j];
				add[i].RemoveAt(j);
				remove[i].RemoveAt(j);
			}
			if(remove[i].Count > 0){
				for(int j=0;j<remove[i].Count;j++){
					mats[i].RemoveAt((int)remove[i][j]);
				}
			}
			remove[i] = new ArrayList();
			
			if(add[i].Count > 0){
				for(int j=0;j<add[i].Count;j++){
					mats[i].Add((face)add[i][j]);
				}
			}
			add[i] = new ArrayList();
			
			for(int j=0;j<mats[i].Count;j++){
				face f = (face)mats[i][j];
				if(f.pos != j){
					f.vertexs.CopyTo(vertexs,pos);
					f.tangents.CopyTo(tangents,pos);
					if(f.added){
						f.uv.CopyTo(uv,pos);
						for(int k=0;k<numPointsPerFace;k++){
							faces[i][j*numPointsPerFace+k]=pos+props.faceList[k];
						}
						f.added = false;
					}
					f.pos = j;
				}
				pos += numVertexsFace;
			}
		}
		mesh.vertices = vertexs;
		mesh.uv = uv;
		mesh.tangents = tangents;
		for(int i=0;i<numMaterials;i++){
			mesh.SetTriangles(faces[i],i);
		}
		mesh.RecalculateNormals();
		mesh.Optimize();
	}

	private void fillProperties(){
		
		props.vertexList = new Vector3[,]{
			{new Vector3(0,0,0), 
				new Vector3(0,1,0), 
				new Vector3(1,1,0), 
				new Vector3(1,0,0)},//front face
			
			{new Vector3(1,0,1), 
				new Vector3(1,1,1), 
				new Vector3(0,1,1), 
				new Vector3(0,0,1)},//rear face
			
			{new Vector3(0,0,1), 
				new Vector3(0,1,1),
				new Vector3(0,1,0),
				new Vector3(0,0,0)},//left face
			
			{new Vector3(1,0,0), 
				new Vector3(1,1,0), 
				new Vector3(1,1,1), 
				new Vector3(1,0,1)},//right face
			
			{new Vector3(0,1,0), 
				new Vector3(0,1,1), 
				new Vector3(1,1,1), 
				new Vector3(1,1,0)},//top face
			
			{new Vector3(0,0,1), 
				new Vector3(0,0,0), 
				new Vector3(1,0,0), 
				new Vector3(1,0,1)},//bottom face
		};
		
		props.uvList = new Vector2[]{
			new Vector2(0,0),
			new Vector2(0,1),
			new Vector2(1,1),
			new Vector2(1,0)
		};
		props.faceList = new int[]{0,1,2,0,2,3};
		props.tangent = new Vector4[]{
			new Vector4(1.0f,0f,0f,1.0f),
			new Vector4(-1.0f,0f,0f,1.0f),
			new Vector4(0f,0f,-1.0f,1.0f),
			new Vector4(0f,0f,1.0f,1.0f),
			new Vector4(1.0f,0f,0f,1.0f),
			new Vector4(1.0f,0f,0f,1.0f)
		};
	}
}
