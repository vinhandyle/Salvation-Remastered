using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : Projectile
{
    private void Awake()
    {
        AI += () => { };
    }
}
