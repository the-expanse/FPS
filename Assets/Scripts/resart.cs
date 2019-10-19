using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class resart : MonoBehaviour {
	Animator[] targets;
	GameObject Magazine;
	public static bool holdingMag = false;
	GameObject tempMag;

	// Use this for initialization
	void Start () {
		Magazine = GameObject.Find ("handMag");
		//Magazine.SetActive (false);
		targets = GameObject.FindObjectsOfType<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter(Collider col){
		if (col.name == "restart") {
			for (int i = 0; i < targets.Length; i++) {
				targets [i].SetBool ("reset", true);
				targets [i].SetBool ("die", false);
				Debug.Log ("RESET");
			}
		}
		if(col.name == "fullMag" && !holdingMag){
			col.gameObject.SetActive (false);
			tempMag = col.gameObject;
			Magazine.SetActive (true);
			holdingMag = true;
			StartCoroutine (spawnMag ());
		}
	}

	IEnumerator spawnMag(){
		yield return new WaitForSeconds (2f);
		tempMag.SetActive (true);
	}
}
