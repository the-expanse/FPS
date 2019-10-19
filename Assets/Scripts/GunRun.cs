using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GunRun : AppBehaviour {
	
	// Use this for initialization
	void Start () {
        InteractionSettings settings = GameObject.Find("SQPistol").AddComponent<InteractionSettings>();
        settings.isGrabable = true;
        settings.isPhysics = true;
        settings.isSpawner = true;
        settings.isFixedGrab = true;
        settings.primaryGrabPosition = new Vector3(0, 0, 0);
        settings.primaryGrabRotation = new Vector3(0, 180, 0);


		GameObject.Find ("Cockpit").AddComponent<player> ();
		GameObject.Find ("SQPistol").AddComponent<shoot>();
		GameObject.Find ("handMag/mag").AddComponent<mag>();
		Transform LeftHand = GameObject.Find("LeftHandAnchor").transform;
		Transform RightHand = GameObject.Find("RightHandAnchor").transform;
		//Transform gun = GameObject.Find("SQPistol").transform;
		//Transform mag = GameObject.Find("handMag").transform;
		/*gun.SetParent (RightHand, false);
		mag.SetParent (LeftHand, false);
		gun.transform.localPosition = new Vector3(0,0,0);
		mag.transform.localPosition = new Vector3(0,0,0);
		mag.localEulerAngles = new Vector3 (-90, 0, 90);
		gun.localEulerAngles = new Vector3 (0, 90, 0);*/
	}

	void onDestroy(){
		Transform revolver = GameObject.Find("SQPistol").transform;
		Transform lazer = GameObject.Find("Lazer").transform;
		Transform mag = GameObject.Find("handMag").transform;
		Destroy (mag.gameObject);
		Destroy (lazer.gameObject);
		Destroy (revolver.gameObject);
	}

	// Update is called once per frame
	void Update () {
		
	}
}
