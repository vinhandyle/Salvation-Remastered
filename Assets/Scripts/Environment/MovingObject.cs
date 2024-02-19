using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    private Rigidbody2D rb;

    private Vector3 initPos;
    private Vector3 endPos;
    private float speed;
    public bool stopped { get; protected set; }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        initPos = transform.position;
        stopped = true;
    }

    protected virtual void Update()
    { 
        // Check if object overshot end position
        if (Vector3.Dot(endPos - transform.position, rb.velocity) < 0)
        {
            rb.velocity = Vector2.zero;
            transform.position = endPos;
            stopped = true;
        }
    }

    /// <summary>
    /// Move towards the target at the specified speed.
    /// </summary>
    public void Move(float speed, Vector3 endPos)
    {
        stopped = false;
        this.endPos = endPos;
        this.speed = speed;
        rb.velocity = speed * (endPos - transform.position).normalized;
    }

    /// <summary>
    /// Move towards the starting point at the specified speed.
    /// </summary>
    public void Return(float speed)
    {
        stopped = false;
        endPos = initPos;
        rb.velocity = speed * (initPos - transform.position).normalized;
    }

    /// <summary>
    /// Stop the object mid-travel.
    /// </summary>
    public void Stop()
    {
        stopped = true;
        rb.velocity = Vector2.zero;
    }

    /// <summary>
    /// Resume travel to the most recent endpoint.
    /// </summary>
    public void Resume()
    {
        Move(speed, endPos);
    }    
}
