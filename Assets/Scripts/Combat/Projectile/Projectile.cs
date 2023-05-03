using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the base class for all projectiles.
/// Can also be used as a standard bullet.
/// </summary>
public class Projectile : DamagingObject
{
    [Header("Sprite")]
    [SerializeField] private Sprite spriteToScaleTo;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Timers")]
    [SerializeField] protected float lifetime;
    [SerializeField] protected List<float> aiTimers;

    protected Transform origin;

    protected event Action<GameObject> OnPlayerHit;
    protected event Action<GameObject> OnEnemyHit;
    protected event Action<GameObject> OnPlayerAttackHit;
    protected event Action<GameObject> OnEnemyAttackHit;
    protected event Action<GameObject> OnTerrainHit;
    protected event Action AI;

    protected virtual void Update()
    {
        lifetime += Time.deltaTime;
        aiTimers.ForEach(timer => timer += Time.deltaTime);
        AI?.Invoke();
    }

    /// <summary>
    /// Sets the origin of the projectile instance.
    /// </summary>
    public void SetOrigin(Transform origin)
    {
        this.origin = origin;
    }

    /// <summary>
    /// Points the projectile towards the target.
    /// Use to homing projectiles.
    /// </summary>
    protected void PointToTarget(Transform target)
    {
        Vector2 direction = Vector2.zero;
        direction = (target.position - transform.position).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.eulerAngles = Vector3.forward * angle;
    }

    /// <summary>
    /// Scale the transform so that the dimensions are consistent with the base sprite.
    /// Useful for animation states with different dimensions.
    /// </summary>
    protected void AdjustScale()
    {
        Sprite sprite = spriteRenderer.sprite;

        float unitLength = spriteToScaleTo.bounds.size.y * spriteToScaleTo.pixelsPerUnit;
        float w = sprite.bounds.size.x * sprite.pixelsPerUnit / unitLength;
        float h = sprite.bounds.size.y * sprite.pixelsPerUnit / unitLength;

        spriteRenderer.size = new Vector2(w, h);
        //((BoxCollider2D)hitbox).size = new Vector2(w, h);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

        if (collision.CompareTag("Player"))
        {
            OnPlayerHit?.Invoke(collision.gameObject);
        }

        switch (collision.gameObject.layer)
        {
            case 6:
                OnTerrainHit?.Invoke(collision.gameObject);
                break;

            case 8:
                OnPlayerAttackHit?.Invoke(collision.gameObject);
                break;

            case 9:
                OnEnemyHit?.Invoke(collision.gameObject);
                break;

            case 10:
                OnEnemyAttackHit?.Invoke(collision.gameObject);
                break;
        }
    }

    protected virtual void OnTriggerStay2D(Collider2D collision)
    {
        // ?
    }
}
