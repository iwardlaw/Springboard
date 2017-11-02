using UnityEngine;
using Vuforia;

public class PositionController : MonoBehaviour {

  public GameObject centralMarker, parentMarker;
  public float maxTransposition = 0.5f;
  public float overallMultiplier = 1f;

  public enum AxisType {X_AXIS, Y_AXIS, Z_AXIS};

  [System.Serializable]
  public class MasterAxes {
    public AxisType xMaster = AxisType.X_AXIS;
    public AxisType yMaster = AxisType.Y_AXIS;
    public AxisType zMaster = AxisType.Z_AXIS;
  }

  [System.Serializable]
  public class Offsets {
    public float xOffset = 0f;
    public float yOffset = 0f;
    public float zOffset = 0f;
  }

  [System.Serializable]
  public class Multipliers {
    public float xMultiplier = 1f;
    public float yMultiplier = 1f;
    public float zMultiplier = 1f;
  }

  [System.Serializable]
  public class Thresholds {
    public float xThreshold = 0.5f;
    public float yThreshold = 0.5f;
    public float zThreshold = 0.5f;
  }

  [System.Serializable]
  public class Locks {
    public bool lockX = false;
    public bool lockY = false;
    public bool lockZ = false;
  }

  Vector3 originalPosition, originalParentPosition, newPosition;
  public MasterAxes masterAxes = new MasterAxes();
  public Offsets offsets = new Offsets();
  public Multipliers multipliers = new Multipliers();
  public Locks locks = new Locks();
  public Thresholds thresholds = new Thresholds();
  DefaultTrackableEventHandler dteh;
  float stopThreshold = 0.5f;
  float xDiff, yDiff, zDiff;

  void Start()
  {
    dteh = parentMarker.GetComponent<DefaultTrackableEventHandler>();
    originalPosition = transform.position;
    originalParentPosition = parentMarker.transform.position;
  }

  void Update()
  {
    if(dteh.visible) {

      if(locks.lockX) newPosition.x = originalPosition.x;
      else {
        newPosition.x = getMasterAxisValue(AxisType.X_AXIS);
        if(Mathf.Abs(transform.position.x - newPosition.x) >= thresholds.xThreshold)
          newPosition.x *= multipliers.xMultiplier;
        else
          newPosition.x = transform.position.x;
        newPosition.x += offsets.xOffset;
      }

      if(locks.lockY) newPosition.y = originalPosition.y;
      else {
        newPosition.y = getMasterAxisValue(AxisType.Y_AXIS);
        if(Mathf.Abs(transform.position.y - newPosition.y) >= thresholds.yThreshold)
          newPosition.y *= multipliers.yMultiplier;
        else
          newPosition.y = transform.position.y;
        newPosition.y += offsets.yOffset;
      }

      if(locks.lockZ) newPosition.z = originalPosition.z;
      else {
        newPosition.z = getMasterAxisValue(AxisType.Z_AXIS);
        if(Mathf.Abs(transform.position.z - newPosition.z) >= thresholds.zThreshold)
          newPosition.z *= multipliers.zMultiplier;
        else
          newPosition.z = transform.position.z;
        newPosition.z += offsets.zOffset;
      }

      xDiff = newPosition.x - transform.position.x;
      yDiff = newPosition.y - transform.position.y;
      zDiff = newPosition.z - transform.position.z;

      if(Mathf.Abs(xDiff) > stopThreshold)
        newPosition.x = transform.position.x + (xDiff > 0f ? Mathf.Min(maxTransposition, xDiff * overallMultiplier) : Mathf.Max(-maxTransposition, xDiff * overallMultiplier));
      if(Mathf.Abs(yDiff) > stopThreshold)
        newPosition.y = transform.position.y + (yDiff > 0f ? Mathf.Min(maxTransposition, yDiff * overallMultiplier) : Mathf.Max(-maxTransposition, yDiff * overallMultiplier));
      if(Mathf.Abs(zDiff) > stopThreshold)
        newPosition.z = transform.position.z + (zDiff > 0f ? Mathf.Min(maxTransposition, zDiff * overallMultiplier) : Mathf.Max(-maxTransposition, zDiff * overallMultiplier));
      
      transform.position = newPosition;
    }
  }

  float getMasterAxisValue(AxisType a)
  {
    switch(a) {
      case AxisType.X_AXIS:
        switch(masterAxes.xMaster) {
          case AxisType.Y_AXIS:
            return parentMarker.transform.position.y;
          case AxisType.Z_AXIS:
            return parentMarker.transform.position.z;
          default:
            return parentMarker.transform.position.x;
        }
      case AxisType.Y_AXIS:
        switch(masterAxes.yMaster) {
          case AxisType.X_AXIS:
            return parentMarker.transform.position.x;
          case AxisType.Z_AXIS:
            return parentMarker.transform.position.z;
          default:
            return parentMarker.transform.position.y;
        }
      case AxisType.Z_AXIS:
        switch(masterAxes.zMaster) {
          case AxisType.X_AXIS:
            return parentMarker.transform.position.x;
          case AxisType.Y_AXIS:
            return parentMarker.transform.position.y;
          default:
            return parentMarker.transform.position.z;
        }
      default:
        return 0f;
    }
  }
}
