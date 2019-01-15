using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using omicron;
using omicronConnector;
using System;

public class OmicronKinectImageClient : OmicronEventClient
{
    public enum ImageType { Color, Depth};
    public ImageType imageType = ImageType.Color;

    public int sourceID = -1; // -1 for any

    public int lastEventFlag;
    public int extraDataSize;

    public Texture2D texture;
    public Material outputMaterial;

    public int[] byteSample = new int[4];
    public int x;
    public int y;

    Color[] colorArray;

    // Use this for initialization
    new void Start()
    {
        eventOptions = EventBase.ServiceType.ServiceTypeGeneric;
        InitOmicron();

        int width = 1920;
        int height = 1080;

        if (imageType == ImageType.Color)
        {
            width = 1920;
            height = 1080;
        }
        else if (imageType == ImageType.Depth)
        {
            width = 512;
            height = 424;
        }

        texture = new Texture2D(width, height);
        outputMaterial.mainTexture = texture;

        int pixelCount = width * height;
        colorArray = new Color[pixelCount];
    }

    public void Update()
    {
        texture.Apply();
    }

    public override void OnEvent(EventData e)
    {
        lastEventFlag = (int)e.flags;

        byte[] bitmapBytes = e.extraData;
        extraDataSize = (int)bitmapBytes.Length;

        int packetID = (int)e.flags; // 0 to 8640

        if (imageType == ImageType.Color)
        {
            y = packetID / 8;
            x = (packetID % 8) * 240; // 8 segments of 240 pixels per line

            for (int i = 0; i < extraDataSize; i += 4)
            {
                int pixelID = i * (extraDataSize / 4);
                Array.Copy(bitmapBytes, i, byteSample, 0, 4);

                Color color = new Color(byteSample[2] / 255.0f, byteSample[1] / 255.0f, byteSample[0] / 255.0f);

                //int overallImageID = packetID * (extraDataSize / 4) + pixelID;
                //Debug.Log(overallImageID);
                //colorArray[overallImageID] = color;

                //Debug.Log(byteSample[0] + " " + byteSample[1] + " " + byteSample[2] + " " + byteSample[3]);
                texture.SetPixel(x, y, color);
                x++;
            }
        }
        else if( imageType == ImageType.Depth)
        {
            y = packetID / 4;
            x = (packetID % 4) * 128;

            extraDataSize = 848;

            for (int i = 0; i < extraDataSize; i += 4)
            {
                int pixelID = i * (extraDataSize / 4);
                Array.Copy(bitmapBytes, i, byteSample, 0, 4);

                Color color = new Color(byteSample[2] / 255.0f, byteSample[1] / 255.0f, byteSample[0] / 255.0f);

                //int overallImageID = packetID * (extraDataSize / 4) + pixelID;
                //Debug.Log(overallImageID);
                //colorArray[overallImageID] = color;

                //Debug.Log(byteSample[0] + " " + byteSample[1] + " " + byteSample[2] + " " + byteSample[3]);
                texture.SetPixel(x, y, color);
                x++;
            }
        }
        
    }
}
