using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BreakableObject : MonoBehaviour, IDamagable<DamageObject> {

	public string hitSFX = "";

	[Header ("被损坏的物体")]
	public GameObject destroyedGO;
	public bool orientToImpactDir;

	[Header ("Spawn an item")]
	public GameObject spawnItem;
	public float spawnChance = 100;

	[Space(10)]
	public bool destroyOnHit;

	void Start(){
		gameObject.layer = LayerMask.NameToLayer("DestroyableObject");
	}

	//用武器攻击
	public void Hit(DamageObject DO){

		//播放音效
		if (hitSFX != "") {
			GlobalAudioPlayer.PlaySFXAtPosition (hitSFX, transform.position);
		}

		//创建被破坏的游戏物体
		if (destroyedGO != null) {
			GameObject BrokenGO = GameObject.Instantiate (destroyedGO);
			BrokenGO.transform.position = transform.position;

			//武器损坏后掉落的方向
			if (orientToImpactDir && DO.inflictor != null) {
				float dir = Mathf.Sign(DO.inflictor.transform.position.x - transform.position.x);
				BrokenGO.transform.rotation = Quaternion.LookRotation(Vector3.forward * dir);
			}
		}

		//spawn an item
		if (spawnItem != null) {
			if (Random.Range (0, 100) < spawnChance) {
				GameObject item = GameObject.Instantiate (spawnItem);
				item.transform.position = transform.position;

				//add up force to object
				item.GetComponent<Rigidbody>().velocity = Vector3.up * 8f;
			}
		}

		//销毁
		if (destroyOnHit) {
			Destroy(gameObject);
		}
	}
}