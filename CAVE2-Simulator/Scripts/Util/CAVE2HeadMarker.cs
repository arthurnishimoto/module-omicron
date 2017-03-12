using UnityEngine;
using System.Collections;

public class CAVE2HeadMarker : MonoBehaviour {

    public int headID = 1;

    LineRenderer headToGroundLine;
    LineRenderer forwardLine;
    public Material lineMaterial;

	// Use this for initialization
	void Start () {
        headToGroundLine = gameObject.AddComponent<LineRenderer>();
        headToGroundLine.SetWidth(0.02f, 0.02f);
        headToGroundLine.material = lineMaterial;

        if (!CAVE2.OnCAVE2Display())
        {
            GameObject forwardReference = new GameObject("Head-ForwardRef");
            forwardReference.transform.parent = transform;
            forwardLine = forwardReference.AddComponent<LineRenderer>();

            forwardLine.SetWidth(0.02f, 0.02f);
            forwardLine.material = lineMaterial;
        }

        CAVE2.RegisterHeadObject(headID, gameObject);
    }
	
	// Update is called once per frame
	void LateUpdate () {
        transform.localPosition = CAVE2.GetHeadPosition(1);
        transform.localRotation = CAVE2.GetHeadRotation(1);

        headToGroundLine.SetPosition(0, new Vector3(transform.position.x, transform.parent.position.y, transform.position.z));
        headToGroundLine.SetPosition(1, transform.position);

        if (!CAVE2.OnCAVE2Display())
        {
            forwardLine.SetPosition(0, transform.position);
            forwardLine.SetPosition(1, transform.position + transform.forward * 0.2f);
        }
    }
}
