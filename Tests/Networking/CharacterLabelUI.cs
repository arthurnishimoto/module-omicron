using UnityEngine;
using System.Collections;

public class CharacterLabelUI : MonoBehaviour {

    public UnityEngine.UI.Text playerName;
    public UnityEngine.UI.Text playerRank;
    public UnityEngine.UI.Text playerTitle;

    public bool showName = true;
    public bool showRank = true;
    public bool showTitle = true;
	
	// Update is called once per frame
	void Update () {
        playerName.enabled = showName;
        playerRank.enabled = showRank;
        playerTitle.enabled = showTitle;
    }

    public void SetName(string text)
    {
        playerName.text = text;
    }

    public void SetRank(string text)
    {
        playerRank.text = text;
    }

    public void SetTitle(string text)
    {
        playerTitle.text = text;
    }
}
