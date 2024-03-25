using AudioManager;
using LayerManager;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the player's combat abilities.
/// </summary>
public class PlayerShooter : MonoBehaviour
{
    private HealthManager health;
    private AudioSource playerAudio;

    [SerializeField] private Camera cam;
    [SerializeField] private Texture2D reticle;
    [SerializeField] private Transform rotator;
    [SerializeField] private Transform shootPoint;

    [SerializeField] private List<GameObject> projectiles;
    [SerializeField] private List<float> projSpeeds;
    private float projSpeed;

    [SerializeField] private List<float> useTimes;
    [SerializeField] private ResourceBar useTimeBar;
    private float useTime;
    private float useTimer;

    [SerializeField] private List<int> energyCost;
    [SerializeField] private EnergyBar energyBar;
    [SerializeField] private int maxEnergy;
    [SerializeField] private float rechargeSpeed;
    private List<float> energyLeft = new List<float>();
    private List<bool> recharging = new List<bool>();
    private EnergySource energySource;

    private enum Mode
    { 
        Standard,
        Shotgun,
        Minigun,
        Railgun,
        Grenade,
        Rocket,
        Heal
    }
    private Mode mode;
    private Mode prevMode;
    private List<Mode> quickSelect;

    [SerializeField] private List<Sprite> weaponSprites;
    [SerializeField] private Image currentWeaponDisplay;

    [Header("Shotgun")]
    [SerializeField] private List<int> shotgunAmt;
    [SerializeField] private float shotgunSpread;

    [Header("Minigun")]
    [SerializeField] private float minigunSpread;

    [Header("Heal")]
    [SerializeField] private float healPct;
    [SerializeField] private GameObject healParticles;

    private void Awake()
    {
        health = GetComponent<HealthManager>();
        playerAudio = GetComponent<AudioSource>();

        Cursor.SetCursor(reticle, new Vector2(), CursorMode.Auto);
        useTimer = useTime;
        energyBar.SetDefaults(maxEnergy);

        for (int i = 0; i < energyCost.Count; i++)
        {
            energyLeft.Add(maxEnergy);
            recharging.Add(false);
        }

        mode = (Mode)PlayerData.Instance.currentEquipped;
        quickSelect = new List<Mode>();
    }

    private void Update()
    {
        int i = (int)mode;

        if (GameStateManager.Instance.currentState != GameStateManager.GameState.PAUSED)
        {
            UpdateQuickSelect();

            Aim();
            Swap();
            useTimer -= Time.deltaTime;
            useTimeBar.SetValue(useTimer);

            // Clicking around menus does not trigger weapon fire
            if (FindObjectsOfType<TerminalMenu>().Length == 0)
            {
                if (Input.GetMouseButton(0) && !recharging[i] && useTimer <= 0)
                {
                    Shoot();
                    useTimer = useTime;
                    useTimeBar.SetDefaults(useTimer);
                }
            }

            Recharge();
            energyBar.SetValue(energyLeft[i]);
            energyBar.SetRecharge(recharging[i]);
        }        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        energySource = collision.GetComponent<EnergySource>() ?? energySource;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<EnergySource>()) energySource = null;
    }

    private void UpdateQuickSelect()
    {
        quickSelect.Clear();

        for (int i = 0; i < 6; i++)
        {
            if (PlayerData.Instance.equipped[i])
                quickSelect.Add((Mode)i);
        }

        if (mode != Mode.Heal && !quickSelect.Contains(mode))
        {
            mode = quickSelect[0];
            PlayerData.Instance.currentEquipped = (int)mode;
        }
    }

    private void Swap()
    {
        int mouseScrollDelta = (int)Input.mouseScrollDelta.y;

        // Quick swap to/from heal
        if (Input.GetMouseButtonDown(1))
        {
            if (mode != Mode.Heal)
            {
                prevMode = mode;
                mode = Mode.Heal;
                useTimer = 0;
            }
            else
            {
                mode = prevMode;
            }
            AudioController.Instance.PlayEffect(playerAudio, SoundEffect.WeaponSwap);
        }
        // Scroll through equipped weapons
        else if (mouseScrollDelta != 0 && !Input.GetMouseButton(0))
        {
            // Swap out of heal first
            if (mode == Mode.Heal) mode = prevMode;

            // Get next weapon
            int i = quickSelect.IndexOf(mode) - mouseScrollDelta;

            // Handle out of bounds
            if (i >= quickSelect.Count)
                i = 0;
            else if (i < 0)
                i = quickSelect.Count - 1;

            useTimer = 0;
            mode = quickSelect[i];            
            PlayerData.Instance.currentEquipped = (int)mode;
            AudioController.Instance.PlayEffect(playerAudio, SoundEffect.WeaponSwap);
        }

        currentWeaponDisplay.sprite = weaponSprites[(int)mode];
    }

    private void Aim()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z));
        Vector2 direction = (mousePos - rotator.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rotator.eulerAngles = Vector3.forward * angle;
        rotator.localScale = transform.localScale;
    }

    private void Shoot()
    {
        int m = (int)mode;
        projSpeed = projSpeeds[m];
        useTime = useTimes[m];
    
        switch (mode)
        {
            case Mode.Standard:
                StandardShoot();
                break;

            case Mode.Shotgun:
                ShotgunShoot();
                break;

            case Mode.Minigun:
                MinigunShoot();
                break;

            case Mode.Railgun:
                RailgunShoot();
                break;

            case Mode.Grenade:
                GrenadeShoot();
                break;

            case Mode.Rocket:
                RocketShoot();
                break;

            case Mode.Heal:
                HealShoot();
                break;
        }

        if (!PlayerData.Instance.godMode[2])
            energyLeft[m] -= energyCost[m];

        if (energyLeft[m] <= 0 && !recharging[m])
        {
            recharging[m] = true;
            energyLeft[m] = 0;
        }
    }

    private void Shoot(int projType, float dRot = 0)
    {
        Projectile proj = Instantiate(projectiles[projType], shootPoint.position, shootPoint.rotation).GetComponent<Projectile>();
        proj.SetDefaults(transform, dRot, projSpeed);
    }

    #region Shoot Types

    private void StandardShoot()
    {
        Shoot(0);
        AudioController.Instance.PlayEffect(playerAudio, SoundEffect.GunShot);
    }

    private void ShotgunShoot()
    {
        for (int i = 0; i < Random.Range(shotgunAmt[0], shotgunAmt[1]); i++)
        {
            Shoot(1, Random.Range(-shotgunSpread, shotgunSpread));
        }
        AudioController.Instance.PlayEffect(playerAudio, SoundEffect.ShotgunShot);
    }

    private void MinigunShoot()
    {
        Shoot(1, Random.Range(-minigunSpread, minigunSpread));
        AudioController.Instance.PlayEffect(playerAudio, SoundEffect.SilencedGunShot);
    }

    private void RailgunShoot()
    {
        RaycastHit2D hit = Physics2D.RaycastAll(transform.position, shootPoint.rotation * shootPoint.localPosition, Mathf.Infinity)
                                               .Where(hit => hit.collider.gameObject.layer == (int)Layer.Terrain)
                                               .OrderBy(hit => hit.distance)
                                               .FirstOrDefault();

        SolidBeam proj = Instantiate(projectiles[2]).GetComponent<SolidBeam>();
        proj.SetDefaults(transform.position, hit.point);
        AudioController.Instance.PlayEffect(playerAudio, SoundEffect.EnergyGunShot);
    }

    private void GrenadeShoot()
    {
        Shoot(3);
        AudioController.Instance.PlayEffect(playerAudio, SoundEffect.GrenadeLaunch);
    }

    private void RocketShoot()
    {
        Shoot(4);
        AudioController.Instance.PlayEffect(playerAudio, SoundEffect.RocketLaunch);
    }

    private void HealShoot()
    {
        health.Heal(0, healPct);
        Instantiate(healParticles, transform);
    }

    #endregion

    private void Recharge()
    {
        if (Input.GetKeyDown(KeyCode.R))
            recharging[(int)mode] = true;

        for (int i = 0; i < recharging.Count; i++)
        {
            float energySourceMult = energySource ? energySource.rechargeMult : 0;
            float rechargeMult = (i == (int)mode) ? 1 : 0.25f;

            if (recharging[i])
                energyLeft[i] += Time.deltaTime * (rechargeSpeed + energySourceMult) * rechargeMult;
            else
                energyLeft[i] += Time.deltaTime * energySourceMult * rechargeMult;

            if (energyLeft[i] >= maxEnergy)
            {
                energyLeft[i] = maxEnergy;
                recharging[i] = false;
            }
        }
    }

    /// <summary>
    /// Drain the specified amount of energy from the player.
    /// </summary>
    /// <param name="amt"></param>
    public void DrainEnergy(int amt, bool drainAll = false)
    {
        if (!PlayerData.Instance.godMode[2])
        {

            for (int i = 0; i < energyLeft.Count; ++i)
            {
                if (i == (int)mode || drainAll)
                {
                    energyLeft[i] -= amt;
                    if (energyLeft[i] <= 0)
                    {
                        recharging[i] = true;
                        energyLeft[i] = 0;
                    }
                }
            }
        }
    }
}
