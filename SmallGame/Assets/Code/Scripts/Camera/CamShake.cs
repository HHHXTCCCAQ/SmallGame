using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Wait for me, I don't want to let you down
//love you into disease, but no medicine can.
//Created By HeXiaoTao
[RequireComponent (typeof(Camera))]
public class CamShake : MonoBehaviour {

	public AnimationCurve camShakeY;
	public AnimationCurve camShakeX;
	public AnimationCurve camShakeZ;
	public float multiplier = 1f;
	public bool randomize; //randomizes the direction of the animationcurves by multiplying them with -1 or 1
	public float time = .5f;

    /// <summary>
    /// 相机震动
    /// </summary>
    /// <param name="intensity"></param>
	public void Shake(float intensity){
		StartCoroutine(DoShake(intensity));
	}

	IEnumerator DoShake(float scale){
		
		Vector3 rand = new Vector3(getRandomValue(), getRandomValue(), getRandomValue());
		scale *= multiplier;

		float t = 0;
		while (t < time) {
			if (randomize) {
				transform.localPosition = new Vector3 (camShakeX.Evaluate(t) * scale * rand.x, camShakeY.Evaluate(t) * scale * rand.y, camShakeZ.Evaluate(t) * scale * rand.z);				
			} else {
				transform.localPosition = new Vector3 (camShakeX.Evaluate(t) * scale, camShakeY.Evaluate(t) * scale, camShakeZ.Evaluate(t) * scale);
			}

			t += Time.deltaTime / time;
			yield return null;
		}
		transform.localPosition = Vector3.zero;
	}

	//returns a value of -1 or 1
	int getRandomValue(){
		int[] i = { -1, 1 };
		return i[Random.Range(0,2)];
	}
}