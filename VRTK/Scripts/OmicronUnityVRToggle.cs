using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK.Prefabs.CameraRig.UnityXRCameraRig.Input;

[ExecuteInEditMode]
public class OmicronUnityVRToggle : MonoBehaviour
{
    public enum TrackerType { UnityXR, Omicron };

    [SerializeField]
    TrackerType trackerSource = TrackerType.UnityXR;

    TrackerType lastTrackerSource;

    // Start is called before the first frame update
    void Start()
    {
        lastTrackerSource = trackerSource;
    }

    // Update is called once per frame
    void Update()
    {
        if (lastTrackerSource != trackerSource)
        {
            int count = 0;
            OmicronButtonAction[] oButtons = GetComponentsInChildren<OmicronButtonAction>();
            foreach(OmicronButtonAction ob in oButtons)
            {
                switch(trackerSource)
                {
                    case (TrackerType.UnityXR): ob.SetAsUnityXR(); break;
                    case (TrackerType.Omicron): ob.SetAsOmicron(); break;
                }
                count++;
            }

            OmicronAxis1DAction[] oAxis = GetComponentsInChildren<OmicronAxis1DAction>();
            foreach (OmicronAxis1DAction ox in oAxis)
            {
                switch (trackerSource)
                {
                    case (TrackerType.UnityXR): ox.SetAsUnityXR(); break;
                    case (TrackerType.Omicron): ox.SetAsOmicron(); break;
                }
                count++;
            }

            TrackedObject[] trackedObjs = GetComponentsInChildren<TrackedObject>();
            foreach (TrackedObject obj in trackedObjs)
            {
                switch (trackerSource)
                {
                    case (TrackerType.UnityXR): obj.SetAsUnityXR(); break;
                    case (TrackerType.Omicron): obj.SetAsOmicron(); break;
                }
                count++;
            }

            Debug.Log("Set " + count + " scripts to " + ((trackerSource == TrackerType.UnityXR) ? "UnityXR" : "Omicron"));
            lastTrackerSource = trackerSource;
        }
    }
}
