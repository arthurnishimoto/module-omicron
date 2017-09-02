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
        }
    }
	
	// Update is called once per frame
	void Update () {

        if (currentMenu == mainMenu && currentMenu.activeMenu == false)
        {
            if (CAVE2.Input.GetButtonDown(1, menuOpenButton))
            {
                CAVE2.BroadcastMessage(mainMenu.name, "ToggleMenu");
                //mainMenu.ToggleMenu();

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
}
