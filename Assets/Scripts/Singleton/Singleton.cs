using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An object that is not destroyed when changing scenes.
/// </summary>
public class Singleton<Object> : MonoBehaviour where Object : Singleton<Object>
{
    public static Object Instance { get;  private set; }

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = (Object)this;

            DontDestroyOnLoad(Instance);
        }
        else
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}
