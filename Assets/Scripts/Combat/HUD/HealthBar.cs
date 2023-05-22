using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the health bar UI.
/// </summary>
public class HealthBar : ResourceBar
{
    public float health
    {
        get => bar.value;
        set => bar.value = value;
    }

    public float maxHealth
    {
        get => bar.maxValue;
    }
}
