using UnityEngine;
using System.Collections;

public class Item:MonoBehaviour {
	private int id;
	private byte quantity;
	private Vector3 posBarra;
	private Vector3 posInv;
	private float tam;
	private Vector3 offsetClick;
	private bool dragging = false;

	public void startGO(int id,byte quant,Texture text,float tam,Vector3 posInv,Vector3 posBarra = default(Vector3)){
		this.id=id;
		this.quantity = quant;
		this.tam = tam;
		Rect r = new Rect(0,0,tam,tam);
		transform.localScale = new Vector3(0,0,0);
		guiTexture.pixelInset = r;
		guiTexture.texture = text;
		guiTexture.enabled = false;
		guiText.enabled = false;
		guiText.text=quantity.ToString();
		this.posInv = posInv;
		this.posBarra = posBarra;
	}

	public void setPosBarra(Vector3 pos){
		posBarra = pos;
	}

	public void setPosInv(Vector3 pos){
		posInv = pos;
	}

	public void cambia(bool modo){//true = inventari false=barra
		if(modo){//inventari
			guiText.transform.position = posInv;
			guiTexture.transform.position = posInv;
			Rect r = new Rect(0,0,tam,tam);
			guiTexture.pixelInset = r;
		}
		else{//barra
			guiTexture.transform.position = posBarra;
			guiText.transform.position = posBarra;
			Rect r = new Rect(6,6,50,50);
			guiTexture.pixelInset = r;
		}
		enable();
	}

	public int getId(){
		return id;
	}

	public byte getQuantity(){
		return quantity;
	}

	public void enable(){
		guiText.enabled = true;
		guiTexture.enabled = true;
	}

	public void disable(){
		guiText.enabled = false;
		guiTexture.enabled = false;
	}

	public byte addQuantity(byte num){
		byte oldQuant = quantity;
		quantity=(byte)Mathf.Min(quantity+num,255);
		guiText.text = quantity.ToString();
		return (byte)(quantity - oldQuant);
	}

	public bool delQuantity(byte num){
		quantity=(byte)Mathf.Max(quantity-num,0);
		guiText.text = quantity.ToString();
		return quantity<=0;
	}	

	public void mou(Vector3 pos){
		guiText.transform.position=pos;
		guiTexture.transform.position=pos;
	}
	
	void Update(){
		if(Input.GetMouseButton(2))InventoryManagment.borrar(this,true);
		if(InventoryManagment.invEnabled){
			if(InventoryManagment.dragging && dragging){
				Vector3 pos = Input.mousePosition;
				pos.x /=Screen.width;
				pos.y/=Screen.height;
				pos.z=3f;
				mou (pos - offsetClick);
				if(Input.GetMouseButton(0)){
					Vector3 posRat = new Vector3();
					posRat.x = Input.mousePosition.x/Screen.width;
					posRat.y = Input.mousePosition.y/Screen.height;
					bool esBarra = false;
					Vector3 posBar = default(Vector3);
					if(InventoryManagment.posValida(ref posRat,this,out esBarra,out posBar)){
						InventoryManagment.dragging = false;
						dragging = false;
						mou(posRat);
						posInv = posRat;
						if(esBarra){
							posBarra = posBar;
						}
					}
				}
//				if(Input.GetMouseButton(1)){
//					if(InventoryManagment.posValida(Input.mousePosition)){
//						
//					}
//					//if posicio on l'he deixat es invalida mou-l'ho a on estava abans
//					//else cambiali la pos per defecte
//				}
			}
			else{
				if(Input.GetMouseButtonDown(0) && !dragging && !InventoryManagment.dragging){
					Vector3 pos = Input.mousePosition;
					pos.x /=Screen.width;
					pos.y/=Screen.height;
					if(pos.x>=guiTexture.transform.position.x && pos.y>=guiTexture.transform.position.y && pos.x<=guiTexture.transform.position.x + tam/Screen.width && pos.y<=guiTexture.transform.position.y + tam/Screen.height){
						InventoryManagment.dragging = true;
						dragging =true;
						pos.z = guiTexture.transform.position.z;
						offsetClick = pos - guiTexture.transform.position;
					}
				}
			}
		}
	}
}


