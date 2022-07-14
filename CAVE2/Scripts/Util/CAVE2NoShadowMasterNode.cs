using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAVE2NoShadowMasterNode : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
#if !UNITY_EDITOR
        if(CAVE2.IsMaster())
        {
            GetComponent<Light>().shadows = LightShadows.None;
        }
#endif
    }
}
