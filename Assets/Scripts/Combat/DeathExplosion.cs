using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathExplosion : MonoBehaviour
{
    [SerializeField] private HealthManager hm;

    public void DestroyObject()
    {
        hm.DestroyObject();
    }
}
