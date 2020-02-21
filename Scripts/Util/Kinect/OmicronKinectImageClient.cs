using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using omicron;
using omicronConnector;
using System;

public class OmicronKinectImageClient : OmicronEventClient
{
    public enum ImageType { Color, Depth };
    public ImageType imageType = ImageType.Color;

    public int numberOfPacketsPerImage = 32;
    int totalImagePixelCount;

    public int imageWidth = 1920;
    public int imageHeight = 1080;

    public int sourceID = -1; // -1 for any

    public int receivedExtraDataSize;
    public int calculatedExtraDataSize;

    Texture2D texture;
    public Material outputMaterial;

    int[] byteSample = new int[4];
    int x;
    int y;

    // Color[] colorArray;

    public bool forceCompleteFrames = false;
    float lastZeroFrameTime;


    struct FrameObject
    {
        public Texture2D texture;
        public int[] packetsReceived;
        public int packetsReady;
    }

    int frameBufferSize = 5;
    FrameObject[] frames;
    Hashtable frameIndexLookup = new Hashtable();
    int nextFrameIndex;
    FrameObject currentFrameObject;

    [SerializeField]
    int[] readyFrames;

    float completeFrameTime;

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

        frames = new FrameObject[frameBufferSize];
        for (int i = 0; i < frameBufferSize; i++)
        {
            frames[i].texture = new Texture2D(imageWidth, imageHeight);
            frames[i].packetsReceived = new int[numberOfPacketsPerImage];
            frames[i].packetsReady = 0;
        }


        outputMaterial.mainTexture = frames[0].texture;

        totalImagePixelCount = imageWidth * imageHeight;
        // colorArray = new Color[totalImagePixelCount];
        readyFrames = new int[frameBufferSize];
    }

    int drawFrame;
    public void Update()
    {
        if (forceCompleteFrames)
        {
            for (int i = 0; i < frames.Length; i++)
            {
                readyFrames[i] = frames[i].packetsReady;
                if (readyFrames[i] == numberOfPacketsPerImage)
                {
                    frames[i].texture.Apply();
                    outputMaterial.mainTexture = frames[i].texture;
                    Debug.Log("Frame complete: " + (Time.time - completeFrameTime));
                    completeFrameTime = Time.time;
                    frames[i].packetsReady = 0;
                }
            }
        }
        else
        {
            texture.Apply();
            outputMaterial.mainTexture = texture;
        }
    }

    public override void OnEvent(EventData e)
    {
        if ((imageType == ImageType.Color && (int)e.posz != 0) ||
            (imageType == ImageType.Depth && (int)e.posz != -1)
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

        int frameID = (int)e.sourceId;

        if (forceCompleteFrames)
        {
            if (frameIndexLookup.ContainsKey(frameID))
            {
                int currentFrameIndex = (int)frameIndexLookup[frameID];
                currentFrameObject = frames[currentFrameIndex];

                currentFrameObject.packetsReceived[packetID] = 1;
                currentFrameObject.packetsReady = currentFrameObject.packetsReady + 1;

                frames[currentFrameIndex] = currentFrameObject;

                // Debug.Log("Updated timestamp " + frameID + " index " + currentFrameIndex + " " + packetID + "/" + numberOfPacketsPerImage);
            }
            else
            {
                // Debug.Log("New timestamp " + frameID + " added to frame index " + nextFrameIndex);

                frameIndexLookup.Add(frameID, nextFrameIndex);
                currentFrameObject = frames[nextFrameIndex];
                currentFrameObject.texture = new Texture2D(imageWidth, imageHeight);
                currentFrameObject.packetsReceived = new int[numberOfPacketsPerImage];
                currentFrameObject.packetsReady = 1;
                frames[nextFrameIndex] = currentFrameObject;

                if (nextFrameIndex < frameBufferSize - 1)
                    nextFrameIndex++;
                else
                    nextFrameIndex = 0;
            }
        }

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

            if (forceCompleteFrames)
                currentFrameObject.texture.SetPixel(x, y, color);
            else
                texture.SetPixel(x, y, color);
            x++;
        }
    }
}
