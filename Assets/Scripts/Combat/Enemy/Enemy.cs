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

    protected GameObject Shoot(int projType, float projSpeed, float dRot = 0)
    {
        GameObject proj = Instantiate(projectiles[projType], shootPoint.position, shootPoint.rotation).gameObject;
        proj.transform.localEulerAngles = new Vector3(0, 0, proj.transform.localEulerAngles.z + dRot);

        proj.GetComponent<Projectile>().SetOrigin(transform);
        proj.GetComponent<Rigidbody2D>().velocity = proj.transform.right * projSpeed;

        return proj;
    }

}
