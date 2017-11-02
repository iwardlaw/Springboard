using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController01 : MonoBehaviour {

  public enum EnemyMvmtBehaviour {STATIONARY, ROAMING};
  public enum EnemyAggressionBehaviour {DEFENSIVE, OFFENSIVE};
  public enum EnemyState {DEAD = -1, IDLE, ATTACK, RETURN_HOME};

  public EnemyMvmtBehaviour mvmtBehaviour = EnemyMvmtBehaviour.STATIONARY;
  public EnemyAggressionBehaviour aggrBehaviour = EnemyAggressionBehaviour.DEFENSIVE;
  public EnemyState state = EnemyState.IDLE;

  Vector3 origPosition;

  [System.Serializable]
  public class RoamSettings {
    public float roamRadius = 1f;
    public bool roamInX = true;
    public bool roamInY = false;
    public bool roamInZ = true;
    public float movementThreshold = 0.9f;
    public float movementCooldownLowerLimit = 1.5f;
    public float movementCooldownUpperLimit = 4.5f;
    public int maxConsecutiveMovements = 3;
    public float cooldown = 10f;
    public float cooldownStart = 0f;
    public int numMvmts = 0;
    public int inputHold = 33;
    public int inputHoldCtr = 0;
    public float fwdMoveRatio = 0.7f;
  }

  [System.Serializable]
  public class AttackSettings {
    public float idleAggroDist = 2f;
    public float idleAggroFOV = 100f;
    public float attackAggroDist = 4f;
    public float attackAggroFOV = 190f;
    public float attackCooldownLowerLimit = 0.8f;
    public float attackCooldownUpperLimit = 1.5f;
    public float criticalMultiplier = 2f;
    public float criticalChance = 0.01f;
    public float cooldown = 10f;
    public float cooldownStart = 0f;
  }

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
    public int maxHP = 5;
    public int attack = 1;
    public int defense = 1;
    public int speed = 1;
  }
  public int hp;
  public bool alive = true;

  public AudioClip dieSFX;
  public AudioClip hitSFX;
  AudioSource audioSrc;

  /*  [System.Serializable]
  public class InputSettings {
    public float inputDelay = 0.1f;
    public string FORWARD_AXIS = "DpadVertical";
    public string TURN_AXIS = "DpadHorizontal";
    public string JUMP_AXIS = "ButtonA";
    public string ATTACK_AXIS = "ButtonB";
  }*/
  public float inputDelay = 0.1f;

  public float attackCooldown = 1f;
  float attackStartTime = 0f;
  public float attackRange = 2f;

  public RoamSettings roamSettings = new RoamSettings();
  public AttackSettings attackSettings = new AttackSettings();
  public MoveSettings moveSettings = new MoveSettings();
  public PhysSettings physSettings = new PhysSettings();
  public CharSettings charSettings = new CharSettings();

  GameObject player;
  Vector3 velocity = Vector3.zero;
  Quaternion targetRotation;
  Rigidbody rBody;
  float forwardInput, turnInput, jumpInput, attackInput;
  public string animationName = "IdleAnimation";
  Animation animation;
  bool midJump, mvmtCoolingDown, atkCoolingDown;
  GameObject groundObject;
  float groundLevel;

  public Quaternion TargetRotation {
    get { return targetRotation; }
  }


  void Start()
  {
    player = GameObject.FindGameObjectWithTag("Player");

    origPosition = transform.position;
    origPosition.y = transform.lossyScale.y * 0.5f;

    hp = charSettings.maxHP;

    targetRotation = transform.rotation;
    
    rBody = GetComponent<Rigidbody>();

    forwardInput = turnInput = jumpInput = 0f;

    //animationName = "IdleAnimation";
    animation = GetComponent<Animation>();

    midJump = false;
    mvmtCoolingDown = atkCoolingDown = false;

    roamSettings.cooldown = Random.value * (roamSettings.movementCooldownUpperLimit - roamSettings.movementCooldownLowerLimit) + roamSettings.movementCooldownLowerLimit;
    attackSettings.cooldown = Random.value * (attackSettings.attackCooldownUpperLimit - attackSettings.attackCooldownLowerLimit) + attackSettings.attackCooldownLowerLimit;

    groundObject = GameObject.FindGameObjectWithTag("Ground");
    groundLevel = groundObject.transform.position.y + groundObject.transform.localScale.y * 0.5f;

    audioSrc = GetComponent<AudioSource>();

    forwardInput = turnInput = jumpInput = 0f;
    attackInput = 0f;
  }

  void GetState()
  {
    
  }

  void GetInput()
  {
    //Debug.Log(" ,, fwdInput = " + forwardInput + "  turnInput = " + turnInput);
    if(roamSettings.inputHoldCtr == roamSettings.inputHold || state != EnemyState.IDLE) {
      forwardInput = 0f;
      turnInput = 0f;
      jumpInput = 0f;
    }
    attackInput = 0f;

    if(state == EnemyState.IDLE) {
      if(roamSettings.inputHoldCtr == roamSettings.inputHold) {
        //Debug.Log("Input hold reached; attempting movement.");
        if(mvmtBehaviour == EnemyMvmtBehaviour.ROAMING) {
          if(mvmtCoolingDown && Time.time - roamSettings.cooldownStart >= roamSettings.cooldown) {
            roamSettings.numMvmts = 0;
            mvmtCoolingDown = false;
          }
          if(roamSettings.numMvmts < roamSettings.maxConsecutiveMovements && Random.value <= roamSettings.movementThreshold) {
            if(Random.value <= roamSettings.fwdMoveRatio /*&& ((transform.position + transform.forward * moveSettings.forwardVel * roamSettings.inputHold) - origPosition).magnitude <= roamSettings.roamRadius*/)
              //forwardInput = Random.value;
              forwardInput = 1f;
            else
              turnInput = Random.value * 2f - 1f;
            ++roamSettings.numMvmts;
            //Debug.Log("Moving w/ fwdInput = " + forwardInput + "  turnInput = " + turnInput + "  mvmt # " + roamSettings.numMvmts);
            roamSettings.cooldownStart = Time.time;
            roamSettings.cooldown = Random.value * (roamSettings.movementCooldownUpperLimit - roamSettings.movementCooldownLowerLimit) + roamSettings.movementCooldownLowerLimit;
            mvmtCoolingDown = true;
          }
        }
        roamSettings.inputHoldCtr = 0;
      }
      else {
        ++roamSettings.inputHoldCtr;
        //Debug.Log("Incrementing input hold counter: " + (roamSettings.inputHoldCtr - 1).ToString() + " -> " + roamSettings.inputHoldCtr);
      }
    }
    else if(state == EnemyState.ATTACK) {
      // Turn to face player.
      float leeway = 0f;
      //float planarFwd = transform.rotation.y;
      //float relRot = (transform.position - player.transform.position).;
      //relPos.y = 0f;
      transform.LookAt(player.transform, Vector3.up);

      //Debug.Log("player tag = " + player.tag + "  pos = " + player.transform.position);
      Vector3 fwd = transform.forward;
      Vector2 fwd2 = new Vector2(fwd.x, fwd.z);
      Vector3 relVec = player.transform.position - transform.position;
      Vector2 relVec2 = new Vector2(relVec.x, relVec.z);
      //Debug.Log(" -- forward = " + fwd + "  player rel pos = " + relVec + "  diff = " + Vector3.Angle(fwd, relVec));

      // Advance toward player if without attack range.
      if(Vector3.Angle(fwd, relVec) <= leeway) {
        if(Vector3.Distance(transform.position, player.transform.position) > attackRange)
          forwardInput = 1f;

        // Signal attack.
        else if(Time.time - attackSettings.cooldownStart >= attackSettings.cooldown) {
          attackInput = 1f;
          attackSettings.cooldownStart = Time.time;
        }
        else
          attackInput = 0f;
      }
      else
        attackInput = 0f;
    }
    else if(state == EnemyState.RETURN_HOME) {
      // Ignore for now.
    }
  }

  void Update()
  {
    if(alive) {
      GetState();
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
    //if(!Attacking()) {
      if(Mathf.Abs(forwardInput) > inputDelay) {
        //Debug.Log("Walking; animation = " + animationName);
        // Move:
        //rBody.velocity = transform.forward * forwardInput * moveSettings.forwardVel;
        velocity.z = moveSettings.forwardVel * forwardInput;
        if(animationName == "IdleAnimation") {
          animationName = "WalkAnimation";
          animation.CrossFade("Walk");
          //Debug.Log("Beginning to walk.");
        }
      }
      else {
        // Zero velocity:
        //rBody.velocity = Vector3.zero;
        velocity.z = 0;
        if(animationName == "WalkAnimation" && Mathf.Abs(turnInput) <= inputDelay) {
          animationName = "IdleAnimation";
          animation.CrossFade("Idle");
          //Debug.Log("Returning to idle animation.");
        }
      }
    //}
  }

  void Turn()
  {
    if(Mathf.Abs(turnInput) > inputDelay) {
      //Debug.Log("Turning; animation = " + animationName);
      targetRotation *= Quaternion.AngleAxis(moveSettings.rotateVel * turnInput * Time.deltaTime, Vector3.up);
      if(animationName == "IdleAnimation") {
        animationName = "WalkAnimation";
        animation.CrossFade("Walk");
        //Debug.Log("Beginning to turn.");
      }
    }
    else if(animationName == "WalkAnimation" && Mathf.Abs(forwardInput) <= inputDelay) {
      animationName = "IdleAnimation";
      animation.CrossFade("Idle");
    }
    transform.rotation = targetRotation;
  }

  void Jump()
  {
    //if(!Attacking()) {
      if(jumpInput > 0f && Grounded() && !midJump && alive) {
        // Jump:
        velocity.y = moveSettings.jumpVel;
        animationName = "JumpAnimation";
        animation.CrossFade("Idle");
        //Debug.Log("Jumping.");
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
            animation.CrossFade("Idle");
          }
          // Debug.Log("Ending jump.");
        }
      }
      else
        // Falling; decrease velocity.y:
        velocity.y -= physSettings.downAccel;
    //}
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
        animation.CrossFade("Attack");
        //Debug.Log("ATTACKING!!");
      }
      else if(Time.time - attackStartTime >= attackCooldown) {
        attackStartTime = 0f;
        if(animationName == "AttackAnimation") {
          animationName = "IdleAnimation";
          animation.CrossFade("Idle");
        }
      }
    }
    else {
      attackStartTime = 0f;
      //animationName = "IdleAnimation";
      //animation.CrossFade("Idle");
    }


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

  void HandleAttack()
  {
    if(Vector3.Distance(transform.position, player.transform.position) <= attackRange) {
      player.GetComponent<CharacterController>().Damage(charSettings.attack);
      Debug.Log("--YOU'VE BEEN HIT--");
    }
  }

  public void Damage(int dmg)
  {
    if(alive) {
      if(dmg < 0) Heal(-dmg);
      else {
        hp -= dmg;
        if(hp < 0) hp = 0;
      }
      if(hp == 0)
        Kill();
      else
        audioSrc.PlayOneShot(hitSFX);
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
      state = EnemyState.DEAD;
      if(Grounded()) rBody.velocity = Vector3.zero;
      animation.CrossFade("Death");
      audioSrc.PlayOneShot(dieSFX);
    }
  }
}
