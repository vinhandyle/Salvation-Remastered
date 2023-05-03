using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the health bar UI.
/// </summary>
public class HealthBar : MonoBehaviour
{
    private Slider healthBar;
    public float health
    {
        get => healthBar.value;
        set => healthBar.value = value;
    }

    public float maxHealth
    {
        get => healthBar.maxValue;
    }

    private void Awake()
    {
        healthBar = GetComponent<Slider>();
    }

    /// <summary>
    /// Initialize the health bar.
    /// </summary>
    public void SetDefaults(float health)
    {
        healthBar.maxValue = health;
        healthBar.value = health;
    }
}
