using UnityEngine;
using System.Collections;

public class getReal3DHeadUpdater : MonoBehaviour
{
#if USING_GETREAL3D
    // Offset to tracking data (ex. object pivot vs tracking marker center)
    [SerializeField]
    Vector3 posOffset = Vector3.zero;

    [SerializeField]
    Vector3 rotOffset = Vector3.zero;

    [SerializeField]
    bool useLateUpdate = false;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!useLateUpdate)
        {
            transform.localPosition = getReal3D.Input.head.position + posOffset;
            transform.localRotation = getReal3D.Input.head.rotation;
            transform.Rotate(rotOffset);
        }
    }

    void LateUpdate()
    {
        if (useLateUpdate)
        {
            transform.localPosition = getReal3D.Input.head.position + posOffset;
            transform.localRotation = getReal3D.Input.head.rotation;
            transform.Rotate(rotOffset);
        }
    }


    public void SetPositionOffset(object[] data)
    {
        float x = posOffset.x;
        float y = posOffset.y;
        float z = posOffset.z;

        float.TryParse((string)data[0], out x);
        float.TryParse((string)data[1], out y);
        float.TryParse((string)data[2], out z);

        posOffset = new Vector3(x, y, z);
    }

    public void SetRotationOffset(object[] data)
    {
        float x = posOffset.x;
        float y = posOffset.y;
        float z = posOffset.z;

        float.TryParse((string)data[0], out x);
        float.TryParse((string)data[1], out y);
        float.TryParse((string)data[2], out z);

        rotOffset = new Vector3(x, y, z);
    }
#endif
}
