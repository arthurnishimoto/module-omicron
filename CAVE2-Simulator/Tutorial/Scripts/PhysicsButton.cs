using UnityEngine;
using System.Collections;

public class PhysicsButton : MonoBehaviour {

    Vector3 restingPosition = new Vector3(-0.3f, 0.0f, 0.0f);
    Vector3 maxPressedPosition = new Vector3(-0.2f, 0.0f, 0.0f);

    public float buttonSpringStrength = 1.5f;

    float pressForce;
    float pressedProgress;
    bool collision;
    public bool pressed;

    public Material pressedMaterial;
    Material baseMaterial;

    // Use this for initialization
    void Start () {
        baseMaterial = GetComponent<Renderer>().material;
	}
	
	// Update is called once per frame
	void Update () {

        pressedProgress = pressForce / buttonSpringStrength;
        transform.localPosition = Vector3.Lerp(restingPosition, maxPressedPosition, pressedProgress);

        if(pressedProgress >= 0.8f)
        {
            pressed = true;
            GetComponent<Renderer>().material = pressedMaterial;
        }
        else
        {
            pressed = false;
            GetComponent<Renderer>().material = baseMaterial;
        }

        if(!collision && pressForce > 0)
        {
            pressForce -= Time.deltaTime * buttonSpringStrength * 10;
            if (pressForce < 0)
                pressForce = 0;
        }
    }

    void OnCollisionStay(Collision c)
    {
        pressForce = c.impulse.magnitude;
        collision = true;
    }

    void OnCollisionExit()
    {
        collision = false;
    }
}
