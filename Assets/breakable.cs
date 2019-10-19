using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class breakable : MonoBehaviour {
	bool pickedUp = false;
	GameObject shards;

	// Use this for initialization
	void Start () {
		shards = GameObject.Find ("shards");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionEnter(Collision col){
		// first collision is the hand picking up glass //
		if (!pickedUp) {
			pickedUp = true;
		}

		// second collision is when it hits something after its been picked up //
		if (pickedUp) {
			// Place an empty game object called "shards" in the prefab and add some bits of glass with a rigidbody on each //

			// Clone the shards at point of collision //
			GameObject shardsClone = Instantiate (shards, transform.position, transform.rotation);
			// Destroy this object //
			Destroy (gameObject);
		}
	}
}