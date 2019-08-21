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
using UnityEngine.UI;

public class NavSpeedUI : MonoBehaviour {

    public Text label;
    public Slider slider;

    public CAVE2WandNavigator navController;
    public AdaptiveWandNavigationSpeed navControllerAdv;

    int adaptiveType = 0;

    // Use this for initialization
    void Start () {
        navController = GetComponentInParent<CAVE2WandNavigator>();
        navControllerAdv = GetComponentInParent<AdaptiveWandNavigationSpeed>();
        slider.value = SpeedToSliderPosition(navController.globalSpeedMod);

        if (navControllerAdv && navControllerAdv.UsingAdaptiveSpeed())
        {
            switch (adaptiveType)
            {
                case (0): label.text = "Nav Speed: Adaptive Orbit"; slider.value = 3; break;
                case (1): label.text = "Nav Speed: Adaptive Landing"; break;
            }
        }
    }

    public void UpdateNavSpeed()
    {
        float sliderVal = slider.value;
        // value: 0 - 10
        // 0 = 0.00001; 10 = 100000
        // sliderVal = Mathf.Pow(10,(sliderVal - 5)); // Omegalib scale

        if (navControllerAdv && navController)
        {
            // Advanced scale for adaptive speed
            switch ((int)sliderVal)
            {
                case (0): sliderVal = 0.01f;
                    navControllerAdv.UseAdaptiveSpeed(false);
                    break;
                case (1): sliderVal = 0.1f;
                    navControllerAdv.UseAdaptiveSpeed(false);
                    break;
                case (2): sliderVal = 0.01f;
                    navControllerAdv.UseAdaptiveSpeed(true);
                    adaptiveType = 1;
                    break;
                case (3): sliderVal = 0.1f;
                    navControllerAdv.UseAdaptiveSpeed(true); // Mars-demo planet speed
                    adaptiveType = 0;
                    break;
                case (4): sliderVal = 1.0f; navControllerAdv.UseAdaptiveSpeed(false, 1.0f); break;
                case (5): sliderVal = 10.0f; navControllerAdv.UseAdaptiveSpeed(false); break;
                case (6): sliderVal = 50.0f; navControllerAdv.UseAdaptiveSpeed(false); break;
                case (7): sliderVal = 100.0f; navControllerAdv.UseAdaptiveSpeed(false); break;
                case (8): sliderVal = 1000.0f; navControllerAdv.UseAdaptiveSpeed(false); break;
                case (9): sliderVal = 10000.0f; navControllerAdv.UseAdaptiveSpeed(false); break;
                case (10): sliderVal = 100000.0f; navControllerAdv.UseAdaptiveSpeed(false); break;
            }
            if (navControllerAdv.UsingAdaptiveSpeed())
            {
                switch (adaptiveType)
                {
                    case (0): label.text = "Nav Speed: Adaptive Orbit"; break;
                    case (1): label.text = "Nav Speed: Adaptive Landing"; break;
                }
            }
            else
            {
                label.text = "Navigation Speed: " + sliderVal + "x";
                navController.globalSpeedMod = sliderVal;
            }            
        }
        else
        {
            // New Scale
            sliderVal = SliderPositionToSpeed(sliderVal);
            if (navController)
            {
                label.text = "Navigation Speed: " + sliderVal + "x";
                navController.globalSpeedMod = sliderVal;
            }
        }
    }

    int SpeedToSliderPosition(float speed)
    {
        if (speed == 0.0001f) return 0;
        else if (speed == 0.001f) return 1;
        else if (speed == 0.01f) return 2;
        else if (speed == 0.1f) return 3;
        else if (speed == 1.0f) return 4;
        else if (speed == 10.0f) return 5;
        else if (speed == 50.0f) return 6;
        else if (speed == 100.0f) return 7;
        else if (speed == 1000.0f) return 8;
        else if (speed == 10000.0f) return 9;
        else if (speed == 100000.0f) return 10;
        else
            return 4;
    }

    float SliderPositionToSpeed(float sliderVal)
    {
        switch ((int)sliderVal)
        {
            case (0): sliderVal = 0.0001f; break;
            case (1): sliderVal = 0.001f; break;
            case (2): sliderVal = 0.01f; break;
            case (3): sliderVal = 0.1f; break;
            case (4): sliderVal = 1.0f; break;
            case (5): sliderVal = 10.0f; break;
            case (6): sliderVal = 50.0f; break;
            case (7): sliderVal = 100.0f; break;
            case (8): sliderVal = 1000.0f; break;
            case (9): sliderVal = 10000.0f; break;
            case (10): sliderVal = 100000.0f; break;
        }
        return sliderVal;
    }
}
