namespace VRTK.Prefabs.CameraRig.UnityXRCameraRig.Input
{
    using UnityEngine;
#if USING_VRTK
    using Malimbe.PropertySerializationAttribute;
    using Malimbe.XmlDocumentationAttribute;
    using Zinnia.Action;

    /// <summary>
    /// Listens for the specified key state and emits the appropriate action.
    /// </summary>
    public class OmicronButtonAction : BooleanAction
    {
        public enum TrackerType { UnityXR, Omicron };

        [SerializeField]
        TrackerType trackerSource = TrackerType.UnityXR;

        /// <summary>
        /// The <see cref="UnityEngine.KeyCode"/> to listen for state changes on.
        /// </summary>
        //[Serialized]
        //[field: DocumentedByXml]
        //public KeyCode KeyCode { get; set; }

        [SerializeField]
        KeyCode KeyCode;

        [Header("Omicron Config")]
        [SerializeField]
        int sourceID;

        [SerializeField]
        CAVE2.Button button;

        [SerializeField]
        bool value;

        protected virtual void Update()
        {
            switch (trackerSource)
            {
                case (TrackerType.UnityXR):
                    //GetComponent<UnityButtonAction>().enabled = true;
                    //Receive(Input.GetKey(KeyCode));
                    value = Input.GetKey(KeyCode);
                    break;
                case (TrackerType.Omicron):
                    //GetComponent<UnityButtonAction>().enabled = false;
                    value = Omicron.GetKey(sourceID, button);
                    break;
            }
            Receive(value);
        }

        public void SetAsOmicron()
        {
            trackerSource = TrackerType.Omicron;
        }

        public void SetAsUnityXR()
        {
            trackerSource = TrackerType.UnityXR;
        }
    }
#else
#endif
}