using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndockMenu : MonoBehaviour {

    [SerializeField]
    public bool undocked;

    // [SerializeField]
    // Transform dockedParent;

    // [SerializeField]
    // Vector3 dockedPosition;

    // [SerializeField]
    // Quaternion dockedRotation;

    // Use this for initialization
    void Start () {
        // dockedParent = transform.parent;
        // dockedPosition = transform.localPosition;
        // dockedRotation = transform.localRotation;
    }

    // Update is called once per frame
    void Update () {

	}

    public void Undock()
    {
        if (!undocked)
        {
            undocked = true;
        }
    }

    public void Redock()
    {
        if (undocked)
        {

        }
    }
}
