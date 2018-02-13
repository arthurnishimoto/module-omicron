using UnityEngine;
using System.Collections;

public class HoverOverIndicator : CAVE2Interactable
{
    [SerializeField]
    bool showHoverOver = true;

    [SerializeField]
    bool showPointingOver = true;

    [SerializeField]
    float highlightScaler = 1.05f;

    [SerializeField]
    Mesh defaultMesh;

    [SerializeField]
    Material hoverOverMaterial;

    GameObject hoverOverHighlight;
    new MeshRenderer renderer;

    [SerializeField]
    bool strobing;

    [SerializeField]
    float strobeSpeed;

    bool progressUp = true;
    float strobeProgress = 0.5f;

    Color originalHoverMatColor;

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
        else
        {
            hoverOverMaterial = new Material(hoverOverMaterial);
        }
        originalHoverMatColor = hoverOverMaterial.color;
        renderer.material = hoverOverMaterial;

        renderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateWandOverTimer();

        if(strobing)
        {
            if (progressUp)
            {
                strobeProgress += Time.deltaTime * strobeSpeed;
                if (strobeProgress > 1)
                    progressUp = false;
            }
            else
            {
                strobeProgress -= Time.deltaTime * strobeSpeed;
                if (strobeProgress < 0)
                    progressUp = true;
            }
            hoverOverMaterial.color = new Color(originalHoverMatColor.r, originalHoverMatColor.g, originalHoverMatColor.b, strobeProgress * 0.5f);
            renderer.enabled = true;
        }

        if((showHoverOver && wandOver) || (showPointingOver && wandPointing))
        {
            hoverOverMaterial.color = originalHoverMatColor;
            renderer.enabled = true;
        }
        else if(!strobing)
        {
            renderer.enabled = false;
        }
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
        return wandOver;
    }

    public void SetStrobe(bool value, float speed)
    {
        strobing = value;
        strobeSpeed = speed;
    }
}
