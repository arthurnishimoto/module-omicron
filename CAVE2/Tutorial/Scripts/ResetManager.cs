using UnityEngine;
using System.Collections;

public class ResetManager : MonoBehaviour {

    public GameObject[] prefabs;

    public GameObject[] exitingObjects;

    public PhysicsButton button;
    bool resetPressed;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if(button.pressed && !resetPressed)
        {
            foreach(GameObject g in exitingObjects)
            {
                Destroy(g);
            }
            int i = 0;
            foreach (GameObject g in prefabs)
            {
                exitingObjects[i] = Instantiate(g);
                i++;
            }
            resetPressed = true;
        }
        else if(!button.pressed)
        {
            resetPressed = false;
        }
	}
}
