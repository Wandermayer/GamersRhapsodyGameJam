using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity {

	public enum State{Idle, Chasing, Attacking, dead};
	State currentState;

	public ParticleSystem deathEffect;
	NavMeshAgent pathfinder;
	Transform target;
	LivingEntity targetEntity;

	Material enemySkinMaterial;
	Color enemyOriginalColor;

	float attackDistanceThreshold = 0.5f;
	float timeBetweenAttack = 1;
	float nextAttackTime;
	float myCollisionRadius;
	float targetCollisionRadius;
	float damage = 1;

	bool hasTarget;



	// Use this for initialization
	protected  override void Start () {

		base.Start();
		pathfinder = GetComponent<NavMeshAgent> ();
		enemySkinMaterial = GetComponent<Renderer> ().material;
		enemyOriginalColor = enemySkinMaterial.color;

		if (GameObject.FindGameObjectWithTag ("Player") != null) {
			currentState = State.Chasing;
			hasTarget = true;

			target = GameObject.FindGameObjectWithTag ("Player").transform;
			targetEntity = target.GetComponent<LivingEntity> ();
			targetEntity.OnDeath += OnTargetDeath;

			myCollisionRadius = GetComponent<CapsuleCollider> ().radius;
			targetCollisionRadius = target.GetComponent<CapsuleCollider> ().radius;

			StartCoroutine (UpdatePath ());
		}
	}

	public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection){
		if (damage >= health) {
			Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, deathEffect.startLifetime);
		}
		base.TakeHit (damage, hitPoint, hitDirection);
	}

	void OnTargetDeath(){
		hasTarget = false;
		currentState = State.Idle;

	}
	// Update is called once per frame
	void Update () {



		if (hasTarget == true) {
			if (Time.time > nextAttackTime) {
				float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;
				if (sqrDstToTarget < Mathf.Pow (attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2)) {
					nextAttackTime = Time.time + timeBetweenAttack;
					StartCoroutine (Attack ());
				}
			}
		}
	}

	IEnumerator Attack(){
		pathfinder.enabled = false;
		currentState = State.Attacking;

		Vector3 OriginalPos = transform.position;
		Vector3 dirToTarget = (target.position - transform.position).normalized;
		Vector3 attackPos = target.position - dirToTarget * (myCollisionRadius);

		float attackSpeed = 3;
		float percent = 0;

		enemySkinMaterial.color = Color.red;
		bool hasAppliedDamage = false;

		while (percent <= 1) {

			if(percent >= 0.5f && !hasAppliedDamage){
				hasAppliedDamage = true;
				targetEntity.TakeDamage(damage);
			}

			percent += Time.deltaTime * attackSpeed;
			float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
			transform.position = Vector3.Lerp (OriginalPos, attackPos, interpolation);


			yield return null;
		}
		enemySkinMaterial.color = enemyOriginalColor;
		currentState = State.Chasing;
		pathfinder.enabled = true;
	}

	IEnumerator UpdatePath(){
		float refreshRate = 0.25f;

		while (hasTarget) {
			if(currentState == State.Chasing){
				Vector3 dirToTarget = (target.position - transform.position).normalized;
				Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold/2);
				if(!dead){
					pathfinder.SetDestination(targetPosition);
				}
			
			}
			yield return new WaitForSeconds(refreshRate);
		}
	}
}
