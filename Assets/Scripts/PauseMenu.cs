using UnityEngine;
using System.Collections;

public class PauseMenu : MonoBehaviour {

	private bool isPause;
	private bool isInstructions;

	private Rect mainMenu;
	private GUI.WindowFunction windowFunction;
	private Rect instrucctions;
	private GUI.WindowFunction theInstructionsFunction;

	// Use this for initialization
	void Start () {
		isPause = false;
		mainMenu = new Rect(10f, 10f, 200f, 100.0f);
		mainMenu.center = new Vector2(Screen.width/2, Screen.height/2);
		windowFunction = theMainMenu;

		isInstructions = false;
		instrucctions = new Rect(10f, 10f, 300.0f, 275.0f);
		instrucctions.center = new Vector2(Screen.width/2, Screen.height/2);
		theInstructionsFunction = theInstructions;
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Escape) && !isInstructions) {
			isPause = !isPause;
			if(isPause) Time.timeScale = 0f;
			else Time.timeScale = 1f;
		}
		Screen.showCursor = isPause;
	}

	void OnGUI() {
		if(isPause && !isInstructions) {
			GUI.Window(0, mainMenu, windowFunction, "Pause Menu");
		} else if(isInstructions) {
			GUI.Window (1, instrucctions, theInstructionsFunction, "Instructions");
		}
	}
	
	void theMainMenu(int windowID) {
		if(GUILayout.Button ("Resume")) {
			isPause = false;
			Time.timeScale = 1f;
		}
		if(GUILayout.Button ("Instructions")) {
			isInstructions = true;
		}
		if(GUILayout.Button ("Quit")) {
			Application.Quit ();
		}
		GUI.DragWindow();
	}

	void theInstructions(int windowID) {
		GUI.skin.textField.alignment = TextAnchor.MiddleCenter;
		GUILayout.TextField("PLAYER CONTROLS\n" +
							"Forward - W\n" +
							"Left - A\n" +
							"Right - D\n" +
		                    "Back - S\n" +
		                    "Change Weapon - Q\n" +
		                    "Open Inventory - E\n" +
		                    "Fast Item - 1..9\n" +
		                    "Attack - Left Click\n" +
		                    "Put Item - Right Click\n" +
		                    "\n" +
		                    "INVENTORY CONTROLS\n" +
		                    "Drag & Drop - Left Click\n" +
		                    "Destroy Item - Middle Click");
		if(GUILayout.Button ("Return")) {
			isInstructions = false;
		}
	}

	public bool isPaused() {
		return isPause;
	}
}
