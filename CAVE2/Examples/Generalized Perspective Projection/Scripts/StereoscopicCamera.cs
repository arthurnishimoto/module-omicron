using UnityEngine;
using System.Collections;

public class StereoscopicCamera : MonoBehaviour {

    [SerializeField]
    bool generateCameras = false;

    [SerializeField]
    float eyeSeparation;

    GameObject leftEye;
    GameObject rightEye;

    [SerializeField]
    Material stereoscopicMaterial;

    RenderTexture leftTexture;
    RenderTexture rightTexture;

    // Use this for initialization
    void Start () {
        if (generateCameras && transform.parent.GetComponent<StereoscopicCamera>() == null)
        {
            SetupStereoCameras();

            if (transform.parent.name == "rightEye")
            {
                Destroy(gameObject);
            }

            GetComponent<Camera>().targetTexture = null;
        }
        else
        {
            Destroy(GetComponent<StereoscopicCamera>());
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

        // Setup stereo materials and render textures
        leftTexture = stereoscopicMaterial.GetTexture("_LeftTex") as RenderTexture;
        rightTexture = stereoscopicMaterial.GetTexture("_RightTex") as RenderTexture;

        leftEye.GetComponent<Camera>().targetTexture = leftTexture;
        rightEye.GetComponent<Camera>().targetTexture = rightTexture;

        // Temp
        leftEye.GetComponent<GeneralizedPerspectiveProjection>().UseProjection(false);
        rightEye.GetComponent<GeneralizedPerspectiveProjection>().UseProjection(false);
    }
}
