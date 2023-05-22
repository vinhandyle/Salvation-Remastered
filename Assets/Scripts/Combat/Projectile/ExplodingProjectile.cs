using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodingProjectile : Projectile
{
    [SerializeField] protected GameObject explosionObject;

    protected override void Awake()
    {
        base.Awake();
        OnDestroy += Explode;
    }

    protected virtual void Explode()
    {
        Instantiate(explosionObject, transform.position, Quaternion.Euler(0,0,0));
    }
}
