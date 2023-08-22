using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnDisplayNode : MonoBehaviour
{
    [SerializeField]
    bool showInEditor = true;

    // Start is called before the first frame update
    void Start()
    {
        if(!CAVE2.IsMaster())
        {
            gameObject.SetActive(false);
        }
#if UNITY_EDITOR
        if(showInEditor)
        {
            gameObject.SetActive(true);
        }
#endif
    }
}
