using UnityEngine;
using System.Collections;

public class HeadMarker : MonoBehaviour {

    public Material lineMaterial;

    LineRenderer headToGroundLine;

	// Use this for initialization
	void Start () {
        headToGroundLine = gameObject.AddComponent<LineRenderer>();
        headToGroundLine.useWorldSpace = true;
        headToGroundLine.material = lineMaterial;
        headToGroundLine.SetWidth(0.05f, 0.05f);
    }
	
	// Update is called once per frame
	void Update () {
        headToGroundLine.SetPosition(0, transform.position);
        headToGroundLine.SetPosition(1, new Vector3(transform.position.x, transform.parent.position.y, transform.position.z));
    }
}
