using UnityEngine;
using System.Collections;

public class ChildTransformScript : MonoBehaviour {

    [SerializeField]
	protected Transform parent;

    [SerializeField]
    bool matchPosition = true;

    [SerializeField]
    bool matchRotation = true;

    [SerializeField]
    bool matchScale = true;

	protected Vector3 positionOffset;
    protected Vector3 rotationOffset;
    protected Vector3 scaleOffset;

    [SerializeField]
    bool useLateUpdate = false;

    public bool useOffset = true;
	// Use this for initialization
	void Start () {
		positionOffset = transform.position;
		rotationOffset = transform.eulerAngles;
		scaleOffset = transform.localScale;
	}
	
	// Update is called once per frame
	void Update () {
		if( !useLateUpdate )
		{
            UpdateTransform();
        }
	}

	void LateUpdate () {
		if( useLateUpdate )
		{
            UpdateTransform();

        }
	}

    void UpdateTransform()
    {
        int offset = 1;
        if (!useOffset)
            offset = 0;

        if (parent && matchPosition)
            transform.position = parent.position + positionOffset * offset;
        if (parent && matchRotation)
            transform.eulerAngles = parent.eulerAngles + rotationOffset * offset;
        if (parent && matchScale)
            transform.localScale = parent.localScale + scaleOffset * offset;
    }
}
