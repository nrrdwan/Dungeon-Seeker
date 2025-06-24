using UnityEngine;
using System.Collections;

public class PlayerJump : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private int maxJumpCount = 2; // Ubah jadi 2 untuk double jump
    [SerializeField] private float groundJumpWindow = 0.3f; // Window untuk double jump dari ground
    
    private int jumpCount = 0;
    private bool jumpPressed = false;
    private bool hasJumpedThisPress = false;
    private bool isJumping = false;
    private float airTime = 0f;
    private float lastGroundedTime = 0f;
    private float lastJumpTime = 0f; // Track kapan terakhir jump

    // Component references
    private Rigidbody2D body;
    private Animator anim;
    private PlayerGroundDetection groundDetection;

    public void Initialize(Rigidbody2D rigidBody, Animator animator, PlayerGroundDetection detection)
    {
        body = rigidBody;
        anim = animator;
        groundDetection = detection;
    }

    public void HandleJumpInput()
    {
        bool jumpKeyDown = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow);
        bool jumpKeyUp = Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.UpArrow);
        
        // Reset tracking saat key dilepas
        if (jumpKeyUp)
        {
            hasJumpedThisPress = false;
        }
        
        // Handle jump input - DOUBLE JUMP SYSTEM (bisa dari ground atau udara)
        if (jumpKeyDown && !hasJumpedThisPress)
        {
            bool grounded = groundDetection.IsGrounded();
            float timeSinceLastJump = Time.time - lastJumpTime;
            
            // First jump dari ground
            if (grounded && jumpCount == 0)
            {
                jumpPressed = true;
                Debug.Log("JUMP INPUT - First jump from ground");
            }
            // Second jump - bisa dari ground (dalam window) atau udara
            else if (jumpCount == 1)
            {
                // Dari ground dalam window waktu tertentu
                if (grounded && timeSinceLastJump < groundJumpWindow)
                {
                    jumpPressed = true;
                    Debug.Log("JUMP INPUT - Second jump from ground (quick succession)");
                }
                // Atau dari udara (normal double jump)
                else if (!grounded)
                {
                    jumpPressed = true;
                    Debug.Log("JUMP INPUT - Second jump in air (double jump)");
                }
                else
                {
                    Debug.Log($"JUMP BLOCKED - Too late for ground double jump (time: {timeSinceLastJump:F2}s)");
                }
            }
            else
            {
                Debug.Log($"JUMP BLOCKED - Jump count: {jumpCount}/{maxJumpCount}, Grounded: {grounded}");
            }
        }
        
        // Track time in the air
        bool isGroundedForAirTime = groundDetection.IsGrounded();
        if (!isGroundedForAirTime)
        {
            airTime += Time.deltaTime;
        }
        else
        {
            airTime = 0f;
            if (isJumping)
            {
                isJumping = false;
                Debug.Log("Landed - isJumping set to false");
            }
        }
    }

    public void HandleJumpPhysics()
    {
        if (jumpPressed)
        {
            bool grounded = groundDetection.IsGrounded();
            
            // Execute jump based on current state
            if (jumpCount == 0) // First jump
            {
                if (grounded && body.velocity.y <= 0.1f)
                {
                    PerformJump("FIRST");
                    jumpCount = 1;
                    lastJumpTime = Time.time;
                    
                    // Notify ground detection about jump start
                    groundDetection.NotifyJumpStart();
                    
                    Debug.Log("FIRST JUMP EXECUTED - Jump count: " + jumpCount);
                }
            }
            else if (jumpCount == 1) // Second jump
            {
                float timeSinceLastJump = Time.time - lastJumpTime;
                
                // Second jump dari ground (dalam window)
                if (grounded && timeSinceLastJump < groundJumpWindow && body.velocity.y <= 0.1f)
                {
                    PerformJump("SECOND_GROUND");
                    jumpCount = 2;
                    lastJumpTime = Time.time;
                    
                    // Notify ground detection lagi
                    groundDetection.NotifyJumpStart();
                    
                    Debug.Log("SECOND JUMP FROM GROUND EXECUTED - Jump count: " + jumpCount);
                }
                // Second jump dari udara (normal double jump)
                else if (!grounded)
                {
                    PerformDoubleJump();
                    jumpCount = 2;
                    lastJumpTime = Time.time;
                    
                    Debug.Log("DOUBLE JUMP IN AIR EXECUTED - Jump count: " + jumpCount);
                }
            }
            
            jumpPressed = false;
        }
        
        // Reset jump count saat landing dengan velocity check
        if (groundDetection.IsGrounded() && body.velocity.y <= 0.1f)
        {
            lastGroundedTime = Time.time;
            
            // Reset jump count hanya saat benar-benar landing
            if (jumpCount > 0 && !isJumping)
            {
                jumpCount = 0;
                Debug.Log("LANDED! Jump count reset to 0 - Can double jump again!");
            }
        }
    }

    private void PerformJump(string jumpType = "NORMAL")
    {
        if (body != null)
        {
            // Apply vertical force for jumping
            body.velocity = new Vector2(body.velocity.x, jumpForce);
            
            // Update animations
            if (anim != null)
            {
                anim.SetBool("jump", true);
                anim.SetBool("fall", false);
                anim.SetBool("grounded", false);
            }
            
            // Set air state variables
            isJumping = true;
            airTime = 0.01f;
            hasJumpedThisPress = true;
            
            // Force disable ground detection temporarily untuk jump dari ground
            if (jumpType == "FIRST" || jumpType == "SECOND_GROUND")
            {
                StartCoroutine(DisableGroundDetection());
            }
            
            Debug.Log($"{jumpType} JUMP EXECUTED! Force: {jumpForce}");
        }
    }

    private void PerformDoubleJump()
    {
        if (body != null)
        {
            // Apply vertical force for double jump (sedikit lebih lemah)
            float doubleJumpForce = jumpForce * 0.9f;
            body.velocity = new Vector2(body.velocity.x, doubleJumpForce);
            
            // Update animations
            if (anim != null)
            {
                anim.SetBool("jump", true);
                anim.SetBool("fall", false);
            }
            
            // Set air state variables
            hasJumpedThisPress = true;
            
            Debug.Log($"DOUBLE JUMP IN AIR EXECUTED! Force: {doubleJumpForce}");
        }
    }

    private IEnumerator DisableGroundDetection()
    {
        groundDetection.SetGroundCheckDistance(0);
        yield return new WaitForSeconds(0.1f);
        groundDetection.RestoreGroundCheckDistance();
    }

    // Method untuk force reset (debugging)
    public void ForceResetJump()
    {
        jumpCount = 0;
        isJumping = false;
        hasJumpedThisPress = false;
        lastJumpTime = 0f;
        Debug.Log("Jump system force reset!");
    }

    // Public getters
    public bool IsJumping() => isJumping;
    public float GetAirTime() => airTime;
    public int GetJumpCount() => jumpCount;
    public bool CanJump() => jumpCount < maxJumpCount;
    public int GetRemainingJumps() => maxJumpCount - jumpCount;
    
    // Method untuk check apakah bisa double jump
    public bool CanDoubleJump() => jumpCount == 1;
    public bool CanGroundDoubleJump() => jumpCount == 1 && groundDetection.IsGrounded() && (Time.time - lastJumpTime) < groundJumpWindow;
}
