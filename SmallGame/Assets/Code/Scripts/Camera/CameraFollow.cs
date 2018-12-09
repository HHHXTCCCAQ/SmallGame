using UnityEngine;
//Wait for me, I don't want to let you down
//love you into disease, but no medicine can.
//Created By HeXiaoTao
public class CameraFollow : MonoBehaviour {

	public Transform target;
	[Header ("跟随设置")]
	public float distanceToTarget = 5; // 和目标的距离
	public float heightOffset = 5; // 相机的相对高度偏移
	public float viewAngle = 10; //旋转角度
	public Vector3 AdditionalOffset; //偏移量
	public bool FollowZAxis; //启用或者禁用z轴的移动

	[Header ("阻尼设置")]
	public float DampX = 3f;
	public float DampY = 3f;
	public float DampZ = 3f;

	[Header ("视线区域")]
	public float MinLeft;
	public float MaxRight;

	

	public bool UseWaveAreaCollider;
	public BoxCollider CurrentAreaCollider;
	public float AreaColliderViewOffset;

	void Start(){

		
		if (!target) SetPlayerAsTarget();
		if (target) {
			Vector3 playerPos = target.transform.position;
			transform.position = new Vector3(playerPos.x, playerPos.y - heightOffset, playerPos.z + (distanceToTarget));
		}
	}

	void Update () {
		if (target){

			
			float currentX = transform.position.x;
			float currentY = transform.position.y;
			float currentZ = transform.position.z;
			Vector3 playerPos = target.transform.position;
		
			currentX = Mathf.Lerp(currentX, playerPos.x, DampX * Time.deltaTime);		
			currentY = Mathf.Lerp(currentY, playerPos.y - heightOffset, DampY * Time.deltaTime);

			if (FollowZAxis) { 
				currentZ = Mathf.Lerp (currentZ, playerPos.z + distanceToTarget, DampZ * Time.deltaTime);
			} else {
				currentZ = distanceToTarget;
			}
			if(CurrentAreaCollider == null) UseWaveAreaCollider = false;
			if (!UseWaveAreaCollider) {
				transform.position = new Vector3 (Mathf.Clamp (currentX, MaxRight, MinLeft), currentY, currentZ) + AdditionalOffset;
			} else {
				transform.position = new Vector3 (Mathf.Clamp (currentX, CurrentAreaCollider.transform.position.x + AreaColliderViewOffset, MinLeft), currentY, currentZ) + AdditionalOffset;
			}
			transform.rotation = new Quaternion(0,180f,viewAngle,0);
		}
	}

	void SetPlayerAsTarget(){
		GameObject player = GameObject.FindGameObjectWithTag ("Player");
		if (player != null) {
			target = player.transform;
		}
	}
}