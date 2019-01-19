using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RandomSeedDisplay : MonoBehaviour {

    Text uiText;

	// Use this for initialization
	void Start () {
        uiText = GetComponent<Text>();
    }
	
	// Update is called once per frame
	void Update () {
	    if(uiText != null)
        {
            uiText.text = "Random Seed: " + Random.state;
        }
	}
}
