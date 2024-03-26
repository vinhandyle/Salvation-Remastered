using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FullscreenToggle : MenuButton
{
    [SerializeField] private Image checkmark;

    protected override void Awake()
    {
        base.Awake();
        checkmark.enabled = Screen.fullScreen;
    }

    public void ToggleFullScreen()
    {
        checkmark.enabled = Screen.fullScreen = !Screen.fullScreen;
    }
}
