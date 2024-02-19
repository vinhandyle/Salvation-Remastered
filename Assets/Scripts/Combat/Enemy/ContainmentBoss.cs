using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainmentBoss : Enemy
{
    private Rigidbody2D rb;
    private Animator anim;

    private enum Stage
    {
        Start,
        Rest,
        Follow,
        Blink,
        Crash,
        Explosion,
        Berserk
    }
    [SerializeField] private Stage stage;
    [SerializeField] private Stage debugStage;

    [Header("Containment")]
    [SerializeField] private HealthBar health;
    [SerializeField] private GameObject core;
    [SerializeField] private GameObject spiral;
    [SerializeField] private float phase2Threshold;
    private bool inPhase2;
    private float coreRadius;

    [Header("Folllow")]
    [SerializeField] private AggroRange aggroRange;
    [SerializeField] private float followSpeed;
    [SerializeField] private float followFarSpeed;
    [SerializeField] private float phase2SpeedMult;

    [Header("Blink")]
    [SerializeField] protected float blinkDist;
    [SerializeField] protected float blinkTime1;
    [SerializeField] protected float blinkTime2;
    [SerializeField] protected float blinkRestTime;
    private float blinkTimer;

    [Header("Crash")]
    [SerializeField] private DamagingObject hurtbox;
    [SerializeField] private float crashSpeed;
    [SerializeField] private float crashWarningTime;
    private bool crashing;

    [Header("Explosion")]
    [SerializeField] private int explosionWaves;
    [SerializeField] private int explosionBullets;
    [SerializeField] private float explosionBulletSpeed;
    [SerializeField] private float explosionWaveDelay;

    [Header("Berserk")]
    [SerializeField] private int berserkWaves;
    [SerializeField] private float berserkWarningTime;
    private int berserkCurrentWave;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Death handlers
        HealthManager hm = core.GetComponent<HealthManager>();
        hm.OnDying += () =>
        {
            if (!PlayerData.Instance.fullCam) vcam.Priority += 2;

            stage = Stage.Start;
            rb.velocity = Vector2.zero;
        };

        hm.OnDeath += () =>
        {
            if (!PlayerData.Instance.fullCam) vcam.Priority -= 2;

            GameStateManager.Instance.UpdateState(GameStateManager.GameState.PAUSED);
            PlayerData.Instance.UpdateBestTime(SceneController.Instance.currentLevel + (PlayerData.Instance.expertMode ? "E" : ""), timer.timer);
            SceneController.Instance.LoadScene("Win", false);
        };

        // Opening move
        coreRadius = core.GetComponent<CircleCollider2D>().radius;
        aggroRange.player = player;

        if (debug)
            StartCoroutine(Rest());
        else
            stage = Stage.Follow;
    }

    private void Update()
    {
        // Stop all attacks if dead
        if (health.health <= 0)
        {
            StopAllCoroutines();
            rb.velocity = Vector2.zero;
        }

        if (stage == Stage.Follow)
        {
            blinkTimer += Time.deltaTime;

            // Move towards player
            float baseSpeed = aggroRange.playerIsInRange ? followSpeed : followFarSpeed;
            float speedMult = inPhase2 ? phase2SpeedMult : 1;

            Aim(player.transform.position);
            rb.velocity = baseSpeed * speedMult * rotator.right; 

            if ((!inPhase2 && blinkTimer >= blinkTime1) || (inPhase2 && blinkTimer >= blinkTime2))
            {
                blinkTimer = 0;

                if (Random.Range(0, 4) == 0)
                    StartCoroutine(Blink()); // 25%
                else
                    StartCoroutine(Rest()); // 75%
            }
        }
    }

    private void DebugStage()
    {
        switch (debugStage)
        {
            case Stage.Blink:
                StartCoroutine(Blink());
                break;

            case Stage.Crash:
                StartCoroutine(Crash());
                break;

            case Stage.Explosion:
                StartCoroutine(Explosion(explosionWaves));
                break;

            case Stage.Berserk:
                StartCoroutine(Berserk());
                break;
        }
    }

    private void SetSpiralColor(float r, float g, float b)
    {
        spiral.GetComponent<SpriteRenderer>().color = new Color(r, g, b);
    }

    private IEnumerator Rest()
    {
        int rand = Random.Range(0, 300);
        stage = Stage.Rest;
        rb.velocity = Vector2.zero;

        // Repeat attack for debugging
        if (debug)
        {
            yield return new WaitForSeconds(debugTime);
            DebugStage();
            yield break;
        }

        // Enter Phase 2
        if (!inPhase2 && health.health <= health.maxHealth * phase2Threshold)
        {
            inPhase2 = true;
            StartCoroutine(Berserk());
            yield break;
        }

        // Choose next move      
        if (inPhase2)
        {
            if (rand < 75)
                stage = Stage.Follow; // 25%
            else if (rand < 150)
                StartCoroutine(Crash()); // 25%
            else if (rand < 225)
                StartCoroutine(Explosion(explosionWaves)); // 25%
            else
                StartCoroutine(Berserk()); // 25%
        }
        else
        {
            if (rand < 100)
                stage = Stage.Follow; // 33%
            else if (rand < 200)
                StartCoroutine(Crash()); // 33%
            else
                StartCoroutine(Explosion(explosionWaves)); // 33%
        }
    }

    private IEnumerator Blink()
    {
        stage = Stage.Blink;
        rb.velocity = Vector2.zero;

        // If player is not moving, blink above them
        Rigidbody2D playerRB = player.GetComponent<Rigidbody2D>();

        if (playerRB.velocity == Vector2.zero)
        {
            float maxDist = DistanceFromArenaBox(playerRB.position, Vector2.up) - coreRadius;
            transform.position = new Vector3(
                playerRB.position.x,
                playerRB.position.y + (blinkDist < maxDist ? blinkDist : maxDist)
            );
        }
        // Else, blink ahead of their trajectory
        else if (!player.GetComponent<PlayerController>().dashing)
        {
            float maxDist = DistanceFromArenaBox(playerRB.position, playerRB.velocity.normalized) - coreRadius;
            transform.position = (blinkDist < maxDist ? blinkDist : maxDist) * playerRB.velocity.normalized + playerRB.position;
        }

        // Don't move to allow player to react
        yield return new WaitForSeconds(blinkRestTime);

        if (debug)
        {
            DebugStage();
        }
        else
        {
            if (Random.Range(0, 4) == 0)
                StartCoroutine(Crash());
            else
                stage = Stage.Follow;
        }
    }

    private IEnumerator Crash()
    {
        // Telegraph
        if (stage != Stage.Berserk)
        {
            stage = Stage.Crash;
            SetSpiralColor(1, 0.75f, 0);
            yield return new WaitForSeconds(crashWarningTime);
        }

        // Perform
        SetSpiralColor(0, 0, 0);
        Aim(player.transform.position);
        rb.velocity = crashSpeed * rotator.right;
        hurtbox.dmgMult = 2;
        crashing = true;

        // Finish
        while (crashing)
            yield return null;
        rb.velocity = Vector2.zero;
        hurtbox.dmgMult = 1;

        if (stage != Stage.Berserk)
        {
            if (inPhase2)
                yield return new WaitForSeconds(1);
            else
                yield return new WaitForSeconds(2);
        }

        if (stage == Stage.Berserk)
            StartCoroutine(Explosion(1));
        else
            StartCoroutine(Rest());
    }

    private IEnumerator Explosion(int waves)
    {
        if (stage != Stage.Berserk)
            stage = Stage.Explosion;

        // Perform Explosion
        float angle = 360f / explosionBullets;
        Aim(transform.position);

        for (int i = 0; i < waves; i++)
        {
            for (int j = 0; j < explosionBullets; j++)
            {
                // Alternate shot angle between waves
                Shoot(0, explosionBulletSpeed, angle * (j + i / 2f));
            }

            yield return new WaitForSeconds(explosionWaveDelay);
        }

        // Cooldown based on partial or full explosion
        if (inPhase2)
        {
            if (berserkCurrentWave == berserkWaves - 1)
                yield return new WaitForSeconds(3);
            else
                yield return new WaitForSeconds(1);
        }
        else
            yield return new WaitForSeconds(2);

        // Cycle if Berserk
        if (stage == Stage.Berserk && berserkCurrentWave < berserkWaves - 1)
        {
            berserkCurrentWave++;
            StartCoroutine(Crash());
        }
        else
        {
            StartCoroutine(Rest());
        }
    }

    private IEnumerator Berserk()
    {
        stage = Stage.Berserk;
        berserkCurrentWave = 0;

        // Telegraph
        SetSpiralColor(1, 0, 0);
        yield return new WaitForSeconds(berserkWarningTime);

        // Perform
        SetSpiralColor(0, 0, 0);
        StartCoroutine(Crash());
        yield break;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Arena Box"))
        {
            // Due to terrain splitting, enforce no multi-correction
            if (crashing)
            {
                crashing = false;

                // Course correct           
                Vector2 contact = (Vector2)transform.position - collision.contacts[0].point;
                Vector2 normal = collision.contacts[0].normal;
                float offset;

                // At most half-way deep
                if (Vector2.Dot(contact, normal) >= 0)
                    offset = coreRadius - contact.magnitude;
                // More than half-way deep
                else
                    offset = coreRadius + contact.magnitude;
               
                transform.position = offset * normal + (Vector2)transform.position;
            }
        }
    }
}
