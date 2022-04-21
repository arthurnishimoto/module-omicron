using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Simple script to toggle CAVE2PlayerController from Drive mode to Walk mode
// after a time delay. This is useful in physics simulations where the world
// needs to 'settle' on startup.

public class DelayPlayerWalkMode : MonoBehaviour
{
    [SerializeField]
    float delay = 5;

    float timer;

    [SerializeField]
    bool done;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(timer < delay)
        {
            timer += Time.deltaTime;
        }
        else if(!done)
        {
            GetComponent<CAVE2WandNavigator>().SetNavModeWalk(true);
            done = true;
            this.enabled = false;
        }
    }
}
