using System.Collections;
using UnityEngine;
//Wait for me, I don't want to let you down
//love you into disease, but no medicine can.
//Created By HeXiaoTao
public class CamSlowMotionDelay : MonoBehaviour
{

    public float slowMotionTimeScale = .2f;

    public void StartSlowMotionDelay(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(SlowMotionRoutine(duration));
    }

    //slow motion delay
    IEnumerator SlowMotionRoutine(float duration)
    {

        //set timescale
        Time.timeScale = slowMotionTimeScale;

        //wait a moment...
        float startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < (startTime + duration))
        {
            yield return null;
        }

        //重置
        GameSettings settings = Resources.Load("GameSettings", typeof(GameSettings)) as GameSettings;
        if (settings != null)
        {
            Time.timeScale = settings.timeScale;
        }
        else
        {
            Time.timeScale = 1;
        }
    }
}
