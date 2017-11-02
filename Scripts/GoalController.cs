using UnityEngine;
using System.Collections;

public class GoalController : MonoBehaviour {

  GameObject goalShine;
  public string victoryMessage = "You win.";
  public AudioClip goalSFX;
  static float margin = 20;
  Rect windowRect = new Rect(margin, margin, Screen.width - (margin * 2), Screen.height - (margin * 2));
  bool msgWindowActive = false;

  void Start()
  {
    goalShine = GameObject.FindGameObjectWithTag("GoalShine");
  }

  void OnTriggerEnter(Collider col)
  {
    if(goalShine.activeSelf && col.tag == "Player") {
      GetComponent<AudioSource>().PlayOneShot(goalSFX);
      msgWindowActive = true;
    }
  }

  void OnGUI()
  {
    if(msgWindowActive)
      windowRect = GUILayout.Window(1001, windowRect, ConsoleWindow, "");
  }

  void ConsoleWindow(int windowID)
  {
    GUILayout.Label("<color=white><size=60>" + victoryMessage + "</size></color>");
  }
}
