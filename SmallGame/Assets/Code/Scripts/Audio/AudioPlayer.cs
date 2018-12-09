using UnityEngine;
//Wait for me, I don't want to let you down
//love you into disease, but no medicine can.
//Created By HeXiaoTao


[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour {

    public AudioItem[] AudioList;
    private AudioSource source;
    private float musicVolume = 1f;
    private float sfxVolume = 1f;

    void Awake() {
        GlobalAudioPlayer.audioPlayer = this;
        source = GetComponent<AudioSource>();

        //设置基本属性
        GameSettings settings = Resources.Load("GameSettings", typeof(GameSettings)) as GameSettings;
        if (settings != null) {
            musicVolume = settings.MusicVolume;
            sfxVolume = settings.SFXVolume;
        }
    }

    //播放混合音效
    public void playSFX(string name) {
        bool SFXFound = false;
        foreach (AudioItem s in AudioList) {
            if (s.name == name) {
                int rand = Random.Range(0, s.clip.Length);
                source.PlayOneShot(s.clip[rand]);
                source.volume = s.volume * sfxVolume;
                source.loop = s.loop;
                SFXFound = true;
            }
        }
        if (!SFXFound) Debug.Log("没有找到: " + name);
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="name">音效名称</param>
    /// <param name="worldPosition">播放位置</param>
    /// <param name="parent">父物体</param>
    public void playSFXAtPosition(string name, Vector3 worldPosition, Transform parent) {
        bool SFXFound = false;
        foreach (AudioItem s in AudioList) {
            if (s.name == name) {

                //check the time threshold
                if (Time.time - s.lastTimePlayed < s.MinTimeBetweenCall) {
                    return;
                } else {
                    s.lastTimePlayed = Time.time;
                }


                int rand = Random.Range(0, s.clip.Length);

                //创建播放器
                GameObject audioObj = new GameObject();
                audioObj.transform.parent = parent;
                audioObj.name = name;
                audioObj.transform.position = worldPosition;
                AudioSource audiosource = audioObj.AddComponent<AudioSource>();

                //音效设置
                audiosource.clip = s.clip[rand];
                audiosource.spatialBlend = 1.0f;
                audiosource.minDistance = 4f;
                audiosource.volume = s.volume * sfxVolume;
                audiosource.outputAudioMixerGroup = source.outputAudioMixerGroup;
                audiosource.loop = s.loop;
                audiosource.Play();

                //播放完成后销毁
                if (!s.loop && audiosource.clip != null) {
                    TimeToLive TTL = audioObj.AddComponent<TimeToLive>();
                    TTL.LifeTime = audiosource.clip.length;
                }
                SFXFound = true;
            }
        }
        if (!SFXFound) Debug.Log("没有找到:  " + name);
    }
    //方法复用
    public void playSFXAtPosition(string name, Vector3 worldPosition) {
        playSFXAtPosition(name, worldPosition, transform.root);
    }
    /// <summary>
    /// 播放音乐
    /// </summary>
    /// <param name="name"></param>
    public void playMusic(string name) {
        GameObject music = new GameObject();
        music.name = "Music";
        AudioSource AS = music.AddComponent<AudioSource>();
        foreach (AudioItem s in AudioList) {
            if (s.name == name) {
                AS.clip = s.clip[0];
                AS.loop = true;
                AS.volume = s.volume * musicVolume;
                AS.Play();
            }
        }
    }
}
