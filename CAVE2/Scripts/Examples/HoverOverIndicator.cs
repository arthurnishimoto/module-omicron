using UnityEngine;
using System.Collections;

public class HoverOverIndicator : CAVE2Interactable
{
    [SerializeField]
    bool hoverOver = false;

    [SerializeField]
    float lastHoverOverTime;

    [SerializeField]
    float wandOverTimeout = 0.15f;

    [SerializeField]
    float highlightScaler = 1.05f;

    Mesh defaultMesh;

    [SerializeField]
    Material hoverOverMaterial;

    GameObject hoverOverHighlight;
    new MeshRenderer renderer;

    [SerializeField]
    bool showHoverOver = true;

    // Use this for initialization
    void Start()
    {
        hoverOverHighlight = new GameObject("wandOverHighlight");
        hoverOverHighlight.transform.parent = transform;
        hoverOverHighlight.transform.position = transform.position;
        hoverOverHighlight.transform.rotation = transform.rotation;
        hoverOverHighlight.transform.localScale = Vector3.one * highlightScaler;

        if (defaultMesh == null)
        {
            defaultMesh = GetComponent<MeshFilter>().mesh;
        }
        hoverOverHighlight.AddComponent<MeshFilter>().mesh = defaultMesh;
        renderer = hoverOverHighlight.AddComponent<MeshRenderer>();

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
    void Update()
    {
        if (Time.time - lastHoverOverTime > wandOverTimeout)
        {
            hoverOver = false;
        }
        renderer.enabled = showHoverOver && hoverOver;
    }

    new void OnWandOver()
    {
        lastHoverOverTime = Time.time;
        hoverOver = true;
    }

    public void ShowHoverOverHighlight()
    {
        showHoverOver = true;
    }

    public void HideHoverOverHighlight()
    {
        showHoverOver = false;
    }

    public bool IsControllerOver()
    {
        return hoverOver;
    }
}
