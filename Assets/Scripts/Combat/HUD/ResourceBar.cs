using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceBar : MonoBehaviour
{
    protected Slider bar;

    private void Awake()
    {
        bar = GetComponent<Slider>();
    }

    public void SetDefaults(float value)
    {
        bar.maxValue = value;
        bar.value = value;
    }

    public void SetValue(float value)
    {
        bar.value = value;
    }
}
