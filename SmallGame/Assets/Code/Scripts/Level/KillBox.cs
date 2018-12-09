using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class KillBox : MonoBehaviour {

	public bool RestartOnPlayerKill;
	public bool RestartOnEnemyKill;

	//destroy everything that enters this trigger
	void OnTriggerEnter(Collider coll){

		//人物死亡重置场景
		if(RestartOnPlayerKill && coll.CompareTag("Player")) StartCoroutine(RestartLevel());

		//敌人全部死亡重置场景
		if(RestartOnEnemyKill && coll.CompareTag("Enemy")) StartCoroutine(RestartLevel());

		Destroy (coll.gameObject);
	}

    /// <summary>
    /// 重置场景
    /// </summary>
    /// <returns></returns>
    IEnumerator RestartLevel(){
		
		
		UIManager UI = GameObject.FindObjectOfType<UIManager>();
		if (UI != null) {
			float fadeOutTime = 0.7f;
			UI.UI_fader.Fade (UIFader.FADE.FadeOut, fadeOutTime, 0);
			yield return new WaitForSeconds (fadeOutTime);
		}
		SceneManager.LoadScene (SceneManager.GetActiveScene().name);
	}
}