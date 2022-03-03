using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnDisplayNode : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(!CAVE2.IsMaster())
        {
            gameObject.SetActive(false);
        }    
    }
}
