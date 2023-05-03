using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverseerBoss : Enemy
{
    private Rigidbody2D rb;
    private Animator anim;

    private enum Stage
    {
        Start,
        Rest,
        GearShift,
        RampingFire,
        ExplodingShot,
        ScorchedEarth,
        ChargeBeam
    }
    [SerializeField] private Stage stage;
    [SerializeField] private Stage debugStage;
    private List<Stage> uninterruptable;

    [Header("Overseer")]
    [SerializeField] private HealthBar health;
    [SerializeField] private GameObject core;
    private Vector3 initPos;
    [SerializeField] private float phase2Threshold;
    private bool inPhase2;

    [Header("Gear Shift")]
    [SerializeField] private float gearShiftSpeed;
    [SerializeField] private float gearShiftTime1;
    [SerializeField] private float gearShiftTime2;
    private float gearShiftTimer;
    private bool topGear;

    [Header("Ramping Fire")]
    [SerializeField] private float rpFireSpeed;
    [SerializeField] private float rpFireRate;
    [SerializeField] private float rpFireRateInc;
    [SerializeField] private int rpBullets;

    [Header("Exploding Shot")]
    [SerializeField] private float esFireSpeed;
    [SerializeField] private float esFragSpeed;
    [SerializeField] private int esFrags;

    [Header("Scorched Earth")]
    [SerializeField] private MovingObject scorchedEarth;
    [SerializeField] private GameObject seWarning;
    [SerializeField] private float seWarningTime;
    [SerializeField] private float seDuration;
    [SerializeField] private float seDeferTime;
    [SerializeField] private float seTravelSpeed;
    private bool seActive;

    [Header("Charge Beam")]
    [SerializeField] private GameObject chargeBeam;
    [SerializeField] private GameObject cbWarning;
    [SerializeField] private float cbWarningTime;
    [SerializeField] private float cbDuration;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        anim = core.GetComponent<Animator>();            

        uninterruptable = new List<Stage> 
        { 
            Stage.GearShift, Stage.RampingFire, Stage.ChargeBeam 
        };
        initPos = transform.position;

        // Death handlers
        HealthManager hm = core.GetComponent<HealthManager>();
        hm.OnDying += () => 
        { 
            vcam.Priority += 2;
            
            scorchedEarth.gameObject.SetActive(false);
            seWarning.SetActive(false);            
            chargeBeam.SetActive(false);
            cbWarning.SetActive(false);
        };

        hm.OnDeath += () => 
        { 
            vcam.Priority -= 2;
           
            GameStateManager.Instance.UpdateState(GameStateManager.GameState.PAUSED);
            PlayerData.Instance.UpdateBestTime(SceneController.Instance.currentLevel + (PlayerData.Instance.expertMode ? "E" : ""), timer.timer);
            SceneController.Instance.LoadScene("Win", false);           
        };        

        // Opening move
        if (debug)
            StartCoroutine(Rest());
        else
            StartCoroutine(Begin());
    }

    protected override void Update()
    {
        base.Update();

        // Stop all attacks if dead
        if (health.health <= 0)
        {
            StopAllCoroutines();
            rb.velocity = Vector2.zero;
        }

        // Gear Shift logic (independent of main AI)
        gearShiftTimer += Time.deltaTime;

        if (!uninterruptable.Contains(stage) &&
            ((!inPhase2 && gearShiftTimer >= gearShiftTime1) ||
            (inPhase2 && gearShiftTimer >= gearShiftTime2)))
        {
            StopAllCoroutines(); // Prevent parallel routines
            StartCoroutine(GearShift());
        }
    }

    private void DebugStage()
    {
        switch (debugStage)
        {
            case Stage.RampingFire:
                StartCoroutine(RampingFire());
                break;

            case Stage.ExplodingShot:
                StartCoroutine(ExplodingShot());
                break;

            case Stage.ScorchedEarth:
                StartCoroutine(ScorchedEarth());
                break;

            case Stage.ChargeBeam:
                StartCoroutine(ChargeBeam());
                break;
        }
    }

    private IEnumerator Begin()
    {
        yield return new WaitForSeconds(1.5f);

        if (Random.Range(0, 2) == 0)
            StartCoroutine(RampingFire()); // 50%
        else
            StartCoroutine(ExplodingShot()); // 50%
    }

    private IEnumerator Rest()
    {
        int rand = Random.Range(0, 300);
        stage = Stage.Rest;

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
            StartCoroutine(ChargeBeam());
            yield break;
        }

        // Choose next move      
        if (inPhase2)
        {
            yield return new WaitForSeconds(2);

            if (rand < 60)
                StartCoroutine(RampingFire()); // 20%
            else if (rand < 120)
                StartCoroutine(ExplodingShot()); // 20%
            else if (rand < 180)
                StartCoroutine(ScorchedEarth()); // 20%
            else
                StartCoroutine(ChargeBeam()); // 40%
        }
        else
        {
            yield return new WaitForSeconds(3);

            if (rand < 100)
                StartCoroutine(RampingFire()); // 30%
            else if (rand < 200)
                StartCoroutine(ExplodingShot()); // 30%
            else
                StartCoroutine(ScorchedEarth()); // 30%
        }
    }

    private IEnumerator GearShift()
    {
        stage = Stage.GearShift;

        // Set shift direction
        if (transform.position.y <= initPos.y)
            rb.velocity = gearShiftSpeed * Vector2.up;
        else
            rb.velocity = -gearShiftSpeed * Vector2.up;

        // Wait until shift is done
        float dist = core.GetComponent<BoxCollider2D>().size.y * core.transform.lossyScale.y;
        float newY = initPos.y + dist;

        while ((rb.velocity.y > 0 && transform.position.y < newY) || 
            (rb.velocity.y < 0 && transform.position.y > initPos.y))
        {
            yield return null;
        }

        // Correct over-shift
        if (transform.position.y > newY)
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        else if (transform.position.y < initPos.y)
            transform.position = initPos;

        // Complete shift
        rb.velocity = Vector2.zero;
        gearShiftTimer = 0;
        topGear = !topGear;
        StartCoroutine(Rest());
    }

    private IEnumerator RampingFire()
    {
        float _rpFireRate = rpFireRate;
        stage = Stage.RampingFire;

        for (int i = 0; i < rpBullets; i++)
        {
            Shoot(0, rpFireSpeed);
            yield return new WaitForSeconds(_rpFireRate);
            _rpFireRate *= 1 - rpFireRateInc;
        }

        StartCoroutine(Rest());
    }

    private IEnumerator ExplodingShot()
    {
        stage = Stage.ExplodingShot;

        FragProjectile proj = Shoot(1, esFireSpeed).GetComponent<FragProjectile>();
        proj.SetDefaults(esFrags, esFragSpeed);

        yield return null;
        StartCoroutine(Rest());
    }

    private IEnumerator ScorchedEarth()
    {
        stage = Stage.ScorchedEarth;

        if (seActive)
        {
            // Wait a bit then pick other random move
            int rand = Random.Range(0, 2);

            yield return new WaitForSeconds(seDeferTime);

            if (rand == 0)
                StartCoroutine(RampingFire()); // 50%
            else
                StartCoroutine(ExplodingShot()); // 50%
        }
        else
        {
            // Warning
            seActive = true;
            seWarning.SetActive(true);

            yield return new WaitForSeconds(seWarningTime);

            // Initiate
            seWarning.SetActive(false);

            Vector3 seInitPos = scorchedEarth.transform.position;
            float dist = scorchedEarth.GetComponent<BoxCollider2D>().size.y * 0.75f;
            scorchedEarth.Move(
                seTravelSpeed,
                new Vector3(seInitPos.x, seInitPos.y + dist, seInitPos.z)
            ); 
            StartCoroutine(Rest());

            yield return new WaitForSeconds(seDuration);

            // Clean up
            scorchedEarth.Return(seTravelSpeed);
            seActive = false;
        }
    }

    private IEnumerator ChargeBeam()
    {
        stage = Stage.ChargeBeam;

        // Warning
        anim.SetTrigger("Charge");
        cbWarning.SetActive(true);

        yield return new WaitForSeconds(cbWarningTime);

        // Initiate
        anim.SetTrigger("Beam");
        cbWarning.SetActive(false);
        chargeBeam.SetActive(true);

        yield return new WaitForSeconds(cbDuration);

        // Clean up
        chargeBeam.SetActive(false);
        anim.SetTrigger("Rest");
        StartCoroutine(Rest());
    }
}
