/**************************************************************************************************
* THE OMICRON PROJECT
 *-------------------------------------------------------------------------------------------------
 * Copyright 2010-2018		Electronic Visualization Laboratory, University of Illinois at Chicago
 * Authors:										
 *  Arthur Nishimoto		anishimoto42@gmail.com
 *-------------------------------------------------------------------------------------------------
 * Copyright (c) 2010-2018, Electronic Visualization Laboratory, University of Illinois at Chicago
 * All rights reserved.
 * Redistribution and use in source and binary forms, with or without modification, are permitted 
 * provided that the following conditions are met:
 * 
 * Redistributions of source code must retain the above copyright notice, this list of conditions 
 * and the following disclaimer. Redistributions in binary form must reproduce the above copyright 
 * notice, this list of conditions and the following disclaimer in the documentation and/or other 
 * materials provided with the distribution. 
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR 
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND 
 * FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE  GOODS OR SERVICES; LOSS OF 
 * USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
 * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *************************************************************************************************/
 
using UnityEngine;
using System.Collections;

public class HoverOverIndicator : CAVE2Interactable
{
    [SerializeField]
    bool showHoverOver = true;

    [SerializeField]
    bool showPointingOver = true;

    [SerializeField]
    Vector3 highlightScaler = new Vector3(1.05f, 1.05f, 1.05f);

    [SerializeField]
    bool useSimplifiedMesh = false;

    [SerializeField]
    Mesh defaultMesh = null;

    [SerializeField]
    Mesh simpleMesh = null;

    [SerializeField]
    Material hoverOverMaterial = null;

    GameObject hoverOverHighlight;
    new MeshRenderer renderer;

    [SerializeField]
    bool strobing = false;

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
        hoverOverHighlight.transform.localScale = highlightScaler;

        if (defaultMesh == null)
        {
            defaultMesh = GetComponent<MeshFilter>().mesh;
        }
        if (useSimplifiedMesh)
        {
            defaultMesh = simpleMesh;
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

        if((showHoverOver && wandTouching) || (showPointingOver && wandPointing))
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
        return wandTouching;
    }

    public void SetStrobe(bool value, float speed)
    {
        strobing = value;
        strobeSpeed = speed;
    }
}
