using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ShootingRange : ExpanseBehaviour {
	
	// Use this for initialization
	void Start () {
		GameObject.Find ("Range").AddComponent<shoot>();
		GameObject.Find ("handMag").AddComponent<mag> ();
		GameObject.Find ("LeftHandAnchor").AddComponent<resart> ();
		Transform LeftHand = GameObject.Find("LeftHandAnchor").transform;
		Transform RightHand = GameObject.Find("RightHandAnchor").transform;
		Transform gun = GameObject.Find("Colt").transform;
		//Transform revolver = GameObject.Find("revolver").transform;
		Transform mag = GameObject.Find("handMag").transform;
		gun.SetParent (RightHand, false);
		//revolver.SetParent (RightHand, false);
		mag.SetParent (LeftHand, false);
		gun.transform.localPosition = new Vector3(.01f, -0.04f, .05f);
		//revolver.transform.localPosition = new Vector3(.01f, -0.04f, .05f);
		mag.transform.localPosition = new Vector3(0.005f, -0.05f, -.02f);
		mag.localEulerAngles = new Vector3 (-90, 0, 90);
		gun.localEulerAngles = new Vector3 (0, 90, 0);
		//revolver.localEulerAngles = new Vector3 (0, 90, 0);
        List<GameObject> rootObjects = new List<GameObject>();
        Scene scene = SceneManager.GetActiveScene();
        scene.GetRootGameObjects( rootObjects );
	     // iterate root objects and do something
	   /*  for (int i = 0; i < rootObjects.Count; i++)
	     {
	         if(rootObjects[ i ].name == "Cockpit")
	         {

	         }
	     }*/
	}

	void onDestroy(){
		Transform revolver = GameObject.Find("revolver").transform;
		Transform mag = GameObject.Find("handMag").transform;
		Destroy (mag.gameObject);
		Destroy (revolver.gameObject);
	}

	// Update is called once per frame
	void Update () {
		
	}
}
