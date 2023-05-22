using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the energy bar UI.
/// </summary>
public class EnergyBar : ResourceBar
{
    [SerializeField] private Image fill;

    public void SetRecharge(bool recharging)
    {
        if (recharging)
            fill.color = new Color(1, 0.5f, 0);
        else
            fill.color = new Color(1, 1, 0);
    }
}
