using UnityEngine;
using System.Collections;

public class CountingOfTime : MonoBehaviour {

	public GameObject sun;
	public Camera sky;
	public GameObject moon;

	private float speed = 360.0f / (24.0f*60f); //one hour in game = minute in real life
	private float time;

	//Days is set to simulate the moon phases
	private float days;
	private float sizeX;
	private float sizeZ;
	private float astroHeight;
	private bool isNight;

	//Controlling the months and the years
	private int day;
	private int month;
	private int year;
	
	void Start () {
		days = 0.0f;
		time = 0.0f;
		isNight = false;

		day = 1;
		month = 1;
		year = 0; //al (after Llaberia)

		sizeX = Chunk.sizex * World.sizex;
		sizeZ = Chunk.sizez * World.sizez;

		sun.transform.position = new Vector3(sizeX / 2.0f, Chunk.sizey*2.0f, sizeZ / 2.0f);
		moon.transform.position = new Vector3(sizeX / 2.0f, -(Chunk.sizey*2.0f), sizeZ / 2.0f);

		astroHeight = sun.transform.position.y;
	}

	// Update is called once per frame
	void Update () {
		time+=Time.deltaTime;
		if(time >= 1.0f) {
			//Sun movement
			sun.transform.RotateAround(new Vector3(sizeX/2.0f, 0.0f, sizeZ/2.0f), Vector3.left, speed*Time.deltaTime);

			//Moon movement
			moon.transform.RotateAround(new Vector3(sizeX/2.0f, 0.0f, sizeZ/2.0f), Vector3.left, speed*Time.deltaTime);

			time = 0.0f;
		}

		//Sky color
		float sunHeight = sun.transform.position.y;	//from [-astroHeight,astroHeight]
		sunHeight += astroHeight; //from [0,astroHeight*2]
		if(sky != null) {
			sky.camera.backgroundColor = new Color(0.0f, 0.0f + sunHeight/(astroHeight*2.0f) - 0.25f, 0.0f + sunHeight/(astroHeight*2.0f) - 0.25f, 1.0f);
			float aux = Mathf.Max(0.0f, sunHeight/astroHeight);
			aux /= 3.0f;
			aux += 0.1f;
			RenderSettings.ambientLight = new Color(aux, aux, aux);
		}

		//Sun and Moon will be less intense as are falling down
		if(sunHeight >= astroHeight) {
			sun.light.intensity = 1.0f * (sunHeight/(astroHeight*2.0f));
			moon.light.intensity = 0.0f;
			if(isNight) isNight = false;
		} else {
			sun.light.intensity = 0.0f;
			moon.light.intensity = 0.1f * (sunHeight/(astroHeight*2.0f));
			if(!isNight) isNight = true;
		}

		//We have to increment the days passed when sunHeight == astroHeight
		//but only one time every two
		if(sunHeight == astroHeight) {
			if(sun.light.intensity <= 0.0f) isNight = false;
			else isNight = true;
			days += 0.5f;
			if(days==28.0f) days = 0.0f;
			day++;
			if(month==1 || month==3 || month==5 || month==7 || month==8 || month==10 || month==12) {
				if(day==32) {
					++month;
					if(month==13) { 
						month=1;
						++year;
					}
					day = 1;
				}
			} else if(month==2) {
				if(day==29) {
					++month;
					day = 1;
				}
			} else {
				if(day==31) {
					++month;
					day = 1;
				}
			}
		}
	}

	public bool ThisIsNight() {
		return isNight;
	}
}
