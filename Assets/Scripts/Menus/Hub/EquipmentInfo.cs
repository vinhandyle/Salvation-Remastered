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
        item.text = TextDatabase.Instance.txtDB["name/" + key];
        description.text = TextDatabase.Instance.txtDB["description/" + key];
    }
}
