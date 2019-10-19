using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mag : MonoBehaviour {
	Animator anim;

	// Use this for initialization
	void Start () {
		anim = GameObject.Find ("SQPistol/Gun").GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter(Collider col){
		if (col.name == "door") {
			Animator anim = col.GetComponent<Animator> ();
			anim.SetBool ("open", true);
		}else if (col.name == "stage2Start") {
			GameObject.Find ("stage2").SetActive (false);
			col.gameObject.SetActive (false);
		}

		/*if (col.transform.name == "magIn" && shoot.ammo <= 0) {
			col.gameObject.SetActive (false);
			gameObject.SetActive (false);
			shoot.ammo = 12;
			anim.SetBool ("reload", true);
			anim.SetBool ("magDrop", false);
			anim.SetBool ("shoot", false);
			Debug.Log ("RELOAD");
			resart.holdingMag = false;
		}*/
	}
}
