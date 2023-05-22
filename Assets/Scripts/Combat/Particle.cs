using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    protected Animator anim;

    [SerializeField] protected bool destroyAfterTime;
    [SerializeField] protected float lifeTime;

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
    }

    protected virtual void Update()
    {
        lifeTime -= Time.deltaTime;
        if (destroyAfterTime && lifeTime <= 0) Destroy(gameObject);
    }

    protected virtual void DestroyEffect()
    {
        Destroy(gameObject);
    }
}
