using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the energy bar UI.
/// </summary>
public class EnergyBar : MonoBehaviour
{
    private Slider energyBar;
    [SerializeField] private Image fill;

    private void Awake()
    {
        energyBar = GetComponent<Slider>();
    }

    /// <summary>
    /// Initialize the energy bar.
    /// </summary>
    public void SetDefaults(float energy)
    {
        energyBar.maxValue = energy;
        energyBar.value = energy;
    }

    /// <summary>
    /// Update the energy bar.
    /// </summary>
    public void SetEnergy(float energy)
    {
        energyBar.value = energy;
    }

    public void SetRecharge(bool recharging)
    {
        if (recharging)
            fill.color = new Color(1, 0.5f, 0);
        else
            fill.color = new Color(1, 1, 0);
    }
}
