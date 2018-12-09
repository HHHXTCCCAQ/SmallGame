using UnityEngine;

public class UnitState : MonoBehaviour {

	public UNITSTATE currentState = UNITSTATE.IDLE;

	public void SetState(UNITSTATE state){
		currentState = state;
	}
}
//人物状态  走 跳  攻击  等状态
public enum UNITSTATE {
	IDLE,
	WALK,
	JUMPING,
	LAND,
	JUMPKICK,
	PUNCH,
	KICK,
	ATTACK,
	DEFEND,
	HIT,//被攻击
	DEATH,
	THROW,
	PICKUPITEM,//捡东西
	KNOCKDOWN,//击倒
	KNOCKDOWNGROUNDED,//倒地
	GROUNDPUNCH,
	GROUNDKICK,
	GROUNDHIT,
	STANDUP,
	USEWEAPON,//用武器
};