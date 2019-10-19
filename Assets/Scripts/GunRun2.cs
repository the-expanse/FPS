using UnityEngine.AI;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GunRun2 : ExpanseBehaviour {

	NavMeshAgent[] agent;
	Transform Player;

	// Use this for initialization
	void Start () {
		agent = GameObject.FindObjectsOfType<NavMeshAgent> ();
		Player = GameObject.Find ("Cockpit").transform;

		foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>()) {
			if (go.name == "fwe") {
				go.SetActive (false);
				go.AddComponent<enemyGun> ();
			}
		}
		GameObject.Find ("PlasmaBall").AddComponent<plasmaBall> ();
		GameObject.Find ("Cockpit").AddComponent<playerGR2> ();
		GameObject.Find ("GunRun2").AddComponent<shootGR2>();
		GameObject.Find ("handMag/mag").AddComponent<mag> ();
		Transform LeftHand = GameObject.Find("LeftHandAnchor").transform;
		Transform RightHand = GameObject.Find("RightHandAnchor").transform;
		Transform gun = GameObject.Find("SQPistol").transform;
		Transform mag = GameObject.Find("handMag").transform;
		gun.SetParent (RightHand, false);
		mag.SetParent (LeftHand, false);
		gun.transform.localPosition = new Vector3(0,0,0);
		mag.transform.localPosition = new Vector3(0,0,0);
		mag.localEulerAngles = new Vector3 (-90, 0, 90);
		gun.localEulerAngles = new Vector3 (0, 90, 0);
	}

	void onDestroy(){
		Transform gun = GameObject.Find("SQPistol").transform;
		Transform mag = GameObject.Find("handMag").transform;
		Destroy (mag.gameObject);
		Destroy (gun.gameObject);
	}

	// Update is called once per frame
	void Update () {
		Vector3 targetPos = new Vector3 (Player.position.x, this.transform.position.y, Player.position.z);
		for (int i = 0; i < agent.Length; i++) {
			agent[i].transform.LookAt (targetPos);	
		}
	}
}
