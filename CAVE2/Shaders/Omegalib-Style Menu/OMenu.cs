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
        UpdateScale();

        if (newScale > 0)
        {
            menuProgress = currentScale / newScale;
            if(showMenu)
                activeMenu = true;
        }

        if (showMenu && activeMenu && menuProgress > 0.5f)
            OnInput();
    }

    void OnInput()
    {
        if (CAVE2.Input.GetButtonDown(1, CAVE2.Button.ButtonDown))
        {
            if (currentItem < menuItems.Length - 1 && menuItems[currentItem+1].IsActive() )
            {
                menuItems[currentItem].OnDeselect(pointerData);
                currentItem++;
                menuItems[currentItem].OnSelect(pointerData);
            }
        }
        if (CAVE2.Input.GetButtonDown(1, CAVE2.Button.ButtonUp))
        {
            if (currentItem > 0 && menuItems[currentItem - 1].IsActive())
            {
                menuItems[currentItem].OnDeselect(pointerData);
                currentItem--;
                menuItems[currentItem].OnSelect(pointerData);
            }
        }

        if(CAVE2.Input.GetButtonDown(1, menuManager.selectButton))
        {
            if (menuItems[currentItem].GetType() == typeof(Button))
            {
                ((Button)menuItems[currentItem]).OnPointerClick(pointerData);
            }
            else if (menuItems[currentItem].GetType() == typeof(Toggle))
            {
                ((Toggle)menuItems[currentItem]).OnPointerClick(pointerData);
            }
        }
        if (CAVE2.Input.GetButtonDown(1, menuManager.menuBackButton))
        {
            ToggleMenu();
        }

        if (CAVE2.Input.GetButtonDown(1, CAVE2.Button.ButtonLeft))
        {
            if (menuItems[currentItem].GetType() == typeof(Slider))
            {
                ((Slider)menuItems[currentItem]).value = ((Slider)menuItems[currentItem]).value - 1;
            }
        }
        if (CAVE2.Input.GetButtonDown(1, CAVE2.Button.ButtonRight))
        {
            if (menuItems[currentItem].GetType() == typeof(Slider))
            {
                ((Slider)menuItems[currentItem]).value = ((Slider)menuItems[currentItem]).value + 1;
            }
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
        }
    }
}
