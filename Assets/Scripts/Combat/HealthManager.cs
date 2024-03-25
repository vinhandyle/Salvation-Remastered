using AudioManager;
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
    [SerializeField] private GameObject rootObject;
    [SerializeField] private DeathExplosion deathExplosion;
    [SerializeField] private int maxHealth;
    private float dmgMult;

    public bool immune { get; private set; }

    [Header("Health Bar")]
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private bool movingHealthBar;

    public bool noHit { get; private set; }

    public event Action OnDamage;
    public event Action OnDying;
    public event Action OnDeath;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        healthBar?.SetDefaults(maxHealth);

        if (movingHealthBar)
            healthBar.transform.parent = GameObject.Find("Canvas").transform;

        noHit = true;
        immune = gameObject.CompareTag("Player") && PlayerData.Instance.godMode[0];
        SetDamageMult();
    }

    private void Update()
    {
        // Set health bar to be just under the object's sprite
        if (movingHealthBar)
        {
            RectTransform canvasRect = healthBar.transform.parent.GetComponent<RectTransform>();
            Vector2 targetPosition = Camera.main.WorldToViewportPoint(
                transform.position - new Vector3(0, sprite.size.y / 2, 0)
            );

            Vector2 screenPosition = new Vector2(
                (targetPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f),
                (targetPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f)
            );
            healthBar.GetComponent<RectTransform>().anchoredPosition = screenPosition;
        }
    }

    /// <summary>
    /// Set whether the player is immune to damage.
    /// </summary>
    public void SetImmunity(bool i)
    {
        immune = PlayerData.Instance.godMode[0] || i;
    }

    public void SetDamageMult(float dmgMult = 1)
    {
        this.dmgMult = PlayerData.Instance.godMode[1] ? 1000 : dmgMult;
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

            if (dmg > 0)
            {
                StartCoroutine(DamageEffect());
                healthBar.health -= dmg;
                OnDamage?.Invoke();
                AudioController.Instance.PlayEffect(audioSource, SoundEffect.MetalHit);
            }

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
    public void Heal(int amt, float pct = 0)
    {
        // Prevent negative healing in god mode
        if (PlayerData.Instance.godMode[0] && amt < 0)
            amt = 0;

        healthBar.health += amt;
        healthBar.health += healthBar.maxHealth * pct;

        // Negative healing cannot kill
        if (healthBar.health < 0)
            healthBar.health = 1;
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