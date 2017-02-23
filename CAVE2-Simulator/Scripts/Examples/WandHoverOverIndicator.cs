using UnityEngine;
using System.Collections;

public class WandHoverOverIndicator : MonoBehaviour {

	public bool wandOver = false;
	public float lastWandOverTime;
	public float wandOverTimeout = 0.15f;

	public Mesh defaultMesh;
	public Material hoverOverMaterial;
	GameObject wandOverHighlight;
	new MeshRenderer renderer;

	// Use this for initialization
	void Start () {
		wandOverHighlight = new GameObject("wandOverHighlight");
		wandOverHighlight.transform.parent = transform;
		wandOverHighlight.transform.position = transform.position;
		wandOverHighlight.transform.rotation = transform.rotation;
		wandOverHighlight.transform.localScale = Vector3.one * 1.1f;

		if( defaultMesh == null )
		{
			defaultMesh = GetComponent<MeshFilter>().mesh;
		}
		wandOverHighlight.AddComponent<MeshFilter>().mesh = defaultMesh;
		renderer = wandOverHighlight.AddComponent<MeshRenderer>();
		renderer.material = hoverOverMaterial;

		renderer.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		if( Time.time - lastWandOverTime > wandOverTimeout )
		{
			wandOver = false;
		}
		renderer.enabled = wandOver;
	}

	public void OnWandOver()
	{
		lastWandOverTime = Time.time;
		wandOver = true;
	}
}
