using LayerManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private Vector2 refDim;

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
        SaveRefDimensions();
    }

    protected virtual void Update()
    {
        if (homing)
        {
            PointToTarget(ClosestTarget());
            rb.velocity = rb.velocity.magnitude * transform.right;
        }

        lifetime -= Time.deltaTime;
        if (mortal && lifetime <= 0) DestroyObject();
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
        GameObject target = null;
        float distance = float.MaxValue;

        foreach (string tag in targetTags)
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag(tag))
            {
                float distanceFromObject = ((Vector2)obj.transform.position - (Vector2)transform.position).sqrMagnitude;
                
                if (distanceFromObject < distance)
                {
                    distance = distanceFromObject;
                    target = obj;
                }
            }
        }

        return target.transform;
    }

    /// <summary>
    /// Points the projectile towards the target.
    /// Use to homing projectiles.
    /// </summary>
    protected virtual void PointToTarget(Transform target)
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
            box.size = refDim * new Vector2(w, h);
        }
        catch
        {
            try
            {
                CircleCollider2D circle = (CircleCollider2D)cldr;
                circle.radius = refDim.x * w;
            }
            catch
            {
                Debug.Log("Invalid collider type.");
            }
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

    private void SaveRefDimensions()
    {
        try
        {
            BoxCollider2D box = (BoxCollider2D)cldr;
            refDim = box.size;
        }
        catch
        {
            try
            {
                CircleCollider2D circle = (CircleCollider2D)cldr;
                refDim = new Vector2(circle.radius, circle.radius);
            }
            catch
            {
                Debug.Log("Invalid collider type.");
            }
        }
    }

    private void OnContact(GameObject obj)
    {
        if (ValidTarget(obj))
        {
            if (obj.CompareTag("Player"))
            {
                OnPlayerHit?.Invoke(obj);
            }

            switch ((Layer)obj.layer)
            {
                case Layer.Terrain:
                    // Center box is only used to catch small, fast projectiles
                    // Do this to prevent duplicate effect activations
                    if (obj.name != "Center")
                        OnTerrainHit?.Invoke(obj);
                    break;

                case Layer.PlayerAttack:
                    OnPlayerAttackHit?.Invoke(obj);
                    break;

                case Layer.Enemy:
                    OnEnemyHit?.Invoke(obj);
                    break;

                case Layer.EnemyAttack:
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