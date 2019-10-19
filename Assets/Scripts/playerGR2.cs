using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class playerGR2 : MonoBehaviour {
	float levelTimer = 0f;
	int timerSeconds;
	bool updateTimer = false;

	int[] last3 = new int[3];
	Text[] last3Text = new Text[3];

	Text Timer;

	AudioSource audio;
	public Animator[] anims;

	// Use this for initialization
	void Start () {
		//audio = GameObject.Find ("alarmSFX").GetComponent<AudioSource> ();
		//Timer = GameObject.Find ("TimeRec").GetComponent<Text> ();

		anims = GameObject.FindObjectsOfType<Animator> ();

		//last3Text [0] = GameObject.Find ("Score 1").GetComponent<Text> ();
		//last3Text [1] = GameObject.Find ("Score 2").GetComponent<Text> ();
		//last3Text [2] = GameObject.Find ("Score 3").GetComponent<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (updateTimer) {
			levelTimer += Time.deltaTime * 1;

			levelTimer = Mathf.Round (levelTimer * 100f) / 100f;
		}
	}

	void OnTriggerEnter(Collider col){
		/*if (col.transform.name == "Trigger1") {
			enemy1.SetBool ("popUp", true);
			enemy2.SetBool ("popUp", true);
			enemy3.SetBool ("popUp", true);
			//StartTimer//
			updateTimer = true;
			levelTimer = 0f;

			for (int i = 0; i < anims.Length; i++) {
				anims[i].SetBool ("reset", false);
			}
		}else if (col.transform.name == "TriggerEnd") {
			audio.Play ();	
			for (int i = 0; i < anims.Length; i++) {
				anims[i].SetBool("die",false);
				anims[i].SetBool("popUp",false);
				anims[i].SetBool ("reset", true);
			}
			updateTimer = false;
			last3Text [2].text = last3Text [1].text;
			last3Text [1].text = last3Text [0].text;
			last3Text[0].text = levelTimer.ToString ();
			Timer.text = "Time: "+ levelTimer.ToString ();
		}*/
	}
}
