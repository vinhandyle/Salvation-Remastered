using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera vcamFull;
    [SerializeField] private CinemachineVirtualCamera vcamFollow;

    private void Awake()
    {
        if (PlayerData.Instance.fullCam != vcamFull.Priority > vcamFollow.Priority)
            SetCamera();
    }

    public void SetCamera()
    {
        if (PlayerData.Instance.fullCam)
        {
            vcamFull.Priority += 2;
            vcamFollow.Priority -= 2;
        }
        else
        {
            vcamFull.Priority -= 2;
            vcamFollow.Priority += 2;
        }   
    }
}
