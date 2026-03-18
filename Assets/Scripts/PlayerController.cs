using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInputSystem playerControls;
    private Rigidbody2D rb;
    private bool InAir = false;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 5f;
    void Start()
    {
        playerControls = new PlayerInputSystem();
        rb = GetComponent<Rigidbody2D>();
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
        UnityEngine.Vector2 input = inputValue.Get<UnityEngine.Vector2>();
        Debug.Log("move");
        rb.linearVelocity = new UnityEngine.Vector2(input.x * moveSpeed, rb.linearVelocity.y);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            InAir = false;
        }
    }
}
