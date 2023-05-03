using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerminalMenu : MonoBehaviour
{
    protected virtual void Awake()
    {
        gameObject.SetActive(false);
    }

    public virtual void DisplayInfo(string key)
    { 
        // Nothing
    }
}
