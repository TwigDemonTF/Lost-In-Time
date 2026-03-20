using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInputSystem playerControls;
    private Rigidbody2D rb;
    private TrailRenderer tr;
    private bool InAir = false;
    private bool CanDash = true;
    private bool IsDashing = false;
    private UnityEngine.Vector2 moveInput;
    private float lastFacingDirection = 1f;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashingTime = 0.3f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 5f;
    void Start()
    {
        playerControls = new PlayerInputSystem();
        rb = GetComponent<Rigidbody2D>();
        tr = GetComponent<TrailRenderer>();
    }

    private void OnJump()
    {
        if(InAir == false)
        {
            rb.AddForce(UnityEngine.Vector2.up * jumpHeight, ForceMode2D.Impulse);
            Debug.Log("jump");
            InAir = true;
        }
    }
    private void OnMove(InputValue inputValue)
    {

        moveInput = inputValue.Get<UnityEngine.Vector2>();
        Debug.Log("move");
        rb.linearVelocity = new UnityEngine.Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    private void OnDash(InputValue inputValue)
    {
        Debug.Log("vroooooooooooooooooooooom");
        if(CanDash == true)
        {
            UnityEngine.Vector2 dashDirection;

        if (moveInput.x != 0)
            dashDirection = new UnityEngine.Vector2(Mathf.Sign(moveInput.x), 0f);
        else
            dashDirection = new UnityEngine.Vector2(lastFacingDirection, 0f);

            StartCoroutine(dash(dashDirection));
        }
    }
    private void OnWallJump()
    {
        dtryfdd
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            InAir = false;
        }
    }
    private IEnumerator dash(UnityEngine.Vector2 input)
    {
        
        IsDashing = true;
        CanDash = false; 
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new UnityEngine.Vector2(input.x * dashSpeed, 0f);
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        rb.gravityScale = originalGravity;
        IsDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        CanDash = true;
    }


}
