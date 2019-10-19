using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class slide : MonoBehaviour {
	public Transform slider;
	Vector3 grabPoint;
	float offset;

	bool grabbed = false;
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (grabbed) {
			/*offset = Vector3.Distance (grabPoint, slider.position);
			slider.localPosition = new Vector3 (slider.localPosition.x, slider.localPosition.y, grabPoint.x);*/
		}

		//slider.position = transform.position;
	}

	void OnTriggerEnter(Collider col){
		grabbed = true;
		Debug.Log ("hit");
		grabPoint = transform.position;
	}

}
