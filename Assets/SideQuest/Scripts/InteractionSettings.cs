using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionSettings : MonoBehaviour {
    public bool isClickable = false;
    public bool isTeleport = false;
    public bool isGrabable = false;
    public bool isResizable = false;
    public bool isTelekinesis = false;
    public bool isPhysics = false;
    public bool physicsGravity = true;
    public float physicsMass = 1;
    public float physicsDrag = 0;
    public float physicsAngularDrag = 0.005f;
    public bool isSpawner = false;
    public float reSpawn = 50f;
    public bool isFixedGrab = false;

    public string uuid = System.Guid.NewGuid().ToString().ToUpper();
    public bool isTwoHandFixedGrab = false;
    public Vector3 primaryGrabPosition = Vector3.zero;
    public Vector3 primaryGrabRotation = Vector3.zero;
    public Vector3 secondaryGrabPosition = Vector3.zero;
}
