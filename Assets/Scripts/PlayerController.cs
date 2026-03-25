using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private TrailRenderer tr;
    private float horizontal;
    private bool isFacingRight = true;
    private bool isWallSliding;
    private float wallSlidingSpeed = 2f;

    private bool isWallJumping;
    private float wallJumpingDirection = 0.4f;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(4f, 7f);    
    private bool CanDash = true;
    private bool IsDashing = false;

    private Vector2 moveInput;
    private float lastFacingDirection = 1f;

    [Header("Timeline Switching")]
    public GameObject timelineA;
    public GameObject timelineB;

    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashingTime = 0.3f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private float groundDeceleration = 20f;
    [SerializeField] private float wallSlideSpeed = 2f;
    
    [SerializeField] Transform wallCheck;
    [SerializeField] LayerMask wallLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;


    void Start()
    {
        rb = GetComponentInChildren<Rigidbody2D>();
        tr = GetComponent<TrailRenderer>();

        // Start with timeline A active
        if (timelineA != null) timelineA.SetActive(true);
        if (timelineB != null) timelineB.SetActive(false);
    }

    void Update()
    {
        horizontal = moveInput.x;

        if (!isWallJumping)
        {
            Flip();
        }
    }

    void FixedUpdate()
    {
        if (IsDashing || isWallJumping)
            return;

        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
            lastFacingDirection = Mathf.Sign(moveInput.x);
        }
        else if (IsGrounded())
        {
            float newX = Mathf.MoveTowards(
                rb.linearVelocity.x,
                0f,
                groundDeceleration * Time.fixedDeltaTime
            );

            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
        }

        if (IsWalled())
        {
            WallSlide();
        }
    }

    // ================= INPUT SYSTEM =================

    private void OnMove(InputValue inputValue)
    {
        moveInput = inputValue.Get<Vector2>();
    }

    private void OnJump()
    {
        if (IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpHeight);
        }
        else
        {
            StartCoroutine(WallJump());
        }
    }

    private void OnDash(InputValue inputValue)
    {
        if (CanDash)
        {
            Vector2 dashDirection;

            if (moveInput.x != 0)
                dashDirection = new Vector2(Mathf.Sign(moveInput.x), 0f);
            else
                dashDirection = new Vector2(lastFacingDirection, 0f);

            StartCoroutine(Dash(dashDirection));
        }
    }

    // Timeline switch using Input System
    private void OnSwitchTimeline()
    {
        if (TimelineManager.Instance != null)
        {
            TimelineManager.Instance.SwitchTimeline();
        }
    }

    // ================= WALL =================

    private void WallSlide()
    {
        if (!IsGrounded() && horizontal != 0f)
        {
            isWallSliding = true;

            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue)
            );
        }
        else
        {
            isWallSliding = false;
        }
    }

    private IEnumerator WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = true;

            wallJumpingDirection = -transform.localScale.x;

            rb.linearVelocity =
                new Vector2(
                    wallJumpingDirection * wallJumpingPower.x,
                    wallJumpingPower.y
                );

            if (transform.localScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;

                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }

            yield return new WaitForSeconds(wallJumpingDuration);

            StopWallJumping();
            isWallSliding = false;
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    // ================= CHECKS =================

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(
            groundCheck.position,
            0.2f,
            groundLayer
        );
    }

    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(
            wallCheck.position,
            0.2f,
            wallLayer
        );
    }

    // ================= DASH =================

    private IEnumerator Dash(Vector2 input)
    {
        IsDashing = true;
        CanDash = false;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        rb.linearVelocity = new Vector2(input.x * dashSpeed, 0f);

        tr.emitting = true;

        yield return new WaitForSeconds(dashingTime);

        tr.emitting = false;

        rb.gravityScale = originalGravity;

        IsDashing = false;

        if (IsGrounded())
        {
            rb.linearVelocity =
                new Vector2(0f, rb.linearVelocity.y);
        }

        yield return new WaitForSeconds(dashCooldown);

        CanDash = true;
    }

    // ================= FLIP =================

    private void Flip()
    {
        if (
            isFacingRight && horizontal < 0f ||
            !isFacingRight && horizontal > 0f
        )
        {
            isFacingRight = !isFacingRight;

            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
}