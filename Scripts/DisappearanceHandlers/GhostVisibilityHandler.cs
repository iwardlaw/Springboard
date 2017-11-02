using UnityEngine;

public class GhostVisibilityHandler : MonoBehaviour {

  public GameObject centralMarker;
  public GameObject realObject;
  MarkerVisibilityHandler h;
  bool markerWasVisible;
  Renderer ren;
  Collider col;

  void Start()
  {
    h = centralMarker.GetComponent<MarkerVisibilityHandler>();
    markerWasVisible = h.visible;
    ren = GetComponent<Renderer>();
    col = GetComponent<Collider>();
    ren.enabled = col.enabled = false;
  }
  
  void Update ()
  {
    if(!markerWasVisible && h.visible) {
      HandleAppearance();
      markerWasVisible = true;
    }
    else if(markerWasVisible && !h.visible) {
      HandleDisappearance();
      markerWasVisible = false;
    }
  }

  void HandleAppearance()
  {
    ren.enabled = col.enabled = false;
  }

  void HandleDisappearance()
  {
    transform.position = realObject.transform.position;
    transform.rotation = realObject.transform.rotation;
    ren.enabled = col.enabled = true;
  }
}
