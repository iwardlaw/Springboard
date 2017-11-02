using UnityEngine;

public class CameraController : MonoBehaviour {

  public Transform target;
  public float lookSmooth = 0.09f;
  public Vector3 offsetFromTarget = new Vector3(0f, 6f, -8f);
  public float xTilt = 10f;

  Vector3 destination = Vector3.zero;
  CharacterController charController;
  float rotateVel = 0f;


  void Start()
  {
    SetCameraTarget(target);
  }

  public void SetCameraTarget(Transform t)
  {
    target = t;
    if(target != null)
      if(target.GetComponent<CharacterController>())
        charController = target.GetComponent<CharacterController>();
      else
        Debug.LogError("Your camera's target needs a CharacterController component attached.");
    else
      Debug.LogError("Your camera needs a target.");
  }

  void LateUpdate()
  {
    // Move:
    MoveToTarget();
    // Rotate:
    LookAtTarget();
  }

  void MoveToTarget()
  {
    destination = charController.TargetRotation * offsetFromTarget;
    destination += target.position;
    transform.position = destination;
  }

  void LookAtTarget()
  {
    float eulerYAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, target.eulerAngles.y, ref rotateVel, lookSmooth);
    transform.rotation = Quaternion.Euler(transform.eulerAngles.x, eulerYAngle, 0f);
  }
}
