using UnityEngine;
using System.Collections;

public class PauseMenu : MonoBehaviour {

	private bool isPause;

	private Rect mainMenu;
	private GUI.WindowFunction windowFunction;

	// Use this for initialization
	void Start () {
		isPause = false;
		mainMenu = new Rect(10f, 10f, 200f, 100.0f);
		mainMenu.center = new Vector2(Screen.width/2, Screen.height/2);
		windowFunction = theMainMenu;
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Escape)) {
			isPause = !isPause;
			if(isPause) Time.timeScale = 0f;
			else Time.timeScale = 1f;
		}
		Screen.showCursor = isPause;
	}

	void OnGUI() {
		if(isPause) {
			GUI.Window(0, mainMenu, windowFunction, "Pause Menu");
		}
	}

	void theMainMenu(int windowID) {
		if(GUILayout.Button ("Resume")) {
			isPause = false;
			Time.timeScale = 1f;
		}
		if(GUILayout.Button ("Quit")) {
			Application.Quit ();
		}
		GUI.DragWindow();
	}

	public bool isPaused() {
		return isPause;
	}
}
