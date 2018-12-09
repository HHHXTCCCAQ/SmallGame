using UnityEngine;

public class HealthSystem : MonoBehaviour {

	public int MaxHp = 20;
	public int CurrentHp = 20;
	public bool invulnerable;
	public delegate void OnHealthChange(float percentage, GameObject GO);
	public static event OnHealthChange onHealthChange;

	void Start(){
		SendUpdateEvent();
	}

	/// <summary>
    /// 减少HP
    /// </summary>
    /// <param name="damage"></param>
	public void SubstractHealth(int damage){
		if(!invulnerable){

			CurrentHp = Mathf.Clamp(CurrentHp -= damage, 0, MaxHp);

			//当HP为0的时候 死亡
			if (isDead()) gameObject.SendMessage("Death", SendMessageOptions.DontRequireReceiver);
		}

		//更新Event
		SendUpdateEvent();
	}

	/// <summary>
    /// HP增加
    /// </summary>
    /// <param name="amount"></param>
	public void AddHealth(int amount){
		CurrentHp = Mathf.Clamp(CurrentHp += amount, 0, MaxHp);
		SendUpdateEvent();
	}
		
	//HP更新事件
	void SendUpdateEvent(){
		float CurrentHealthPercentage = 1f/MaxHp * CurrentHp;
		if(onHealthChange != null) onHealthChange(CurrentHealthPercentage, gameObject);
	}

	//死亡
	bool isDead(){
		return CurrentHp == 0;
	}
}
