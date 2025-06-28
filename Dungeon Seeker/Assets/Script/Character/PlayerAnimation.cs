using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float idleThreshold = 0.1f;
    [SerializeField] private float runThreshold = 0.5f;

    // Component references
    private Animator anim;
    private PlayerDodge playerDodge;
    private PlayerGroundDetection groundDetection;

    // Animation state tracking
    private bool wasGrounded = false;
    private float lastVelocityY = 0f;

    public void Initialize(Animator animator, PlayerDodge dodge, PlayerGroundDetection detection)
    {
        anim = animator;
        playerDodge = dodge;
        groundDetection = detection;
    }

    public void UpdateAnimations(float horizontalVelocity)
    {
        if (anim == null) return;

        // Get current states
        bool isGrounded = groundDetection.IsGrounded();
        bool isDodging = playerDodge.IsDodging();

        // Update basic movement animations
        UpdateMovementAnimations(horizontalVelocity, isGrounded);

        // Update air state animations
        UpdateAirStateAnimations(isGrounded);

        // Update dodge animation
        UpdateDodgeAnimation(isDodging);

        // Store previous states
        wasGrounded = isGrounded;
        lastVelocityY = GetRigidbody().velocity.y;
    }

    private void UpdateMovementAnimations(float horizontalVelocity, bool isGrounded)
    {
        if (!isGrounded) return;

        float absVelocity = Mathf.Abs(horizontalVelocity);

        // Determine movement state
        if (absVelocity < idleThreshold)
        {
            // Idle
            anim.SetBool("run", false);
            anim.SetFloat("speed", 0f);
        }
        else if (absVelocity >= runThreshold)
        {
            // Running
            anim.SetBool("run", true);
            anim.SetFloat("speed", absVelocity);
        }
        else
        {
            // Walking
            anim.SetBool("run", false);
            anim.SetFloat("speed", absVelocity);
        }
    }

    private void UpdateAirStateAnimations(bool isGrounded)
    {
        Rigidbody2D body = GetRigidbody();
        if (body == null) return;

        float currentVelocityY = body.velocity.y;

        // Handle landing
        if (!wasGrounded && isGrounded)
        {
            anim.SetBool("grounded", true);
            anim.SetBool("jump", false);
            anim.SetBool("fall", false);
            anim.SetTrigger("land");
            Debug.Log("Landing animation triggered");
        }
        // Handle leaving ground
        else if (wasGrounded && !isGrounded)
        {
            anim.SetBool("grounded", false);
        }

        // Update jump/fall states when in air
        if (!isGrounded)
        {
            if (currentVelocityY > 0.1f)
            {
                // Rising (jumping)
                anim.SetBool("jump", true);
                anim.SetBool("fall", false);
            }
            else if (currentVelocityY < -0.01f) // ubah threshold
            {
                // Falling
                anim.SetBool("jump", false);
                anim.SetBool("fall", true);
            }
            else
            {
                // Jika melayang (nyaris 0), pastikan fall tetap false
                anim.SetBool("fall", false);
            }
        }
    }

    private void UpdateWallSlideAnimation(bool isTouchingWall, bool isGrounded)
    {
        if (!isGrounded && isTouchingWall)
        {
            anim.SetBool("wallSlide", true);
            // Override jump/fall when wall sliding
            anim.SetBool("jump", false);
            anim.SetBool("fall", false);
        }
        else
        {
            anim.SetBool("wallSlide", false);
        }
    }

    private void UpdateDodgeAnimation(bool isDodging)
    {
        anim.SetBool("dodge", isDodging);
        
        if (isDodging)
        {
            // Override other animations during dodge
            anim.SetBool("run", false);
            anim.SetFloat("speed", 0f);
        }
    }

    // Methods for combat animations (called by PlayerCombat)
    public void PlayAttackAnimation(int comboIndex)
    {
        if (anim == null) return;

        // Reset all attack animations
        ResetAttackAnimations();

        // Play specific combo animation
        switch (comboIndex)
        {
            case 1:
                anim.SetBool("attack1", true);
                break;
            case 2:
                anim.SetBool("attack2", true);
                break;
            case 3:
                anim.SetBool("attack3", true);
                break;
        }

        anim.SetTrigger("attack");
        Debug.Log($"Playing attack animation {comboIndex}");
    }

    public void ResetAttackAnimations()
    {
        if (anim == null) return;

        anim.SetBool("attack1", false);
        anim.SetBool("attack2", false);
        anim.SetBool("attack3", false);
    }

    // Method to force specific animation states
    public void ForceGroundedState(bool grounded)
    {
        if (anim != null)
        {
            anim.SetBool("grounded", grounded);
        }
    }

    public void ForceJumpState()
    {
        if (anim != null)
        {
            anim.SetBool("jump", true);
            anim.SetBool("fall", false);
            anim.SetBool("grounded", false);
        }
    }

    public void ForceFallState()
    {
        if (anim != null)
        {
            anim.SetBool("jump", false);
            anim.SetBool("fall", true);
            anim.SetBool("grounded", false);
        }
    }

    // Helper method to get rigidbody
    private Rigidbody2D GetRigidbody()
    {
        return GetComponent<Rigidbody2D>();
    }

    // Method to get current animation state info
    public AnimatorStateInfo GetCurrentStateInfo()
    {
        if (anim != null)
        {
            return anim.GetCurrentAnimatorStateInfo(0);
        }
        return new AnimatorStateInfo();
    }

    public bool IsAnimationPlaying(string animationName)
    {
        if (anim != null)
        {
            return anim.GetCurrentAnimatorStateInfo(0).IsName(animationName);
        }
        return false;
    }

    // Debug information
    public void LogCurrentAnimationState()
    {
        if (anim != null)
        {
            var stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"Current Animation: {stateInfo.shortNameHash}, Progress: {stateInfo.normalizedTime}");
            
            // Log boolean parameters
            Debug.Log($"Grounded: {anim.GetBool("grounded")}, Jump: {anim.GetBool("jump")}, " +
                     $"Fall: {anim.GetBool("fall")}, Run: {anim.GetBool("run")}, " +
                     $"WallSlide: {anim.GetBool("wallSlide")}, Dodge: {anim.GetBool("dodge")}");
        }
    }
}
