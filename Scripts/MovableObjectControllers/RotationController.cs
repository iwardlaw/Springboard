using UnityEngine;
using Vuforia;

public class RotationController : MonoBehaviour {

  public GameObject centralMarker, parentMarker;
  public float maxRotation = 5f;
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
    public float xThreshold = 1f;
    public float yThreshold = 1f;
    public float zThreshold = 1f;
  }

  [System.Serializable]
  public class Locks {
    public bool lockX = false;
    public bool lockY = false;
    public bool lockZ = false;
  }

  Vector3 originalRotation, newRotation, prevNewRotation, targetRotation;
  public MasterAxes masterAxes = new MasterAxes();
  public Offsets offsets = new Offsets();
  public Multipliers multipliers = new Multipliers();
  public Locks locks = new Locks();
  public Thresholds thresholds = new Thresholds();
  DefaultTrackableEventHandler dteh;
  float minRotation = 0f, stopThreshold = 5f;
  Vector3 thresholdTest;
  float xDiff, yDiff, zDiff;

  void Start()
  {
    dteh = parentMarker.GetComponent<DefaultTrackableEventHandler>();
    originalRotation = targetRotation = transform.rotation.eulerAngles;
    prevNewRotation = parentMarker.transform.rotation.eulerAngles;
    thresholdTest = new Vector3(0f, 0f, 0f);
  }
  
  void Update ()
  {
    if(dteh.visible) {
      if(locks.lockX) newRotation.x = originalRotation.x;
      else {
        newRotation.x = getMasterAxisValue(AxisType.X_AXIS);
        minRotation = Mathf.Min(Mathf.Abs(transform.rotation.eulerAngles.x - newRotation.x), Mathf.Abs(360f + transform.rotation.eulerAngles.x - newRotation.x));
        if(minRotation >= thresholds.xThreshold)
          newRotation.x *= multipliers.xMultiplier;
        else
          newRotation.x = transform.rotation.eulerAngles.x;
        newRotation.x += offsets.xOffset;
      }

      if(locks.lockY) newRotation.y = originalRotation.y;
      else {
        newRotation.y = getMasterAxisValue(AxisType.Y_AXIS);
        minRotation = Mathf.Min(Mathf.Abs(transform.rotation.eulerAngles.y - newRotation.y), Mathf.Abs(360f + transform.rotation.eulerAngles.y - newRotation.y));
        if(minRotation >= thresholds.yThreshold)
          newRotation.y *= multipliers.yMultiplier;
        else
          newRotation.y = transform.rotation.eulerAngles.y;
        newRotation.y += offsets.yOffset;
      }

      if(locks.lockZ) newRotation.z = originalRotation.z;
      else {
        newRotation.z = getMasterAxisValue(AxisType.Z_AXIS);
        minRotation = Mathf.Min(Mathf.Abs(transform.rotation.eulerAngles.z - newRotation.z), Mathf.Abs(360f + transform.rotation.eulerAngles.z - newRotation.z));
        if(minRotation >= thresholds.zThreshold)
          newRotation.z *= multipliers.zMultiplier;
        else
          newRotation.z = transform.rotation.eulerAngles.z;
        newRotation.z += offsets.zOffset;
      }

      newRotation.x = wrapMod(newRotation.x, 360);
      newRotation.y = wrapMod(newRotation.y, 360);
      newRotation.z = wrapMod(newRotation.z, 360);

      xDiff = newRotation.x - transform.eulerAngles.x;
      if(xDiff > 180f) xDiff = xDiff - 360f;
      else if(xDiff < -180f) xDiff = xDiff + 360f;
      yDiff = newRotation.y - transform.eulerAngles.y;
      if(yDiff > 180f) yDiff = yDiff - 360f;
      else if(yDiff < -180f) yDiff = yDiff + 360f;
      zDiff = newRotation.z - transform.eulerAngles.z;
      if(zDiff > 180f) zDiff = zDiff - 360f;
      else if(zDiff < -180f) zDiff = zDiff + 360;

      if(Mathf.Abs(xDiff) > stopThreshold)
        newRotation.x = transform.eulerAngles.x + (xDiff > 0f ? Mathf.Min(maxRotation, xDiff * overallMultiplier) : Mathf.Max(-maxRotation, xDiff * overallMultiplier));
      if(Mathf.Abs(yDiff) > stopThreshold)
        newRotation.y = transform.eulerAngles.y + (yDiff > 0f ? Mathf.Min(maxRotation, yDiff * overallMultiplier) : Mathf.Max(-maxRotation, yDiff * overallMultiplier));
      if(Mathf.Abs(zDiff) > stopThreshold)
        newRotation.z = transform.eulerAngles.z + (zDiff > 0f ? Mathf.Min(maxRotation, zDiff * overallMultiplier) : Mathf.Max(-maxRotation, zDiff * overallMultiplier));
      transform.eulerAngles = newRotation;
    }
  }

  float getMasterAxisValue(AxisType a)
  {
    switch(a) {
      case AxisType.X_AXIS:
        switch(masterAxes.xMaster) {
          case AxisType.Y_AXIS:
            return parentMarker.transform.rotation.eulerAngles.y;
          case AxisType.Z_AXIS:
            return parentMarker.transform.rotation.eulerAngles.z;
          default:
            return parentMarker.transform.rotation.eulerAngles.x;
        }
      case AxisType.Y_AXIS:
        switch(masterAxes.yMaster) {
          case AxisType.X_AXIS:
            return parentMarker.transform.rotation.eulerAngles.x;
          case AxisType.Z_AXIS:
            return parentMarker.transform.rotation.eulerAngles.z;
          default:
            return parentMarker.transform.rotation.eulerAngles.y;
        }
      case AxisType.Z_AXIS:
        switch(masterAxes.zMaster) {
          case AxisType.X_AXIS:
            return parentMarker.transform.rotation.eulerAngles.x;
          case AxisType.Y_AXIS:
            return parentMarker.transform.rotation.eulerAngles.y;
          default:
            return parentMarker.transform.rotation.eulerAngles.z;
        }
      default:
        return 0f;
    }
  }

  float wrapMod(float a, int b)
  {
    int aInt = (int)a;
    float aDec = a - aInt;

    aInt %= b;
    if(aInt < 0) aInt += b;
    if(aInt == 0 && aDec < 0f) aInt = b;
    return (float)aInt + aDec;
  }
}
