using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraToggle : MenuButton
{
    [SerializeField] private Image checkmark;

    protected override void Awake()
    {
        base.Awake();
        checkmark.enabled = PlayerData.Instance.fullCam;
    }

    /// <summary>
    /// Switch between a small, moving camera and a level-wide static camera.
    /// </summary>
    public void ToggleFullCam()
    {
        checkmark.enabled = PlayerData.Instance.fullCam = !PlayerData.Instance.fullCam;
        FindObjectOfType<CameraManager>().SetCamera();
    }
}
