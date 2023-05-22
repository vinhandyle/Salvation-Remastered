using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolidBeam : Projectile
{
    protected override void Awake()
    {
        gameObject.SetActive(false); // Disable until after setup
    }

    public void SetDefaults(Vector2 start, Vector2 end)
    {
        transform.position = (start + end) / 2;
        transform.right = end - start;
        transform.localScale = new Vector3((end - start).magnitude, transform.localScale.y, transform.localScale.z);
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Called during animation
    /// </summary>
    private void DisableHitBox()
    {
        cldr.enabled = false;
    }
}
