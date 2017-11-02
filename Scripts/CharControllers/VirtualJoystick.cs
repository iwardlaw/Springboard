using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler {

  Image bgImg, jsImg;
  Vector3 inputVec;
  CharacterController player;
  
  void Start ()
  {
    bgImg = GetComponent<Image>();
    jsImg = transform.GetChild(0).GetComponent<Image>();
    player = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterController>();
  }

  void Update()
  {
    if(!player.alive)
      OnPointerUp(null);
  }

  public virtual void OnDrag(PointerEventData ped)
  {
    Vector2 pos;
    if(player.alive && RectTransformUtility.ScreenPointToLocalPointInRectangle(bgImg.rectTransform, ped.position, ped.pressEventCamera, out pos)) {
      pos.x /= bgImg.rectTransform.sizeDelta.x;
      pos.y /= bgImg.rectTransform.sizeDelta.y;

      inputVec = new Vector3(pos.x * 2 - 1, 0f, pos.y * 2 - 1);
      if(inputVec.magnitude > 1f) inputVec.Normalize();

      jsImg.rectTransform.anchoredPosition =
        new Vector3(inputVec.x * bgImg.rectTransform.sizeDelta.x * 0.4f,
          inputVec.z * bgImg.rectTransform.sizeDelta.y * 0.4f);
    }
  }

  public virtual void OnPointerDown(PointerEventData ped)
  {
    OnDrag(ped);
  }

  public virtual void OnPointerUp(PointerEventData ped)
  {
    inputVec = Vector3.zero;
    jsImg.rectTransform.anchoredPosition = Vector3.zero;
  }

  public float Horizontal()
  {
    if(inputVec.x != 0) return inputVec.x;
    else return Input.GetAxis("Horizontal");
  }

  public float Vertical()
  {
    if(inputVec.z != 0) return inputVec.z;
    else return Input.GetAxis("Vertical");
  }
}
