using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Valve.VR;

public class shooting : MonoBehaviour {
	Animator gun;
	Transform barrel;
	GameObject magIn;
	AudioSource shot;
	bool gunReady = true;
	public static int ammo = 12;
	public float spread = .01f;
	GameObject bullethole;
	// Use this for initialization
	void Start () {
		bullethole = GameObject.Find ("hitPoint");
		gun = GameObject.Find ("SQPistol").transform.GetChild(0).GetComponent<Animator>();
		magIn = GameObject.Find ("magIn");
		shot = GameObject.Find ("gunshot").GetComponent<AudioSource>();
		magIn.SetActive (false);
		barrel = GameObject.Find ("myBarrel").transform;
	}
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown ("Fire1") && ammo > 0) {
		//if (SteamVR_Actions._default.GrabPinch.GetStateUp(SteamVR_Input_Sources.Any) && ammo > 0) {
			shot.Play ();
			StartCoroutine (shotCooldown ());
			Debug.Log ("shoot");
			gun.Play ("shoots", 0);
			RaycastHit hit;
			Vector3 direction = barrel.transform.forward;
			direction.x += Random.Range (-spread, spread);
			direction.y += Random.Range (-spread, spread);
			direction.z += Random.Range (-spread, spread);
			if (Physics.Raycast (barrel.position, direction, out hit, 1000f)) {
				GameObject bulletholeClone = Instantiate (bullethole, hit.point, Quaternion.FromToRotation (Vector3.up, hit.normal));
				bulletholeClone.transform.parent = hit.transform;
				if (hit.transform.name == "Enemy") {
					Debug.Log ("KILL");
					Animator anim = hit.transform.gameObject.GetComponent<Animator> ();
					anim.SetBool ("die", true);
					Destroy (hit.transform.gameObject, 5);
				}

				if (hit.transform.name == "reactBox") {
					Animator anim = hit.transform.GetChild(0).gameObject.GetComponent<Animator> ();
					anim.SetBool ("crouch", true);
					anim.SetBool ("stand", false);
					enemyGun eg = hit.transform.GetChild(0).GetChild(0).gameObject.GetComponent<enemyGun> ();
					eg.inCover = true;
				}
			}
		}	
	}

	IEnumerator shotCooldown(){
		yield return new WaitForSeconds(.06f);
		gunReady = true;
	}
}
