using UnityEngine;
using UnityEngine.UI;

public class TransformUI : MonoBehaviour {

    public Text positionUIText;
    public Transform targetTransform;

    // Use this for initialization
    void Start () {
        if (positionUIText == null && GetComponent<Text>())
            positionUIText = GetComponent<Text>();

        if (targetTransform == null)
            targetTransform = transform;
    }
	
	// Update is called once per frame
	void Update () {
        if (positionUIText)
        {
            positionUIText.text = "Position: " + targetTransform.position.ToString("N3") + "\nRotation: " + targetTransform.eulerAngles.ToString("N3");
        }
    }
}
