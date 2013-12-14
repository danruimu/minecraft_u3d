using UnityEngine;
using System.Collections;

public class InventoryManagment : MonoBehaviour {
	public static bool invEnabled = false;
	public GUITexture inventory;
	public GUITexture barra;
	public GUITexture crosshair;
	public GUITexture selector;
	public GameObject prefab;
	public Texture[] textures;

	private int posInv = 1;
	private Item[] itemsBarra;
	private Item[] itemsInv;
	private float stepBarra;
	private float startBarra;
	private float inventoryXStart;
	private float inventoryYStart;
	private float inventoryBarYStart;
	private float itemSizePx;
	private float offset;
	private float offsetY;

	void Start(){
		posInv=0;
		Vector3 pos = barra.transform.position;
		startBarra = pos.x - (barra.pixelInset.width/2)/Screen.width;
		stepBarra = (barra.pixelInset.width/9) / Screen.width;
		pos.x = startBarra+posInv*stepBarra;
		pos.z = 1;
		selector.transform.position = pos;
		itemsInv = new Item[27];
		itemsBarra = new Item[9];
		inventory.pixelInset = new Rect(-((Screen.height-64)/2),-((Screen.height-64)/2),Screen.height-64,Screen.height-64);
		itemSizePx = (Screen.height-64)/12.0f;
		float ample = (Screen.height-64)/2.0f;
		inventoryXStart = inventory.transform.position.x - ample/Screen.width + (itemSizePx*0.52f)/Screen.width;
		inventoryYStart = inventory.transform.position.y - ample/Screen.height + (itemSizePx/Screen.height)*2.36f;
		inventoryBarYStart = inventory.transform.position.y - ample/Screen.height + (itemSizePx*0.8f)/Screen.height;
		offset = itemSizePx*0.2f;
		offsetY = itemSizePx*0.3f;
	}

	void pintaItems(bool inventoryMode){
		foreach(Item i in itemsBarra){
			if(i!=null)i.cambia(inventoryMode);
		}
		if(inventoryMode){
			foreach(Item i in itemsInv){
				if(i!=null)i.enable();
			}
		}
		else{
			foreach(Item i in itemsInv){
				if(i!=null)i.disable();
			}
		}
	}

	private void addInventory(bool barra,int pos,int id,byte quantitat){
		GameObject go = (GameObject)Instantiate(prefab);
		Item it = go.GetComponent<Item>();
		if(barra){
			Vector3 posInv = new Vector3(inventoryXStart + (offset*pos)/Screen.width + (itemSizePx*pos)/Screen.width,inventoryBarYStart,1f);
			Vector3 posBarra = new Vector3(startBarra+stepBarra*pos,0f,1f);
			it.startGO(id,quantitat,textures[id],itemSizePx,posInv,posBarra);
			go.name = "barra["+pos+"]";
			it.cambia(invEnabled);
			itemsBarra[pos] = it;
		}
		else {
			it.startGO(id,quantitat,textures[id],itemSizePx,new Vector3(inventoryXStart + (offset*(pos%9))/Screen.width + (itemSizePx*(pos%9))/Screen.width,inventoryYStart + ((offsetY)*(pos/9))/Screen.height + (itemSizePx*(pos/9))/Screen.height,1f));
			if(invEnabled)
				it.cambia(true);
			go.name = "inventari["+pos+"]";
			itemsInv[pos] = it;
		}
			
	}
	
	void Update () {
		Transform t = transform;
		if(Input.GetKeyDown(KeyCode.E)){
			invEnabled = !invEnabled;
			inventory.enabled = invEnabled;
			selector.enabled = !invEnabled;
			barra.enabled = !invEnabled;
			crosshair.enabled = !invEnabled;
			GetComponent<MouseLook>().enabled = !invEnabled;
			GetComponent<MovementPlayer>().enabled = !invEnabled;
			pintaItems(invEnabled);
		}
		else if(!invEnabled){
			if (Input.GetAxis("Mouse ScrollWheel") > 0 && posInv > 0) {
				posInv--;
				Vector3 pos = selector.transform.position;
				pos.x = startBarra+posInv*stepBarra;
				selector.transform.position = pos;
			}
			else if (Input.GetAxis("Mouse ScrollWheel") < 0 && posInv < 8) {
				posInv++;
				Vector3 pos = selector.transform.position;
				pos.x = startBarra+posInv*stepBarra;
				selector.transform.position = pos;
			}
			else{
				for(int i = 0;i<9;i++){
					if(Input.GetKeyDown(i+KeyCode.Alpha1)){
						Vector3 pos = selector.transform.position;
						pos.x = startBarra+i*stepBarra;
						selector.transform.position = pos;
						posInv = i;
					}
				}
			}
		}
		else{
			if(Input.GetKeyDown(KeyCode.A)){
				for (int i=0;i<1;i++){
					addInventory(true,i,UnityEngine.Random.Range(0,textures.Length),(byte)42);
				}
//				for (int i=0;i<9*3;i++){
//					addInventory(false,i,UnityEngine.Random.Range(0,textures.Length),(byte)42);
//				}
			}
		}
	}
}
