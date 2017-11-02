using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class VirtualBButton : MonoBehaviour, IPointerUpHandler, IPointerDownHandler {

  Image img;
  float radius;
  Vector2 center;
  bool input;
  
  void Start ()
  {
    img = GetComponent<Image>();
    radius = img.rectTransform.sizeDelta.x * 0.5f;
    center = new Vector2(img.rectTransform.position.x - radius, img.rectTransform.position.y + radius);
    input = false;
  }

  public virtual void OnPointerDown(PointerEventData ped)
  {
    if(Vector2.Distance(ped.position, center) <= radius)
      input = true;
    else
      input = false;
  }

  public virtual void OnPointerUp(PointerEventData ped)
  {
    input = false;
  }

  public float Pressure()
  {
    if(input) return 1f;
    else return Input.GetAxis("Fire1");
  }
}
