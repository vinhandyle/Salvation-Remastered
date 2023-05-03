using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    private Rigidbody2D rb;

    private Vector3 initPos;
    private Vector3 endPos;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        initPos = transform.position;
    }

    protected virtual void Update()
    { 
        // Check if object overshot end position
        if (Vector3.Dot(endPos - transform.position, rb.velocity) < 0)
        {
            rb.velocity = Vector2.zero;
            transform.position = endPos;
        }
    }

    /// <summary>
    /// Move towards the target at the specified speed.
    /// </summary>
    public void Move(float speed, Vector3 endPos)
    {
        this.endPos = endPos;
        rb.velocity = speed * (endPos - transform.position).normalized;
    }

    /// <summary>
    /// Move towards the starting point at the specified speed.
    /// </summary>
    public void Return(float speed)
    {
        endPos = initPos;
        rb.velocity = speed * (initPos - transform.position).normalized;
    }
}
