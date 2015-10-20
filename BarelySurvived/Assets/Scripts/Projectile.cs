using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	float speed = 10;
	float damage = 1;
	public LayerMask collisionMask;
	float lifetime = 3;
	float skinWidth = 0.1f;

	void Start(){
		Destroy (gameObject, lifetime);

		Collider[] initialCollisions = Physics.OverlapSphere (transform.position, 0.1f, collisionMask);
		if (initialCollisions.Length > 0) {
			OnHitObject(initialCollisions[0], transform.position);
		}
	}

	public void setSpeed(float newSpeed){
		speed = newSpeed;
	}
	
	// Update is called once per frame
	void Update () {
		float moveDistance = speed * Time.deltaTime;
		CheckCollision (moveDistance);
		transform.Translate(Vector3.forward * Time.deltaTime * speed);
	}

	void CheckCollision (float moveDistance){
		Ray ray = new Ray (transform.position, transform.forward);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide)) {
			OnHitObject(hit.collider, hit.point);
		}

	}
		

	void OnHitObject(Collider c, Vector3 hitPoint){
		IDamageable damageableObject = c.GetComponent<IDamageable> ();
		if (damageableObject != null) {
			damageableObject.TakeHit(damage, hitPoint, transform.forward);
		}
		GameObject.Destroy (gameObject);
	}
}
