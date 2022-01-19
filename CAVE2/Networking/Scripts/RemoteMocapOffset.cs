using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteMocapOffset : MonoBehaviour
{
    [SerializeField]
    string targetGameObject = null;

    [SerializeField]
    RemoteTerminal remoteTerminal = null;

    [SerializeField]
    Vector3 newOffset = Vector3.zero;

    [SerializeField]
    bool sendOffset = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(sendOffset)
        {
            remoteTerminal.SendCommand("setMocapOffset \"" + targetGameObject + "\" " + newOffset.x + " " + newOffset.y + " " + newOffset.z);
            sendOffset = false;
        }
    }
}
