using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObjectsTest : MonoBehaviour
{
    [SerializeField]
    GameObject prefabObject = null;

    [SerializeField]
    float spawnDelay = 0.05f;

    [SerializeField]
    int remainingSpawnCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("Spawn");
    }

    IEnumerator Spawn()
    {
        while (true)
        {
            while (remainingSpawnCount > 0)
            {
                remainingSpawnCount--;
                Instantiate(prefabObject, transform.position, transform.rotation);
            }
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    public void SetSpawnCount(int value)
    {
        CAVE2.SendMessage(gameObject.name, "SetSpawnCountRPC", value);
    }

    void SetSpawnCountRPC(int value)
    {
        remainingSpawnCount = value;
    }

    public void ClearSpawnedObjectsByTag()
    {
        CAVE2.SendMessage(gameObject.name, "ClearSpawnedObjectsByTagRPC");
    }

    void ClearSpawnedObjectsByTagRPC()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(prefabObject.tag);
        foreach (GameObject g in objs)
        {
            Destroy(g);
        }
    }
}
