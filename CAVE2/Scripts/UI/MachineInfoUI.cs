using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineInfoUI : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GetComponent<UnityEngine.UI.Text>().text += ": " + CAVE2Manager.GetMachineName();
	}
}
