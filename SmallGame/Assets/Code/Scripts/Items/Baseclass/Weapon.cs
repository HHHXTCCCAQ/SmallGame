using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Weapon {

	public string weaponName;
	public GameObject playerHandPrefab;
	public GameObject WeaponEndState; // 游戏武器对象的视觉状态
	public int timesToUse = 1;
	public DEGENERATETYPE degenerateType; //武器的状态，是否还能再次使用

	public DamageObject damageObject;
	[Header("Sound Effects")]
	public string useSound = "";
	public string breakSound = "";

	public void useWeapon(){
		timesToUse = Mathf.Clamp(timesToUse-1, 0, 1000);
	}

	public void onHitSomething(){
		if(degenerateType == DEGENERATETYPE.DEGENERATEONHIT) useWeapon();

		//播放毁坏音效
		if(timesToUse == 1) damageObject.hitSFX = breakSound;
	}

	public void BreakWeapon(){
		if(WeaponEndState) {
			GameObject g = GameObject.Instantiate(WeaponEndState) as GameObject;
			g.transform.position = playerHandPrefab.transform.position;
		}
	}
}

public enum DEGENERATETYPE {
	DEGENERATEONUSE,
	DEGENERATEONHIT,
}
