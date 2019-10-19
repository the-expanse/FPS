using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//using Valve.VR;

public class shootGR2 : MonoBehaviour {
	Animator gun;
	Transform barrel;
	AudioSource shot;
	bool gunReady = true;
	GameObject magIn;
	public static int ammo = 12;

	GameObject bullethole;



	// Use this for initialization
	void Start () {
		bullethole = GameObject.Find ("bullethole");
		gun = GameObject.Find ("SQPistol").transform.GetChild(0).GetComponent<Animator>();
		magIn = GameObject.Find ("magIn");
		magIn.SetActive (false);
		shot = GameObject.Find ("gunshot").GetComponent<AudioSource>();
		barrel = GameObject.Find ("barrel").transform;
	}
	
	// Update is called once per frame
	void Update () {
		//if (SteamVR_Actions._default.GrabPinch.GetStateUp(SteamVR_Input_Sources.Any) && ammo > 0) {
		if (Input.GetButtonDown ("Fire1") && ammo > 0) {
			//ammo--;
			shot.Play ();
			gunReady = false;
			StartCoroutine (shotCooldown ());
			Debug.Log ("shoot");
			gun.Play ("shoots", 0);
			RaycastHit hit;
			if (Physics.Raycast (barrel.position, barrel.transform.forward, out hit, 1000f)) {
				Debug.Log (hit.transform.name);
				//GameObject bulletholeClone = Instantiate (bullethole, hit.point, Quaternion.FromToRotation (Vector3.up, hit.normal));
				//bulletholeClone.transform.parent = hit.transform;
				if (hit.transform.name == "Target") {
					Animator anim = hit.transform.GetChild(0).gameObject.GetComponent<Animator> ();
					anim.SetBool ("die", true);
					anim.SetBool ("popUp", false);
					//AudioSource src = hit.transform.gameObject.GetComponent<AudioSource> ();
					//src.Play ();
				}
			}
		}

		if (ammo <= 0) {
			gun.SetBool ("magDrop", true);
			gun.SetBool ("shoot", false);
			gun.SetBool ("reload", false);
			magIn.SetActive (true);
		}
	}
	
	IEnumerator shotCooldown(){
		yield return new WaitForSeconds(.06f);
		gunReady = true;
	}
}
