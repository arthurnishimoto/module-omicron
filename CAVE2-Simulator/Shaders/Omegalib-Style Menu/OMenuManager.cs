using UnityEngine;
using System.Collections;

public class OMenuManager : MonoBehaviour {

    public OMenu mainMenu;
    public OMenu currentMenu;

    public int openMenus;

    float newScale = 0;
    public float showMenuSpeed = 5;

    public bool followWand;

    public Vector3 angleOffset;
    public Vector3 distOffset = Vector3.forward;

    // Use this for initialization
    void Start () {
        currentMenu = mainMenu;
    }
	
	// Update is called once per frame
	void Update () {

        if (currentMenu == mainMenu && currentMenu.activeMenu == false)
        {
            if (CAVE2.Input.GetButtonDown(1, CAVE2.Button.Button2))
            {
                mainMenu.ToggleMenu();

                if (followWand)
                {
                    angleOffset = new Vector3(0, CAVE2.Input.GetWandRotation(1).eulerAngles.y, 0);
                    transform.localEulerAngles = angleOffset;
                    transform.localPosition = Vector3.zero + Quaternion.Euler(angleOffset) * distOffset;

                }
            }
        }

        CAVE2.Input.SetWandMenuLock(1, openMenus > 0);
    }
}
