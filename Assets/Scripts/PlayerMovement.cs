using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody2D myRigidbody;
    Vector2 moveInput;
    Animator animator;
    BoxCollider2D myBoxCollider;
    CapsuleCollider2D myCapsuleCollider;
    SpriteRenderer mySpriteRenderer;
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;

    [Header("Jump")]
    [SerializeField] float jumpHeight = 15f;
    [SerializeField] float coyoteTime = 0.1f;

    [Header("Climbing")]
    [SerializeField] float climbingSpeed = 5f;

    [Header("Death")]
    [SerializeField] Color deathColor = new Color(0.75f, 0.25f, 0.25f, 1f);
    [SerializeField] Vector2 deathKick = new Vector2(0f, 15f);

    [Header("Swimming")]
    [SerializeField] float swimmingSpeed = 3f;
    [SerializeField] float swimmingGravity = 1f;
    
    bool isGrounded;
    bool isAlive = true;
    float coyoteTimeCounter;
    float myRigidbodyGravityAtStart;
    void Start()
    {
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        myCapsuleCollider = GetComponent<CapsuleCollider2D>();
        myBoxCollider = GetComponent<BoxCollider2D>();
        myRigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        myRigidbodyGravityAtStart = myRigidbody.gravityScale;
    }

    void Update()
    {
        Die();
        if (isAlive)
        {
            Run();
            FlipSprite();
            ClimbLadder();
            Swim();
        }
    }
    private void FixedUpdate()
    {
        CheckForGround();
    }
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        Debug.Log(moveInput);
    }
    void OnJump(InputValue value)
    {
        if (!isAlive) { return; }
        if (value.isPressed)
        {
            if (isGrounded || coyoteTimeCounter > 0f)
            {
                Jump();
                coyoteTimeCounter = 0f;
            }
        }
    }


    void Jump()
    {
        myRigidbody.velocity += new Vector2(0f, jumpHeight);
    }
    void CheckForGround()
    {
        isGrounded = myBoxCollider.IsTouchingLayers(LayerMask.GetMask("Ground", "Climbing"));
        if (isGrounded && coyoteTimeCounter < 0f)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }
    }
    void Run()
    {
        Vector2 playerVelocity = new Vector2(moveInput.x * moveSpeed, myRigidbody.velocity.y);
        myRigidbody.velocity = playerVelocity;

    }
    void FlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;
        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidbody.velocity.x), 1f);
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
    }
    void ClimbLadder()
    {
        if (!myBoxCollider.IsTouchingLayers(LayerMask.GetMask("Climbing")))
        {
            myRigidbody.gravityScale = myRigidbodyGravityAtStart;
            animator.SetBool("isClimbing", false);
            return;
        }

        Vector2 climbingVelocity = new Vector2(moveInput.x * moveSpeed, moveInput.y * climbingSpeed);
        myRigidbody.velocity = climbingVelocity;
        myRigidbody.gravityScale = 0f;
        bool playerHasVerticalSpeed = Mathf.Abs(myRigidbody.velocity.y) > Mathf.Epsilon;
        animator.SetBool("isClimbing", playerHasVerticalSpeed);


    }
    void Die()
    {
        if (myCapsuleCollider.IsTouchingLayers(LayerMask.GetMask("Enemy", "Hazards")))
        {
            isAlive = false;
            animator.SetTrigger("Die");
            mySpriteRenderer.color = deathColor;
            myRigidbody.velocity += deathKick;
            myBoxCollider.enabled = false;
            myCapsuleCollider.enabled = false;
        }
    }
    void Swim()
    {
        if (!myBoxCollider.IsTouchingLayers(LayerMask.GetMask("Water")))
        {
            myRigidbody.gravityScale = myRigidbodyGravityAtStart;            
            return;
        }

        Vector2 swimingVelocity = new Vector2(moveInput.x * swimmingSpeed, moveInput.y * swimmingSpeed);
        myRigidbody.velocity = swimingVelocity;
        myRigidbody.gravityScale = swimmingGravity;
        Debug.Log("Swimming");
    }

}
