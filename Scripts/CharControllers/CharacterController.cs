using UnityEngine;

public class CharacterController : MonoBehaviour {

  [System.Serializable]
  public class MoveSettings {
    public float forwardVel = 3f;
    public float rotateVel = 80f;
    public float jumpVel = 15f;
    public float distToGrounded = 0.4f;
    public LayerMask ground;
  }

  [System.Serializable]
  public class PhysSettings {
    public float downAccel = 0.75f;
  }

  [System.Serializable]
  public class CharSettings {
    public int maxHP = 7;
    public int attack = 1;
    public int defense = 1;
    public int speed = 1;
  }
  public int hp;
  public bool alive = true;

  public AudioClip dieSFX;
  public AudioClip hitSFX1;
  public AudioClip hitSFX2;
  public AudioClip attackSFX1;
  public AudioClip attackSFX2;
  public AudioClip jumpSFX;
  AudioSource audioSrc;
  public float hitSFX1Rate = 0.5f;
  public float hitSFX2Rate = 0.5f;
  public float attackSFX1Rate = 0.5f;
  public float attackSFX2Rate = 0.5f;
  
  public float inputDelay = 0.1f;

  public float attackCooldown = 1f;
  float attackStartTime = 0f;
  public float attackRange = 2f;

  public float invincibilityWindow = 1f;
  float timeLastHit;

  public MoveSettings moveSettings = new MoveSettings();
  public PhysSettings physSettings = new PhysSettings();
  public CharSettings charSettings = new CharSettings();
  VirtualJoystick joystick;
  VirtualAButton aButton;
  VirtualBButton bButton;

  Vector3 velocity = Vector3.zero;
  Quaternion targetRotation;
  Rigidbody rBody;
  float forwardInput, turnInput, jumpInput, attackInput;
  public string animationName;
  Animation anim;
  bool midJump;
  GameObject groundObject;
  float groundLevel;

  static float msgWindowMargin = 20;
  Rect msgWindowRect = new Rect(msgWindowMargin, msgWindowMargin, Screen.width - (msgWindowMargin * 2), Screen.height - (msgWindowMargin * 2));
  public string gameOverMsg = "Game Over";

  public Quaternion TargetRotation {
    get { return targetRotation; }
  }

  void Start()
  {
    hp = charSettings.maxHP;

    targetRotation = transform.rotation;

    if(GetComponent<Rigidbody>())
      rBody = GetComponent<Rigidbody>();
    else
      Debug.LogError("Your character must have a Rigidbody component attached.");

    joystick = GameObject.FindGameObjectWithTag("Joystick").GetComponent<VirtualJoystick>();
    aButton = GameObject.FindGameObjectWithTag("ButtonA").GetComponent<VirtualAButton>();
    bButton = GameObject.FindGameObjectWithTag("ButtonB").GetComponent<VirtualBButton>();

    forwardInput = turnInput = jumpInput = 0f;

    animationName = "IdleAnimation";
    anim = GetComponent<Animation>();

    midJump = false;

    groundObject = GameObject.FindGameObjectWithTag("Ground");
    groundLevel = groundObject.transform.position.y + groundObject.transform.localScale.y * 0.5f;
    
    audioSrc = GetComponentInChildren<AudioSource>();

    timeLastHit = Time.time;
  }

  void GetInput()
  {
    forwardInput = joystick.Vertical();
    turnInput = joystick.Horizontal();
    jumpInput = aButton.Pressure();
    attackInput = bButton.Pressure();
  }

  void Update()
  {
    if(alive) {
      GetInput();
      Turn();
      Attack();
    }
  }

  void FixedUpdate()
  {
    if(alive) {
      Walk();
    }

    Jump();

    FallthroughCorrect();

    rBody.velocity = transform.TransformDirection(velocity);
  }

  void Walk()
  {
    if(Mathf.Abs(forwardInput) > inputDelay) {
      velocity.z = moveSettings.forwardVel * forwardInput;
      if(animationName == "IdleAnimation") {
        animationName = "WalkAnimation";
        anim.CrossFade("walk");
        //Debug.Log("Beginning to walk.");
      }
    }
    else {
      velocity.z = 0;
      if(animationName == "WalkAnimation" && Mathf.Abs(turnInput) <= inputDelay) {
        animationName = "IdleAnimation";
        anim.CrossFade("idle");
      }
    }
  }

  void Turn()
  {
    if(Mathf.Abs(turnInput) > inputDelay) {
      targetRotation *= Quaternion.AngleAxis(moveSettings.rotateVel * turnInput * Time.deltaTime, Vector3.up);
      if(animationName == "IdleAnimation") {
        animationName = "WalkAnimation";
        anim.CrossFade("walk");
        Debug.Log("Beginning to turn.");
      }
    }
    else if(animationName == "WalkAnimation" && Mathf.Abs(forwardInput) <= inputDelay) {
      animationName = "IdleAnimation";
      anim.CrossFade("idle");
    }
    transform.rotation = targetRotation;
  }

  void Jump()
  {
    if(jumpInput > 0f && Grounded() && !midJump && alive) {
      // Jump:
      velocity.y = moveSettings.jumpVel;
      animationName = "JumpAnimation";
      anim.CrossFade("jump");
      Debug.Log("Jumping.");
      midJump = true;
      //Debug.Log("Beginning jump.");
    }
    else if(Grounded()) {
      // Zero out velocity.y:
      velocity.y = 0f;
      if(jumpInput == 0f) {
        midJump = false;
        if(animationName == "JumpAnimation") {
          animationName = "IdleAnimation";
          anim.CrossFade("idle");
        }
        //Debug.Log("Ending jump.");
      }
    }
    else
      // Falling; decrease velocity.y:
      velocity.y -= physSettings.downAccel;
  }

  bool Grounded()
  {
    return Physics.Raycast(transform.position, Vector3.down, moveSettings.distToGrounded, moveSettings.ground);
  }

  void Attack()
  {
    if(!midJump && Grounded()) {
      if(attackInput > 0f && !Attacking()) {
        attackStartTime = Time.time;
        animationName = "AttackAnimation";
        anim.CrossFade("attack1");
        float rand = Random.value;
        if(rand <= attackSFX1Rate)
          audioSrc.PlayOneShot(attackSFX1);
        else if(rand <= attackSFX1Rate + attackSFX2Rate)
          audioSrc.PlayOneShot(attackSFX2);
        Debug.Log("ATTACKING!!");
        if(!HandleAttack())
          Debug.Log("You hit nothing.");
      }
      else if(Time.time - attackStartTime >= attackCooldown) {
        attackStartTime = 0f;
        if(animationName == "AttackAnimation") {
          animationName = "IdleAnimation";
          anim.CrossFade("idle");
        }
      }
    }
    else
      attackStartTime = 0f;
  }

  bool Attacking()
  {
    return attackStartTime == 0f ? false : Time.time - attackStartTime >= attackCooldown;
  }

  void FallthroughCorrect()
  {
    if(transform.position.y < groundLevel)
      transform.position = new Vector3(transform.position.x, groundLevel + 1f, transform.position.z);
  }

  GameObject getNearestEnemy()
  {
    GameObject nearest = null;
    foreach(GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
      if(nearest == null || Vector3.Distance(transform.position, enemy.transform.position) < Vector3.Distance(transform.position, nearest.transform.position))
        nearest = enemy;
    return nearest;
  }

  bool HandleAttack()
  {
    bool attackSuccessful = false;
    RaycastHit hit;
    Ray ray = new Ray(transform.position, transform.forward);
    if(Physics.Raycast(ray, out hit, attackRange)) {
      attackSuccessful = true;
      if(hit.transform.tag == "Enemy") {
        hit.collider.GetComponent<EnemyController01>().Damage(charSettings.attack);
        Debug.Log("--YOU HIT THE BADDIE--");
      }
      else if(hit.transform.tag == "Destructible") {
        
      }
      else if(hit.transform.tag == "Movable") {
        
      }
    }
    return attackSuccessful;
  }

  //void HandleAttack00()
  //{
  //  GameObject nearestEnemy = getNearestEnemy();
  //  if(nearestEnemy != null && Vector3.Distance(transform.position, nearestEnemy.transform.position) <= attackRange) {
  //    nearestEnemy.GetComponent<EnemyController01>().Damage(charSettings.attack);
  //    Debug.Log("--YOU HIT THE BADDIE--");
  //  }
  //}

  public void Damage(int dmg)
  {
    if(alive) {
      if(dmg < 0) Heal(-dmg);
      else if(Time.time - timeLastHit > invincibilityWindow) {
        hp -= dmg;
        if(hp < 0) hp = 0;
        timeLastHit = Time.time;
      }
      if(hp == 0)
        Kill();
      else {
        float rand = Random.value;
        if(rand <= hitSFX1Rate)
          audioSrc.PlayOneShot(hitSFX1);
        else if(rand <= hitSFX1Rate + hitSFX2Rate)
          audioSrc.PlayOneShot(hitSFX2);
      }
    }
  }

  public void Heal(int health)
  {
    if(alive) {
      if(health < 0) Damage(-health);
      else {
        hp += health;
        if(hp > charSettings.maxHP) hp = charSettings.maxHP;
      }
    }
  }

  public void Kill()
  {
    if(alive) {
      alive = false;
      if(Grounded()) rBody.velocity = Vector3.zero;
      anim.CrossFade("death1");
      audioSrc.PlayOneShot(dieSFX);
    }
  }

  void OnGUI()
  {
    if(!alive)
      msgWindowRect = GUILayout.Window(-1, msgWindowRect, ConsoleWindow, "");
  }

  void ConsoleWindow(int windowID)
  {
    GUILayout.Label("<color=whilte><size=60>" + gameOverMsg + "</size></color>");
  }

  void OnTriggerEnter(Collider col)
  {
    if(col.tag == "LavaFloor")
      Damage(1);
  }

  void OnTriggerStay(Collider col)
  {
    if(col.tag == "LavaFloor")
      Damage(1);
  }
}
