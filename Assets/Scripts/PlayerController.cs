using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Misc
    private Rigidbody2D rb;
    private TrailRenderer tr;
    private float horizontal;
    private bool isFacingRight = true;
    private PlayerInputSystem controls;

    // WallSliding
    private bool isWallSliding;
    private float wallSlidingSpeed = 2f;
    // WallJumping
    private bool isWallJumping;
    private float wallJumpingDirection = 0.4f;
    private float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(7f, 9f); 

    // dash   
    private bool CanDash = true;
    private bool IsDashing = false;
    // jump
    private bool isJumping;
    private bool jumpHeld;
    private float jumpHoldCounter;
    private Vector2 moveInput;
    private float lastFacingDirection = 1f;

    [Header("Timeline Switching")]
    public GameObject timelineA;
    public GameObject timelineB;

    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashingTime = 0.3f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float groundAcceleration = 60f;
    [SerializeField] private float airAcceleration = 20f;
    [SerializeField] private float groundDeceleration = 20f;
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float tapJumpHeight = 5f;
    [SerializeField] private float holdJumpForce = 10f;
    [SerializeField] private float maxHoldTime = 0.2f;

    // gravity
    [SerializeField] private float fallGravityMultiplier = 2.5f;
    [SerializeField] private float lowJumpGravityMultiplier = 2f;
    
    [SerializeField] Transform wallCheck;
    [SerializeField] LayerMask wallLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private GameObject interactTextPrefab;
    private IInteractable currentInteractable;
    private Collider2D currentCollider;
    private GameObject currentTextInstance;

    private void Awake()
    {
        controls = new PlayerInputSystem();
    }
    private void OnEnable()
    {
        controls.Player.Enable();

        controls.Player.Move.performed += OnMove;
        controls.Player.Move.canceled += OnMove;

        controls.Player.Jump.started += OnJump;
        controls.Player.Jump.canceled += OnJump;

        controls.Player.Dash.started += OnDash;

        controls.Player.SwitchTimeline.started += OnSwitchTimeline;

        controls.Player.Interact.started += OnInteract;
    }
    private void OnDisable()
    {
        controls.Player.Move.performed -= OnMove;
        controls.Player.Move.canceled -= OnMove;

        controls.Player.Jump.started -= OnJump;
        controls.Player.Jump.canceled -= OnJump;

        controls.Player.Dash.started -= OnDash;

        controls.Player.SwitchTimeline.started -= OnSwitchTimeline;

        controls.Player.Disable();
    }
    void Start()
    {
        rb = GetComponentInChildren<Rigidbody2D>();
        tr = GetComponent<TrailRenderer>();

        if (timelineA != null) timelineA.SetActive(true);
        if (timelineB != null) timelineB.SetActive(false);
    }

    void Update()
    {
        horizontal = moveInput.x;

        CheckForInteractable();

        if (!isWallJumping)
        {
            Flip();
        }
if (isJumping && jumpHeld)
    {
        if (jumpHoldCounter > 0)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y + holdJumpForce * Time.deltaTime
            );

            jumpHoldCounter -= Time.deltaTime;
        }
        else
        {
            isJumping = false;
        }
    }

        if (!isWallJumping)
        {
            if (rb.linearVelocity.y < 0)
            {
                rb.linearVelocity += Vector2.up *
                    Physics2D.gravity.y *
                    (fallGravityMultiplier - 1) *
                    Time.deltaTime;
            }
            else if (rb.linearVelocity.y > 0 && !jumpHeld)
            {
                rb.linearVelocity += Vector2.up *
                    Physics2D.gravity.y *
                    (lowJumpGravityMultiplier - 1) *
                    Time.deltaTime;
            }
        }
    }

    void FixedUpdate()
    {
        if (IsDashing)
            return;

        float targetSpeed = moveInput.x * moveSpeed;

        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            float accelRate = IsGrounded() ? groundAcceleration : airAcceleration;

            float newX = Mathf.MoveTowards(
                rb.linearVelocity.x,
                targetSpeed,
                accelRate * Time.fixedDeltaTime
            );

            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
            lastFacingDirection = Mathf.Sign(moveInput.x);
        }
        else
        {
            float decelRate = IsGrounded() ? groundDeceleration : airAcceleration * 0.5f;

            float newX = Mathf.MoveTowards(
                rb.linearVelocity.x,
                0f,
                decelRate * Time.fixedDeltaTime
            );

            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
        }

        if (IsWalled())
        {
            WallSlide();
        }
    }

    // ================= INPUT SYSTEM =================

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (IsGrounded())
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, tapJumpHeight);

                isJumping = true;
                jumpHeld = true;
                jumpHoldCounter = maxHoldTime;
            }
            else
            {
                StartCoroutine(WallJump());
            }
        }

        if (context.canceled)
        {
            jumpHeld = false;
            isJumping = false;
        }

    }

    private void OnDash(InputAction.CallbackContext context)
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
    private void OnSwitchTimeline(InputAction.CallbackContext context)
    {
        if (TimelineManager.Instance != null)
        {
            TimelineManager.Instance.SwitchTimeline();
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log("Interact Pressed");
        TryInteract();
    }
    private void TryInteract()
    {
        currentInteractable?.Interact();
    }

    private void CheckForInteractable()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange, interactLayer);

        Collider2D hit = null;
        float closestDist = Mathf.Infinity;

        foreach (var h in hits)
        {
            float dist = Vector2.Distance(transform.position, h.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                hit = h;
            }
        }

        if (hit != currentCollider)
        {
            // Remove old highlight
            if (currentCollider != null)
            {
                currentCollider.GetComponent<InteractableVisual>()?.SetHighlight(false);
            }

            currentCollider = hit;

            if (hit != null)
            {
                currentInteractable = hit.GetComponent<IInteractable>();
                hit.GetComponent<InteractableVisual>()?.SetHighlight(true);
            }
            else
            {
                currentInteractable = null;            }
        }
    }

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