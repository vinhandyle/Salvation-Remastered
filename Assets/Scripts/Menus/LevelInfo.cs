using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelInfo : TerminalMenu
{
    [SerializeField] private TextMeshProUGUI level;
    [SerializeField] private TextMeshProUGUI bestTime;
    [SerializeField] private TextMeshProUGUI challenge;

    public override void DisplayInfo(string key)
    {
        SetLevelName(key);
        SetBestTime(key);
        SetChallenge(key);
    }

    private void SetLevelName(string key)
    {
        level.text = string.Format("INFO - {0}", key.Substring(key.Length - 2));
    }

    private void SetBestTime(string key)
    {
        bestTime.text = "<color=yellow>BEST TIME:</color> ";

        // Standard Mode
        if (PlayerData.Instance.bestTimes.ContainsKey(key))
            bestTime.text += string.Format("<color=grey>{0}</color>", LevelTimer.ConvertTime(PlayerData.Instance.bestTimes[key]));
        else
            bestTime.text += "<color=grey>N/A</color>";

        // Spacing
        bestTime.text += "\n";
        for (int i = 0; i < "BEST TIME:   ".Length; i++)
            bestTime.text += " ";

        // Expert Mode
        if (PlayerData.Instance.bestTimes.ContainsKey(key + "E"))
            bestTime.text += string.Format("<color=red>{0}</color>", LevelTimer.ConvertTime(PlayerData.Instance.bestTimes[key + "E"]));
        else
            bestTime.text += "<color=red>N/A</color>";
    }

    private void SetChallenge(string key)
    {
        challenge.text = "<color=red>CHALLENGE:</color>\n";

        switch (key)
        {
            case "Boss01":
                challenge.text += "REGULAR GUN ONLY\nNO DASH\nNO WALL CLIMB\nNO DOUBLE JUMP";
                break;

            case "Boss02":
                challenge.text += "REGULAR GUN ONLY\nNO WALL CLIMB\nNO DOUBLE JUMP";
                break;

            case "Boss03":
                challenge.text += "DON'T TOUCH WATER";
                break;

            case "Boss04":
                challenge.text += "REGULAR GUN ONLY";
                break;

            case "Boss05":
                challenge.text += "NO WALL CLIMB\nNO DOUBLE JUMP";
                break;

            case "Boss08":
                challenge.text += "DELIVER THE FINISHING BLOW WITH THE RAIL GUN";
                break;

            default:
                challenge.text += "TBD";
                break;
        }
    }
}
