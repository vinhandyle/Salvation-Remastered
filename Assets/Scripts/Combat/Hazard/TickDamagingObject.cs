using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TickDamagingObject : DamagingObject
{
    [Header("Tick Damager")]
    [SerializeField] private float tickSpeed;
    private float tickTimer;
    protected bool ticking;

    protected virtual void Update()
    {
        if (ticking)
        {
            tickTimer += Time.deltaTime;
            if (tickTimer >= tickSpeed)
            {
                if (health != null) health.TakeDamage(Mathf.RoundToInt(damage * dmgMult));
                tickTimer = 0;
            }
        } 
        else
        {
            tickTimer = 0;
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (cldr.isTrigger && ValidTarget(collision.gameObject)) ticking = true;

        base.OnTriggerEnter2D(collision);
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (cldr.isTrigger && ValidTarget(collision.gameObject)) ticking = false;
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (!cldr.isTrigger && ValidTarget(collision.gameObject)) ticking = true;

        base.OnCollisionEnter2D(collision);
    }

    protected virtual void OnCollisionExit2D(Collision2D collision)
    {
        if (!cldr.isTrigger && ValidTarget(collision.gameObject)) ticking = false;
    }
}
