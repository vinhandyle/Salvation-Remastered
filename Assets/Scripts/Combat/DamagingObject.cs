using LayerManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Allows an object to deal damage on contact.
/// </summary>
public class DamagingObject : MonoBehaviour
{
    [SerializeField] protected Animator anim;
    [SerializeField] protected Collider2D cldr;
    protected Rigidbody2D rb;

    protected HealthManager health;
    protected event Action OnDestroy;

    [SerializeField] protected int damage;
    [HideInInspector] public float dmgMult = 1;

    [SerializeField] protected bool destroyOnHit;
    [SerializeField] protected bool destroyOnCrossFire;
    protected bool destroyed;

    [SerializeField] protected bool canKnockback;
    [SerializeField] protected float knockbackAmt;
    [SerializeField] protected List<string> targetTags;

    protected virtual void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();
        cldr = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (cldr.isTrigger) OnContact(collision.gameObject);
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (!cldr.isTrigger) OnContact(collision.gameObject);
    }

    /// <summary>
    /// Generic invoke for trigger or collision.
    /// </summary>
    private void OnContact(GameObject other)
    {
        if (ValidTarget(other))
        {
            health = other.GetComponent<HealthManager>();
            if (health != null) health.TakeDamage(Mathf.RoundToInt(damage * dmgMult));

            // Knockback player
            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();

                if (canKnockback && !health.immune)
                {
                    Vector2 kb = 100 * knockbackAmt * (other.transform.position - transform.position);
                    player.KnockbackAsync(kb);
                }
            }

            // Destroy object if appropriate
            if (!destroyed && (destroyOnHit || destroyOnCrossFire))
                DestroyObject();
        }
    }

    protected bool ValidTarget(GameObject obj)
    {
        bool crossFire = new Layer[] 
        { 
            Layer.Terrain, 
            Layer.Player, 
            Layer.PlayerAttack, 
            Layer.Enemy, 
            Layer.EnemyAttack
        }
        .Contains((Layer)obj.layer);

        if (targetTags.Count == 0)
        {
            if (!destroyOnCrossFire)
                return 
                    new Layer[] 
                    { 
                        Layer.Terrain,
                        Layer.Player, 
                        Layer.Enemy
                    }
                    .Contains((Layer)obj.layer);
            else
                return crossFire;
        }
        else
            return targetTags.Contains(obj.tag) || (destroyOnCrossFire && crossFire);
    }

    protected void DestroyObject()
    {
        destroyed = true; // Prevent simultaneous collision from causing multiple destroys

        cldr.enabled = false;
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;

        // Play collision animation
        if (anim == null)
        {
            OnDestroy?.Invoke();
            Destroy(gameObject);
        }
        else
            anim.SetTrigger("OnDestroy");
    }

    /// <summary>
    /// Disable the object's collider when the sprite is empty during the animation.
    /// </summary>
    protected void DisableHitboxPostAnim()
    {
        cldr.enabled = false;
    }

    /// <summary>
    /// Call to destroy object after collision animation finishes.
    /// </summary>
    protected void DestroyObjectPostAnim()
    {        
        OnDestroy?.Invoke();
        Destroy(gameObject);
    }
}
