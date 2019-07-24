using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRTK.Prefabs.CameraRig.UnityXRCameraRig.Input
{
    using UnityEngine;
#if USING_VRTK
    using Malimbe.PropertySerializationAttribute;
    using Malimbe.XmlDocumentationAttribute;
    using Zinnia.Action;

    /// <summary>
    /// Listens for the specified axis and emits the appropriate action.
    /// </summary>
    public class OmicronAxis1DAction : FloatAction
    {
        public enum TrackerType { UnityXR, Omicron };

        [SerializeField]
        TrackerType trackerSource = TrackerType.UnityXR;

        [SerializeField]
        string AxisName;

        [Header("Omicron Config")]
        [SerializeField]
        int sourceID;

        [SerializeField]
        CAVE2.Axis axis;

        [SerializeField]
        CAVE2.Button button = CAVE2.Button.None;

        [SerializeField]
        float value;

        protected virtual void Update()
        {
            switch (trackerSource)
            {
                case (TrackerType.UnityXR):
                    //GetComponent<UnityAxis1DAction>().enabled = true;
                    value = Input.GetAxis(AxisName);
                    break;
                case (TrackerType.Omicron):
                    //GetComponent<UnityAxis1DAction>().enabled = false;
                    if (button == CAVE2.Button.None)
                    {
                        value = Omicron.GetAxis(sourceID, axis);
                    }
                    else
                    {
                        value = Omicron.GetKey(sourceID, button) ? 1 : 0;
                    }
                    break;
            }
            Receive(value);
        }
    }
#else
    public class OmicronAxis1DAction : MonoBehaviour
    {
        [SerializeField]
        int sourceID;

        [SerializeField]
        CAVE2.Axis axis;

        [SerializeField]
        float value;
    }
#endif
}