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

    public int numberOfPacketsPerImage = 32;
    int totalImagePixelCount;

    public int imageWidth = 1920;
    public int imageHeight = 1080;

    public int sourceID = -1; // -1 for any

    public int receivedExtraDataSize;
    public int calculatedExtraDataSize;

    public Texture2D texture;
    public Material outputMaterial;

    int[] byteSample = new int[4];
    int x;
    int y;

    Color[] colorArray;

    // Use this for initialization
    new void Start()
    {
        eventOptions = EventBase.ServiceType.ServiceTypeGeneric;
        InitOmicron();

        if (imageType == ImageType.Color)
        {
            imageWidth = 1920;
            imageHeight = 1080;

            numberOfPacketsPerImage = 270;
        }
        else if (imageType == ImageType.Depth)
        {
            imageWidth = 512;
            imageHeight = 424;

            numberOfPacketsPerImage = 32;
        }

        texture = new Texture2D(imageWidth, imageHeight);
        outputMaterial.mainTexture = texture;

        totalImagePixelCount = imageWidth * imageHeight;
        colorArray = new Color[totalImagePixelCount];
    }

    public void Update()
    {
        texture.Apply();
    }

    public override void OnEvent(EventData e)
    {
        if((imageType == ImageType.Color && e.sourceId != 0) ||
            (imageType == ImageType.Depth && e.sourceId != 1)
            )
        {
            return;
        }

        byte[] bitmapBytes = e.extraData;
        receivedExtraDataSize = (int)bitmapBytes.Length;

        // Total pixels * RGBA channels (byte per channel)
        int bytesPerImage = totalImagePixelCount * 4;
        int bytesPerPacket = bytesPerImage / numberOfPacketsPerImage;
        calculatedExtraDataSize = bytesPerPacket;

        int packetID = (int)e.flags; // Packet number 0 - (numberOfPacketsPerImage - 1)

        int extraDataSize = calculatedExtraDataSize;

        // Calculate the pixel (x,y) position from segment of a
        // linear array representing all the pixels
        int pixelsPerPacket = extraDataSize / 4;
        int linesPerPacket = pixelsPerPacket / imageWidth;

        // Determine if the packet of pixels ends in the middle
        // of a horizontal line and determine the offset/shift
        // that needs to be applied
        int packetShift = 0;
        int lineOffset = pixelsPerPacket % imageWidth;
        if (lineOffset != 0)
            packetShift = packetID % (imageWidth / lineOffset);

        x = packetShift * lineOffset;
        y = packetID * linesPerPacket;

        // Read through all the bytes in the array
        // Pulling out every 4 bytes (each RGBA element)
        for (int i = 0; i < extraDataSize; i += 4)
        {
            Array.Copy(bitmapBytes, i, byteSample, 0, 4);

            // Format from Kinect is BGRA (not RGBA)
            Color color = new Color(byteSample[2] / 255.0f, byteSample[1] / 255.0f, byteSample[0] / 255.0f);

            // If pixel array reaches end of image width, next line
            if (x >= imageWidth)
            {
                x = 0;
                y++;
            }

            texture.SetPixel(x, y, color);
            x++;
        }

    }
}
