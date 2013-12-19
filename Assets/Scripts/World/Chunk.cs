using UnityEngine;
using System.Collections;
using System;

public struct cubeProperties{
	public Vector3[,] vertexList;
	public Vector2[] uvList;
	public Vector4[] tangent;
	public int[] faceList;
	public Vector3[] posiciones;
}

public enum faceType{
	front=0,
	rear,
	left,
	right,
	top,
	bottom
}

public class face{
	public int posV;
	public int mat;
	public int posM;
}

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]

//TODO: provar eliminar y afegir block edge chunk

public class Chunk : MonoBehaviour {

	public const int numFaces = 6;
	public const int numVertexsFace = 4;
	public const int numPointsPerFace = 6;
	public const int numMaxFaces = 16384;
	public static int sizex = 16;
	public static int sizey = 128;
	public static int sizez = 16;

	public cubeProperties props;

	private face []refs;
	private Vector3 []vertexs;
	private Vector2 []uvs;
	private Vector4 []tangents;
	private int [][]faces;

	private Mesh mesh;

	private byte[,] height;
	private Block[,,] cubes;
	private int numFacesAdded;
	private int[] numFacesAddedMaterial;
	private int numMaterials;
	private World father;

	private Vector3 chunkPosition;

	public void init(Vector3 position,World padre,Material[] materials){
		vertexs = new Vector3[50*numVertexsFace];
		uvs = new Vector2[50*numVertexsFace];
		tangents = new Vector4[50*numVertexsFace];
		refs = new face[50];
		numMaterials = materials.Length;
		faces = new int[numMaterials][];
		numFacesAdded = 0;
		numFacesAddedMaterial = new int[numMaterials];
		for(int i=0;i<numMaterials;i++){
			faces[i] = new int[50*numPointsPerFace];
			numFacesAddedMaterial[i] = 0;
		}
		transform.position = position;
		chunkPosition = position;
		father = padre;

		fillProperties();
		height= new byte[sizex,sizez];

		cubes = new Block[sizex,sizey,sizez];
		mesh = new Mesh();
		GetComponent<MeshFilter>().sharedMesh = mesh;
		GetComponent<MeshRenderer>().sharedMaterials = materials;
		//TODO: properly done terraingeneration
	}

	public void fillColums(byte[] heightmap,byte[] data){
		//TODO:barra progres
		for (int cx=0;cx<Chunk.sizex;cx++){
			for(int cz=0;cz<Chunk.sizez;cz++){
				height[cx,cz] = heightmap[cx*Chunk.sizez + cz];
				for(int cy=0;cy<Chunk.sizey && cy < height[cx,cz];cy++){
					BlockType val = (BlockType)data[cz*Chunk.sizex*Chunk.sizey + cx*Chunk.sizey + cy];
					/*8=aiguaquieta,9=aiguamoving,10=lavamoving,11=lavaquieta
					,31=tallgrass,78=nieve,106=vines,111=lylipas,39&40=setas,37=dandelion,52=monsterspawner,
					 54=chest,85=fence*/
					if((int)val > 0 && (int)val != 8 && (int)val != 10 &&(int)val != 9 && (int)val != 11 && (int)val != 31
					   && (int)val != 111 && (int)val != 78 && (int)val != 106 && (int)val != 39 && (int)val != 40
					   && (int)val != 37 && (int)val != 52 && (int) val != 54 && (int)val!=85)
						cubes[cx,cy,cz] = new Block(val);
				}
			}
		}
	}

	public byte getHeight(int x,int z){
		return height[x,z];
	}

	public void ompleMesh(int alturaMaxima){
		//TODO:barra progres
		for(int x = 0; x < sizex; x++){
			for (int z=0; z < sizez; z++){
				for(int y = 0;y <= height[x,z] && y <sizey; y++){
					if(y<alturaMaxima && cubes[x,y,z]==null){
						cubes[x,y,z] = new Block(BlockType.Stone);
						continue;
					}
					if(y >= alturaMaxima && cubes[x,y,z]!=null){
						addCube(cubes[x,y,z],new Vector3(x,y,z),false);
					}
				}
			}
		}
		meshLoad(false);
	}

	public void meshLoad(bool update = true){
		if(update)
			mesh.Clear();
		mesh.subMeshCount = numMaterials;
		mesh.vertices = vertexs;
		mesh.uv = uvs;
		mesh.tangents = tangents;
		for(int i=0;i<numMaterials;i++){
			if(numFacesAddedMaterial[i] > 0)
			mesh.SetTriangles(faces[i],i);
		}
		mesh.RecalculateNormals();
		mesh.Optimize();
		//TODO: colisions per box colliders
	}

	private Block getBlock(Vector3 pos){
		return cubes[(int)pos.x,(int)pos.y,(int)pos.z];
	}

	public bool removeCube(int x,int y,int z){
		Vector3 position = new Vector3 (x, y, z);
		if(getBlockType(x,y,z) == BlockType.Bedrock)return false;
		if(!existsCube(position))return false;
	//TODO: casos limits de numMaxFaces
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
			Vector3 colliding = props.posiciones[face] + position;
			if(existsCube(colliding)){
				if(insideChunk(colliding)){
					if(addFace(colliding,(faceType)(face^1))){
						getBlock(colliding).initCollider(colliding,this);
					}
				}
				else{
					father.addFace(chunkPosition + colliding,(faceType)(face^1));
				}
			}
			else{
				delFace(position,(faceType)face);
			}
		}
		cubes[(int)position.x,(int)position.y,(int)position.z] = null;
		meshLoad();
		return true;
	}

	public BlockType getBlockType(int x,int y,int z){
		return cubes[x,y,z].getType();
	}

	public bool newCube(int x,int y,int z, BlockType type){
		Vector3 pos = new Vector3 (x, y, z);
		if(!insideChunk(pos))return false;
		if(cubes[x,y,z]!=null)return false;
		Block c = new Block(type);
		cubes[(int)pos.x,(int)pos.y,(int)pos.z] = c;
		if(!addCube(c,pos,true)) {
			cubes[(int)pos.x,(int)pos.y,(int)pos.z] = null;
			return false;
		}
		if((int)height[x,z] < y)height[x,z] = (byte)y;
		meshLoad();
		return true;
	}

	public bool addFace(Vector3 pos,faceType face){
		Block cub = cubes[(int)pos.x,(int)pos.y,(int)pos.z];
		int material = cub.getMatIndex(face);
		if(cub.getFace(face)==null){
			int indV = numFacesAdded++;
			int indM = numFacesAddedMaterial[material]++;
			//TODO: menys array.resizes
			Array.Resize(ref vertexs,numFacesAdded*numVertexsFace);
			Array.Resize(ref uvs,numFacesAdded*numVertexsFace);
			Array.Resize(ref tangents,numFacesAdded*numVertexsFace);
			Array.Resize(ref refs,numFacesAdded);
			Array.Resize(ref faces[material],numFacesAddedMaterial[material]*numPointsPerFace);
			for(int vertex = 0;vertex<numVertexsFace;vertex++){
				vertexs[indV*numVertexsFace + vertex] = props.vertexList[(int)face,vertex] + pos;
				uvs[indV*numVertexsFace + vertex] = props.uvList[vertex];
				tangents[indV*numVertexsFace + vertex] = props.tangent[(int)face];
			}
			for(int pface = 0;pface<numPointsPerFace;pface++){
				faces[material][indM*numPointsPerFace+pface] = props.faceList[pface]+indV*numVertexsFace;
			}
			face f = new face();
			f.posM = indM;
			f.posV = indV;
			f.mat = material;
			refs[indV] = f;
			return cub.addFace(f,face);
		}
		else{
			Debug.LogError("cara a afegir ja afegida");
			return false;
		}
		return true;
	}

	public void delFace(Vector3 pos,faceType face,bool update=false){
		Block cub = cubes[(int)pos.x,(int)pos.y,(int)pos.z];
		int material = cub.getMatIndex(face);
		face borrar = cub.getFace(face);
		cub.delFace(face);
		if(borrar!=null){
			numFacesAdded--;
			numFacesAddedMaterial[material]--;
			if(numFacesAdded > 0){
				for(int vertex = 0;vertex<numVertexsFace;vertex++){
					vertexs[borrar.posV*numVertexsFace + vertex] = vertexs[numFacesAdded*numVertexsFace + vertex];
					uvs[borrar.posV*numVertexsFace + vertex] = uvs[numFacesAdded*numVertexsFace + vertex];
					tangents[borrar.posV*numVertexsFace + vertex] = tangents[numFacesAdded*numVertexsFace + vertex];
				}
				refs[borrar.posV] = refs[numFacesAdded];
				refs[borrar.posV].posV = borrar.posV;
				for(int pface = 0;pface<numPointsPerFace;pface++){
					faces[refs[numFacesAdded].mat][refs[numFacesAdded].posM*numPointsPerFace + pface] = borrar.posV*numVertexsFace + props.faceList[pface];
				}
			}
			if(numFacesAddedMaterial[material] > 0){
				for(int pface = 0;pface<numPointsPerFace;pface++){
					faces[material][borrar.posM*numPointsPerFace+pface] = faces[material][numFacesAddedMaterial[material]*numPointsPerFace+pface];
				}
				int vert = faces[material][numFacesAddedMaterial[material]*numPointsPerFace];
				refs[vert/numVertexsFace].posM = borrar.posM;
			}
			//TODO: menys resizes
			Array.Resize(ref vertexs,numFacesAdded*numVertexsFace);
			Array.Resize(ref uvs,numFacesAdded*numVertexsFace);
			Array.Resize(ref tangents,numFacesAdded*numVertexsFace);
			Array.Resize(ref refs,numFacesAdded);
			Array.Resize(ref faces[material],numFacesAddedMaterial[material] * numPointsPerFace);
		}
		else{
			if(update)Debug.LogError("cara a eliminar no present");
		}
	}

	private bool addCube (Block cub,Vector3 position,bool update = true) {
		Vector3 p;

		for(int face=0;face<numFaces;face++){
			p = props.posiciones[face] + position;
			//TODO: casos maxims de numMaxFacesAdded en diferents chunks
			if(!existsCube(p)){
				if(addFace(position,(faceType)face))cub.initCollider(position,this);
			}
			else{
				if(update){
					if(insideChunk(p))
						delFace(p,(faceType)(face^1));
					else
						father.delFace(p,(faceType)(face^1));
				}
			}
		}
		
		return true;
	}

	private bool insideChunk(Vector3 pos){
		if(pos.x <0 || pos.y<0 || pos.z<0)return false;
		if(pos.x >=sizex || pos.y>=sizey || pos.z>=sizez)return false;
		return true;
	}

	public bool existsCube(Vector3 position){
		if(insideChunk(position)){
			return (cubes[(int)position.x,(int)position.y,(int)position.z] != null);
		}
		else{
			return father.existsCube(chunkPosition + position);
		}
	}


	private void fillProperties(){

		props.posiciones = new Vector3[]{
			new Vector3(0,0,-1),
			new Vector3(0,0,1),
			new Vector3(-1,0,0),
			new Vector3(1,0,0),
			new Vector3(0,1,0),
			new Vector3(0,-1,0)
		};
		
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

