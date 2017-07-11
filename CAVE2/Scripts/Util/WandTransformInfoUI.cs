using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WandTransformInfoUI : MonoBehaviour {

    Text text;

    // Use this for initialization
    void Start () {
        text = GetComponent<Text>();
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 wandPos = CAVE2.Input.GetWandPosition(1);
        Vector3 wandRot = CAVE2.Input.GetWandRotation(1).eulerAngles;

        text.text = "Wand Position: (" + wandPos.x.ToString("F3") + ", " + wandPos.y.ToString("F3") + ", " + wandPos.z.ToString("F3") + ") m";
        text.text += "\nWand Orientation: (" + wandRot.x.ToString("F3") + ", " + wandRot.y.ToString("F3") + ", " + wandRot.z.ToString("F3") + ") deg";
    }
}
