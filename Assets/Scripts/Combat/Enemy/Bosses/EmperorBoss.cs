using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmperorBoss : Enemy
{
    private Rigidbody2D rb;
    private Animator anim;

    private enum Stage
    {
        Start,
        Rest,
        CannonFire,
        Gale,
        Judgement
    }
    [SerializeField] private Stage stage;
    [SerializeField] private Stage debugStage;

    [Header("Emperor")]
    [SerializeField] private HealthBar health;
    [SerializeField] private List<float> phaseThresholds;
    private int phase = 1;

    [Header("Shields")]
    [SerializeField] private List<GameObject> shields;
    [SerializeField] private GameObject shieldRotator;
    [SerializeField] private float shieldRotateSpeed;

    [Header("Overheat")]
    [SerializeField] private List<ZoneTrap> ohFireColumns;
    [SerializeField] private List<float> ohTimesToTrigger;
    [SerializeField] private float ohDuration;

    [Header("Summon")]
    [SerializeField] private Enemy summon;
    [SerializeField] private Transform summonOrigin;
    [SerializeField] private float summonThreshold;
    [SerializeField] private int maxSummons;
    private List<Enemy> activeSummons;
    private int summonCounter;
    private int summonTokens;

    [Header("Cannon Fire")]
    [SerializeField] private float cfFireSpeed;
    [SerializeField] private float cfFireRate;
    [SerializeField] private int cfBullets;

    [Header("Gale")]
    [SerializeField] private List<ProjectileSpawner> galeSpawners;
    [SerializeField] private float galeTravelSpeed;
    [SerializeField] private float galeDuration;

    [Header("Judgement")]
    [SerializeField] private GameObject jmWarning;
    [SerializeField] private GameObject jmDeathZone;    
    [SerializeField] private GameObject jmSafeZone;
    [SerializeField] private List<BoxCollider2D> jmSafeZoneBounds;
    [SerializeField] private float jmChannelTime;
    [SerializeField] private float jmDuration;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Death handlers
        HealthManager hm = GetComponent<HealthManager>();
        hm.OnDying += () =>
        {
            if (!PlayerData.Instance.fullCam) vcam.Priority += 2;

            activeSummons.ForEach(s => s.GetComponent<HealthManager>().TakeDamage(int.MaxValue));
        };

        hm.OnDeath += () =>
        {
            if (!PlayerData.Instance.fullCam) vcam.Priority -= 2;

            GameStateManager.Instance.UpdateState(GameStateManager.GameState.PAUSED);
            PlayerData.Instance.UpdateBestTime(SceneController.Instance.currentLevel + (PlayerData.Instance.expertMode ? "E" : ""), timer.timer);
            SceneController.Instance.LoadScene("Win", false);
        };
        
        hm.OnDamage += () =>
        {
            // Shields
            float shieldThreshold = health.maxHealth / (1 + shields.Count);
            for (int i = 0; i < shields.Count; ++i)
            {
                shields[i].SetActive(i < Mathf.Floor((health.maxHealth - health.health) / shieldThreshold));
            }

            // Summon
            int summonCounterExpected = (int)Mathf.Floor((health.maxHealth - health.health) / (health.maxHealth * summonThreshold));
            if (summonCounter < summonCounterExpected)
            {
                summonTokens += summonCounterExpected - summonCounter;
                summonCounter = summonCounterExpected;
            }
        };

        // Opening move
        ohFireColumns.ForEach(fc => fc.SetDefaults(ohTimesToTrigger[0], ohDuration));
        activeSummons = new List<Enemy>();
        StartCoroutine(Rest());
    }

    private void Update()
    {
        shieldRotator.transform.Rotate(shieldRotateSpeed * Time.deltaTime * Vector3.forward);   
        
        if (activeSummons.Count < maxSummons && summonTokens > 0)
        {           
            summonTokens--;
            Summon();
        }
    }

    private void DebugStage()
    {
        switch (debugStage)
        {
            case Stage.CannonFire:
                StartCoroutine(CannonFire());
                break;

            case Stage.Gale:
                StartCoroutine(Gale());
                break;

            case Stage.Judgement:
                StartCoroutine(Judgement());
                break;
        }
    }

    private void Summon()
    {
        Enemy _summon = Instantiate(summon, summonOrigin);
        _summon.GetComponent<HealthManager>().OnDeath += () => 
        { 
            activeSummons.Remove(_summon);  
        };
        activeSummons.Add(_summon);
    }

    private IEnumerator Rest()
    {
        int rand = Random.Range(0, 100);
        stage = Stage.Rest;

        // Repeat attack for debugging
        if (debug)
        {
            yield return new WaitForSeconds(debugTime);
            DebugStage();
            yield break;
        }

        yield return new WaitForSeconds(1.5f);

        // Enter Phase 2
        if (phase == 1 && health.health <= health.maxHealth * phaseThresholds[0])
        {
            phase = 2;
            StartCoroutine(Gale());
            yield break;
        }
        // Enter Phase 3
        else if (phase == 2 && health.health <= health.maxHealth * phaseThresholds[1])
        {
            phase = 3;
            ohFireColumns.ForEach(fc => fc.SetDefaults(ohTimesToTrigger[1], ohDuration));
            StartCoroutine(Judgement());
            yield break;
        }

        // Choose next move      
        switch (phase)
        {
            case 1:
                StartCoroutine(CannonFire()); // 100%
                break;

            case 2:
                if (rand < 75)
                    StartCoroutine(CannonFire()); // 75%
                else
                    StartCoroutine(Gale()); // 25%
                break;

            case 3:
                if (rand < 70)
                    StartCoroutine(CannonFire()); // 70%
                else if (rand < 80)
                    StartCoroutine(Gale()); // 10%
                else
                    StartCoroutine(Judgement()); // 20%
                break;
        }
    }

    private IEnumerator CannonFire()
    {
        stage = Stage.CannonFire;

        int rand = Random.Range(0, 3);
        float _cfFireRate = cfFireRate;

        if (rand > 0)
            _cfFireRate /= (rand == 1) ? 1.5f : 0.75f;

        for (int i = 0; i < cfBullets; ++i)
        {
            Aim(player.transform.position);
            Shoot(rand, cfFireSpeed);
            yield return new WaitForSeconds(_cfFireRate);
        }

        StartCoroutine(Rest());
    }

    private IEnumerator Gale()
    {
        stage = Stage.Gale;

        foreach (ProjectileSpawner gs in galeSpawners)
        {
            gs.SetProjectileSpeed(galeTravelSpeed);
            gs.Toggle();
        }

        yield return new WaitForSeconds(galeDuration);

        galeSpawners.ForEach(gs => gs.Toggle());
        StartCoroutine(Rest());
    }

    private IEnumerator Judgement()
    {
        stage = Stage.Judgement;

        // Randomize position of safe zone
        // (must be easily accessible to the player)
        Vector3 randPos = SelectRandomPointFromBounds(jmSafeZoneBounds);
        jmSafeZone.transform.position = new Vector3(
            randPos.x,
            randPos.y,
            jmSafeZone.transform.position.z
        );

        jmSafeZone.SetActive(true);
        jmWarning.SetActive(true);        

        yield return new WaitForSeconds(jmChannelTime);

        jmDeathZone.SetActive(true);
        jmWarning.SetActive(false);     

        // Reduce player's health to 1 and drain all weapons of energy
        if (!jmSafeZone.GetComponent<TriggerZone>().playerInRange)
        {
            player.GetComponent<HealthManager>().Heal(int.MinValue);
            player.GetComponent<PlayerShooter>().DrainEnergy(int.MaxValue, true);
        }

        yield return new WaitForSeconds(jmDuration);

        jmSafeZone.SetActive(false);
        jmDeathZone.SetActive(false);

        StartCoroutine(Rest());
    }
}
