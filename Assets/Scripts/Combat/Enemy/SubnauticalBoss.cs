using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubnauticalBoss : Enemy
{
    private Rigidbody2D rb;
    private Animator anim;

    private enum Stage
    {
        Start,
        Rest,
        Dive,
        Surface,
        Leap,
        ScatterShot,
        CrystalBarrage,
        Torpedo,
        Downpour
    }
    [SerializeField] private Stage stage;
    [SerializeField] private Stage debugStage;
    private List<Stage> uninterruptable;

    [Header("Subnautical")]
    [SerializeField] private HealthBar health;
    [SerializeField] private GameObject core;
    [SerializeField] private float phase2Threshold;
    private bool inPhase2;

    [Header("Dive / Surface")]
    [SerializeField] private float swimSpeed;
    [SerializeField] private float leapSpeed;
    [SerializeField] private float surfaceDistance;
    [SerializeField] private float leapDistance;
    [SerializeField] private float diveTime;
    [SerializeField] private float surfaceTime;
    private float swimTimer;
    private Vector3 initPos;
    private bool isUnderwater;

    [Header("Scatter Shot")]
    [SerializeField] private int ssBullets1;
    [SerializeField] private int ssBullets2;    
    [SerializeField] private int ssSpreadAngle;
    [SerializeField] private int ssWaves;
    [SerializeField] private float ssFireRate;
    [SerializeField] private float ssBulletSpeed;    

    [Header("Crystal Barrage")]
    [SerializeField] private int cbBullets1;
    [SerializeField] private int cbBullets2;    
    [SerializeField] private List<float> cbFireRates;
    [SerializeField] private List<float> cbBulletSpeeds;
    [SerializeField] private float cbFragSpeed;
    [SerializeField] private List<int> cbNumFrags;

    [Header("Torpedo")]
    [SerializeField] private int torpedoAmt1;
    [SerializeField] private int torpedoAmt2;
    [SerializeField] private float torpedoFireRate;
    [SerializeField] private float torpedoSpeed;

    [Header("Downpour")]
    [SerializeField] private float dpFallSpeed;
    [SerializeField] private float dpDuration;
    [SerializeField] private float dpTimer;
    private float dpTime;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        uninterruptable = new List<Stage>
        {
            Stage.Dive, Stage.Surface, Stage.Leap, Stage.Downpour,
            Stage.ScatterShot, Stage.CrystalBarrage, Stage.Torpedo,
        };
        initPos = transform.position;

        // Death handlers
        HealthManager hm = core.GetComponent<HealthManager>();
        hm.OnDying += () =>
        {
            vcam.Priority += 2;

            stage = Stage.Start;
            rb.velocity = Vector2.zero;
        };

        hm.OnDeath += () =>
        {
            vcam.Priority -= 2;

            GameStateManager.Instance.UpdateState(GameStateManager.GameState.PAUSED);
            PlayerData.Instance.UpdateBestTime(SceneController.Instance.currentLevel + (PlayerData.Instance.expertMode ? "E" : ""), timer.timer);
            SceneController.Instance.LoadScene("Win", false);
        };

        // Opening move
        StartCoroutine(Rest());
    }

    private void Update()
    {
        // Stop all attacks if dead
        if (health.health <= 0)
        {
            StopAllCoroutines();
            rb.velocity = Vector2.zero;
        }

        swimTimer += Time.deltaTime;
        dpTime += Time.deltaTime;

        if (!debug && !uninterruptable.Contains(stage))
        {           
            if ((isUnderwater && swimTimer >= surfaceTime) || (!isUnderwater && swimTimer >= diveTime))
            {
                // Dive / Surface

                StopAllCoroutines(); // Prevent parallel routines

                if (!isUnderwater)
                    StartCoroutine(Dive());
                else if (inPhase2)
                    StartCoroutine(Leap());
                else
                    StartCoroutine(Surface());
            }
            else
            {
                // Phase 2 + Downpour

                if (!inPhase2 && health.health <= health.maxHealth * phase2Threshold)
                {
                    inPhase2 = true;
                    StopAllCoroutines(); // Prevent parallel routines
                    StartCoroutine(Downpour());
                }
                else if (dpTimer == dpTime)
                {
                    StopAllCoroutines(); // Prevent parallel routines
                    StartCoroutine(Downpour());
                }
            }
        }        
    }

    private void DebugStage()
    {
        switch (debugStage)
        {
            case Stage.Dive:
                StartCoroutine(Dive());
                break;

            case Stage.Surface:
                StartCoroutine(Surface());
                break;

            case Stage.Leap:
                StartCoroutine(Leap());
                break;

            case Stage.ScatterShot:
                StartCoroutine(ScatterShot());
                break;

            case Stage.CrystalBarrage:
                StartCoroutine(CrystalBarrage());
                break;

            case Stage.Torpedo:
                StartCoroutine(Torpedo());
                break;

            case Stage.Downpour:
                StartCoroutine(Downpour());
                break;
        }
    }

    private IEnumerator Rest()
    {
        stage = Stage.Rest;
        rb.velocity = Vector2.zero;

        // Repeat attack for debugging
        if (debug)
        {
            yield return new WaitForSeconds(debugTime);
            DebugStage();
            yield break;
        }

        // Choose next move
        yield return new WaitForSeconds(4);

        if (isUnderwater)
        {
            if (Random.Range(0, 2) == 0)
                StartCoroutine(CrystalBarrage());
            else
                StartCoroutine(Torpedo());
        }
        else
        {
            StartCoroutine(ScatterShot());
        }
    }

    private IEnumerator Dive()
    {
        stage = Stage.Dive;
        yield return null;
    }

    private IEnumerator Surface()
    {
        stage = Stage.Surface;
        yield return null;
    }

    private IEnumerator Leap()
    {
        stage = Stage.Leap;
        yield return null;
    }

    private IEnumerator ScatterShot()
    {
        int bullets = inPhase2 ? ssBullets2 : ssBullets1;
        float angle = ssSpreadAngle / (bullets - 1);
        float startAngle = -ssSpreadAngle / 2f;
        stage = Stage.ScatterShot;

        for (int i = 0; i < ssWaves; ++i)
        {
            Aim(player.transform.position);

            for (int j = 0; j < bullets; ++j)
            {
                Shoot(0, ssBulletSpeed, startAngle + angle * j);
            }

            yield return new WaitForSeconds(ssFireRate);
        }

        StartCoroutine(Rest());
    }

    private IEnumerator CrystalBarrage()
    {
        int bullets = inPhase2 ? cbBullets2 : cbBullets1;
        stage = Stage.CrystalBarrage;

        for (int i = 0; i < bullets; ++i)
        {
            Aim(player.transform.position);

            // Snowball
            if (i == cbBullets1 - 1 || i == cbBullets2 - 1)
            {
                FragProjectile frag = Shoot(2, cbBulletSpeeds[1]).GetComponent<FragProjectile>();
                frag.SetDefaults(cbNumFrags[1], cbFragSpeed);
                yield return new WaitForSeconds(cbFireRates[1]);
            }
            // Crystal
            else
            {
                FragProjectile frag = Shoot(1, cbBulletSpeeds[0]).GetComponent<FragProjectile>();
                frag.SetDefaults(cbNumFrags[0], cbFragSpeed);

                if (i < cbBullets1)
                    yield return new WaitForSeconds(cbFireRates[0]);
                else
                    yield return new WaitForSeconds(cbFireRates[2]);
            }
        }

        StartCoroutine(Rest());
    }

    private IEnumerator Torpedo()
    {
        int bullets = inPhase2 ? torpedoAmt2 : torpedoAmt1;
        stage = Stage.Torpedo;

        for (int i = 0; i < bullets; ++i)
        {
            Aim(transform.position);
            Shoot(3, torpedoSpeed);
            yield return new WaitForSeconds(torpedoFireRate);
        }

        StartCoroutine(Rest());
    }

    private IEnumerator Downpour()
    {
        stage = Stage.Downpour;
        dpTime = 0;

        float _dpFallSpeed = dpFallSpeed;
        if (health.health <= health.maxHealth * phase2Threshold / 2)
            _dpFallSpeed /= 2;

        yield return null;

        StartCoroutine(Rest());
    }
}
