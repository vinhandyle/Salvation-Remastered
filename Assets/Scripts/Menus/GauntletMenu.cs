using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GauntletMenu : WinMenu
{
    [SerializeField] private GameObject container;
    [SerializeField] private TextMeshProUGUI menuText;
    [SerializeField] private string finalLevel;

    protected override void Awake()
    {
        if (SceneController.Instance.currentLevel == finalLevel)
        {
            menuText.text = "Gauntlet Complete";
            DisplaySpecialRanks();
        }
    }

    public void SetActive(bool b)
    {
        container.SetActive(b);
    }    
}
