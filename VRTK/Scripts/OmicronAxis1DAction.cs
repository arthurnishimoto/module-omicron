using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRTK.Prefabs.CameraRig.UnityXRCameraRig.Input
{
    using UnityEngine;
    using Malimbe.PropertySerializationAttribute;
    using Malimbe.XmlDocumentationAttribute;
    using Zinnia.Action;

    /// <summary>
    /// Listens for the specified axis and emits the appropriate action.
    /// </summary>
    public class OmicronAxis1DAction : FloatAction
    {
        [SerializeField]
        int sourceID;

        [SerializeField]
        CAVE2.Axis axis;

        [SerializeField]
        float value;

        protected virtual void Update()
        {
            value = Omicron.GetAxis(sourceID, axis);
            Receive(value);
        }
    }
}