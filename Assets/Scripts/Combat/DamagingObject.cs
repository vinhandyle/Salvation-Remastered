using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows an object to damage the player on contact.
/// </summary>
public class DamagingObject : MonoBehaviour
{
    [SerializeField] protected Animator anim;
    [SerializeField] protected Collider2D cldr;
    protected Rigidbody2D rb;

    protected HealthManager health;

    [SerializeField] protected int damage;
    [SerializeField] protected bool destroyOnHit;
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
            if (health != null) health.TakeDamage(damage);

            // Knockback player
            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();

                if (canKnockback)
                {
                    Vector2 kb = 100 * knockbackAmt * (other.transform.position - transform.position);
                    StartCoroutine(player.Knockback(kb));
                }
            }

            // Destroy object if appropriate
            if (destroyOnHit)
                DestroyObject();
        }
    }

    protected bool ValidTarget(GameObject obj)
    {
        return targetTags.Count == 0 || targetTags.Contains(obj.tag);
    }

    protected void DestroyObject()
    {
        cldr.enabled = false;
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;

        // Play collision animation
        if (anim == null)
            Destroy(gameObject);
        else
            anim.SetTrigger("OnDestroy");
    }

    /// <summary>
    /// Call to destroy object after collision animation finishes.
    /// </summary>
    private void DestroyObjectPostAnim()
    {
        Destroy(gameObject);
    }
}
