using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private GameObject player;

    [Header("Level")]
    [SerializeField] protected CinemachineVirtualCamera vcam;
    [SerializeField] protected LevelTimer timer;

    [Header("Targeting")]
    [SerializeField] protected Transform rotator;
    [SerializeField] protected Transform shootPoint;
    [SerializeField] protected List<GameObject> projectiles;

    [Header("Debugging")]
    [SerializeField] protected bool debug;
    [SerializeField] protected float debugTime;

    protected virtual void Awake()
    {
        player = GameObject.Find("Player");
    }

    protected virtual void Update()
    {
        Aim(player.transform.position);
    }

    protected void Aim(Vector3 target)
    {
        Vector2 direction = (target - rotator.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rotator.eulerAngles = Vector3.forward * angle;
    }

    protected Projectile Shoot(int projType, float projSpeed, float dRot = 0)
    {
        Projectile proj = Instantiate(projectiles[projType], shootPoint.position, shootPoint.rotation).GetComponent<Projectile>();
        proj.SetDefaults(transform, dRot, projSpeed);
        return proj;
    }
}
