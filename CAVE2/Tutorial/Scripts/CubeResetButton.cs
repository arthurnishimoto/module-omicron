using UnityEngine;
using UnityEngine.UI;

public class CubeResetButton : MonoBehaviour {

    [SerializeField]
    PhysicsButton button;

    bool resetPressed;

    [SerializeField]
    Text text;

    // Use this for initialization
    void Start () {
	
	}

    // Update is called once per frame
    void Update()
    {
        GameObject[] cubes = GameObject.FindGameObjectsWithTag("Cube");
        text.text = "Cube Count: \n" + cubes.Length;

        if (button.pressed && !resetPressed)
        {
            foreach (GameObject g in cubes)
            {
                Destroy(g);
            }
            resetPressed = true;
        }
        else if (!button.pressed)
        {
            resetPressed = false;
        }
    }
}
