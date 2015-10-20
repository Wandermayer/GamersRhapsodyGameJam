using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {

	Rigidbody myRigidbody;
	Vector3 velocity;

	void Start () {
		myRigidbody = GetComponent<Rigidbody>();
	}
	
	public void move(Vector3 _velocity){
		velocity = _velocity;
	}

	 void FixedUpdate(){
		myRigidbody.MovePosition(myRigidbody.position + velocity * Time.deltaTime);
	}

	public void LookAt(Vector3 lookPoint){
		Vector3 heightCorrection = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
		transform.LookAt(heightCorrection);
	}

	void OnTriggerStay(Collider other){

	}

}

