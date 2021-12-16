using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCubeColor : MonoBehaviour
{
    MeshRenderer rm;

    // Start is called before the first frame update
    void Start()
    {
        rm = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetColor(int colorID)
    {
        switch(colorID)
        {
            case (0): rm.material.color = Color.white; break;
            case (1): rm.material.color = Color.red; break;
            case (2): rm.material.color = Color.green; break;
            case (3): rm.material.color = Color.blue; break;
            case (4): rm.material.color = Color.black; break;
            case (5): rm.material.color = Color.cyan; break;
            case (6): rm.material.color = Color.gray; break;
            case (7): rm.material.color = Color.magenta; break;
            case (8): rm.material.color = Color.yellow; break;
        }
    }
}
