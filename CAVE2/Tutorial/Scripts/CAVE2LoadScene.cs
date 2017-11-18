using UnityEngine;
using System.Collections;

public class CAVE2LoadScene : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        // When using the CAVE2 Player Controller, the Head Sphere (child of Head)
        // is the trigger, not the body/capsule collider or center of CAVE2
        if(other.CompareTag("Player"))
        {
            // Normally to load a scene in Unity use use this:
            // UnityEngine.SceneManagement.SceneManager.LoadScene("Cube World Example"); // Or using the scene build index (int) 

            // To properly load a scene on the CAVE2 cluster use this instead:
            CAVE2.LoadScene("Cube World Example");
        }
    }
}
