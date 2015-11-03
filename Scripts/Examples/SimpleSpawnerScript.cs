using UnityEngine;
using System.Collections;

public class SimpleSpawnerScript : MonoBehaviour {
	public GameObject prefab;

	public bool spawn = false;
	public bool spawnMax = false;

	public bool spawnOnTimer = true;
	public float spawnDelay = 2.0f;

	public int spawnCount;
	public int spawnCapacity = 35;

	public Vector3 maxRange = Vector3.zero;
	public Vector3 minRange = Vector3.zero;
	float spawnTimer;

	public bool useRPC = false;

    public string spawnName;
    int spawnNumber = 1;

    public Transform initialWaypoint;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {

		spawnTimer += Time.deltaTime;

		GameObject[] obj = GameObject.FindGameObjectsWithTag(prefab.tag);
		spawnCount = obj.Length;

		if( spawnOnTimer && spawnTimer > spawnDelay && spawnCount < spawnCapacity){
			SpawnObject();
			spawnTimer = 0;
		}

		if( spawnMax && spawnCount < spawnCapacity){
			SpawnObject();
		}
		else if( spawnMax )
		{
			spawnMax = false;
		}

		if( spawn )
		{
			SpawnObject();
			spawn = false;
		}
	}

	void SpawnObject()
	{
		Random.seed = 1138;
		SpawnObjectRPC( transform.position + new Vector3(Random.Range(minRange.x,maxRange.x), Random.Range(minRange.y,maxRange.y), Random.Range(minRange.z,maxRange.z)), transform.localRotation);
	}

	void SpawnObjectRPC(Vector3 pos, Quaternion rot)
	{
		GameObject g = Instantiate( prefab, pos, rot ) as GameObject;
        if (initialWaypoint)
        {
            g.SendMessage("SetWaypoint", initialWaypoint);
        }
        if (spawnName != "")
        {
            g.name = spawnName + " " + spawnNumber;
            spawnNumber++;
        }
	}
}
