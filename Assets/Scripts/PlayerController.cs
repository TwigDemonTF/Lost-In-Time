using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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

    [SerializeField] private TimelineUIController timelineUI;
    private bool isSwitchingTimeline = false;

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

    // AUDIO
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource musicSource;

    [SerializeField] private AudioClip overworldMusic;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip wallJumpSound;
    [SerializeField] private AudioClip dashSound;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;

    [Range(0f, 1f)] public float jumpVolume = 0.6f;
    [Range(0f, 1f)] public float wallJumpVolume = 0.8f;
    [Range(0f, 1f)] public float dashVolume = 0.7f;

    [Range(0f, 1f)] public float footstepVolume = 0.5f;
    [Range(0f, 1f)] public float musicVolume = 0.7f;

    // Walking sounds (per surface)
    [SerializeField] private AudioClip grassStepSound;
    [SerializeField] private AudioClip stoneStepSound;
    [SerializeField] private UnityEngine.Tilemaps.Tilemap grassTilemap;
    [SerializeField] private UnityEngine.Tilemaps.Tilemap stoneTilemap;

    [SerializeField] private float stepInterval = 0.4f;
    private float stepTimer;
    
    [SerializeField] Transform wallCheck;
    [SerializeField] LayerMask wallLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private GameObject interactTextPrefab;
    private SimpleInteract currentInteractable;
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

        musicSource.volume = musicVolume * masterVolume;
        if (musicSource != null && overworldMusic != null)
        {
            musicSource.clip = overworldMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Update()
    {
        HandleFootsteps();
        if (Keyboard.current.f5Key.wasPressedThisFrame)
        {
            ReloadScene();
        }

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

                PlaySound(jumpSound, jumpVolume);

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
        if (!isSwitchingTimeline)
        {
            StartCoroutine(SwitchTimelineRoutine());
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log("Interact Pressed");
        TryInteract();
    }
    private void TryInteract()
    {
        CheckForInteractable();

        if (currentInteractable != null)
        {
            Debug.Log("Calling interact on: " + currentInteractable.name);
            currentInteractable.Interact();
        }
        else
        {
            Debug.Log("No interactable found");
        }
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
                currentInteractable = hit.GetComponent<SimpleInteract>();
                hit.GetComponent<InteractableVisual>()?.SetHighlight(true);
            }
            else
            {
                currentInteractable = null;
            }
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
                PlaySound(wallJumpSound, wallJumpVolume);
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

        PlaySound(dashSound, dashVolume);
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

    private void HandleFootsteps()
    {
        if (!IsGrounded() || Mathf.Abs(rb.linearVelocity.x) < 0.1f)
        {
            return;
        }

        // Dynamic step speed
        float speedPercent = Mathf.Abs(rb.linearVelocity.x) / moveSpeed;
        stepInterval = Mathf.Lerp(0.5f, 0.25f, speedPercent);

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f) 
        {
            AudioClip stepSound = GetFootstepSound();
            PlaySound(stepSound, footstepVolume);

            stepTimer = stepInterval;
        }
    }

    private AudioClip GetFootstepSound()
    {
        Vector3 worldPos = groundCheck.position;

        Vector3Int cellPosGrass = grassTilemap.WorldToCell(worldPos);
        if (grassTilemap.HasTile(cellPosGrass))
        {
            return grassStepSound;
        }

        Vector3Int cellPosStone = stoneTilemap.WorldToCell(worldPos);
        if (stoneTilemap.HasTile(cellPosStone))
        {
            return stoneStepSound;
        }

        return grassStepSound; // fallback
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clip, volume * masterVolume);
        }
    }

    private IEnumerator SwitchTimelineRoutine()
    {
        isSwitchingTimeline = true;

        // Play hourglass animation first
        yield return StartCoroutine(timelineUI.PlayTimelineAnimation());

        // Only switch after animation is finished
        if (TimelineManager.Instance != null)
        {
            TimelineManager.Instance.SwitchTimeline();
        }

        isSwitchingTimeline = false;
    }
}