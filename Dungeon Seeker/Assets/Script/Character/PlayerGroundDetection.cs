using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundDetection : MonoBehaviour
{
    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float jumpGraceTime = 0.15f; // Grace time setelah jump

    private bool grounded;
    private float originalGroundCheckDistance;
    private float lastJumpTime = -1f; // Track kapan terakhir jump
    private bool wasJumping = false;

    // Component references
    private Rigidbody2D body;
    private Animator anim;
    private Transform playerTransform;

    public void Initialize(Rigidbody2D rigidBody, Animator animator, Transform transform)
    {
        body = rigidBody;
        anim = animator;
        playerTransform = transform;
        originalGroundCheckDistance = groundCheckDistance;
        
        CreateGroundCheckIfNeeded();
    }

    private void CreateGroundCheckIfNeeded()
    {
        if (groundCheck == null)
        {
            GameObject groundCheckObject = new GameObject("GroundCheck");
            groundCheckObject.transform.parent = playerTransform;
            groundCheckObject.transform.localPosition = new Vector3(0, -0.6f, 0);
            groundCheck = groundCheckObject.transform;
        }
    }

    public void CheckGrounded()
    {
        Vector2 position = groundCheck != null ? groundCheck.position : 
                          new Vector2(playerTransform.position.x, playerTransform.position.y - 0.6f);
        
        bool wasGrounded = grounded;
        
        // DETECT JUMP START - saat velocity naik cepat
        if (body.velocity.y > 3f && !wasJumping)
        {
            lastJumpTime = Time.time;
            wasJumping = true;
            grounded = false; // FORCE FALSE saat mulai jump
            Debug.Log("JUMP DETECTED - Force grounded = FALSE");
        }
        
        // GRACE PERIOD - setelah jump, ignore ground detection sebentar
        bool inGracePeriod = (Time.time - lastJumpTime) < jumpGraceTime;
        
        if (inGracePeriod)
        {
            grounded = false; // FORCE FALSE selama grace period
            Debug.Log($"IN GRACE PERIOD - Grounded = FALSE (Time since jump: {Time.time - lastJumpTime:F3})");
        }
        else
        {
            // NORMAL GROUND DETECTION setelah grace period
            RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, groundCheckDistance, groundLayer);
            bool raycastGrounded = hit.collider != null;
            
            // STRICT CONDITIONS untuk grounded
            bool velocityNearZero = Mathf.Abs(body.velocity.y) < 0.5f; // Velocity hampir nol
            bool movingDown = body.velocity.y <= 0.1f; // Bergerak turun atau diam
            
            // HANYA grounded jika SEMUA kondisi terpenuhi
            grounded = raycastGrounded && velocityNearZero && movingDown;
            
            // Reset jumping state jika sudah grounded
            if (grounded && wasJumping)
            {
                wasJumping = false;
                Debug.Log("LANDING DETECTED - wasJumping = FALSE");
            }
        }
        
        // FORCE FALSE jika masih naik
        if (body.velocity.y > 0.2f)
        {
            grounded = false;
        }
        
        // Debug visualization
        Color debugColor = grounded ? Color.green : Color.red;
        Debug.DrawRay(position, Vector2.down * groundCheckDistance, debugColor, 0.1f);
        
        // Log state changes dengan detail
        if (wasGrounded != grounded)
        {
            Debug.Log($"GROUNDED CHANGED: {grounded} | Velocity: {body.velocity.y:F3} | Grace: {inGracePeriod} | WasJumping: {wasJumping}");
        }
        
        // Update animator - FORCE SET
        if (anim != null)
        {
            anim.SetBool("grounded", grounded);
        }
    }

    // Method untuk notify saat jump dimulai (dipanggil dari PlayerJump)
    public void NotifyJumpStart()
    {
        lastJumpTime = Time.time;
        wasJumping = true;
        grounded = false;
        
        if (anim != null)
        {
            anim.SetBool("grounded", false);
        }
        
        Debug.Log("JUMP START NOTIFIED - Grounded = FALSE");
    }

    // Public methods
    public bool IsGrounded() => grounded;
    public LayerMask GetGroundLayer() => groundLayer;
    
    public void SetGroundCheckDistance(float distance)
    {
        groundCheckDistance = distance;
    }
    
    public void RestoreGroundCheckDistance()
    {
        groundCheckDistance = originalGroundCheckDistance;
    }

    // Method untuk force set grounded state (debugging)
    public void ForceGroundedState(bool state)
    {
        grounded = state;
        if (anim != null)
            anim.SetBool("grounded", state);
        Debug.Log($"Force grounded state to: {state}");
    }

    // Enhanced debugging method
    public void DebugGroundDetection()
    {
        Vector2 position = groundCheck != null ? groundCheck.position : 
                          new Vector2(playerTransform.position.x, playerTransform.position.y - 0.6f);
        
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, groundCheckDistance, groundLayer);
        bool inGrace = (Time.time - lastJumpTime) < jumpGraceTime;
        
        Debug.Log("=== GROUND DETECTION DEBUG ===");
        Debug.Log($"Velocity Y: {body.velocity.y:F3}");
        Debug.Log($"Hit Object: {(hit.collider != null ? hit.collider.name : "NONE")}");
        Debug.Log($"In Grace Period: {inGrace}");
        Debug.Log($"Was Jumping: {wasJumping}");
        Debug.Log($"Final Grounded: {grounded}");
        Debug.Log($"Animator Grounded: {(anim != null ? anim.GetBool("grounded") : false)}");
        Debug.Log("==============================");
    }

    [ContextMenu("Force Test Ground Detection")]
    public void ForceTestGroundDetection()
    {
        DebugGroundDetection();
    }
}
