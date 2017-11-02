using UnityEngine;
using System.Collections;
using Vuforia;

public class ObjVisibilityHandler : MonoBehaviour {

  public GameObject centralMarker, origParentMarker;
  MarkerVisibilityHandler h;
  bool markerWasVisible;
  Renderer ren;
  Collider col;
  Vector3 origPos;
  Quaternion origRot;
  DefaultTrackableEventHandler dteh;
  bool parentWasVisible;

  void Start()
  {
    h = centralMarker.GetComponent<MarkerVisibilityHandler>();
    markerWasVisible = h.visible;
    ren = GetComponent<Renderer>();
    col = GetComponent<Collider>();
    origPos = transform.position;
    origRot = transform.rotation;
    dteh = origParentMarker.GetComponent<DefaultTrackableEventHandler>();
    parentWasVisible = dteh.visible;

    //Debug.Log("Translating object " + gameObject.name + " by " + (centralMarker.transform.position.y - transform.position.y + 0.5f * transform.localScale.y).ToString());
    //Debug.Log("-- Original y: " + transform.position.y.ToString() + "  New y: " + centralMarker.transform.position.y.ToString() + 0.5f * transform.localScale.y);
  }
  
	void Update ()
  {
    if(dteh.visible && !parentWasVisible) {
      HandleAppearance();
      markerWasVisible = true;
    }
    else if(!dteh.visible && parentWasVisible) {
      HandleDisappearance();
      markerWasVisible = false;
    }
	}

  void HandleAppearance()
  {
    gameObject.transform.parent = origParentMarker.transform;
    gameObject.transform.localPosition = Vector3.zero;
  }

  void HandleDisappearance()
  {
    gameObject.transform.parent = centralMarker.transform;
    gameObject.transform.localPosition = Vector3.zero;
    ren.enabled = col.enabled = false;
  }
}
