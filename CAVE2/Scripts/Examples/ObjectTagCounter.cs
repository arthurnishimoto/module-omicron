using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectTagCounter : MonoBehaviour
{
    [SerializeField]
    new string tag = "";

    [SerializeField]
    Text[] uiTextObjects = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GameObject[] g = GameObject.FindGameObjectsWithTag(tag);

        foreach (Text uiText in uiTextObjects)
        {
            uiText.text = "'" + tag + "' count = " + g.Length;
        }
    }
}
