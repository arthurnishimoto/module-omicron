using UnityEngine;
using System.Collections;

public class WandHoverOverIndicator : MonoBehaviour {

	public bool wandOver = false;
	public float lastWandOverTime;
	public float wandOverTimeout = 0.15f;

    public float highlightScaler = 1.05f;

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
		wandOverHighlight.transform.localScale = Vector3.one * highlightScaler;

		if( defaultMesh == null )
		{
			defaultMesh = GetComponent<MeshFilter>().mesh;
		}
		wandOverHighlight.AddComponent<MeshFilter>().mesh = defaultMesh;
		renderer = wandOverHighlight.AddComponent<MeshRenderer>();

        if (hoverOverMaterial == null)
        {
            // Create a basic highlight material
            hoverOverMaterial = new Material(Shader.Find("Standard"));
            hoverOverMaterial.SetColor("_Color", new Color(0, 1, 1, 0.25f));
            hoverOverMaterial.SetFloat("_Mode", 3); // Transparent
            hoverOverMaterial.SetFloat("_Glossiness", 0);
        }
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
