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

    [Header("Other")]
    [SerializeField] protected bool homing;
    [SerializeField] protected bool mortal;
    [SerializeField] protected float lifetime;
    
    protected Transform origin;
    protected event Action OnInit;

    protected event Action<GameObject> OnPlayerHit;
    protected event Action<GameObject> OnEnemyHit;
    protected event Action<GameObject> OnPlayerAttackHit;
    protected event Action<GameObject> OnEnemyAttackHit;
    protected event Action<GameObject> OnTerrainHit;

    protected event Action OnBounce;

    protected override void Awake()
    {
        base.Awake();
    }

    protected virtual void Update()
    {
        if (homing) PointToTarget(ClosestTarget());

        lifetime -= Time.deltaTime;
        if (mortal && lifetime <= 0) DestroyObject();

        if (spriteToScaleTo) AdjustScale();
    }

    #region AI Helper

    /// <summary>
    /// Sets the origin of the projectile instance.
    /// </summary>
    public void SetOrigin(Transform origin)
    {
        this.origin = origin;
    }

    protected Transform ClosestTarget()
    {
        return null;
    }

    /// <summary>
    /// Points the projectile towards the target.
    /// Use to homing projectiles.
    /// </summary>
    protected void PointToTarget(Transform target)
    {
        Vector2 direction = (target.position - transform.position).normalized;
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

        try
        {
            BoxCollider2D box = (BoxCollider2D)cldr;
            box.size = new Vector2(w, h);
        }
        catch
        {
            try
            {
                CircleCollider2D circle = (CircleCollider2D)cldr;
                circle.radius = w / 2;
            }
            catch
            { }
        }               
    }

    #endregion

    public void SetDefaults(Transform origin, float angle, float projSpeed)
    {
        SetOrigin(origin);
        transform.localEulerAngles = new Vector3(0, 0, transform.localEulerAngles.z + angle);       
        rb.velocity = transform.right * projSpeed;

        OnInit?.Invoke();
    }

    private void OnContact(GameObject obj)
    {
        if (ValidTarget(obj))
        {
            if (obj.CompareTag("Player"))
            {
                OnPlayerHit?.Invoke(obj);
            }

            switch (obj.layer)
            {
                case 6:
                    // Center box is only used to catch small, fast projectiles
                    // Do this to prevent duplicate effect activations
                    if (obj.name != "Center")
                        OnTerrainHit?.Invoke(obj);
                    break;

                case 8:
                    OnPlayerAttackHit?.Invoke(obj);
                    break;

                case 9:
                    OnEnemyHit?.Invoke(obj);
                    break;

                case 10:
                    OnEnemyAttackHit?.Invoke(obj);
                    break;
            }
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (cldr.isTrigger) OnContact(collision.gameObject);
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
        if (!cldr.isTrigger) OnContact(collision.gameObject);        
    }
}