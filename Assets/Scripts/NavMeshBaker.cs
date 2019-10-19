using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class NavMeshBaker : MonoBehaviour {
	NavMeshSurface surface;

	// Use this for initialization
	void Start () {
		surface = gameObject.GetComponent<NavMeshSurface> ();
		/*for(int i = 0; i<surfaceObjects.Length; i++){
		}*/
		StartCoroutine (waitToBake ());

	}

	IEnumerator waitToBake(){
		yield return new WaitForSeconds (1);
		surface.BuildNavMesh();
		Debug.Log ("BAKED YO");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
