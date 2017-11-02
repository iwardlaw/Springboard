using UnityEngine;
using System.Collections;

public class InputControllerTest : MonoBehaviour {

  public bool checkForControllers = true;
  public float measurementInterval = 1f;
  float measurementStart;
  bool firstRun = true;
  bool connected = false;

  public float msgWindowTimeout = 3f;
  float msgWindowTimeStart;
  bool msgWindowActive = false;
  const int margin = 20;
  Rect windowRect = new Rect(margin, margin, Screen.width - (margin * 2), Screen.height - (margin * 2));
  string displayMessage = "";

  VirtualAButton aButton;
  VirtualBButton bButton;
  
	void Start ()
  {
    measurementStart = msgWindowTimeStart = Time.time;
    if(Input.GetJoystickNames().Length > 0)
      connected = true;
    else
      connected = false;
    HandleConnectionStateChange();

    aButton = GameObject.FindGameObjectWithTag("ButtonA").GetComponent<VirtualAButton>();
    bButton = GameObject.FindGameObjectWithTag("ButtonB").GetComponent<VirtualBButton>();
  }
	
	void Update ()
  {
    if(aButton.Pressure() == 1f) {
      msgWindowActive = true;
      displayMessage = "'A' button pressed.";
    }
    if(bButton.Pressure() == 1f) {
      msgWindowActive = false;
      displayMessage = "'B' button pressed.";
    }

    if(msgWindowActive && connected && Time.time - msgWindowTimeStart >= msgWindowTimeout)
      msgWindowActive = false;
  }

  IEnumerator CheckForControllers()
  {
    while(checkForControllers) {
      var controllers = Input.GetJoystickNames();
      if(!connected && controllers.Length > 0) {
        connected = true;
        HandleConnectionStateChange();
      }
      else if(connected && controllers.Length == 0) {
        connected = false;
        HandleConnectionStateChange();
      }
      //Debug.Log("Yielding from CheckForControllers().");
      yield return new WaitForSeconds(measurementInterval);
    }
  }

  void Awake()
  {
    StartCoroutine(CheckForControllers());
  }

  void HandleConnectionStateChange()
  {
    msgWindowActive = true;

    if(connected) {
      msgWindowTimeStart = Time.time;
      // Do something.
    }
    else {
      // Do something else.
    }

    DisplayConnectionState();
  }

  void DisplayConnectionState()
  {
    if(connected) {
      displayMessage = "Controller(s) connected:\n";
      if(Input.GetJoystickNames().Length == 0)
        displayMessage += "[none]";
      else foreach(string name in Input.GetJoystickNames())
        displayMessage += name + "\n";
    }
    else
      displayMessage = "No controllers connected.";
    Debug.Log(displayMessage);
  }

  void OnGUI()
  {
    if(false) {
      if(msgWindowActive)
        windowRect = GUILayout.Window(888, windowRect, ConsoleWindow, "");
    }
  }

  void ConsoleWindow(int windowID)
  {
    //GUI.contentColor = Color.white;
    GUILayout.Label("<color=white><size=40>" + displayMessage + "</size></color>");
  }
}
