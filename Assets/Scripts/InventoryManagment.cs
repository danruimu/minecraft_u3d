using UnityEngine;
using System.Collections;

public class InventoryManagment : MonoBehaviour {
	public static bool invEnabled = false;
	public GUITexture inventory;
	public GUITexture barra;
	public GUITexture selector;
	private GUITexture _inventory;
	private GUITexture _barra;
	private GUITexture _selector;
	public GameObject prefab;
	public static Texture[] texturesItem;

	public static int numCrafting = 1;
	public static int[][] craftings;
	public static int[][] quantities;
	public static int[] ids;

	private int posInv = 1;
	public static bool dragging;
	private static Item[] itemsBarra;
	private static Item[] itemsInv;
	private static Item[] itemsCraft;
	private static Item itemCraftejat;
	private static float stepBarra;
	private static float startBarra;
	private static float inventoryXStart;
	private static float inventoryYStart;
	private static float inventoryBarYStart;
	private static float itemSizePx;
	private static float offset;
	private static float offsetY;
	private static float craftXStart;
	private static float craftYStart;

	private void ompleCraftings(){
		craftings = new int[numCrafting][];
		quantities = new int[numCrafting][];
		ids = new int[numCrafting];
		for(int i=0;i<numCrafting;i++){
			craftings[i] = new int[]{-1,-1,0,0};
			quantities[i] = new int[]{0,0,1,2};
			ids[i] = 2;

		}
	}

	void Start(){
		ompleCraftings();
		itemCraftejat = null;
		dragging = false;
		_inventory = (GUITexture)Instantiate(inventory);
		_inventory.name="Inventory GUI";
		_barra = (GUITexture)Instantiate(barra);
		_barra.name="Inventory Bar";
		_selector = (GUITexture)Instantiate(selector);
		_selector.name="Inventory Bar Selector";
		posInv=0;
		Vector3 pos = _barra.transform.position;
		startBarra = pos.x - (_barra.pixelInset.width/2)/Screen.width;
		stepBarra = (_barra.pixelInset.width/9) / Screen.width;
		pos.x = startBarra+posInv*stepBarra;
		pos.z = 1;
		_selector.transform.position = pos;
		itemsInv = new Item[27];
		itemsBarra = new Item[9];
		itemsCraft = new Item[4];
		_inventory.pixelInset = new Rect(-((Screen.height-64)/2),-((Screen.height-64)/2),Screen.height-64,Screen.height-64);
		itemSizePx = (Screen.height-64)/12.0f;
		float ample = (Screen.height-64)/2.0f;
		inventoryXStart = _inventory.transform.position.x - ample/Screen.width + (itemSizePx*0.52f)/Screen.width;
		inventoryYStart = _inventory.transform.position.y - ample/Screen.height + (itemSizePx/Screen.height)*2.36f;
		offset = itemSizePx*0.2f;
		offsetY = itemSizePx*0.3f;
		craftXStart = inventoryXStart + (itemSizePx*5f + offset*1.68f)/Screen.width;
		craftYStart = inventoryYStart + (itemSizePx*5f + offsetY*1.6f)/Screen.height;
		inventoryBarYStart = _inventory.transform.position.y - ample/Screen.height + (itemSizePx*0.8f)/Screen.height;
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
			foreach(Item i in itemsCraft){
				if(i!=null)i.enable();
			}
			if(itemCraftejat != null)itemCraftejat.enable();
		}
		else{
			foreach(Item i in itemsCraft){
				if(i!=null)i.disable();
			}
			foreach(Item i in itemsInv){
				if(i!=null)i.disable();
			}
			if(itemCraftejat != null)itemCraftejat.disable();
		}
	}

	public void addInventory(int id,byte quantitat){
		for(int i=0;i<9;i++){
			if(itemsBarra[i] != null && itemsBarra[i].getId() == id){
				byte added = itemsBarra[i].addQuantity(quantitat);
				if( quantitat != added) {
					addInventory(id,(byte)(quantitat-added));
				}
				else{
					return;
				}
			}
		}
		for(int i=0;i<27;i++){
			if(itemsInv[i] != null && itemsInv[i].getId() == id){
				byte added = itemsInv[i].addQuantity(quantitat);
				if( quantitat != added) {
					addInventory(id,(byte)(quantitat-added));
				}
				else{
					return;
				}
			}
		}
		for(int i=0;i<9;i++){
			if(itemsBarra[i]== null){
				addInventory(true,i,id,quantitat);
				return;
			}
		}
		for(int i=0;i<27;i++){
			if(itemsInv[i]== null){
				addInventory(true,i,id,quantitat);
				return;
			}
		}
	}

	private void addInventory(bool barra,int pos,int id,byte quantitat){
		GameObject go = (GameObject)Instantiate(prefab);
		Item it = go.GetComponent<Item>();
		Texture t;
		if(id < 0){
			t = texturesItem[-id];
		}
		else{
			t = World.getTextureBlock((BlockType)id);
		}

		if(barra){
			Vector3 posInv = new Vector3(inventoryXStart + (offset*pos)/Screen.width + (itemSizePx*pos)/Screen.width,inventoryBarYStart,2f);
			Vector3 posBarra = new Vector3(startBarra+stepBarra*pos,0f,2f);
			it.startGO(id,quantitat,t,itemSizePx,posInv,posBarra);
			go.name = "barra["+pos+"]";
			it.cambia(invEnabled);
			itemsBarra[pos] = it;
		}
		else {
			it.startGO(id,quantitat,t,itemSizePx,new Vector3(inventoryXStart + (offset*(pos%9))/Screen.width + (itemSizePx*(pos%9))/Screen.width,inventoryYStart + ((offsetY)*(pos/9))/Screen.height + (itemSizePx*(pos/9))/Screen.height,2f));
			if(invEnabled)
				it.cambia(true);
			go.name = "inventari["+pos+"]";
			itemsInv[pos] = it;
		}
			
	}

	public static void borrar(Item target,bool agresiu=false){
		for(int i=0;i<9;i++){
			if(itemsBarra[i] == target)itemsBarra[i]=null;
		}
		for(int i=0;i<27;i++){
			if(itemsInv[i] == target)itemsInv[i]=null;
		}
		for(int i=0;i<4;i++){
			if(itemsCraft[i] == target)itemsCraft[i]=null;
		}
		if(itemCraftejat == target)itemCraftejat = null;
		if(agresiu)Destroy(target.gameObject);
	}

	public static bool posValida(ref Vector3 pos,Item target,out bool esBarra,out Vector3 posBar){
		Vector3 pIt = new Vector3(inventoryXStart,inventoryBarYStart,2f);
		float despX = (offset+ itemSizePx)/Screen.width;
		float despY = (offsetY+ itemSizePx)/Screen.height;
		float tamX = itemSizePx/Screen.width;
		float tamY = itemSizePx/Screen.height;
		esBarra=false;
		posBar = default(Vector3);
		if(pos.x < inventoryXStart)return false;//borrar-lo
		if(pos.y < inventoryBarYStart)return false;
		for(int col=0;col<9;col++){
			if((pIt.x<=pos.x)&&(pIt.x+tamX>=pos.x)&&(pIt.y<=pos.y)&&(pIt.y+tamY>=pos.y)){
				if(itemsBarra[col] == null){ 
					pos = pIt;
					borrar(target);
					itemsBarra[col] = target;
					esBarra = true;
					posBar = new Vector3(startBarra+stepBarra*col,0f,2f);
					return true;
				}
				else{
					return false;
				}
			}
			pIt.x+=despX;
		}

		pIt.y=inventoryYStart;
		for(int fila=0;fila<3;fila++){
			pIt.x=inventoryXStart;
			for(int col=0;col<9;col++){
				if((pIt.x<=pos.x)&&(pIt.x+tamX>=pos.x)&&(pIt.y<=pos.y)&&(pIt.y+tamY>=pos.y)){
					if(itemsInv[fila*9+col] == null){ 
						pos = pIt;
						borrar(target);
						itemsInv[fila*9+col] = target;
						return true;
					}
					else{
						return false;
					}
				}
				pIt.x+=despX;
			}
			pIt.y+=despY;
		}
		//zona de craft
		pIt.y=craftYStart;
		for(int fila=0;fila<2;fila++){
			pIt.x=craftXStart;
			for(int col=0;col<2;col++){
				if((pIt.x<=pos.x)&&(pIt.x+tamX>=pos.x)&&(pIt.y<=pos.y)&&(pIt.y+tamY>=pos.y)){
					if(itemsCraft[fila*2+col] == null){ 
						pos = pIt;
						borrar(target);
						itemsCraft[fila*2+col] = target;
						int craftPotencial = comprovaCraft();
						if(craftPotencial!=-1){
							GameObject go = new GameObject();
							GUIText text = go.AddComponent<GUIText>();
							text.anchor = TextAnchor.LowerLeft;
							text.pixelOffset = new Vector2(10,10);
							text.fontSize = 10;
							text.color = Color.black;
							go.AddComponent<GUITexture>();
							Item it = go.AddComponent<Item>();
							int id = ids[craftPotencial];
							int min = 256;
							for(int i=0;i<4;i++){
								if(quantities[craftPotencial][i] > 0){
									if(min > itemsCraft[i].getQuantity()/quantities[craftPotencial][i])
										min = itemsCraft[i].getQuantity()/quantities[craftPotencial][i];
								}
							}
							Texture t;
							if(id < 0){
								t = texturesItem[-id];
							}
							else{
								t = World.getTextureBlock((BlockType)id);
							}
							it.startGO(id,(byte)min,t,itemSizePx,new Vector3(craftXStart+(itemSizePx*3.75f)/Screen.width,craftYStart+(itemSizePx*0.58f)/Screen.height,2f));
							go.name = "item craftejat";
							for(int i=0;i<4;i++){
								if(quantities[craftPotencial][i] != 0){
									if(itemsCraft[i].delQuantity((byte)(quantities[craftPotencial][i] * min))){
										borrar(itemsCraft[i],true);
									}
								}
							}
							itemCraftejat = it;
							it.cambia(true);
							//"eliminar" els items
						}
						return true;
					}
					else{
						return false;
					}
				}
				pIt.x+=despX;
			}
			pIt.y+=despY;
		}

		return false;
	}

	private static int comprovaCraft(){
		int[]mostra=new int[itemsCraft.Length];
		for(int i=0;i<mostra.Length;i++){
			if(itemsCraft[i]==null)mostra[i]= -1;
			else mostra[i] = itemsCraft[i].getId();
		}
		bool nextCraft;
		for(int i=0;i<numCrafting;i++){
			nextCraft = false;
			for(int pos = 0;pos<4 && !nextCraft;pos++){
				if(craftings[i][pos] != mostra[pos]){
					nextCraft = true;
				}
			}
			if(!nextCraft)return i;
		}
		return -1;
	}
	
	void Update () {
		if(Input.GetKeyDown(KeyCode.E)){
			invEnabled = !invEnabled;
			_inventory.enabled = invEnabled;
			_selector.enabled = !invEnabled;
			_barra.enabled = !invEnabled;
			GetComponent<MouseLook>().enabled = !invEnabled;
			GetComponent<MovementPlayer>().enabled = !invEnabled;
			GetComponent<MouseClick>().enabled = !invEnabled;
			pintaItems(invEnabled);
		}
		else if(!invEnabled){
			if (Input.GetAxis("Mouse ScrollWheel") > 0 && posInv > 0) {
				posInv--;
				Vector3 pos = _selector.transform.position;
				pos.x = startBarra+posInv*stepBarra;
				_selector.transform.position = pos;
			}
			else if (Input.GetAxis("Mouse ScrollWheel") < 0 && posInv < 8) {
				posInv++;
				Vector3 pos = _selector.transform.position;
				pos.x = startBarra+posInv*stepBarra;
				_selector.transform.position = pos;
			}
			else{
				for(int i = 0;i<9;i++){
					if(Input.GetKeyDown(i+KeyCode.Alpha1)){
						Vector3 pos = _selector.transform.position;
						pos.x = startBarra+i*stepBarra;
						_selector.transform.position = pos;
						posInv = i;
					}
				}
			}
		}
		else{
			if(Input.GetKeyDown(KeyCode.A)){
				for (int i=0;i<5;i++){
					addInventory(0,(byte)64);
				}
//				GameObject go = (GameObject)Instantiate(prefab);
//				Item it = go.GetComponent<Item>();
//				it.startGO(1,42,textures[1],itemSizePx,new Vector3(craftXStart,craftYStart,2f));
//				if(invEnabled)
//					it.cambia(true);
//				for (int i=0;i<9*3;i++){
//					addInventory(false,i,UnityEngine.Random.Range(0,textures.Length),(byte)42);
//				}
			}
		}
	}

	public bool getItem(out BlockType bt) {
		bt = BlockType.GoldOre;
		return false;
	}
}
