using UnityEngine;
using System.Collections;

public class StereoscopicCamera : MonoBehaviour {

    GameObject leftEye;
    GameObject rightEye;

    float eyeSeparation = 0.064f; // m

    [SerializeField]
    RenderTexture leftTexture;

    [SerializeField]
    RenderTexture rightTexture;

    // Use this for initialization
    void Start () {

        leftEye = new GameObject("LeftEye");
        rightEye = new GameObject("rightEye");

        leftEye.AddComponent<Camera>().CopyFrom(GetComponent<Camera>());
        rightEye.AddComponent<Camera>().CopyFrom(GetComponent<Camera>());

        leftEye.transform.SetParent(transform);
        rightEye.transform.SetParent(transform);

        leftEye.transform.localPosition = Vector3.left * eyeSeparation;
        rightEye.transform.localPosition = -Vector3.left * eyeSeparation;

        //leftTexture = new RenderTexture(Screen.width, Screen.height, 16);
        leftEye.GetComponent<Camera>().targetTexture = leftTexture;

        //rightTexture = new RenderTexture(Screen.width, Screen.height, 16);
        rightEye.GetComponent<Camera>().targetTexture = rightTexture;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
