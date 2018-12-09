using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Wait for me, I don't want to let you down
//love you into disease, but no medicine can.
//Created By HeXiaoTao
public class EnemyAI : EnemyActions, IDamagable<DamageObject>{

	[Space(10)]
	public bool enableAI;
	//人物状态的列表
	private List<UNITSTATE> ActiveAIStates = new List<UNITSTATE> { 
		UNITSTATE.IDLE, 
		UNITSTATE.WALK 
	};

	void Start(){

		//添加到敌人列表
		EnemyManager.enemyList.Add(gameObject);

		//设置两个敌人之间z轴的分布
		ZSpread = (EnemyManager.enemyList.Count-1);
		Invoke ("SetZSpread", .1f);	

		//随机化敌人的运动
		if(randomizeValues) SetRandomValues();

		OnStart();
	}

	void FixedUpdate(){
		OnFixedUpdate();
	}

	void LateUpdate(){
		OnLateUpdate();
	}

	void Update(){

		//当没目标和AI被禁止的时候
		if (target == null || !enableAI) {
			Ready ();
			return;

		} else {

			//获取与目标之间的距离
			range = GetDistanceToTarget ();
		}
			
		if(!isDead && enableAI){
			if(ActiveAIStates.Contains(enemyState) && targetSpotted) {

				
				AI();

			} else {

				//当目标在视线范围内
				if(distanceToTarget.magnitude < sightDistance) targetSpotted = true;
			}
		}
	}

	void AI(){
		LookAtTarget(target.transform);
		if (range == RANGE.ATTACKRANGE){

			//攻击目标
			if (!cliffSpotted){
				if (Time.time - lastAttackTime > attackInterval) {
					ATTACK();
				} else {
					Ready();
				}
				return;
			}

			//不同距离下的敌人的攻击动作
			if (enemyTactic == ENEMYTACTIC.KEEPCLOSEDISTANCE) WalkTo(closeRangeDistance, 0f);
			if (enemyTactic == ENEMYTACTIC.KEEPMEDIUMDISTANCE) WalkTo(midRangeDistance, RangeMarging);
			if (enemyTactic == ENEMYTACTIC.KEEPFARDISTANCE) WalkTo(farRangeDistance, RangeMarging);
			if (enemyTactic == ENEMYTACTIC.STANDSTILL) Ready ();

		} else {

			//近中远敌人的动作
			if (enemyTactic == ENEMYTACTIC.ENGAGE) WalkTo (attackRangeDistance, 0f);
			if (enemyTactic == ENEMYTACTIC.KEEPCLOSEDISTANCE) WalkTo(closeRangeDistance, RangeMarging);
			if (enemyTactic == ENEMYTACTIC.KEEPMEDIUMDISTANCE) WalkTo(midRangeDistance, RangeMarging);
			if (enemyTactic == ENEMYTACTIC.KEEPFARDISTANCE) WalkTo(farRangeDistance, RangeMarging);
			if (enemyTactic == ENEMYTACTIC.STANDSTILL) Ready();
		}
	}

	/// <summary>
    /// 获取与目标的距离
    /// </summary>
    /// <returns>距离范围</returns>
	private RANGE GetDistanceToTarget(){
		if (target != null) {

			//获取与目标的距离
			distanceToTarget = target.transform.position - transform.position;
			distance = Vector3.Distance (target.transform.position, transform.position);

			float distX = Mathf.Abs(distanceToTarget.x);
			float distZ = Mathf.Abs(distanceToTarget.z);

			//攻击范围
			if(distX <= attackRangeDistance){
				if(distZ < (hitZRange/2f)) 
					return RANGE.ATTACKRANGE;
				else
					return RANGE.CLOSERANGE;
			}

			//近距离
			if (distX > attackRangeDistance && distX < midRangeDistance) return RANGE.CLOSERANGE;

			//中距离
			if(distX > closeRangeDistance && distance < farRangeDistance) return RANGE.MIDRANGE;

			//远距离
			if(distX > farRangeDistance) return RANGE.FARRANGE;

		}
		return RANGE.FARRANGE;
	}
		
	//设置敌人的攻击方式
	public void SetEnemyTactic(ENEMYTACTIC tactic){
		enemyTactic = tactic;
	}

	///将敌人按照z轴的方向分开
	void SetZSpread(){
		ZSpread = (ZSpread - (float)(EnemyManager.enemyList.Count - 1) / 2f) * (capsule.radius*2) * zSpreadMultiplier;
		if (ZSpread > attackRangeDistance) ZSpread = attackRangeDistance - 0.1f;
	}

	/// <summary>
    /// 单位死亡
    /// </summary>
	void Death(){
		StopAllCoroutines();
		CancelInvoke();

		enableAI = false;
		isDead = true;
		animator.SetAnimatorBool("isDead", true);
		Move(Vector3.zero, 0);
		EnemyManager.RemoveEnemyFromList(gameObject);
		gameObject.layer = LayerMask.NameToLayer ("Default");

		//倒地状态死亡
		if(enemyState == UNITSTATE.KNOCKDOWNGROUNDED) {
			StartCoroutine(GroundHit());
		} else {

			//正常状态死亡
			animator.SetAnimatorTrigger("Death");
		}

		GlobalAudioPlayer.PlaySFXAtPosition("EnemyDeath", transform.position);
		StartCoroutine (animator.FlickerCoroutine(2));
		enemyState = UNITSTATE.DEATH;
		DestroyUnit();
	}
}
//敌人的状态  
public enum ENEMYTACTIC {
	ENGAGE = 0,
	KEEPCLOSEDISTANCE = 1,//保持近距离
	KEEPMEDIUMDISTANCE = 2,//保持中距离
	KEEPFARDISTANCE = 3,//远距离
	STANDSTILL = 4,//停止不动
}
///距离范围  攻击距离 近 中 远距离
public enum RANGE {
	ATTACKRANGE,
	CLOSERANGE,
	MIDRANGE,
	FARRANGE,
}