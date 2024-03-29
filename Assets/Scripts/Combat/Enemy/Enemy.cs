using AudioManager;
using Cinemachine;
using LayerManager;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    protected GameObject player;
    [SerializeField] protected AudioSource enemyAudio;
    [SerializeField] protected SoundEffect deathSfx;

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

    #region Audio

    protected void PlaySoundEffect(SoundEffect effect, bool loop = false)
    {
        AudioController.Instance.PlayEffect(enemyAudio, effect, loop);
    }

    protected void PlayDeathSoundEffect()
    {
        PlaySoundEffect(deathSfx);
    }

    protected void ClearSoundEffects()
    {
        AudioController.Instance.ClearEffects(enemyAudio);
    }

    #endregion

    #region Shooter

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
            Mathf.Infinity).Where(hit => hit.collider.gameObject != gameObject && hit.collider.gameObject.layer == (int)Layer.Terrain)
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
            Mathf.Infinity).Where(hit => hit.collider.gameObject != gameObject && hit.collider.gameObject.layer == (int)Layer.Terrain)
                           .OrderBy(hit => hit.distance)
                           .FirstOrDefault();
        }

        return hit.distance > (shootPoint.position - target).magnitude;
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

    #endregion

    protected bool IsObstructedFromPlayer()
    {
        Vector2 dir = player.transform.position - transform.position;
        RaycastHit2D hit = Physics2D.RaycastAll(transform.position, dir)
                                    .Where(ray => ray.transform.gameObject.layer == (int)Layer.Terrain)
                                    .First();

        return hit.distance > dir.magnitude;
    }

    protected float DistanceFromArenaBox(Vector2 origin, Vector2 dir)
    {
        RaycastHit2D hit = Physics2D.RaycastAll(origin, dir)
                                    .Where(ray => ray.transform.CompareTag("Arena Box"))
                                    .First();

        return hit.distance;
    }

    protected Vector3 SelectRandomPointFromBounds(List<BoxCollider2D> bounds)
    {
        BoxCollider2D box = bounds[Random.Range(0, bounds.Count)];
        Vector3 extents = box.size / 2f;
        return box.transform.position + new Vector3(
            Random.Range(-extents.x, extents.x),
            Random.Range(-extents.y, extents.y),
            Random.Range(-extents.z, extents.z)
        );
    }
}
