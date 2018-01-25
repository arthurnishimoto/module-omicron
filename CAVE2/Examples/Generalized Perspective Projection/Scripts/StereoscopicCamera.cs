using UnityEngine;
using System.Collections;

public class StereoscopicCamera : MonoBehaviour {

    [SerializeField]
    bool generateCameras = true;

    [SerializeField]
    float eyeSeparation;

    GameObject leftEye;
    GameObject rightEye;

    [SerializeField]
    Material stereoscopicMaterial;

    RenderTexture cameraTexture;

    // Use this for initialization
    void Start () {
        if (generateCameras)
        {
            Destroy(GetComponent<StereoscopicCamera>());
            SetupStereoCameras();

            if (transform.parent.name == "rightEye")
            {
                Destroy(gameObject);
            }
        }
        else
        {
            cameraTexture = GetComponent<Camera>().targetTexture;
            GetComponent<Camera>().targetTexture = null;
            stereoscopicMaterial.SetTexture("_ResultTex", cameraTexture);
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void SetupStereoCameras()
    {
        // Create the gameobjects for the eyes
        leftEye = Instantiate(gameObject, transform) as GameObject;
        leftEye.name = "leftEye";

        // Cloning the leftEye instead of the source since cloning
        // the source now will add another left eye
        rightEye = Instantiate(leftEye, transform) as GameObject;
        rightEye.name = "rightEye";

        // Set the eye separation
        leftEye.transform.localPosition = Vector3.left * eyeSeparation / 2.0f;
        rightEye.transform.localPosition = Vector3.right * eyeSeparation / 2.0f;

        // Cleanup unnecessary duplicated components
        Destroy(leftEye.GetComponent<AudioListener>());
        Destroy(rightEye.GetComponent<AudioListener>());

        // Temp
        leftEye.GetComponent<GeneralizedPerspectiveProjection>().UseProjection(false);
        rightEye.GetComponent<GeneralizedPerspectiveProjection>().UseProjection(false);
    }
}
