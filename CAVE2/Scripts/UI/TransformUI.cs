using UnityEngine;
using UnityEngine.UI;

public class TransformUI : MonoBehaviour {

    public Text positionUIText;
    public Transform targetTransform;

    public bool local;

    Vector3 position;
    Vector3 eulerAngles;

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
            if (local)
            {
                position = targetTransform.localPosition;
                eulerAngles = targetTransform.localEulerAngles;
            }
            else
            {
                position = targetTransform.position;
                eulerAngles = targetTransform.eulerAngles;
            }

            positionUIText.text = "Position: " + position.ToString("N3") + "\nRotation: " + eulerAngles.ToString("N3");
        }
    }
}
