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
using UnityEngine.EventSystems;

public class OMenu : MonoBehaviour {

    public Selectable[] menuItems;
    public int currentItem = 0;

    PointerEventData pointerData;

    public bool showMenu;
    float newScale = 0;
    float currentScale;
    public float showMenuSpeed = 5;

    public OMenuManager menuManager;
    public bool activeMenu = false;
    public OMenu previousMenu;

    public float menuProgress;

    float maxScale = 1;

    // Use this for initialization
    void Start () {
        maxScale = transform.localScale.x;
        menuManager = GetComponentInParent<OMenuManager>();
        pointerData = new PointerEventData(EventSystem.current);

        if(menuItems.Length > 0)
            menuItems[currentItem].OnSelect(pointerData);

        if(!showMenu)
        {
            transform.localScale = Vector3.zero;
            menuProgress = 0;
            activeMenu = false;
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (showMenu)
        {
            newScale = maxScale;
        }
        else
        {
            newScale = 0;
        }

        if(GetComponent<UndockMenu>())
        {
            if(GetComponent<UndockMenu>().undocked)
            {
                newScale = maxScale;
                showMenu = false;

                if (activeMenu)
                {
                    transform.Translate(Vector3.right);
                    if (previousMenu)
                    {
                        previousMenu.showMenu = true;
                        menuManager.currentMenu = previousMenu;
                    }
                    activeMenu = false;
                    menuManager.openMenus--;
                    menuManager.PlayCloseMenuSound();
                }
                return;
            }
            else if(!showMenu)
            {
                newScale = 0;
            }
        }
        UpdateScale();

        if (newScale > 0)
        {
            menuProgress = currentScale / newScale;
            if(showMenu)
                activeMenu = true;
        }

        if (showMenu && activeMenu && menuProgress > 0.5f && CAVE2.IsMaster())
        {
            OnInput();
        }
    }

    void OnInput()
    {
        if (CAVE2.Input.GetButtonDown(menuManager.menuWandID, CAVE2.Button.ButtonDown))
        {
            if (currentItem < menuItems.Length - 1 && menuItems[currentItem + 1].IsActive() )
            {
                CAVE2.SendMessage(gameObject.name, "MenuNextItemDown");
            }
            else if(currentItem >= menuItems.Length - 1)
            {
                CAVE2.SendMessage(gameObject.name, "MenuSetItem", 0);
            }
        }
        if (CAVE2.Input.GetButtonDown(menuManager.menuWandID, CAVE2.Button.ButtonUp))
        {
            if (currentItem > 0 && menuItems[currentItem - 1].IsActive())
            {
                CAVE2.SendMessage(gameObject.name, "MenuNextItemUp");
            }
            else if (currentItem <= 0)
            {
                CAVE2.SendMessage(gameObject.name, "MenuSetItem", menuItems.Length - 1);
            }
        }

        if(CAVE2.Input.GetButtonDown(menuManager.menuWandID, menuManager.selectButton))
        {
            CAVE2.SendMessage(gameObject.name, "MenuSelectItem");
        }
        if (CAVE2.Input.GetButtonDown(menuManager.menuWandID, menuManager.menuBackButton))
        {
            CAVE2.SendMessage(gameObject.name, "ToggleMenu");
        }

        if (CAVE2.Input.GetButtonDown(menuManager.menuWandID, CAVE2.Button.ButtonLeft))
        {
            CAVE2.SendMessage(gameObject.name, "MenuNextItemLeft");
        }
        if (CAVE2.Input.GetButtonDown(menuManager.menuWandID, CAVE2.Button.ButtonRight))
        {
            CAVE2.SendMessage(gameObject.name, "MenuNextItemRight");
        }
    }

    void UpdateScale()
    {
        currentScale = transform.localScale.x;
        currentScale += (newScale - currentScale) * Time.deltaTime * showMenuSpeed;
        if (Mathf.Abs(currentScale - newScale) > 0.001)
        {
            transform.localScale = Vector3.one * maxScale * currentScale;
        }
        else if (showMenu)
        {
            transform.localScale = Vector3.one * maxScale;
        }
        else
        {
            transform.localScale = Vector3.zero;
            menuProgress = 0;
            activeMenu = false;
        }
    }

    public void ToggleMenu()
    {
        if (GetComponent<UndockMenu>() && GetComponent<UndockMenu>().undocked)
        {
            GetComponent<UndockMenu>().undocked = false;
        }

        showMenu = !showMenu;
        if( showMenu )
        {
            if(menuManager.mainMenu != this )
                previousMenu = menuManager.currentMenu;

            menuManager.currentMenu = this;

            activeMenu = true;

            if (previousMenu)
            {
                previousMenu.showMenu = false;
                transform.position = previousMenu.transform.position;
            }

            menuManager.openMenus++;
            menuManager.PlayOpenMenuSound();
        }
        else
        {
            if(previousMenu)
            {
                previousMenu.showMenu = true;
                menuManager.currentMenu = previousMenu;
            }
            activeMenu = false;
            menuManager.openMenus--;
            menuManager.PlayCloseMenuSound();
        }
    }

    public void MenuNextItemDown()
    {
        menuItems[currentItem].OnDeselect(pointerData);
        currentItem++;
        menuItems[currentItem].OnSelect(pointerData);
        menuManager.PlayScrollMenuSound();
    }

    public void MenuNextItemUp()
    {
        menuItems[currentItem].OnDeselect(pointerData);
        currentItem--;
        menuItems[currentItem].OnSelect(pointerData);
        menuManager.PlayScrollMenuSound();
    }

    public void MenuNextItemLeft()
    {
        if (menuItems[currentItem].GetType() == typeof(Slider))
        {
            ((Slider)menuItems[currentItem]).value = ((Slider)menuItems[currentItem]).value - 1;
        }
        menuManager.PlayScrollMenuSound();
    }

    public void MenuNextItemRight()
    {
        if (menuItems[currentItem].GetType() == typeof(Slider))
        {
            ((Slider)menuItems[currentItem]).value = ((Slider)menuItems[currentItem]).value + 1;
        }
        menuManager.PlayScrollMenuSound();
    }

    public void MenuSetItem(int id)
    {
        if (menuItems[id] != null && menuItems[id].IsActive())
        {
            menuItems[currentItem].OnDeselect(pointerData);
            currentItem = id;
            menuItems[currentItem].OnSelect(pointerData);
            menuManager.PlayScrollMenuSound();
        }
    }

    public void MenuSelectItem()
    {
        if (menuItems[currentItem].GetType() == typeof(Button))
        {
            ((Button)menuItems[currentItem]).OnPointerClick(pointerData);
        }
        else if (menuItems[currentItem].GetType() == typeof(Toggle))
        {
            ((Toggle)menuItems[currentItem]).OnPointerClick(pointerData);
        }
        menuManager.PlaySelectMenuSound();
    }
}
