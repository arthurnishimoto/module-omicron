using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChildSpriteFader : MonoBehaviour {

    [SerializeField]
    float alpha = 1.0f;

    bool fade;

    [SerializeField]
    float fadeSpeed = 1;
    bool fadeOut;

    MaskableGraphic[] images;
    Color[] origColors;

	// Use this for initialization
	void Start () {
        images = GetComponentsInChildren<MaskableGraphic>();
        origColors = new Color[images.Length];
        for (int i = 0; i < images.Length; i++)
        {
            origColors[i] = images[i].color;
            images[i].color = new Color(origColors[i].r, origColors[i].g, origColors[i].b, origColors[i].a * alpha);
        }
        if( alpha == 1)
        {
            fadeOut = true;
        }
        else
        {
            fadeOut = false;
        }
    }
	
    public void Toggle()
    {
        fade = !fade;
    }

    public void FadeOut()
    {
        fadeOut = true;
        fade = true;
    }

    public void FadeIn()
    {
        fadeOut = false;
        fade = true;
    }

    // Update is called once per frame
    void Update () {
        if (fade)
        {
            if (fadeOut)
            {
                alpha -= Time.deltaTime * fadeSpeed;
                if (alpha < 0)
                {
                    alpha = 0;
                    fade = false;
                    fadeOut = false;
                }
            }
            else
            {
                alpha += Time.deltaTime * fadeSpeed;
                if (alpha > 1)
                {
                    alpha = 1;
                    fade = false;
                    fadeOut = true;
                }
            }

            for (int i = 0; i < images.Length; i++)
            {
                images[i].color = new Color(origColors[i].r, origColors[i].g, origColors[i].b, origColors[i].a * alpha);
            }
            
        }
	}
}
