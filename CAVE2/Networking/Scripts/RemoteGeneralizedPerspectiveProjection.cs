using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteGeneralizedPerspectiveProjection : MonoBehaviour
{
    [SerializeField]
    string targetGameObject;

    [SerializeField]
    protected Vector3 screenUL = new Vector3(-1.0215f, 2.476f, -0.085972f);

    [SerializeField]
    protected Vector3 screenLL = new Vector3(-1.0215f, 1.324f, -0.085972f);

    [SerializeField]
    protected Vector3 screenLR = new Vector3(1.0215f, 1.324f, -0.085972f);

    [SerializeField]
    bool sendUpdate;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(sendUpdate)
        {
            CAVE2.BroadcastMessage(targetGameObject, "SetScreenUL", screenUL);
            CAVE2.BroadcastMessage(targetGameObject, "SetScreenLL", screenLL);
            CAVE2.BroadcastMessage(targetGameObject, "SetScreenLR", screenLR);

            sendUpdate = false;
        }
    }
}
