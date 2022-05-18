using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetScript : MonoBehaviour
{
    [SerializeField]
    MonoBehaviour targetScript;

    [SerializeField]
    bool enableReset;

    [SerializeField]
    float resetStartDelay = 2;

    [SerializeField]
    float resetWaitDuration = 2;

    [SerializeField]
    float resetTimer;

    // Update is called once per frame
    void Update()
    {
        if(enableReset)
        {
            if(resetTimer > resetStartDelay && targetScript.enabled)
            {
                targetScript.enabled = false;
            }
            else if (resetTimer > resetStartDelay + resetWaitDuration && !targetScript.enabled)
            {
                targetScript.enabled = true;
                enableReset = false;
            }

            resetTimer += Time.deltaTime;
        }
        else
        {
            resetTimer = 0;
        }
    }
}
