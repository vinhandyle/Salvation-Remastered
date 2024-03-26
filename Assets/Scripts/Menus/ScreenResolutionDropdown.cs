using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ScreenResolutionDropdown : MonoBehaviour
{
    TMP_Dropdown dd;

    private void Awake()
    {
        dd = GetComponent<TMP_Dropdown>();
        dd.value = MatchScreenResolution();
    }

    private int MatchScreenResolution()
    {
        string wxh = string.Format("{0}x{1}", Screen.width, Screen.height);
        for (int i = 0; i < dd.options.Count; ++i)
        {
            if (dd.options[i].text == wxh)
                return i;
        }

        return -1;
    }

    public void SetScreenResolution()
    {
        string res = dd.options[dd.value].text;
        int[] wxh = res.Split("x").Select(s => int.Parse(s)).ToArray();
        Screen.SetResolution(wxh[0], wxh[1], Screen.fullScreen);
    }
}
