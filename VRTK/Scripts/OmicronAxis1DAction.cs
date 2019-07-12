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
#else
    public class OmicronAxis1DAction : MonoBehaviour
    {
        [SerializeField]
        int sourceID;

        [SerializeField]
        CAVE2.Axis axis;

        [SerializeField]
        float value;
#endif
}