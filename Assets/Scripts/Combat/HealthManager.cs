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

    [SerializeField] private HealthBar healthBar;
    [SerializeField] private GameObject rootObject;
    [SerializeField] private int maxHealth;
    [SerializeField] private bool immune;

    public event Action OnDying;
    public event Action OnDeath;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        healthBar?.SetDefaults(maxHealth);
    }

    /// <summary>
    /// Set whether the player is immune to damage.
    /// </summary>
    public void SetImmunity(bool i)
    {
        immune = i;
    }

    /// <summary>
    /// Deal the specified amount of damage to the player.
    /// </summary>
    public void TakeDamage(int amt)
    {
        if (!immune)
        {
            healthBar.health -= amt * (CompareTag("Player") ? PlayerData.Instance.dmgMult : 1);
            StartCoroutine(DamageEffect());
            //AudioController.Instance.PlayEffect(1);           

            if (healthBar.health <= 0)
            {
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
                        anim.SetTrigger("OnDestroy");
                    }
                }
            }
        }
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