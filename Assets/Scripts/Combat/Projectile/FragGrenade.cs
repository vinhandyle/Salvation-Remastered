using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragGrenade : FragProjectile
{
    protected BouncingObject bo;

    protected override void Awake()
    {
        base.Awake();

        bo = GetComponent<BouncingObject>();
        bo.OnBounceFinish += DestroyObject;
        
        OnTerrainHit -= Fragment;
        OnPlayerHit -= Fragment;

        OnTerrainHit += bo.Bounce;
        OnDestroy += Fragment;
    }
}
