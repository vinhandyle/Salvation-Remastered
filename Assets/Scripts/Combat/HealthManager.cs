using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks and updates health.
/// </summary>
public class HealthManager : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer sprite;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private GameObject rootObject;
    [SerializeField] private DeathExplosion deathExplosion;
    [SerializeField] private int maxHealth;
    [SerializeField] private bool immune;
    private float dmgMult;

    public bool noHit { get; private set; }

    public event Action OnDying;
    public event Action OnDeath;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        healthBar?.SetDefaults(maxHealth);

        noHit = true;
        SetDamageMult();
    }

    /// <summary>
    /// Set whether the player is immune to damage.
    /// </summary>
    public void SetImmunity(bool i)
    {
        immune = i;
    }

    public void SetDamageMult(float dmgMult = 1)
    {
        this.dmgMult = dmgMult;
    }

    /// <summary>
    /// Deal the specified amount of damage to the player.
    /// </summary>
    public void TakeDamage(int amt)
    {
        if (!immune)
        {
            float dmg = amt * (CompareTag("Player") ? PlayerData.Instance.dmgMult : dmgMult);
            noHit = false;

            if (dmg > 0) StartCoroutine(DamageEffect());
            healthBar.health -= dmg;            
            AudioController.Instance.PlayEffect(audioSource, 8);

            if (healthBar.health <= 0)
            {
                // Prevent simultaneous damage from multiple sources triggering multi-death
                immune = true;

                // Player: Game over
                if (CompareTag("Player"))
                {
                    GameStateManager.Instance.UpdateState(GameStateManager.GameState.PAUSED);
                    SceneController.Instance.LoadScene("Game Over", false);
                }
                // Enemy: Death animation -> Destroy
                else
                {
                    healthBar.gameObject.SetActive(false);

                    if (anim == null)
                    {
                        OnDeath?.Invoke();
                        OnDeath = null;
                        Destroy(rootObject);
                    }
                    else
                    {
                        OnDying?.Invoke();
                        OnDying = null;

                        if (deathExplosion)
                            deathExplosion.gameObject.SetActive(true);
                        anim.SetTrigger("OnDestroy");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Restore health by a flat amount and/or percent.
    /// </summary>
    public void Heal(int amt, float pct)
    {
        healthBar.health += amt;
        healthBar.health += healthBar.maxHealth * pct;
    }

    /// <summary>
    /// Call to destroy object after death animation finishes.
    /// </summary>
    public void DestroyObject()
    {
        OnDeath?.Invoke();
        OnDeath = null;
        Destroy(rootObject);
    }

    private IEnumerator DamageEffect()
    {
        sprite.color = new Color(1, 1, 1, 0.5f);

        yield return new WaitForSeconds(0.1f);

        sprite.color = new Color(1, 1, 1, 1);
    }
}