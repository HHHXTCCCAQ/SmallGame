using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
//Wait for me, I don't want to let you down
//love you into disease, but no medicine can.
//Created By HeXiaoTao
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class EnemyActions : MonoBehaviour {

	[Space(10)]
	[Header ("Linked components")]
	public GameObject target; //目标
	public UnitAnimator animator; //动画控制
	public GameObject GFX; //GFX of this unit
	public Rigidbody rb; 
	public CapsuleCollider capsule;

	[Header("Attack Data")]
	public DamageObject[] AttackList; //攻击列表
	public bool PickRandomAttack; //是否随机选择攻击方式
	public float hitZRange = 2; //攻击的Z轴范围
	public float defendChance = 0; //被攻击时候防御的概率（闪避几率）
	public float hitRecoveryTime = .4f; //被攻击的硬直时间
	public float standUpTime = 1.1f; //倒地后站起来需要的时间
	public bool canDefendDuringAttack; //是否能够在攻击的时候防御
	public bool AttackPlayerAirborne; //是否能够在空中攻击
	private DamageObject lastAttack; //上次攻击的数据
	private int AttackCounter = 0; //攻击方式index
	public bool canHitEnemies; //是否能够攻击其他敌人（敌人是否能够相互攻击）
	public bool canHitDestroyableObjects; //敌人是否能够攻击可以摧毁的物体
	[HideInInspector]
	public float lastAttackTime; //最后一次攻击的时间

	[Header ("Settings")]
	public bool pickARandomName; //定义一个随机的名字
	public TextAsset enemyNamesList; //敌人的名称
	public string enemyName = ""; //敌人的名称
	public float attackRangeDistance = 1.4f; //攻击距离
    //敌人离目标的距离
	public float closeRangeDistance = 2f; //近距离
	public float midRangeDistance = 3f; //中距离
	public float farRangeDistance = 4.5f; //远距离


	public float RangeMarging = 1f; //the amount of space that is allowed between the player and enemy before we reposition ourselves
	public float walkSpeed = 1.95f; //行走速度
	public float walkBackwardSpeed = 1.2f; //后退的速度
	public float sightDistance = 10f; //能看到目标的距离
	public float attackInterval = 1.2f; //攻击速率
	public float rotationSpeed = 15f; //转向速度
	public float lookaheadDistance; //检查前方是否有障碍物的距离
	public bool ignoreCliffs; //ignore cliff detection
	public float KnockdownTimeout = 0f; //被击倒站起来的时间
	public float KnockdownUpForce = 5f; //击倒向上施加的力
	public float KnockbackForce = 4; //击倒水平施加的力
	private LayerMask HitLayerMask; //能够到达的层
	public LayerMask CollisionLayer; //碰撞体响应的层
	public bool randomizeValues = true; //是否随机设置  避免同步
	[HideInInspector]
	public float zSpreadMultiplier = 2f; //敌人和多个玩家z轴的距离

	[Header ("Stats")]
	public RANGE range;
	public ENEMYTACTIC enemyTactic;
	public UNITSTATE enemyState;
	public DIRECTION currentDirection; 
	public bool targetSpotted;
	public bool cliffSpotted;
	public bool wallspotted;
	public bool isGrounded;
	public bool isDead;
	private Vector3 moveDirection;
	public float distance;
	private Vector3 fixedVelocity;
	private bool updateVelocity;

	//敌人无法移动时候的状态
	private List<UNITSTATE> NoMovementStates = new List<UNITSTATE> {
		UNITSTATE.DEATH,
		UNITSTATE.ATTACK,
		UNITSTATE.DEFEND,
		UNITSTATE.GROUNDHIT,
		UNITSTATE.HIT,
		UNITSTATE.IDLE,
		UNITSTATE.KNOCKDOWNGROUNDED,
		UNITSTATE.STANDUP,
	};

	//可以攻击的状态
	private List<UNITSTATE> HitableStates = new List<UNITSTATE> {
		UNITSTATE.ATTACK,
		UNITSTATE.DEFEND,
		UNITSTATE.HIT,
		UNITSTATE.IDLE,
		UNITSTATE.KICK,
		UNITSTATE.PUNCH,
		UNITSTATE.STANDUP,
		UNITSTATE.WALK,
		UNITSTATE.KNOCKDOWNGROUNDED,
	};

	[HideInInspector]
	public float ZSpread; //敌人相互间z轴的距离

	//[HideInInspector]
	public Vector3 distanceToTarget;

	private List<UNITSTATE> defendableStates = new List<UNITSTATE> { UNITSTATE.IDLE, UNITSTATE.WALK, UNITSTATE.DEFEND }; //默认状态

	//敌人的事件委托
	public delegate void UnitEventHandler(GameObject Unit);
	public static event UnitEventHandler OnUnitDestroy;

	//---
   /// <summary>
   /// 初始化
   /// </summary>
	public void OnStart(){

		//定义敌人的名字
		if(pickARandomName) enemyName = GetRandomName();

		//设定玩家为目标
		if(target == null) target = GameObject.FindGameObjectWithTag("Player");

		//更新敌人的名单
		EnemyManager.getActiveEnemies();

		//在击中中能够防御
		if (canDefendDuringAttack) defendableStates.Add (UNITSTATE.ATTACK);

		//设置敌人攻击的优先级
		HitLayerMask = 1 << LayerMask.NameToLayer("Player");
		if(canHitEnemies)HitLayerMask |= (1 << LayerMask.NameToLayer("Enemy"));
		if(canHitDestroyableObjects)HitLayerMask |= (1 << LayerMask.NameToLayer("DestroyableObject"));
	}

	#region Update

	
	public void OnLateUpdate(){

		//对物体的偏移量应用到父类
		if(animator && animator.GetComponent<Animator>().applyRootMotion && animator.transform.localPosition != Vector3.zero) {
			Vector3 offset = animator.transform.localPosition;
			animator.transform.localPosition = Vector3.zero;
			transform.position += offset * (int)currentDirection;
		}
	}

	public void OnFixedUpdate() {
		if(updateVelocity) {
			rb.velocity = fixedVelocity;
			updateVelocity = false;
		}
	}

	//设置速度
	void SetVelocity(Vector3 velocity) {
		fixedVelocity = velocity;
		updateVelocity = true;
	}
	#endregion

	#region 攻击

	//Attack
	public void ATTACK() {

		//当玩家跳起来时无法攻击
		var playerMovement = target.GetComponent<PlayerMovement>();
		if (!AttackPlayerAirborne && playerMovement != null && playerMovement.jumpInProgress) {
			return;

		} else {

			//初始化
			enemyState = UNITSTATE.ATTACK;
			Move(Vector3.zero, 0f);
			LookAtTarget(target.transform);
			TurnToDir(currentDirection);

			//攻击类型
			if (PickRandomAttack) AttackCounter = Random.Range (0, AttackList.Length);

			//动画
			animator.SetAnimatorTrigger (AttackList[AttackCounter].animTrigger);

			//连击
			if (!PickRandomAttack) {
				AttackCounter += 1;
				if (AttackCounter >= AttackList.Length) AttackCounter = 0;
			}

			lastAttackTime = Time.time;
			lastAttack = AttackList [AttackCounter];
			lastAttack.inflictor = gameObject;

			//resume
			Invoke ("Ready", AttackList [AttackCounter].duration);
		}
	}

	#endregion

	#region 被攻击的时候
    /// <summary>
    /// 被攻击
    /// </summary>
    /// <param name="d"></param>
	public void Hit(DamageObject d){
		if(HitableStates.Contains(enemyState)) {

			//只有在第一次被击倒时才能进行地面攻击
			if(enemyState == UNITSTATE.KNOCKDOWNGROUNDED && !d.isGroundAttack) return;

			CancelInvoke();
			StopAllCoroutines();
			animator.StopAllCoroutines();
			Move(Vector3.zero, 0f);

			//给敌人添加硬直时间
			lastAttackTime = Time.time;

			//在倒地状态只能被攻击一次
			if((enemyState == UNITSTATE.KNOCKDOWNGROUNDED || enemyState == UNITSTATE.GROUNDHIT) && !d.isGroundAttack)
				return;

			//防御状态能够攻击
			if(!d.DefenceOverride && defendableStates.Contains(enemyState)) {
				int rand = Random.Range(0, 100);
				if(rand < defendChance) {
					Defend();
					return;
				}
			}

			//攻击声音播放
			GlobalAudioPlayer.PlaySFXAtPosition(d.hitSFX, transform.position);

			//攻击特效
			ShowHitEffectAtPosition(new Vector3(transform.position.x, d.inflictor.transform.position.y + d.collHeight, transform.position.z));

			//相机震动
			CamShake camShake = Camera.main.GetComponent<CamShake>();
			if(camShake != null)
				camShake.Shake(.1f);

			//摄像机慢动作
			if(d.slowMotionEffect) {
				CamSlowMotionDelay cmd = Camera.main.GetComponent<CamSlowMotionDelay>();
				if(cmd != null)
					cmd.StartSlowMotionDelay(.2f);
			}

			//减少HP
			HealthSystem hs = GetComponent<HealthSystem>();
			if(hs != null) {
				hs.SubstractHealth(d.damage);
				if(hs.CurrentHp == 0)
					return;
			}

			//地面攻击
			if(enemyState == UNITSTATE.KNOCKDOWNGROUNDED) {
				StopAllCoroutines();
				enemyState = UNITSTATE.GROUNDHIT;
				StartCoroutine(GroundHit());
				return;
			}
				
			//转向攻击的方向
			int dir = d.inflictor.transform.position.x > transform.position.x? 1 : -1;
			TurnToDir((DIRECTION)dir);

			//检测是否倒地
			if(d.knockDown) {
				StartCoroutine(KnockDownSequence(d.inflictor));
				return;

			} else {

				//默认攻击
				int rand = Random.Range(1, 3);
				animator.SetAnimatorTrigger("Hit" + rand);
				enemyState = UNITSTATE.HIT;

				//对攻击对象施加力
				LookAtTarget(d.inflictor.transform);
				animator.AddForce(-KnockbackForce);

				//当收到攻击的时候将敌人的状态从被动切换到主动
				if(enemyTactic != ENEMYTACTIC.ENGAGE) {
					EnemyManager.setAgressive(gameObject);
				}

				Invoke("Ready", hitRecoveryTime);
				return;
			}
		}
	}

	/// <summary>
	/// 防御
	/// </summary>
	void Defend(){
		enemyState = UNITSTATE.DEFEND;
		animator.ShowDefendEffect();
		animator.SetAnimatorTrigger ("Defend");
		GlobalAudioPlayer.PlaySFX ("DefendHit");
		animator.SetDirection (currentDirection);
	}

	#endregion

	#region 检测攻击

	
	public void CheckForHit() {

		//被攻击的敌人数量
		Vector3 boxPosition = transform.position + (Vector3.up * lastAttack.collHeight) + Vector3.right * ((int)currentDirection * lastAttack.collDistance);
		Vector3 boxSize = new Vector3 (lastAttack.CollSize/2, lastAttack.CollSize/2, hitZRange/2);
		Collider[] hitColliders = Physics.OverlapBox(boxPosition, boxSize, Quaternion.identity, HitLayerMask); 

		int i=0;
		while (i < hitColliders.Length) {

			//攻击
			IDamagable<DamageObject> damagableObject = hitColliders[i].GetComponent(typeof(IDamagable<DamageObject>)) as IDamagable<DamageObject>;
			if (damagableObject != null && damagableObject != (IDamagable<DamageObject>)this) {
				damagableObject.Hit(lastAttack);
			}
			i++;
		}
	}
    //编辑器模式下画出攻击框和视线方向
	#if UNITY_EDITOR 
	void OnDrawGizmos(){

		
		if (lastAttack != null && (Time.time - lastAttackTime) < lastAttack.duration) {
			Gizmos.color = Color.red;
			Vector3 boxPosition = transform.position + (Vector3.up * lastAttack.collHeight) + Vector3.right * ((int)currentDirection * lastAttack.collDistance);
			Vector3 boxSize = new Vector3 (lastAttack.CollSize, lastAttack.CollSize, hitZRange);
			Gizmos.DrawWireCube (boxPosition, boxSize);
		}

		
		Gizmos.color = Color.yellow;
		Vector3 offset = -moveDirection.normalized * lookaheadDistance;
		Gizmos.DrawWireSphere (transform.position + capsule.center - offset, capsule.radius); 
	}

	#endif

	#endregion

	#region 倒地的状态处理

	IEnumerator KnockDownSequence(GameObject inflictor) {
		enemyState = UNITSTATE.KNOCKDOWN;
		yield return new WaitForFixedUpdate();

		//攻击方向
		int dir = 1;
		if(inflictor != null) dir = inflictor.transform.position.x > transform.position.x? 1 : -1;
		currentDirection = (DIRECTION)dir;
		animator.SetDirection(currentDirection);
		TurnToDir(currentDirection);

		//添加力
		animator.SetAnimatorTrigger("KnockDown_Up");
		while(IsGrounded()){
			SetVelocity(new Vector3(KnockbackForce * -dir, KnockdownUpForce, 0));
			yield return new WaitForFixedUpdate();
		}

		//向上的速度
		while(rb.velocity.y >= 0) yield return new WaitForFixedUpdate();

		animator.SetAnimatorTrigger ("KnockDown_Down");
		while(!IsGrounded()) yield return new WaitForFixedUpdate();

		//倒地
		animator.SetAnimatorTrigger ("KnockDown_End");
		GlobalAudioPlayer.PlaySFXAtPosition("Drop", transform.position);
		animator.SetAnimatorFloat ("MovementSpeed", 0f);
		animator.ShowDustEffectLand();
		enemyState = UNITSTATE.KNOCKDOWNGROUNDED;
		Move(Vector3.zero, 0f);

		
		CamShake camShake = Camera.main.GetComponent<CamShake>();
		if (camShake != null) camShake.Shake(.3f);

		animator.ShowDustEffectLand();


		float t = 0;
		float speed = 2;
		Vector3 fromVelocity = rb.velocity;
		while (t<1){
			SetVelocity(Vector3.Lerp (new Vector3(fromVelocity.x, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, fromVelocity.z), new Vector3(0, rb.velocity.y, 0), t));
			t += Time.deltaTime * speed;
			yield return new WaitForFixedUpdate();
		}

		//倒地硬直
		Move(Vector3.zero, 0f);
		yield return new WaitForSeconds(KnockdownTimeout);

		//站起来
		enemyState = UNITSTATE.STANDUP;
		animator.SetAnimatorTrigger ("StandUp");
		Invoke("Ready", standUpTime);
	}

	//倒地攻击
	public IEnumerator GroundHit(){
		CancelInvoke();
		GlobalAudioPlayer.PlaySFXAtPosition ("EnemyGroundPunchHit", transform.position);
		animator.SetAnimatorTrigger ("GroundHit");
		yield return new WaitForSeconds(KnockdownTimeout);
		if(!isDead)	animator.SetAnimatorTrigger ("StandUp");
		Invoke("Ready", standUpTime);
	}

	#endregion

	#region Movement

	/// <summary>
	/// 移动到目标
	/// </summary>
	/// <param name="proximityRange">距离范围</param>
	/// <param name="movementMargin"></param>
	public void WalkTo(float proximityRange, float movementMargin){
		Vector3 dirToTarget;
		LookAtTarget(target.transform);
		enemyState = UNITSTATE.WALK;

		//判读距离
		if (enemyTactic == ENEMYTACTIC.ENGAGE) {
			dirToTarget = target.transform.position - (transform.position + new Vector3 (0, 0, Mathf.Clamp(ZSpread, 0, attackRangeDistance)));
		} else {
			dirToTarget = target.transform.position - (transform.position + new Vector3 (0, 0, ZSpread));
		}

		//太远
		if (distance >= proximityRange ) {
			moveDirection = new Vector3(dirToTarget.x,0,dirToTarget.z);
			if (IsGrounded() && !WallSpotted() && !PitfallSpotted()) {
				Move(moveDirection.normalized, walkSpeed);
				animator.SetAnimatorFloat ("MovementSpeed", rb.velocity.sqrMagnitude);
				return;
			}
		}

		//太近
		if (distance <= proximityRange - movementMargin) {
			moveDirection = new Vector3(-dirToTarget.x,0,0);
			if (IsGrounded() && !WallSpotted() && !PitfallSpotted()) {
				Move(moveDirection.normalized, walkBackwardSpeed);
				animator.SetAnimatorFloat ("MovementSpeed", -rb.velocity.sqrMagnitude);
				return;
			}
		}

		//什么也不做
		Move(Vector3.zero, 0f);
		animator.SetAnimatorFloat ("MovementSpeed", 0);
	}

	/// <summary>
	/// 移动
	/// </summary>
	/// <param name="vector">目标</param>
	/// <param name="speed">速度</param>
	public void Move(Vector3 vector, float speed){
		if(!NoMovementStates.Contains(enemyState)) {
			SetVelocity(new Vector3(vector.x * speed, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, vector.z * speed));
		} else {
			SetVelocity(Vector3.zero);
		}
	}

	//returns true if there is an environment collider in front of us
	bool WallSpotted(){
		Vector3 Offset =  moveDirection.normalized * lookaheadDistance;
		Collider[] hitColliders = Physics.OverlapSphere (transform.position + capsule.center + Offset, capsule.radius, CollisionLayer);

		int i = 0;
		bool hasHitwall = false;
		while (i < hitColliders.Length) {
			if(CollisionLayer == (CollisionLayer | 1 << hitColliders[i].gameObject.layer)) {
				hasHitwall = true;
			}
			i++;
		}
		wallspotted = hasHitwall;
		return hasHitwall;
	}

	//returns true if there is a cliff in front of us
	bool PitfallSpotted(){
		if (!ignoreCliffs) {
			float lookDownDistance = 1f;
			Vector3 StartPoint = transform.position + (Vector3.up * .3f) + (Vector3.right * (capsule.radius + lookaheadDistance) * moveDirection.normalized.x);
			RaycastHit hit;

			#if UNITY_EDITOR 
			Debug.DrawRay (StartPoint, Vector3.down * lookDownDistance, Color.red);
			#endif

			if (!Physics.Raycast (StartPoint, Vector3.down, out hit, lookDownDistance, CollisionLayer)) {
				cliffSpotted = true;
				return true;
			}
		}
		cliffSpotted = false;
		return false;
	}

	//returns true if this unit is grounded
	public bool IsGrounded(){
		float colliderSize = capsule.bounds.extents.y - .1f;
		if (Physics.CheckCapsule (capsule.bounds.center, capsule.bounds.center + Vector3.down*colliderSize, capsule.radius, CollisionLayer)) {
			isGrounded = true;
			return true;
		} else {
			isGrounded = false;
			return false;
		}
	}

	//turn towards a direction
	public void TurnToDir(DIRECTION dir) {
		transform.rotation = Quaternion.LookRotation(Vector3.forward * (int)dir);
	}

	#endregion

	//show hit effect
	public void ShowHitEffectAtPosition(Vector3 pos) {
		GameObject.Instantiate (Resources.Load ("HitEffect"), pos, Quaternion.identity);
	}

	//unit is ready for new actions
	public void Ready()	{
		enemyState = UNITSTATE.IDLE;
		animator.SetAnimatorTrigger("Idle");
		animator.SetAnimatorFloat ("MovementSpeed", 0f);
		Move(Vector3.zero, 0f);
	}

	//look at the current target
	public void LookAtTarget(Transform _target){
		if(_target != null){
			Vector3 newDir = Vector3.zero;
			int dir = _target.transform.position.x >= transform.position.x ? 1 : -1;
			currentDirection = (DIRECTION)dir;
			if (animator != null) animator.currentDirection = currentDirection;
			newDir = Vector3.RotateTowards(transform.forward, Vector3.forward * dir, rotationSpeed * Time.deltaTime, 0.0f);	
			transform.rotation = Quaternion.LookRotation(newDir);
		}
	}

	//randomizes values
	public void SetRandomValues(){
		walkSpeed *= Random.Range(.8f, 1.2f);
		walkBackwardSpeed *= Random.Range(.8f, 1.2f);
		attackInterval *= Random.Range(.7f, 1.5f);
		KnockdownTimeout *= Random.Range(.7f, 1.5f);
		KnockdownUpForce *= Random.Range(.8f, 1.2f);
		KnockbackForce *= Random.Range(.7f, 1.5f);
	}

	//destroy event
	public void DestroyUnit(){
		if(OnUnitDestroy != null) OnUnitDestroy(gameObject);
	}

	//returns a random name
	string GetRandomName(){
		if(enemyNamesList == null) {
			Debug.Log("no list of names was found, please create 'EnemyNames.txt' that contains a list of enemy names and put it in the 'Resources' folder.");
			return "";
		}

		//convert the lines of the txt file to an array
		string data = enemyNamesList.ToString();
		string cReturns = System.Environment.NewLine + "\n" + "\r"; 
		string[] lines = data.Split(cReturns.ToCharArray());

		//pick a random name from the list
		string name = "";
		int cnt = 0;
		while(name.Length == 0 && cnt < 100) {
			int rand = Random.Range(0, lines.Length);
			name = lines[rand];
			cnt += 1;
		}
		return name;
	}
}