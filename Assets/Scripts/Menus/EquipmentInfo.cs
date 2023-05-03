using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EquipmentInfo : TerminalMenu
{
    [SerializeField] private TextMeshProUGUI item;
    [SerializeField] private TextMeshProUGUI description;

    public override void DisplayInfo(string key)
    {
        SetItemName(key);
        SetItemDescription(key);
    }

    private void SetItemName(string key)
    {
        switch (key)
        {
            case "Gun0":
                item.text = "<color=yellow>Handgun</color>";
                break;

            case "Gun1":
                item.text = "<color=yellow>Shotgun</color>";
                break;

            case "Gun2":
                item.text = "<color=yellow>Minigun</color>";
                break;

            case "Gun3":
                item.text = "<color=yellow>Railgun</color>";
                break;

            case "Gun4":
                item.text = "<color=yellow>Grenade Launcher</color>";
                break;

            case "Gun5":
                item.text = "<color=yellow>Rocket Launcher</color>";
                break;

            case "Dash":
                item.text = "<color=yellow>Booster Rocket</color>";
                break;

            case "Wall Jump":
                item.text = "<color=yellow>Climbing Claws</color>";
                break;

            case "Double Jump":
                item.text = "<color=yellow>Booster Rocket V2</color>";
                break;

            case "Expert:false":
                item.text = "<color=grey>Standard Badge</color>";
                break;

            case "Expert:true":
                item.text = "<color=red>Expert Badge</color>";
                break;

            case "Dmg:0":
                item.text = "<color=#00FFFF>Practice Reactor</color>";
                break;

            case "Dmg:1":
                item.text = "<color=green>Basic Reactor</color>";
                break;

            case "Dmg:1000":
                item.text = "<color=#EFA700>Unstable Reactor</color>";
                break;

            default:
                break;
        }
    }

    private void SetItemDescription(string key)
    {
        switch (key)
        {
            case "Gun0":
                description.text = "A gun.";
                break;

            case "Gun1":
                description.text = "A gun with spread shot.";
                break;

            case "Gun2":
                description.text = "A gun with rapid fire.";
                break;

            case "Gun3":
                description.text = "A gun with insane pierce.";
                break;

            case "Gun4":
                description.text = "Lots of explosions.";
                break;

            case "Gun5":
                description.text = "Massive explosions.";
                break;

            case "Dash":
                description.text = "Grants the ability to dash along the ground or through the air.";
                break;

            case "Wall Jump":
                description.text = "Grants the ability to cling to and leap off walls.";
                break;

            case "Double Jump":
                description.text = "Grants the ability to jump again while mid-air.";
                break;

            case "Expert:false":
                description.text = "Wear to access the standard combat simulations.";
                break;

            case "Expert:true":
                description.text = "Wear to access the enhanced combat simulations.";
                break;

            case "Dmg:0":
                description.text = "You take no damage. Timers and challenges are disabled.";
                break;

            case "Dmg:1":
                description.text = "Equip for the intended experience.";
                break;

            case "Dmg:1000":
                description.text = "You will die in one hit.";
                break;

            default:
                break;
        }
    }
}
