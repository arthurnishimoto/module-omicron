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

public class OMenuManager : MonoBehaviour {

    public int menuWandID = 1;

    public OMenu mainMenu;
    public OMenu currentMenu;

    public int openMenus;

    public float showMenuSpeed = 5;

    public bool followWand;

    public Vector3 angleOffset;
    public Vector3 distOffset = Vector3.forward;

    // CAVE2 Omegalib-style
    public CAVE2.Button menuOpenButton = CAVE2.Button.Button2;
    public CAVE2.Button menuBackButton = CAVE2.Button.Button3;
    public CAVE2.Button selectButton = CAVE2.Button.Button2;

    [SerializeField]
    AudioClip openMenuSound;

    [SerializeField]
    AudioClip closeMenuSound;

    [SerializeField]
    AudioClip selectMenuSound;

    [SerializeField]
    AudioClip scrollMenuSound;

    AudioSource audioSource;

    // Use this for initialization
    void Start () {
        currentMenu = mainMenu;
        audioSource = GetComponent<AudioSource>();
        if(audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.volume = 0.25f;
        }
        
    }
	
	// Update is called once per frame
	void Update () {

        if (currentMenu == mainMenu && currentMenu.activeMenu == false)

        if (CAVE2.Input.GetButtonDown(menuWandID, menuOpenButton))
        {
           if(CAVE2.IsMaster())
           {
                angleOffset = new Vector3(0, CAVE2.Input.GetWandRotation(menuWandID).eulerAngles.y, 0);
                CAVE2.SendMessage(gameObject.name, "SetWandAngle", angleOffset);
                CAVE2.SendMessage(mainMenu.name, "ToggleMenu");
           }
        }

        CAVE2.Input.SetWandMenuLock(menuWandID, openMenus > 0);
    }

    public void PlayOpenMenuSound()
    {
        audioSource.clip = openMenuSound;
        audioSource.Play();
    }

    public void PlayCloseMenuSound()
    {
        audioSource.clip = closeMenuSound;
        audioSource.Play();
    }

    public void PlayScrollMenuSound()
    {
        audioSource.clip = scrollMenuSound;
        audioSource.Play();
    }

    public void PlaySelectMenuSound()
    {
        audioSource.clip = selectMenuSound;
        audioSource.Play();
    }

    public void SetWandAngle(Vector3 angleOffset)
    {
        if (followWand)
        {
            transform.localEulerAngles = angleOffset;
            transform.localPosition = Vector3.zero + Quaternion.Euler(angleOffset) * distOffset;
        }
    }
}
