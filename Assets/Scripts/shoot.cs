using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
//using Valve.VR;

public class shoot : MonoBehaviour {
	Animator gun;
	Transform barrel;
	AudioSource shot;
	bool gunReady = true;
	ParticleSystem smoke;
	GameObject magIn;
	public static int ammo = 12;
	public float spread = .02f;

	public static bool onGunRange = false;
	public bool onGRange = false;

	GameObject bullethole;

	float highScore = 0;
	public static int targetsHit = 0; 
	bool scoreSet = false;

	public float levelTimer = 0f;
	int timerSeconds;
	public static bool updateTimer = false;

	bool range1ScoreSet;
	bool range2ScoreSet;
	bool range3ScoreSet;
	bool range4ScoreSet;
	public bool rang1 = false;
	public bool rang2 = false;
	public bool rang3 = false;
	public bool rang4 = false;
	public static float s1FT;
	public static float s2FT;
	public static float s3FT;
	public static float s4FT;
	Text P1FT;
	Text P2FT;
	Text P3FT;
	Text P4FT;

	public static bool zedCrwaling = false;

	// Use this for initialization
	void Start () {
		P1FT = GameObject.Find ("rangeP1Time").GetComponent<Text>();
		P2FT = GameObject.Find ("rangeP2Time").GetComponent<Text>();
		P3FT = GameObject.Find ("rangeP3Time").GetComponent<Text>();
		P4FT = GameObject.Find ("rangeP4Time").GetComponent<Text>();

		bullethole = GameObject.Find ("bullethole");
		gun = transform.GetChild(0).GetComponent<Animator>();
		magIn = transform.Find ("Gun/Gun3/magIn").gameObject;
		magIn.SetActive (false);
		shot = transform.Find ("Gun/Gun3/gunshot").gameObject.GetComponent<AudioSource>();
		barrel = transform.Find ("Gun/Gun3/barrel");
	}


	// Update is called once per frame
	void Update () {
		onGRange = onGunRange;
		rang1 = player.rangeP1;
		rang2 = player.rangeP2;
		rang3 = player.rangeP3;
		rang4 = player.rangeP4;
		if (updateTimer) {
			levelTimer += Time.deltaTime * 1;

			levelTimer = Mathf.Round (levelTimer * 100f) / 100f;
		}

		//if (SteamVR_Actions._default.GrabPinch.GetStateUp(SteamVR_Input_Sources.Any) && ammo > 0) {
		if (Input.GetButtonDown ("Fire1") && ammo > 0) {
			//ammo--;
			shot.Play ();
			//smoke.Play ();
			gunReady = false;
			StartCoroutine (shotCooldown ());
			Debug.Log ("shoot");
			gun.Play ("shoots", 0);
			//revolver.Play ("revShoot", 0);
			RaycastHit hit;
			Vector3 direction = barrel.transform.forward;
			direction.x += Random.Range (-spread, spread);
			direction.y += Random.Range (-spread, spread);
			direction.z += Random.Range (-spread, spread);
			if (Physics.Raycast (barrel.position, direction, out hit, 1000f)) {
				//GameObject bulletholeClone = Instantiate (bullethole, hit.point, Quaternion.FromToRotation (Vector3.up, hit.normal));
				//bulletholeClone.transform.parent = hit.transform;
				if (!onGunRange && hit.transform.name == "Target") {
					Animator anim = hit.transform.parent.parent.parent.gameObject.GetComponent<Animator> ();
					anim.SetBool ("die", true);
					anim.SetBool ("reset", false);
					AudioSource src = hit.transform.gameObject.GetComponent<AudioSource> ();
					src.Play ();
				} else if (onGunRange && hit.transform.name == "Target") {
					targetsHit++;
					if (targetsHit == 1) {
						Debug.Log ("TIMER");
						updateTimer = true;
						levelTimer = 0f;
					} else if (targetsHit == 3) {
						targetsHit = 0;
						Debug.Log ("TIMER END");
						updateTimer = false;
						if (player.rangeP1 && !range1ScoreSet) {
							range1ScoreSet = true;
							s1FT = levelTimer;
							P1FT.text = "Fastest Time: " + levelTimer.ToString ();
						} else if(player.rangeP1 && range1ScoreSet && levelTimer <= s1FT){
							s1FT = levelTimer;
							P1FT.text = "Fastest Time: " + levelTimer.ToString ();
						}

						if (player.rangeP2 && !range2ScoreSet) {
							range2ScoreSet = true;
							s2FT = levelTimer;
							P2FT.text = "Fastest Time: " + levelTimer.ToString ();
						} else if(player.rangeP2 && range2ScoreSet && levelTimer <= s2FT){
							s2FT = levelTimer;
							P2FT.text = "Fastest Time: " + levelTimer.ToString ();
						}

						if (player.rangeP3 && !range3ScoreSet) {
							range3ScoreSet = true;
							s3FT = levelTimer;
							P3FT.text = "Fastest Time: " + levelTimer.ToString ();
						} else if(player.rangeP3 && range3ScoreSet && levelTimer <= s3FT){
							s3FT = levelTimer;
							P3FT.text = "Fastest Time: " + levelTimer.ToString ();
						}

						if (player.rangeP4 && !range4ScoreSet) {
							range4ScoreSet = true;
							s4FT = levelTimer;
							P4FT.text = "Fastest Time: " + levelTimer.ToString ();
						} else if(player.rangeP4 && range4ScoreSet && levelTimer <= s4FT){
							s4FT = levelTimer;
							P4FT.text = "Fastest Time: " + levelTimer.ToString ();
						}
					}
					Animator anim = hit.transform.parent.gameObject.GetComponent<Animator> ();
					anim.SetBool ("die", true);
					anim.SetBool ("reset", false);
					AudioSource src = hit.transform.gameObject.GetComponent<AudioSource> ();
					src.Play ();
				}
			}
		}

		if (ammo <= 0) {
			gun.SetBool ("magDrop", true);
			gun.SetBool ("shoot", false);
			gun.SetBool ("reload", false);
			magIn.SetActive (true);
		}
	}
	
	IEnumerator shotCooldown(){
		yield return new WaitForSeconds(.06f);
		gunReady = true;
	}
}
