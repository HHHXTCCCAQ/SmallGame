using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EnemyWaveSystem : MonoBehaviour {

	public int MaxAttackers = 3; //能同时攻击玩家的最大数 

	[Header ("敌人队列")]
	public EnemyWave[] EnemyWaves;
	public int currentWave;

	[Header ("Slow Motion Settings")]
	public bool activateSlowMotionOnLastHit;
	public float effectDuration = 1.5f;

	[Header ("等级")]
	public bool loadNewLevel;
	public string levelName;

	void OnEnable(){
		EnemyActions.OnUnitDestroy += onUnitDestroy;
	}

	void OnDisable(){
		EnemyActions.OnUnitDestroy -= onUnitDestroy;
	}

	void Awake(){
		if (enabled) DisableAllEnemies();
	}

	void Start(){
		currentWave = 0;
		UpdateAreaColliders();
		StartNewWave();
	}

	void DisableAllEnemies(){
		foreach(EnemyWave wave in EnemyWaves){
			for(int i=0; i<wave.EnemyList.Count; i++){
				if (wave.EnemyList[i] != null){

					//deactivate enemy
					wave.EnemyList[i].SetActive(false);
				} else {
					
					//remove empty fields from the list
					wave.EnemyList.RemoveAt(i);
				}
			}
			foreach(GameObject g in wave.EnemyList){
				if (g != null) g.SetActive(false);
			}
		}
	}
		
	//开始创建敌人
	public void StartNewWave(){

		//隐藏指针
		HandPointer hp = GameObject.FindObjectOfType<HandPointer>();
		if (hp != null)	hp.DeActivateHandPointer ();

		//激活敌人
		foreach (GameObject g in EnemyWaves[currentWave].EnemyList) {
			if(g!=null)	g.SetActive (true);
		}
		Invoke("SetEnemyTactics", .1f);
	}

	//更新活动区域
	void UpdateAreaColliders(){

		//switch current area collider to a trigger
		if (currentWave > 0) {
			BoxCollider areaCollider = EnemyWaves [currentWave - 1].AreaCollider;
			if (areaCollider != null) {
				areaCollider.enabled = true;
				areaCollider.isTrigger = true;
				AreaColliderTrigger act = areaCollider.gameObject.AddComponent<AreaColliderTrigger> ();
				act.EnemyWaveSystem = this;
			}
		}

		//set next collider as camera area restrictor
		if(EnemyWaves[currentWave].AreaCollider != null) { 
			EnemyWaves[currentWave].AreaCollider.gameObject.SetActive(true);
		}

		CameraFollow cf = GameObject.FindObjectOfType<CameraFollow>();
		if (cf != null)	cf.CurrentAreaCollider = EnemyWaves [currentWave].AreaCollider;

		//show UI hand pointer
		HandPointer hp = GameObject.FindObjectOfType<HandPointer>();
		if (hp != null)	hp.ActivateHandPointer ();
	}

	//销毁敌人
	void onUnitDestroy(	GameObject g){
		if(EnemyWaves.Length > currentWave){
			EnemyWaves[currentWave].RemoveEnemyFromWave(g);
			if(EnemyWaves[currentWave].waveComplete()){
				currentWave += 1;
				if(!allWavesCompleted()){ 
					UpdateAreaColliders();
				} else{
					StartCoroutine (LevelComplete());
				}
			}
		}
	}

	//返回所有敌人是否创建
	bool allWavesCompleted(){
		int waveCount = EnemyWaves.Length;
		int waveFinished = 0;

		for(int i=0; i<waveCount; i++){
			if(EnemyWaves[i].waveComplete()) waveFinished += 1;
		}

		if(waveCount == waveFinished) 
			return true;
		else 
			return false;
	}

	//更新敌人状态
	void SetEnemyTactics(){
		EnemyManager.SetEnemyTactics();
	}

	//通过当前关卡
	IEnumerator LevelComplete(){

		//activate slow motion effect
		if (activateSlowMotionOnLastHit) {
			CamSlowMotionDelay cmd = Camera.main.GetComponent<CamSlowMotionDelay>();
			if (cmd != null) {
				cmd.StartSlowMotionDelay (effectDuration);
				yield return new WaitForSeconds (effectDuration);
			}
		}

		//Timeout before continuing
		yield return new WaitForSeconds (1f);

		//Fade to black
		UIManager UI = GameObject.FindObjectOfType<UIManager>();
		if (UI != null) {
			UI.UI_fader.Fade (UIFader.FADE.FadeOut, 2f, 0);
			yield return new WaitForSeconds (2f);
		}

		//Disable players
		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		foreach (GameObject p in players) {
			Destroy(p);
		}

		//Go to next level or show GAMEOVER screen
		if (loadNewLevel) {
			if (levelName != "")
				SceneManager.LoadScene (levelName);

		} else {

			//Show game over screen
			if (UI != null) {
				UI.DisableAllScreens ();
				UI.ShowMenu ("LevelComplete");
			}
		}
	}
}