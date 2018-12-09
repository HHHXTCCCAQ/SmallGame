using UnityEngine;

public class LevelInit : MonoBehaviour {

	[Space(5)]
	[Header ("Settings")]
	public string LevelMusic = "Music";
	public string showMenuAtStart = "";
	public bool playMusic = true;
	public bool createUI;
	public bool createInputManager;
	public bool createAudioPlayer;
	public bool createGameCamera;
	private GameObject audioplayer;
	private GameSettings settings;

	void Awake() {

        settings = Resources.Load("GameSettings", typeof(GameSettings)) as GameSettings;
        if(settings != null){
            Time.timeScale = settings.timeScale;
            Application.targetFrameRate = settings.framerate;
        }

		
		if(!GameObject.FindObjectOfType<AudioPlayer>() && createAudioPlayer)	audioplayer = GameObject.Instantiate(Resources.Load("AudioPlayer"), Vector3.zero, Quaternion.identity) as GameObject;

		
		if(!GameObject.FindObjectOfType<InputManager>() && createInputManager) GameObject.Instantiate(Resources.Load("InputManager"), Vector3.zero, Quaternion.identity);

		
		if(!GameObject.FindObjectOfType<UIManager>() && createUI) GameObject.Instantiate(Resources.Load("UI"), Vector3.zero, Quaternion.identity);
	
		
		if(!GameObject.FindObjectOfType<CameraFollow>() && createGameCamera) GameObject.Instantiate(Resources.Load("GameCamera"), Vector3.zero, Quaternion.identity);

		if(playMusic && createAudioPlayer) Invoke("PlayMusic", 1f);

		if(!string.IsNullOrEmpty(showMenuAtStart)) ShowMenuAtStart();
	}

	void PlayMusic() {
		if(audioplayer != null)	audioplayer.GetComponent<AudioPlayer>().playMusic(LevelMusic);
	}

	void ShowMenuAtStart() {
		 GameObject.FindObjectOfType<UIManager>().ShowMenu(showMenuAtStart);
	}
}