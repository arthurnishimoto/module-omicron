using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSpawner : MonoBehaviour {

    [SerializeField]
    GameObject prefabObject;

    [SerializeField]
    float spawnDelay = 0.5f;

	void Awake () {
        StartCoroutine("Spawn");
	}


    IEnumerator Spawn()
    {
        while (true)
        {
            Instantiate(prefabObject, transform.position, transform.rotation);
            yield return new WaitForSeconds(spawnDelay);
        }
    }
}
