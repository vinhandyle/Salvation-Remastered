using AudioManager;
using LayerManager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncingObject : MonoBehaviour
{
    private Rigidbody2D rb;
    private AudioSource audioSource;
    [SerializeField] private SoundEffect soundEffect;

    [Header("Bounce Info")]
    [SerializeField] private bool staticBounce;
    [SerializeField] private float numBounces;
    [SerializeField] private float bounceDamp;
    [SerializeField] private float bounceOffset;

    public event Action OnBounce;
    public event Action OnBounceFinish;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == (int)Layer.Terrain)
        {
            if (Mathf.Abs(rb.velocity.x) < 0.25)
                rb.velocity = new Vector2(0, rb.velocity.y);
            if (Mathf.Abs(rb.velocity.y) < 0.25)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
                rb.gravityScale = 0;
            }
        }
    }

    public void Bounce(GameObject obj)
    {
        if (numBounces != 0)
        {
            OnBounce?.Invoke();
            numBounces--;
            AudioController.Instance.PlayEffect(audioSource, soundEffect);

            float angle = Vector2.SignedAngle(Vector3.right, rb.velocity.normalized);
            float rflAngle = obj.transform.rotation.eulerAngles.z;
            Quaternion newRot = Quaternion.Euler(0, 0, rflAngle * 2 - angle - bounceOffset);

            if (!staticBounce) transform.rotation = newRot;
            rb.velocity = (1 - bounceDamp) * rb.velocity.magnitude * (newRot * Vector3.right);
        }
        else
        {
            OnBounceFinish?.Invoke();
        }
    }
}
