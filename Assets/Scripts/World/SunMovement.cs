using UnityEngine;
using System.Collections;

public class SunMovement : MonoBehaviour {

	public GameObject sun;
	public GameObject sky;
	public GameObject moon;

	private float speed = 360.0f / (24.0f*30.0f); //one hour in game = half minute in real life

	//Days is set to simulate the moon phases
	private float days;
	private float sizeX;
	private float sizeZ;
	private float astroHeight;
	
	void Start () {
		days = 0.0f;

		sizeX = MineChunk.sizeX * MineWorld.sizeX;
		sizeZ = MineChunk.sizeZ * MineWorld.sizeZ;

		astroHeight = sun.transform.position.y;
	}

	// Update is called once per frame
	void Update () {
		//Sun movement
		sun.transform.RotateAround(new Vector3(sizeX/2.0f, 0.0f, sizeZ/2.0f), Vector3.left, speed*Time.deltaTime);

		//Moon movement
		moon.transform.RotateAround(new Vector3(sizeX/2.0f, 0.0f, sizeZ/2.0f), Vector3.left, speed*Time.deltaTime);

		//Sky color
		float sunHeight = sun.transform.position.y;	//from [-astroHeight,astroHeight]
		sunHeight += astroHeight; //from [0,astroHeight*2]
		sky.camera.backgroundColor = new Color(0.0f, 0.0f + sunHeight/(astroHeight*2.0f) - 0.25f, 0.0f + sunHeight/(astroHeight*2.0f) - 0.25f, 1.0f);

		//Sun and Moon will be less intense as is falling down
		if(sunHeight >= astroHeight) {
			sun.light.intensity = 0.3f * (sunHeight/(astroHeight*2.0f));
			moon.light.intensity = 0.0f;
		} else {
			sun.light.intensity = 0.0f;
			moon.light.intensity = 0.1f * (sunHeight/(astroHeight*2.0f));
		}

		//We have to increment the days passed when sunHeight == astroHeight
		//but only one time every two
		if(sunHeight == astroHeight) {
			days += 0.5f;
			if(days==28.0f) days = 0.0f;
		}
	}
}
