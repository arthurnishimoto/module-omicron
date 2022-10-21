using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteGeneralizedPerspectiveProjection : MonoBehaviour
{
    [SerializeField]
    string targetGameObject = "";

    [Header("Projection")]
    [SerializeField]
    protected Vector3 screenUL = new Vector3(-1.0215f, 2.476f, -0.085972f);

    [SerializeField]
    protected Vector3 screenLL = new Vector3(-1.0215f, 1.324f, -0.085972f);

    [SerializeField]
    protected Vector3 screenLR = new Vector3(1.0215f, 1.324f, -0.085972f);

    [SerializeField]
    Vector3 widthHeightDepth = Vector3.zero;

    [SerializeField]
    bool sendUpdate = false;

    [SerializeField]
    bool continuousUpdate = false;

    [SerializeField]
    bool updateByEdge = false;

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

    [Header("Networking")]
    [SerializeField]
    float sendDelay = 10; // ms

    float sendTimer;

    [SerializeField]
    bool useExternalScriptToSend = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (updateByEdge)
        {
            screenUL = new Vector3(leftEdge, topEdge, depth);
            screenLL = new Vector3(leftEdge, bottomEdge, depth);
            screenLR = new Vector3(-leftEdge + rightOffset, bottomEdge, depth);
        }

        widthHeightDepth.x = screenLL.x - screenLR.x;
        widthHeightDepth.y = screenUL.y - screenLL.y;
        widthHeightDepth.z = screenUL.z;

        if (sendUpdate || continuousUpdate)
        {
            if (sendTimer <= 0)
            {
                if (useExternalScriptToSend)
                {
                    CAVE2.BroadcastMessage(gameObject.name, "SetScreenUL", screenUL);
                    CAVE2.BroadcastMessage(gameObject.name, "SetScreenLL", screenLL);
                    CAVE2.BroadcastMessage(gameObject.name, "SetScreenLR", screenLR);
                    CAVE2.BroadcastMessage(gameObject.name, "SetEyeSeparation", eyeSeparation);
                }
                else
                {
                    CAVE2.BroadcastMessage(targetGameObject, "SetScreenUL", screenUL);
                    CAVE2.BroadcastMessage(targetGameObject, "SetScreenLL", screenLL);
                    CAVE2.BroadcastMessage(targetGameObject, "SetScreenLR", screenLR);
                    CAVE2.BroadcastMessage(targetGameObject, "SetEyeSeparation", eyeSeparation);
                }

                sendUpdate = false;
                sendTimer = sendDelay / 1000.0f;
            }
            else
            {
                sendTimer -= Time.deltaTime;
            }
        }
    }

    // These only update the local state of this script
    void SetRemoteScreenUL(Vector3 screenUL)
    {
        this.screenUL = screenUL;
    }

    void SetRemoteScreenLL(Vector3 screenLL)
    {
        this.screenLL = screenLL;
    }

    void SetRemoteScreenLR(Vector3 screenLR)
    {
        this.screenLR = screenLR;
    }

    void SetRemoteEyeSeparation(float eyeSeparation)
    {
        this.eyeSeparation = eyeSeparation;
    }

}
