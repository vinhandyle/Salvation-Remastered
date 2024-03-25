using AudioManager;
using LayerManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : Enemy
{
    private Rigidbody2D rb;

    [Header("Drone")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float orbitSpeed;
    [SerializeField] private float minOrbitDistance;
    [SerializeField] private float maxOrbitDistance;

    [Header("Drone Attack")]
    [SerializeField] private SoundEffect attackSfx;
    [SerializeField] private int bulletNum;
    [SerializeField] private float fireRate;
    [SerializeField] private float fireSpeed;
    [SerializeField] private float restTime;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();

        // Death handler
        HealthManager hm = GetComponent<HealthManager>();
        hm.OnDying += () =>
        {
            rb.velocity = Vector2.zero;
            PlayDeathSoundEffect();
            Destroy(this);
        };

        StartCoroutine(Attack());
    }

    private void Update()
    {
        Aim(player.transform.position);
        transform.eulerAngles = rotator.eulerAngles;

        if ((player.transform.position - transform.position).magnitude > maxOrbitDistance)
        {
            rb.velocity = moveSpeed * rotator.right;
        }
        else if ((player.transform.position - transform.position).magnitude < minOrbitDistance)
        {
            rb.velocity = -moveSpeed * rotator.right;
        }
        else
        {            
            if (!IsObstructedFromPlayer())
                orbitSpeed *= -1;
            rb.velocity = Vector2.zero;
            transform.RotateAround(player.transform.position, Vector3.forward, orbitSpeed * Time.deltaTime);
        }
    }

    private IEnumerator Attack()
    {
        yield return new WaitForSeconds(restTime);

        for (int i = 0; i < bulletNum; ++i)
        {
            Shoot(0, fireSpeed);
            PlaySoundEffect(attackSfx);
            yield return new WaitForSeconds(fireRate);
        }
       
        StartCoroutine(Attack());
    }
}
