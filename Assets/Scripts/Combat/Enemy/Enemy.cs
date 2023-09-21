using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    protected GameObject player;

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

    /// <summary>
    /// Checks if the projectile has a clear path to the target.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="proj"></param>
    /// <returns></returns>
    protected bool IsClearAim(Vector3 target, GameObject proj)
    {
        RaycastHit2D hit;
        BoxCollider2D box = proj.GetComponent<BoxCollider2D>();

        if (box)
        {
            hit = Physics2D.BoxCastAll(
            shootPoint.position,
            box.size * proj.transform.localScale,
            shootPoint.rotation.z,
            shootPoint.rotation * shootPoint.localPosition,
            Mathf.Infinity).Where(hit => hit.collider.gameObject != gameObject && hit.collider.gameObject.layer == 6)
                           .OrderBy(hit => hit.distance)
                           .FirstOrDefault();
        }
        else
        {
            CircleCollider2D circle = proj.GetComponent<CircleCollider2D>();
            CapsuleCollider2D capsule = proj.GetComponent<CapsuleCollider2D>();
            Vector2 size = circle ? Vector2.one * circle.radius : capsule.size;

            hit = Physics2D.CapsuleCastAll(
            shootPoint.position,
            size * proj.transform.localScale,
            CapsuleDirection2D.Vertical,
            shootPoint.rotation.z,
            shootPoint.rotation * shootPoint.localPosition,
            Mathf.Infinity).Where(hit => hit.collider.gameObject != gameObject && hit.collider.gameObject.layer == 6)
                           .OrderBy(hit => hit.distance)
                           .FirstOrDefault();
        }      

        return hit.distance > (shootPoint.position - target).magnitude;
    }

    protected Projectile Shoot(int projType, float projSpeed, float dRot = 0)
    {
        Projectile proj = Instantiate(projectiles[projType], shootPoint.position, shootPoint.rotation).GetComponent<Projectile>();
        proj.SetDefaults(transform, dRot, projSpeed);
        return proj;
    }
}
