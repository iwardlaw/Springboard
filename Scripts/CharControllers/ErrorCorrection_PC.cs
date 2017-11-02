using UnityEngine;

public class ErrorCorrection_PC : MonoBehaviour {

  public float floatTime = 1f;
  float startTime;
  bool checking;

  void Start()
  {
    startTime = Time.time;
    checking = true;
  }
	
	void Update ()
  {
    if(checking && Time.time - startTime >= floatTime) {
      GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
      checking = false;
    }
	}
}
