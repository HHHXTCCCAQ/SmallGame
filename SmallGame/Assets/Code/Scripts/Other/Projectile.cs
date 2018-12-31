using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
[RequireComponent (typeof(Collider))]
public class Projectile : MonoBehaviour {

	public float speed = 10;
	public DIRECTION direction;
	public bool destroyOnHit;
	public GameObject EffectOnSpawn;
	private DamageObject damage;

	void Start () {
		GetComponent<Rigidbody>().velocity = new Vector2((int)direction * speed, 0);
		GetComponent<Collider>().isTrigger = true;

		//抛射武器的方向
		transform.rotation = Quaternion.Euler(0f, (direction == DIRECTION.Right? 180 : 0), 0f);

		//实例化特效
		if(EffectOnSpawn) {
			GameObject effect = GameObject.Instantiate(EffectOnSpawn) as GameObject;
			effect.transform.position = transform.position;
		}
	}


	void OnTriggerEnter(Collider coll) {
		if(coll.CompareTag("Enemy")) {

		
			IDamagable<DamageObject> damagableObject = coll.GetComponent(typeof(IDamagable<DamageObject>)) as IDamagable<DamageObject>;
			if(damagableObject != null) {
				damagableObject.Hit(damage);
				if(destroyOnHit) Destroy(gameObject);
			}
		}
	}

	
	public void SetDamage(DamageObject d){
		damage = d;
	}
}
