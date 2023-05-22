using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI display;
    public float timer { get; private set; }

    private void Update()
    {
        timer += Time.deltaTime;
        display.text = ConvertTime(timer);
    }

    public static string ConvertTime(float timer)
    {
        int minutes = (int)timer / 60;
        float seconds = (int)(timer - minutes * 60);
        float remainder = timer - minutes * 60 - seconds;

        return string.Format("{0}:{1}{2}", 
            minutes < 10 ? "0" + minutes : minutes, 
            seconds < 10 ? "0" + seconds : seconds, 
            remainder.ToString("f3").Substring(1));
    }
}
