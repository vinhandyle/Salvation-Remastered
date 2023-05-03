using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// A slider that controls the volume from a specific audio channel.
/// </summary>
public class VolumeSlider : MonoBehaviour
{
    private Slider slider;
    private string channel;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        channel = GetComponentInChildren<TMPro.TextMeshProUGUI>().text;
    }

    private void OnEnable()
    {
        slider.value = AudioController.Instance.GetVolume(channel);
    }

    /// <summary>
    /// Called when the slider is moved.
    /// </summary>
    public void OnValueChanged()
    {
        AudioController.Instance.ChangeVolume(channel, slider.value);
    }
}