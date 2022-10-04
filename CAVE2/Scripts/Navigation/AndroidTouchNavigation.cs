using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidTouchNavigation : MonoBehaviour
{
    [SerializeField]
    float rotateSpeed = 1;

    [SerializeField]
    float translateSpeed = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.touchCount == 1)
        {
            Vector2 touchDelta = Input.GetTouch(0).deltaPosition;

            transform.Rotate(touchDelta.y * Vector3.right * Time.deltaTime * rotateSpeed);
            transform.Rotate(touchDelta.x * Vector3.up * Time.deltaTime * rotateSpeed);
        }
        else if (Input.touchCount == 2)
        {
            Vector2 touchDelta = Input.GetTouch(0).deltaPosition;

            transform.Translate(touchDelta.y * Vector3.forward * Time.deltaTime * translateSpeed);
            transform.Translate(touchDelta.x * Vector3.right * Time.deltaTime * translateSpeed);
        }

    }
}
