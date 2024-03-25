using AudioManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragProjectile : Projectile
{   
    [SerializeField] protected GameObject fragment;
    [SerializeField] private SoundEffect fragmentSfx;
    private int numFrags;
    private float fragSpeed;

    private const float fragOffset = 0.1f;

    protected override void Awake()
    {
        base.Awake();

        OnTerrainHit += Fragment;
        OnPlayerHit += Fragment;
    }

    public void SetDefaults(int numFrags, float fragSpeed)
    {
        this.numFrags = numFrags;
        this.fragSpeed = fragSpeed;
    }

    protected virtual void Fragment(GameObject obj)
    {
        if (targetTags.Contains(obj.tag))
        {
            AudioController.Instance.PlayEffect(projAudio, fragmentSfx);
            Fragment();
        }
    }

    protected virtual void Fragment()
    {
        for (int i = 0; i < numFrags; i++)
        {
            float dRot = 360 / numFrags * i;
            float angle = transform.localEulerAngles.z + dRot;

            GameObject proj = Instantiate(
                fragment,
                new Vector3(
                    transform.position.x + Mathf.Cos(Mathf.Deg2Rad * angle) * fragOffset,
                    transform.position.y + Mathf.Sin(Mathf.Deg2Rad * angle) * fragOffset,
                    transform.position.z
                ),
                transform.rotation
            ).gameObject;

            proj.transform.localEulerAngles = new Vector3(0, 0, angle);
            proj.GetComponent<Projectile>().SetOrigin(transform);
            proj.GetComponent<Rigidbody2D>().velocity = proj.transform.right * fragSpeed;
        }
    }
}
