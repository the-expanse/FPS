using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class player : MonoBehaviour {
	GameObject[] EnemySet1 = new GameObject[3];
	GameObject[] EnemySet2 = new GameObject[3];
	GameObject[] EnemySet3 = new GameObject[3];
	GameObject[] EnemySet4 = new GameObject[2];
	GameObject[] EnemySet5 = new GameObject[2];
	GameObject[] EnemySet6 = new GameObject[3];
	GameObject[] EnemySet7 = new GameObject[3];
	GameObject[] EnemySet8 = new GameObject[2];

	float highScore = 0;
	bool scoreSet = false;

	float levelTimer = 0f;
	int timerSeconds;
	bool updateTimer = false;

	int[] last3 = new int[3];
	Text[] last3Text = new Text[3];
	Text highScoreText;

	Text Timer;

	AudioSource audio;
	public Animator[] anims;

	public static bool rangeP1;
	public static bool rangeP2;
	public static bool rangeP3;
	public static bool rangeP4;

	public static int kills = 0;
	public int kils = 0;


	GameObject wp;

	// Use this for initialization
	void Start () {
		wp = GameObject.Find ("Waypoint");
		audio = GameObject.Find ("alarmSFX").GetComponent<AudioSource> ();
		Timer = GameObject.Find ("TimeRec").GetComponent<Text> ();
		highScoreText = GameObject.Find ("highScoreText").GetComponent<Text> ();

		EnemySet1 [0] = GameObject.Find ("Target1");
		EnemySet1 [1] = GameObject.Find ("Target2");
		EnemySet1 [2] = GameObject.Find ("Target3");

		EnemySet2 [0] = GameObject.Find ("Target4");
		EnemySet2 [1] = GameObject.Find ("Target5");
		EnemySet2 [2] = GameObject.Find ("Target6");

		EnemySet3 [0] = GameObject.Find ("Target7");
		EnemySet3 [1] = GameObject.Find ("Target8");

		EnemySet4 [0] = GameObject.Find ("Target9");
		EnemySet4 [1] = GameObject.Find ("Target10");

		EnemySet5 [0] = GameObject.Find ("Target11");
		EnemySet5 [1] = GameObject.Find ("Target12");

		EnemySet6 [0] = GameObject.Find ("Target13");
		EnemySet6 [1] = GameObject.Find ("Target14");

		EnemySet7 [0] = GameObject.Find ("Target15");
		EnemySet7 [1] = GameObject.Find ("Target16");
		EnemySet7 [2] = GameObject.Find ("Target17");

		EnemySet8 [0] = GameObject.Find ("Target18");
		EnemySet8 [1] = GameObject.Find ("Target19");

		anims = GameObject.FindObjectsOfType<Animator> ();

		last3Text [0] = GameObject.Find ("Score 1").GetComponent<Text> ();
		last3Text [1] = GameObject.Find ("Score 2").GetComponent<Text> ();
		last3Text [2] = GameObject.Find ("Score 3").GetComponent<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
		kils = kills;
		if (updateTimer) {
			levelTimer += Time.deltaTime * 1;

			levelTimer = Mathf.Round (levelTimer * 100f) / 100f;
		}
	}

	void OnTriggerExit(Collider col){
		if (col.transform.name == "rangeP1") {
			//shoot.FT = 0;
			shoot.targetsHit = 0;
			shoot.updateTimer = false;
			rangeP1 = false;
			for (int i = 0; i < anims.Length; i++) {
				if (anims [i].transform.name != "block" && anims [i].transform.name != "Gun") {
					//anims[i].SetBool("shoot",false);
					anims[i].SetBool("die",false);
					anims[i].SetBool ("reset", true);
				}
			}
		}else if (col.transform.name == "rangeP2") {
			//shoot.FT = 0;
			shoot.targetsHit = 0;
			shoot.updateTimer = false;
			rangeP2 = false;
			for (int i = 0; i < anims.Length; i++) {
				//anims[i].SetBool("shoot",false);
				anims[i].SetBool("die",false);
				anims[i].SetBool ("reset", true);
			}
		}else if (col.transform.name == "rangeP3") {
			//shoot.FT = 0;
			shoot.targetsHit = 0;
			shoot.updateTimer = false;
			rangeP3 = false;
			for (int i = 0; i < anims.Length; i++) {
				//anims[i].SetBool("shoot",false);
				anims[i].SetBool("die",false);
				anims[i].SetBool ("reset", true);
			}
		}else if (col.transform.name == "rangeP4") {
			//shoot.FT = 0;
			shoot.targetsHit = 0;
			shoot.updateTimer = false;
			rangeP4 = false;
			for (int i = 0; i < anims.Length; i++) {
				//anims[i].SetBool("shoot",false);
				anims[i].SetBool("die",false);
				anims[i].SetBool ("reset", true);
			}
		}
	}

	void OnTriggerEnter(Collider col){
		if (col.transform.name == "modeSwap" && !shoot.onGunRange) {
			shoot.onGunRange = true;
		} else if(col.transform.name == "modeSwap" && shoot.onGunRange){
			shoot.onGunRange = false;
		}
		if (col.transform.name == "Trigger1") {
			//StartTimer//
			updateTimer = true;
			levelTimer = 0f;

			for (int i = 0; i < anims.Length; i++) {
				anims[i].SetBool ("reset", false);
			}

			audio.Play ();
			Animator anim1 = EnemySet1 [0].GetComponent<Animator> ();
			anim1.SetBool ("popUp", true);
			AudioSource popUp1 = EnemySet1[0].gameObject.GetComponent<AudioSource> ();
			popUp1.Play ();
			Animator anim2 = EnemySet1 [1].GetComponent<Animator> ();
			anim2.SetBool ("popUp", true);
			AudioSource popUp2 = EnemySet1[1].gameObject.GetComponent<AudioSource> ();
			popUp2.Play ();

			Animator anim3 = EnemySet1 [2].GetComponent<Animator> ();
			anim3.SetBool ("popUp", true);
			AudioSource popUp3 = EnemySet1[2].gameObject.GetComponent<AudioSource> ();
			popUp3.Play ();
		}else if (col.transform.name == "Trigger2") {
			Animator anim1 = EnemySet2 [0].GetComponent<Animator> ();
			anim1.SetBool ("popUp", true);
			AudioSource popUp1 = EnemySet2[0].gameObject.GetComponent<AudioSource> ();
			popUp1.Play ();

			Animator anim2 = EnemySet2 [1].GetComponent<Animator> ();
			anim2.SetBool ("popUp", true);
			AudioSource popUp2 = EnemySet2[1].gameObject.GetComponent<AudioSource> ();
			popUp2.Play ();

			Animator anim3 = EnemySet2 [2].GetComponent<Animator> ();
			anim3.SetBool ("popUp", true);
			AudioSource popUp3 = EnemySet2[2].gameObject.GetComponent<AudioSource> ();
			popUp3.Play ();

		}else if (col.transform.name == "Trigger3") {
			Animator anim1 = EnemySet3 [0].GetComponent<Animator> ();
			anim1.SetBool ("popUp", true);
			AudioSource popUp1 = EnemySet3[0].gameObject.GetComponent<AudioSource> ();
			popUp1.Play ();

			Animator anim2 = EnemySet3 [1].GetComponent<Animator> ();
			anim2.SetBool ("popUp", true);
			AudioSource popUp2 = EnemySet3[1].gameObject.GetComponent<AudioSource> ();
			popUp2.Play ();

		}else if (col.transform.name == "Trigger4") {
			Animator anim1 = EnemySet4 [0].GetComponent<Animator> ();
			anim1.SetBool ("popUp", true);
			AudioSource popUp1 = EnemySet4[0].gameObject.GetComponent<AudioSource> ();
			popUp1.Play ();

			Animator anim2 = EnemySet4 [1].GetComponent<Animator> ();
			anim2.SetBool ("popUp", true);
			AudioSource popUp2 = EnemySet4[1].gameObject.GetComponent<AudioSource> ();
			popUp2.Play ();
		}else if (col.transform.name == "Trigger5") {
			Animator anim1 = EnemySet5 [0].GetComponent<Animator> ();
			anim1.SetBool ("popUp", true);
			AudioSource popUp1 = EnemySet5[0].gameObject.GetComponent<AudioSource> ();
			popUp1.Play ();
			Animator anim2 = EnemySet5 [1].GetComponent<Animator> ();
			anim2.SetBool ("popUp", true);
			AudioSource popUp2 = EnemySet5[1].gameObject.GetComponent<AudioSource> ();
			popUp2.Play ();
		}else if (col.transform.name == "Trigger6") {
			Animator anim1 = EnemySet6 [0].GetComponent<Animator> ();
			anim1.SetBool ("popUp", true);
			AudioSource popUp1 = EnemySet6[0].gameObject.GetComponent<AudioSource> ();
			popUp1.Play ();
			Animator anim2 = EnemySet6 [1].GetComponent<Animator> ();
			anim2.SetBool ("popUp", true);
			AudioSource popUp2 = EnemySet6[1].gameObject.GetComponent<AudioSource> ();
			popUp2.Play ();
		}else if (col.transform.name == "Trigger7") {
			Animator anim1 = EnemySet7 [0].GetComponent<Animator> ();
			anim1.SetBool ("popUp", true);
			AudioSource popUp1 = EnemySet7[0].gameObject.GetComponent<AudioSource> ();
			popUp1.Play ();
			Animator anim2 = EnemySet7 [1].GetComponent<Animator> ();
			anim2.SetBool ("popUp", true);
			AudioSource popUp2 = EnemySet7[1].gameObject.GetComponent<AudioSource> ();
			popUp2.Play ();
			Animator anim3 = EnemySet7 [2].GetComponent<Animator> ();
			anim3.SetBool ("popUp", true);
			AudioSource popUp3 = EnemySet7[2].gameObject.GetComponent<AudioSource> ();
			popUp3.Play ();
		}else if (col.transform.name == "Trigger8") {
			Animator anim1 = EnemySet8 [0].GetComponent<Animator> ();
			anim1.SetBool ("popUp", true);
			AudioSource popUp1 = EnemySet8[0].gameObject.GetComponent<AudioSource> ();
			popUp1.Play ();
			Animator anim2 = EnemySet8 [1].GetComponent<Animator> ();
			anim2.SetBool ("popUp", true);
			AudioSource popUp2 = EnemySet8[1].gameObject.GetComponent<AudioSource> ();
			popUp2.Play ();
		}
		else if (col.transform.name == "TriggerEnd") {
			audio.Play ();	
			for (int i = 0; i < anims.Length; i++) {
				//anims[i].SetBool("shoot",false);
				anims[i].SetBool("die",false);
				anims[i].SetBool("popUp",false);
				anims[i].SetBool ("reset", true);
			}
			updateTimer = false;
			last3Text [2].text = last3Text [1].text;
			last3Text [1].text = last3Text [0].text;
			last3Text[0].text = levelTimer.ToString ();
			Timer.text = "Time: "+ levelTimer.ToString ();
			if(!scoreSet){
				highScore = levelTimer;
				highScoreText.text = "Fastest Time: " + highScore.ToString ();
				scoreSet = true;
			}else if (scoreSet && levelTimer <= highScore) {
				highScore = levelTimer;
				highScoreText.text = "Fastest Time: " + highScore.ToString ();
			}
		}

		if (col.transform.name == "rangeP1") {
			rangeP1 = true;
		}

		if (col.transform.name == "rangeP2") {
			rangeP2 = true;
		}
		if (col.transform.name == "rangeP3") {
			rangeP3 = true;
		}
		if (col.transform.name == "rangeP4") {
			rangeP4 = true;
		}
	}
}