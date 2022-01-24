using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteGeneralizedPerspectiveProjection : MonoBehaviour
{
    [SerializeField]
    string targetGameObject;

    [Header("Projection")]
    [SerializeField]
    protected Vector3 screenUL = new Vector3(-1.0215f, 2.476f, -0.085972f);

    [SerializeField]
    protected Vector3 screenLL = new Vector3(-1.0215f, 1.324f, -0.085972f);

    [SerializeField]
    protected Vector3 screenLR = new Vector3(1.0215f, 1.324f, -0.085972f);

    [SerializeField]
    bool sendUpdate;

    [SerializeField]
    bool continuousUpdate;

    [SerializeField]
    RemoteTerminal remoteTerminal;

    [SerializeField]
    bool updateByEdge;

    [SerializeField]
    float leftEdge = -1.025f;

    [SerializeField]
    float topEdge = 2.476f;

    [SerializeField]
    float rightOffset = 0;

    [SerializeField]
    float bottomEdge = 1.324f;

    [SerializeField]
    float depth = -0.859f;

    [SerializeField]
    float eyeSeparation = 0.065f;

    [Header("Camera Offset")]
    [SerializeField]
    Vector3 cameraOffset;

    [SerializeField]
    bool updateCameraOffset;


    [Header("Networking")]
    [SerializeField]
    float sendDelay = 10; // ms

    float sendTimer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(updateByEdge)
        {
            screenUL = new Vector3(leftEdge, topEdge, depth);
            screenLL = new Vector3(leftEdge, bottomEdge, depth);
            screenLR = new Vector3(-leftEdge + rightOffset, bottomEdge, depth);
        }

        if(sendUpdate || continuousUpdate)
        {
            if (sendTimer <= 0)
            {
                remoteTerminal.SendCommand("setGeneralizedPerspectiveProjection " + screenUL.x + " " + screenUL.y + " " + screenUL.z + " " + screenLL.x + " " + screenLL.y + " " + screenLL.z + " " + screenLR.x + " " + screenLR.y + " " + screenLR.z);
                remoteTerminal.SendCommand("setEyeSeparation " + eyeSeparation);
                //CAVE2.BroadcastMessage(targetGameObject, "SetScreenUL", screenUL);
                //CAVE2.BroadcastMessage(targetGameObject, "SetScreenLL", screenLL);
                //CAVE2.BroadcastMessage(targetGameObject, "SetScreenLR", screenLR);

                sendUpdate = false;
                sendTimer = sendDelay / 1000.0f;
            }
            else
            {
                sendTimer -= Time.deltaTime;
            }
        }
        if(updateCameraOffset)
        {
            remoteTerminal.SendCommand("setGPPCameraOffset " + cameraOffset.x + " " + cameraOffset.y + " " + cameraOffset.z);
            //CAVE2.BroadcastMessage(targetGameObject, "SetScreenUL", screenUL);
            updateCameraOffset = false;
        }
    }
}
