using AudioManager;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SubnauticalBoss : Enemy
{
    private Rigidbody2D rb;
    private Animator anim;
    private HealthManager hm;
    private MovingObject movingObject;

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
        Downpour,
        Death
    }
    [SerializeField] private Stage stage;
    [SerializeField] private Stage debugStage;
    private List<Stage> uninterruptable;

    [Header("Subnautical")]
    [SerializeField] private HealthBar health;
    [SerializeField] private GameObject core;
    [SerializeField] private float phase2Threshold;
    private bool inPhase2;

    [Header("Swim")]
    [SerializeField] private float swimSpeed;
    [SerializeField] private float swimRange;
    [SerializeField] private int swimDir;
    private Vector3 initPos;

    [Header("Dive / Surface")]
    [SerializeField] private SoundEffect diveSfx;
    [SerializeField] private SoundEffect surfaceSfx;
    [SerializeField] private SoundEffect leapStartSfx;
    [SerializeField] private SoundEffect leapEndSfx;
    [SerializeField] private float surfaceSpeed;
    [SerializeField] private float leapSpeed;   
    [SerializeField] private float surfaceDistance;
    [SerializeField] private float leapDistance;
    [SerializeField] private float diveTime;
    [SerializeField] private float surfaceTime;
    private float swimTimer;
    private bool isUnderwater = true;

    [Header("Scatter Shot")]
    [SerializeField] private SoundEffect ssSfx;
    [SerializeField] private int ssBullets1;
    [SerializeField] private int ssBullets2;    
    [SerializeField] private int ssSpreadAngle;
    [SerializeField] private int ssWaves;
    [SerializeField] private float ssFireRate;
    [SerializeField] private float ssBulletSpeed;

    [Header("Crystal Barrage")]
    [SerializeField] private SoundEffect cbSfx;
    [SerializeField] private int cbBullets1;
    [SerializeField] private int cbBullets2;    
    [SerializeField] private List<float> cbFireRates;
    [SerializeField] private List<float> cbBulletSpeeds;
    [SerializeField] private float cbFragSpeed;
    [SerializeField] private List<int> cbNumFrags;

    [Header("Torpedo")]
    [SerializeField] private SoundEffect torpedoSfx;
    [SerializeField] private int torpedoAmt1;
    [SerializeField] private int torpedoAmt2;
    [SerializeField] private float torpedoFireRate;
    [SerializeField] private float torpedoSpeed;

    [Header("Downpour")]
    [SerializeField] private SoundEffect dpSfx;
    [SerializeField] private ProjectileSpawner dpSpawner;
    [SerializeField] private float dpFallSpeed;
    [SerializeField] private float dpDuration;
    [SerializeField] private float dpCooldown;
    private float dpTimer;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        movingObject = GetComponent<MovingObject>();

        uninterruptable = new List<Stage>
        {
            Stage.Dive, Stage.Surface, Stage.Leap, Stage.Downpour,
            Stage.ScatterShot, Stage.CrystalBarrage, Stage.Torpedo,
            Stage.Death
        };
        initPos = transform.position;

        // Death handlers
        hm = core.GetComponent<HealthManager>();
        hm.SetDamageMult(0.5f);

        hm.OnDying += () =>
        {
            if (!PlayerData.Instance.fullCam) vcam.Priority += 2;

            stage = Stage.Death;
            rb.velocity = Vector2.zero;

            ClearSoundEffects();
            PlayDeathSoundEffect();
        };

        hm.OnDeath += () =>
        {
            GameStateManager.Instance.UpdateState(GameStateManager.GameState.PAUSED);

            if (!PlayerData.Instance.fullCam) vcam.Priority -= 2;

            if (BossGauntlet.Instance.inGauntlet)
            {
                FindObjectOfType<GauntletMenu>().SetActive(true);
                BossGauntlet.Instance.AddToTimer(timer.timer);
                BossGauntlet.Instance.UpdateNoHit(player.GetComponent<HealthManager>().noHit);
            }
            else
            {
                PlayerData.Instance.UpdateBestTime(SceneController.Instance.currentLevel + (PlayerData.Instance.expertMode ? "E" : ""), timer.timer);
                SceneController.Instance.LoadScene("Win", false);
            }
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
        dpTimer += Time.deltaTime;

        if (!debug && !uninterruptable.Contains(stage))
        {
            if ((isUnderwater && swimTimer >= surfaceTime) || (!isUnderwater && swimTimer >= diveTime))
            {
                // Dive / Surface / Leap

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
                // Underwater Movement

                if (isUnderwater && movingObject.stopped)
                {
                    Swim(swimDir *= -1);
                }

                // Phase 2 + Downpour

                if (!inPhase2 && health.health <= health.maxHealth * phase2Threshold)
                {
                    inPhase2 = true;
                    StopAllCoroutines(); // Prevent parallel routines
                    StartCoroutine(Downpour());
                }
                else if (dpTimer == dpCooldown)
                {
                    StopAllCoroutines(); // Prevent parallel routines
                    dpTimer = 0;

                    if (Random.Range(0,3) < 2)
                        StartCoroutine(Downpour()); // 66%
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

    private void Swim(int dir)
    {
        movingObject.Move(swimSpeed, new Vector2(initPos.x + dir * swimRange, initPos.y));
    }

    private IEnumerator Rest()
    {
        if (stage != Stage.Start)
            movingObject.Resume();
        stage = Stage.Rest;       

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
            movingObject.Stop();

            if (Random.Range(0, 2) == 0)
                StartCoroutine(CrystalBarrage()); // 50%
            else
                StartCoroutine(Torpedo()); // 50%
        }
        else
        {
            StartCoroutine(ScatterShot());
        }
    }

    private IEnumerator Dive()
    {
        stage = Stage.Dive;

        PlaySoundEffect(diveSfx);
        movingObject.Move(surfaceSpeed, new Vector2(transform.position.x, transform.position.y - surfaceDistance));
        while (!movingObject.stopped)
            yield return null;

        swimTimer = 0;
        isUnderwater = true;
        hm.SetDamageMult(0.5f);
        Swim(swimDir);
        StartCoroutine(Rest());
    }

    private IEnumerator Surface()
    {
        stage = Stage.Surface;

        PlaySoundEffect(surfaceSfx);
        movingObject.Move(surfaceSpeed, new Vector2(transform.position.x, transform.position.y + surfaceDistance));
        while (!movingObject.stopped)
            yield return null;

        swimTimer = 0;
        isUnderwater = false;
        hm.SetDamageMult();
        StartCoroutine(Rest());
    }

    private IEnumerator Leap()
    {
        stage = Stage.Leap;

        PlaySoundEffect(leapStartSfx);
        movingObject.Move(leapSpeed, new Vector2(transform.position.x, transform.position.y + leapDistance));
        while (!movingObject.stopped)
            yield return null;

        movingObject.Move(leapSpeed * 0.6f, new Vector2(transform.position.x, initPos.y + surfaceDistance));
        while (!movingObject.stopped)
            yield return null;

        PlaySoundEffect(leapEndSfx);
        swimTimer = 0;
        isUnderwater = false;
        hm.SetDamageMult();
        StartCoroutine(Rest());
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
            PlaySoundEffect(ssSfx);

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
            PlaySoundEffect(cbSfx);

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
            PlaySoundEffect(torpedoSfx);
            yield return new WaitForSeconds(torpedoFireRate);
        }

        StartCoroutine(Rest());
    }

    private IEnumerator Downpour()
    {
        stage = Stage.Downpour;
        dpTimer = 0;

        float _dpFallSpeed = dpFallSpeed;
        if (health.health <= health.maxHealth * phase2Threshold / 2)
            _dpFallSpeed *= 2;
       
        dpSpawner.SetProjectileSpeed(_dpFallSpeed);
        dpSpawner.Toggle();
        PlaySoundEffect(dpSfx, true);

        yield return new WaitForSeconds(dpDuration);

        dpSpawner.Toggle();
        ClearSoundEffects();
        StartCoroutine(Rest());
    }
}
