using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SDK : MonoBehaviour {

    public readonly static Queue<Action> RunOnMainThread = new Queue<Action>();

    public API API;

    private void Start() {
        API = new API(this);
        API.Start((JObject _session) => {});
    }
    void Update() {
        while (RunOnMainThread.Count > 0) {
            RunOnMainThread.Dequeue().Invoke();
        }
    }
}
