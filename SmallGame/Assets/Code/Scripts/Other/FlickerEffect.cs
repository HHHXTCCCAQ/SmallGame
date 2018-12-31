using UnityEngine;
using System.Collections;
/// <summary>
/// 武器或者物体被毁坏的动画， 闪烁然后消失
/// </summary>
public class FlickerEffect : MonoBehaviour {

	public float pauzeBeforeStart = 1.3f;
	public float flickerSpeedStart = 15f;
	public float flickerSpeedEnd = 35f;
	public float Duration = 2f;
	public bool DestroyOnFinish;

	public GameObject[] GFX;

	public void Start () {
		StartCoroutine(FlickerCoroutine());
	}
	
	IEnumerator FlickerCoroutine(){

		
		yield return new WaitForSeconds (pauzeBeforeStart);

        //闪烁
        float t =0;
		while(t < 1){
			float speed = Mathf.Lerp (flickerSpeedStart, flickerSpeedEnd, MathUtilities.Coserp(0,1,t));
			float i = Mathf.Sin(Time.time * speed);
			foreach(GameObject g in GFX) g.SetActive(i>0);
			t += Time.deltaTime/Duration;
			yield return null;
		}

		
		foreach(GameObject g in GFX) g.SetActive(false);

		
		if (DestroyOnFinish) {
			Destroy (gameObject);
		}
	}
}
