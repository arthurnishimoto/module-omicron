using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TransformUIText : MonoBehaviour {

    public Transform trackedObject;

    public bool local;

    Text canvasText;
    Vector3 position;
    Vector3 eulerAngles;

    // Use this for initialization
    void Start () {
        canvasText = GetComponent<Text>();
    }
	
	// Update is called once per frame
	void Update () {
        
        if(canvasText != null)
        {
            if(local)
            {
                position = trackedObject.localPosition;
                eulerAngles = trackedObject.localEulerAngles;
            }
            else
            {
                position = trackedObject.position;
                eulerAngles = trackedObject.eulerAngles;
            }

            canvasText.text = "Position: (" + position.x.ToString("F3") + ", " + position.y.ToString("F3") + ", " + position.z.ToString("F3") + ")";
            canvasText.text += "\nRotation: (" + eulerAngles.x.ToString("F3") + ", " + eulerAngles.y.ToString("F3") + ", " + eulerAngles.z.ToString("F3") + ")";
        }
    }
}
