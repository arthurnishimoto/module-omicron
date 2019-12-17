using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrecisionTransformModifier : MonoBehaviour {

    [SerializeField]
    float currentIncrement = 0.1f;

    public enum TransformMode { Translate, Scale };

    [SerializeField]
    TransformMode currentTransformMode = TransformMode.Scale;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (CAVE2.Input.GetButtonDown(1, CAVE2.Button.Button3))
        {
            switch (currentTransformMode)
            {
                case (TransformMode.Scale):
                    currentTransformMode = TransformMode.Translate;
                    break;
                case (TransformMode.Translate):
                    currentTransformMode = TransformMode.Scale;
                    break;
            }
        }

        if ( CAVE2.Input.GetButtonDown(1, CAVE2.Button.ButtonUp))
        {
            switch(currentTransformMode)
            {
                case (TransformMode.Scale):
                    transform.localScale += Vector3.up * currentIncrement;
                    break;
                case (TransformMode.Translate):
                    transform.localPosition += Vector3.up * currentIncrement;
                    break;
            }
        }
        if (CAVE2.Input.GetButtonDown(1, CAVE2.Button.ButtonDown))
        {
            switch (currentTransformMode)
            {
                case (TransformMode.Scale):
                    transform.localScale += Vector3.down * currentIncrement;
                    break;
                case (TransformMode.Translate):
                    transform.localPosition += Vector3.down * currentIncrement;
                    break;
            }
        }
        if (CAVE2.Input.GetButtonDown(1, CAVE2.Button.ButtonLeft))
        {
            switch (currentTransformMode)
            {
                case (TransformMode.Scale):
                    transform.localScale += Vector3.left * currentIncrement;
                    break;
                case (TransformMode.Translate):
                    transform.localPosition += Vector3.left * currentIncrement;
                    break;
            }
        }
        if (CAVE2.Input.GetButtonDown(1, CAVE2.Button.ButtonRight))
        {
            switch (currentTransformMode)
            {
                case (TransformMode.Scale):
                    transform.localScale += Vector3.right * currentIncrement;
                    break;
                case (TransformMode.Translate):
                    transform.localPosition += Vector3.right * currentIncrement;
                    break;
            }
        }
    }
}
