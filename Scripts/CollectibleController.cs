using UnityEngine;
using System.Collections;

public class CollectibleController : MonoBehaviour {

  public AudioClip sfx;
  GameObject goalshine;

  void Start()
  {
    goalshine = GameObject.FindGameObjectWithTag("GoalShine");
    goalshine.SetActive(false);
  }

  void OnTriggerEnter(Collider col)
  {
    if(col.tag == "Player") {
      TriggerFinalSequence();
    }
  }

  void TriggerFinalSequence()
  {
    GetComponent<AudioSource>().PlayOneShot(sfx);
    goalshine.SetActive(true);
    gameObject.active = false;
  }

  void Update()
  {
    if(Input.GetAxis("Cancel") != 0)
      TriggerFinalSequence();
  }
}
