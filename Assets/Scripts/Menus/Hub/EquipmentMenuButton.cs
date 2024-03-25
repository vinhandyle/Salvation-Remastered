using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EquipmentMenuButton : MonoBehaviour
{
    [SerializeField] private GameObject backPanel;
    [SerializeField] private string key;

    private void Awake()
    {
        SetDefaultToggles();
    }

    private void OnEnable()
    {
        if (key.Substring(0, 3) == "Gun")
        {
            int i = int.Parse(key.Substring("Gun".Length));
            gameObject.SetActive(PlayerData.Instance.unlocked[i] || PlayerData.Instance.unlockAll);
        }
    }

    private void SetDefaultToggles()
    {
        if (key == "Dash")
        {
            backPanel.SetActive(PlayerData.Instance.canDash);
        }
        else if (key == "Wall Jump")
        {
            backPanel.SetActive(PlayerData.Instance.canWallJump);
        }
        else if (key == "Double Jump")
        {
            backPanel.SetActive(PlayerData.Instance.canDoubleJump);
        }
        else if (key.Substring(0, 3) == "Dmg")
        {
            backPanel.SetActive(int.Parse(key.Substring(4)) == PlayerData.Instance.dmgMult);
        }
        else if (key.Substring(0, 3) == "Gun")
        {
            backPanel.SetActive(PlayerData.Instance.equipped[int.Parse(key.Substring(3))]);
        }
        else if (key.Substring(0, 6) == "Expert")
        {
            backPanel.SetActive(bool.Parse(key.Substring(7)) == PlayerData.Instance.expertMode);
        }
    }

    private void ToggleSelected()
    {
        backPanel.SetActive(!backPanel.activeSelf);
    }

    public void SelectMode(EquipmentMenuButton other)
    {
        bool prevMode = PlayerData.Instance.expertMode;
        PlayerData.Instance.expertMode = bool.Parse(key.Substring("Expert:".Length));

        // Only change back panels if switching to new mode
        if (prevMode != PlayerData.Instance.expertMode)
        {
            ToggleSelected();
            other.ToggleSelected();
        }
    }

    public void SelectDifficulty()
    {
        int prevDmgMult = PlayerData.Instance.dmgMult;
        PlayerData.Instance.dmgMult = int.Parse(key.Substring("Dmg:".Length));

        // Only change back panels if switching to new difficulty
        if (prevDmgMult != PlayerData.Instance.dmgMult)
        {
            ToggleSelected();
            foreach (var other in 
                FindObjectsOfType(typeof(EquipmentMenuButton)) as EquipmentMenuButton[])
            {
                if (other.key.Substring(0, 4) == key.Substring(0, 4) 
                    && other.backPanel.activeSelf && other.key != key)
                    other.ToggleSelected();
            }
        }
    }

    public void SelectMovement()
    {
        switch (key)
        {
            case "Dash":
                PlayerData.Instance.canDash = !PlayerData.Instance.canDash;
                break;

            case "Wall Jump":
                PlayerData.Instance.canWallJump = !PlayerData.Instance.canWallJump;
                break;

            case "Double Jump":
                PlayerData.Instance.canDoubleJump = !PlayerData.Instance.canDoubleJump;
                break;
        }
        ToggleSelected();
    }

    public void SelectWeapon()
    {
        int i = int.Parse(key.Substring("Gun".Length));
        PlayerData.Instance.equipped[i] = !PlayerData.Instance.equipped[i];       
        ToggleSelected();

        // Player has at least 1 weapon equipped
        if (PlayerData.Instance.equipped.All(x => !x))
        {
            PlayerData.Instance.equipped[i] = !PlayerData.Instance.equipped[i];
            ToggleSelected();
        }
    }

    public void OpenMenu(TerminalMenu menu)
    {
        menu.gameObject.SetActive(true);
        menu.DisplayInfo(key);
    }

    public void CloseMenu(TerminalMenu menu)
    {
        menu.gameObject.SetActive(false);
    }
}
