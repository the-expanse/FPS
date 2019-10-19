using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyGun : MonoBehaviour {
	GameObject bullet;

	float shootInterval;
	float coverInterval;
	public bool inCover = false;
	public Transform player1;

	// Use this for initialization
	void Start () {
		bullet = GameObject.Find ("bullet");
		StartCoroutine (shoot ());
		StartCoroutine (Cover ());
		StartCoroutine (updateSight ());
	}

	IEnumerator updateSight(){
		yield return new WaitForSeconds (.008f);
		Vector3 lookPos = new Vector3 (player1.position.x,player1.position.y, player1.position.z);
		this.transform.LookAt (lookPos);
		StartCoroutine (updateSight ());
	}
	
	// Update is called once per frame
	void Update () {

	}

	IEnumerator Cover(){
		coverInterval = Random.Range (3, 9);
		yield return new WaitForSeconds (coverInterval + .2f);
		//ENTER COVER ANIM//
		if (transform.root.name == "reactBox") {
			Animator anim = transform.root.GetChild (0).GetComponent<Animator> ();
			anim.SetBool ("crouch", true);
			anim.SetBool ("stand", false);
		} else {
			Animator anim = transform.root.GetComponent<Animator>();	
			anim.SetBool ("crouch", true);
			anim.SetBool ("stand", false);
		}
		inCover = true;
		Debug.Log("IN COVER");
	}

	void fire(){
		RaycastHit hit;
		float spread = .01f;
		Vector3 direction = transform.forward;
		direction.x += Random.Range (-spread, spread);
		direction.y += Random.Range (-spread, spread);
		direction.z += Random.Range (-spread, spread);
		if (Physics.Raycast (transform.position, direction, out hit, 1000f)) {
			GameObject bullethole = GameObject.Find ("hitPoint");
			GameObject bulletholeClone = Instantiate (bullethole, hit.point, Quaternion.FromToRotation (Vector3.up, hit.normal));
			bulletholeClone.transform.parent = hit.transform;
			Destroy (bulletholeClone, 3);
			if (hit.transform.name == "Sphere") {
				Operator op = hit.transform.gameObject.GetComponent<Operator>();
				if (op.playerReady) {
					op.playerReady = false;
					Animator bloodSplat = hit.transform.GetChild (0).gameObject.GetComponent<Animator> ();
					bloodSplat.Play ("fade", 0);
					op.health--;
					op.hurt++;
					Debug.Log ("HURT");	
				}
			}
		}
	}

	IEnumerator shoot(){
		if (!inCover) {
			shootInterval = (Random.Range (1, 4));
			yield return new WaitForSeconds (shootInterval);
			//Debug.Log("SHOOT");
			fire ();
			AudioSource src = gameObject.GetComponent<AudioSource> ();
			src.Play ();
			StartCoroutine (shoot ());
		} else {
			coverInterval = Random.Range (4, 9);
			yield return new WaitForSeconds (coverInterval + .2f);
			//EXIT COVER ANIM//
			inCover = false;
			Debug.Log("EXIT COVER");
			if (transform.root.name == "reactBox") {
				Animator anim = transform.root.GetChild (0).GetComponent<Animator> ();
				anim.SetBool ("crouch", false);
				anim.SetBool ("stand", true);
			} else {
				Animator anim = transform.root.GetComponent<Animator>();	
				anim.SetBool ("crouch", false);
				anim.SetBool ("stand", true);
			}
			StartCoroutine (shoot ());
			StartCoroutine (Cover ());
		}
	}


}
