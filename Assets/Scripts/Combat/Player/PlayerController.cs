using AudioManager;
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
    private AudioSource playerAudio;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [HideInInspector] public float moveSpeedMult = 1;
    private int direction;

    [SerializeField] private SoundEffect jumpSfx;
    [SerializeField] private float fallMultiplier;
    [HideInInspector] public float fallMultiplierBoost = 1;
    [SerializeField] private float terminalVelocity;
    [HideInInspector] public float terminalVelocityMult = 1;
    [SerializeField] private float jumpHeight;
    [HideInInspector] public float jumpHeightMult = 1;
    [SerializeField] private int coyoteTime;
    private bool coyoteWalking;
    public bool canJumpMidair { get; private set; }

    [SerializeField] private float slidingSpeed;
    public bool sliding { get; private set; }

    [SerializeField] private float wallJumpHeight;
    [SerializeField] private float wallJumpWidth;
    [HideInInspector] public float wallJumpMult = 1;
    [SerializeField] private float wallJumpTime;
    public bool wallJumping { get; private set; }

    [SerializeField] private SoundEffect dashSfx;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashLength;
    [HideInInspector] public float dashSpeedMult = 1;
    private float dashTimer;
    [SerializeField] private float dashCd;
    [SerializeField] private bool dashImmunity;
    public bool canDash { get; private set; }
    public bool dashing { get; private set; }

    [Header("Ground & Wall Detection")]
    [SerializeField] private List<LayerMask> isTerrain;
    [SerializeField] private float groundRadius;
    [SerializeField] private float wallRadius;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private List<Transform> wallChecks;
    [SerializeField] private bool tiltWithGround;
    [SerializeField] private float maxGroundTilt;
    public bool onGround { get; private set; }
    public bool onWall { get; private set; }

    [Header("Combat")]
    [SerializeField] private SpriteRenderer stunIndicator;
    [SerializeField] private float stunTime;
    private bool stunned;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<HealthManager>();
        playerAudio = GetComponent<AudioSource>();
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
        if (rb.velocity.y < 0 && rb.velocity.y >= -terminalVelocity * terminalVelocityMult)
            rb.velocity += (fallMultiplier * fallMultiplierBoost - 1) * Physics2D.gravity.y * Time.fixedDeltaTime * Vector2.up;
    }

    #region Detectors

    /// <summary>
    /// Checks if the ground detector is in contact with non-vertical terrain.
    /// </summary>
    private bool CheckPlayerOnGround()
    {
        Collider2D[] cldrs = (from layer in isTerrain select Physics2D.OverlapCircleAll(groundCheck.position, groundRadius, layer)).SelectMany(x => x).ToArray();
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

        return angles.Count() > 0 && rb.velocity.y == 0;
    }

    /// <summary>
    /// Checks if any wall detector is in contact horizontal terrain.
    /// </summary>
    private bool CheckPlayerOnWall()
    {
        foreach (Transform wallCheck in wallChecks)
        {
            Collider2D[] cldrs = (from layer in isTerrain select Physics2D.OverlapCircleAll(wallCheck.position, wallRadius, layer)).SelectMany(x => x).ToArray();

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

            SetHorizontalVelocity(dir, moveSpeed * moveSpeedMult, new Vector2(dir * moveSpeed * moveSpeedMult, vY));            
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
                AudioController.Instance.PlayEffect(playerAudio, jumpSfx);
                rb.velocity = new Vector2(-direction * wallJumpWidth * moveSpeed * moveSpeedMult, wallJumpHeight * wallJumpMult);
                FlipX();
                StartCoroutine(WallJumpTimer());
            }
            // Normal jump
            else if (onGround || canJumpMidair)
            {
                AudioController.Instance.PlayEffect(playerAudio, jumpSfx);
                rb.velocity = new Vector2(rb.velocity.x, jumpHeight * jumpHeightMult);
                if (!onGround) canJumpMidair = false;
            }

            dashTimer = 0;
            CheckCoyoteTime();
        }

        // Hold jump
        if (Input.GetKey(KeyCode.Space) && onGround)
        {
            AudioController.Instance.PlayEffect(playerAudio, jumpSfx);
            rb.velocity = new Vector2(rb.velocity.x, jumpHeight * jumpHeightMult);
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

                AudioController.Instance.PlayEffect(playerAudio, dashSfx);
                SetHorizontalVelocity(direction, dashSpeed * dashSpeedMult, dashSpeed * dashSpeedMult * direction * Vector2.right);
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
    /// Start the knockback coroutine on the player object
    /// so that it isn't affected by the origin object's state
    /// </summary>
    public void KnockbackAsync(Vector2 kb)
    {
        StartCoroutine(Knockback(kb));
    }

    /// <summary>
    /// Knockback the player by the specified force.
    /// Player is stunned during knockback to prevent momentum cancel.
    /// </summary>
    private IEnumerator Knockback(Vector2 kb)
    {
        stunIndicator.enabled = stunned = true;
        rb.AddForce(kb);

        yield return new WaitForSeconds(stunTime);

        stunIndicator.enabled = stunned = false;
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