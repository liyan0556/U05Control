using UnityEngine;
using System.Collections;

public class EventListenerControl : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
        EventDispatcher.Instance().OnTick();
	}
}
