using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class PlayerInput : LivingEntity {
	public AudioClip BGMusic;
	public AudioClip deathMusic;
	public AudioClip highEnergyMusic;

	public float moveSpeed = 5f;
	public Text energyBar;
	public float energyLevel = 50f;
	bool canAddEnergy = true;
	bool CanSubtractEnergy = true;
	public Vector3 moveVelocity;
	int numberOfShots = 0;

	Camera viewCamera;
	PlayerController controller;
	GunController gunController;

	// Use this for initialization
	protected override void Start () {

		base.Start ();
		controller = GetComponent<PlayerController>();
		viewCamera = Camera.main;
		gunController = GetComponent<GunController>();


	}
	
	// Update is called onceper frame
	void Update () {
		energyLevel = Mathf.Clamp (energyLevel, 0, 100);
		energyBar.text = "Energy Level: " + energyLevel;
		if (CanSubtractEnergy) {
			StartCoroutine (subtractEnergy ());
			CanSubtractEnergy = false;
		}
		if (moveVelocity.x != 0 || moveVelocity.z != 0) {
			if(canAddEnergy){
			StartCoroutine (addEnergy ());
				canAddEnergy = false;
			}
		}
		//MoveInput
		Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		moveVelocity = moveInput.normalized * moveSpeed;
		controller.move(moveVelocity);

		//LookInput
		Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
		Plane groundPlane = new Plane(Vector3.up,Vector3.zero);
		float rayDistance;

		if(groundPlane.Raycast(ray, out rayDistance)){
			Vector3 point = ray.GetPoint(rayDistance);
			//Debug.DrawLine(ray.origin, point, Color.red);
			controller.LookAt(point);
		}

		//WeaponInput
		if(Input.GetMouseButton(0)){

			gunController.Shoot();
			numberOfShots++;
			if(numberOfShots >= 5){
				energyLevel -= 2;
				numberOfShots = 0;
			}
		}
	}

	//Adding Energy
	IEnumerator addEnergy(){
		 {
			Debug.Log("Enery Movement Subtracted!");
			energyLevel -= 2f;

		}
		yield return new WaitForSeconds (0.5f);
		//I know im subtracting but i cant be bothered to change it! :D
		canAddEnergy = true;
	}

	//Constantly subtracting energy
	IEnumerator subtractEnergy(){
		{
			energyLevel -= 1f;
			Debug.Log ("EnerySubtracted");
		}
		yield return new WaitForSeconds (0.8f);
		CanSubtractEnergy = true;
	}


}
