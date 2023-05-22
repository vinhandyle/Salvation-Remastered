using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : ExplodingProjectile
{
    protected BouncingObject bo;

    protected override void Awake()
    {
        base.Awake();

        bo = GetComponent<BouncingObject>();
        OnTerrainHit += (obj) => { bo.Bounce(obj); };
    }
}
