using UnityEngine;
//Wait for me, I don't want to let you down
//love you into disease, but no medicine can.
//Created By HeXiaoTao

[System.Serializable]
//音效的属性
public class AudioItem {

    public string name;
    public float volume = 1f;
    public float MinTimeBetweenCall = 0;
    public bool loop;
    public AudioClip[] clip;
    [HideInInspector]
    public float lastTimePlayed = 0;
}
