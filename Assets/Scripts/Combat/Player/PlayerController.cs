using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the player's movement.
/// </summary>
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private HealthManager health;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    private int direction;

    [SerializeField] private float fallMultiplier;
    [SerializeField] private float terminalVelocity;
    [SerializeField] private float jumpHeight;
    [SerializeField] private int coyoteTime;
    private bool coyoteWalking;
    private bool canJumpMidair;

    [SerializeField] private float slidingSpeed;
    private bool sliding;

    [SerializeField] private float wallJumpHeight;
    [SerializeField] private float wallJumpWidth;
    [SerializeField] private float wallJumpTime;
    private bool wallJumping;

    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashLength;
    private float dashTimer;
    [SerializeField] private float dashCd;
    [SerializeField] private bool dashImmunity;
    private bool canDash;
    private bool dashing;

    [Header("Ground & Wall Detection")]
    [SerializeField] private LayerMask isTerrain;
    [SerializeField] private float groundRadius;
    [SerializeField] private float wallRadius;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private List<Transform> wallChecks;
    [SerializeField] private bool tiltWithGround;
    [SerializeField] private float maxGroundTilt;
    private bool onGround;
    private bool onWall;

    [Header("Combat")]
    [SerializeField] private float stunTime;
    private bool stunned;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<HealthManager>();
        direction = (int)transform.right.x;
        canDash = true;
    }

    private void Update()
    {
        // Hot key for pause menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameStateManager.Instance.TogglePause();
        }

        // Player control
        canJumpMidair = PlayerData.Instance.canDoubleJump && (onGround || sliding || canJumpMidair);
        sliding = PlayerData.Instance.canWallJump && (onWall && !onGround && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)));

        if (sliding)
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -slidingSpeed, float.MaxValue));

        if (!stunned)
        {
            if (!wallJumping && !dashing)
            {
                Move();
            }
            Dash();
            Jump();
        }
    }

    private void FixedUpdate()
    {        
        bool _onGround = CheckPlayerOnGround();

        // If the player is grounded again, clear coyote time
        if (onGround && _onGround)
        {
            StopCoroutine("CoyoteTime");
            coyoteWalking = false;
        }
        else if (!coyoteWalking)
        {
            // If the player leaves the ground without jumping or dashing, allow them to jump within some frames
            if (onGround && !_onGround && !dashing && rb.velocity.y <= 0)
            {
                StartCoroutine("CoyoteTime");
            }
            // Normal behavior if not coyote walking
            else
            {
                onGround = _onGround;
            }
        }

        onWall = CheckPlayerOnWall();        

        // Fast fall
        if (rb.velocity.y < 0 && rb.velocity.y >= -terminalVelocity)
            rb.velocity += (fallMultiplier - 1) * Physics2D.gravity.y * Time.fixedDeltaTime * Vector2.up;
    }

    #region Detectors

    /// <summary>
    /// Checks if the ground detector is in contact with non-vertical terrain.
    /// </summary>
    private bool CheckPlayerOnGround()
    {
        Collider2D[] cldrs = Physics2D.OverlapCircleAll(groundCheck.position, groundRadius, isTerrain);
        List<float> angles = new List<float>();

        // Check steepness of colliders relative to player
        if (cldrs.Length > 0)
        {
            foreach (Collider2D cldr in cldrs)
            {
                angles.Add(AdjustAngle(cldr.transform.eulerAngles.z));
            }
        }

        angles = angles.Where(a => Mathf.Abs(a) < maxGroundTilt).ToList();

        // Update player tilt based on # of terrain colliders in contact
        if (tiltWithGround)
        {
            switch (angles.Count())
            {
                case 0:
                    transform.eulerAngles = Vector3.zero;
                    break;

                case 1:
                    transform.eulerAngles = new Vector3(0, 0, angles[0]);
                    break;
            }
        }

        return angles.Count() > 0;
    }

    /// <summary>
    /// Checks if any wall detector is in contact horizontal terrain.
    /// </summary>
    private bool CheckPlayerOnWall()
    {
        foreach (Transform wallCheck in wallChecks)
        {
            Collider2D[] cldrs = Physics2D.OverlapCircleAll(wallCheck.position, wallRadius, isTerrain);

            foreach (Collider2D cldr in cldrs)
            {
                // Check steepness of collider relative to player
                if (Mathf.Abs(AdjustAngle(cldr.transform.eulerAngles.z)) > 0)
                    return true;                
            }
        }

        return false;
    }

    /// <summary>
    /// Fit angle to be with 180 to -180 degree range.
    /// </summary>
    private float AdjustAngle(float a)
    {
        if (a >= 270)
        {
            a -= 360;
        }
        return a;
    }

    private IEnumerator CoyoteTime()
    {
        int _coyoteTime = coyoteTime;
        coyoteWalking = true;

        // Count frames
        while (_coyoteTime > 0)
        {
            yield return new WaitForEndOfFrame();
            _coyoteTime--;
        }

        onGround = false;
        coyoteWalking = false;
    }

    #endregion

    #region Movement

    private void Move()
    {
        int dir = 0;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            dir = -1;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            dir = 1;
        }

        // Prevent getting stuck on walls
        if (dir == direction && onWall && !onGround)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        else
        {
            if (direction == -dir) FlipX();

            // Remove momentum when stopping on slants
            float vY = rb.velocity.y;
            if (dir == 0 && onGround)
                vY = 0;

            SetHorizontalVelocity(dir, moveSpeed, new Vector2(dir * moveSpeed, vY));            
        }
    }

    private void Jump()
    {       
        // Tap jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Wall jump
            if (sliding)
            {
                rb.velocity = new Vector2(-direction * wallJumpWidth * moveSpeed, wallJumpHeight);
                FlipX();
                StartCoroutine(WallJumpTimer());
            }
            // Normal jump
            else if (onGround || canJumpMidair)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpHeight);
                if (!onGround) canJumpMidair = false;
            }

            dashTimer = 0;
            CheckCoyoteTime();
        }

        // Hold jump
        if (Input.GetKey(KeyCode.Space) && onGround)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpHeight);
            CheckCoyoteTime();
        }

        // Stop jump short if button is released before apex
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (rb.velocity.y > 0)
                rb.velocity = new Vector2(rb.velocity.x, 0);
        }
    }

    private void Dash()
    {
        // Start dash
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {           
            if (PlayerData.Instance.canDash && canDash)
            {
                if (dashImmunity) health.SetImmunity(true);

                SetHorizontalVelocity(direction, dashSpeed, dashSpeed * direction * Vector2.right);
                dashTimer = dashLength;
                dashing = true;               
                StartCoroutine(DashCooldown());
                CheckCoyoteTime();
            }
        }

        // When the dash is over, reset player velocity to prevent excess movement
        if (dashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0)
            {
                if (dashImmunity) health.SetImmunity(false);

                rb.velocity = Vector2.zero;
                dashing = false;
            }
        }
    }

    /// <summary>
    /// Prevent moving out of wall jump for pre-specified time.
    /// </summary>
    private IEnumerator WallJumpTimer()
    {
        wallJumping = true;
        yield return new WaitForSeconds(wallJumpTime);
        wallJumping = false;
    }

    /// <summary>
    /// Start cooldown for dash.
    /// </summary>
    private IEnumerator DashCooldown()
    {
        canDash = false;
        yield return new WaitForSeconds(dashCd);
        canDash = true;
    }

    #endregion

    #region External

    /// <summary>
    /// Knockback the player by the specified force.
    /// Player is stunned during knockback to prevent momentum cancel.
    /// </summary>
    public IEnumerator Knockback(Vector2 kb)
    {
        stunned = true;
        rb.AddForce(kb);

        yield return new WaitForSeconds(stunTime);

        stunned = false;
    }

    #endregion

    #region Helper

    /// <summary>
    /// Flip the player horizontally.
    /// </summary>
    private void FlipX()
    {
        Vector3 oppDirection = transform.localScale;
        oppDirection.x *= -1;
        transform.localScale = oppDirection;
        direction *= -1;
    }

    /// <summary>
    /// Given direction and speed, set the player's velocity.
    /// If tilt is disabled, adjusts for slanted floors.
    /// If on ground or tilt is disabled, use given velocity.
    /// </summary>
    private void SetHorizontalVelocity(int direction, float speed, Vector2 defVelocity)
    {
        if (tiltWithGround)
        {
            // Special calc to move across slant
            float dX = transform.right.x;
            float dY = transform.right.y;
            float rot = transform.eulerAngles.z;

            // Flip vector if going down slant
            if (rot * direction < 0) dY *= -1;

            if (onGround)
                rb.velocity = new Vector2(direction * dX, Mathf.Abs(direction) * dY) * speed;
            else
                rb.velocity = defVelocity;
        }
        else
        {
            rb.velocity = defVelocity;
        }
    }

    /// <summary>
    /// Remove coyote time if it is on.
    /// </summary>
    private void CheckCoyoteTime()
    {
        if (coyoteWalking)
        {
            StopCoroutine("CoyoteTime");
            coyoteWalking = false;
            onGround = false;
        }
    }

    #endregion
}