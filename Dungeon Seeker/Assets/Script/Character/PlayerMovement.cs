using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D body;
    private Animator anim;

    [Header("Basic Movement")]
    [SerializeField] private float speed = 7f; 
    [SerializeField] private bool useRigidbody = true;
    private Vector3 originalScale;
    private Vector3 startPosition;

    [Header("Movement Variables")]
    private float horizontalInputRaw;
    private float horizontalVelocity;
    private float velocityXSmoothing;
    [SerializeField] private float accelerationTime = 0.08f;

    [Header("Throwable Reference")]
    [SerializeField] private Lempar lempar;
    [SerializeField] private float throwCooldown = 0.5f;

    // Component References
    private PlayerJump playerJump;
    private PlayerGroundDetection groundDetection;
    private PlayerCombat playerCombat;
    private PlayerDodge playerDodge;
    private PlayerAnimation playerAnimation;


    private void Awake()
    {
        // Get reference for rigidbody from object
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        
        // Store the original scale of the character
        originalScale = transform.localScale;
        startPosition = transform.position;
        
        // Configure rigidbody
        ConfigureRigidbody();
        
        // Initialize components
        InitializeComponents();
    }

    private void ConfigureRigidbody()
    {
        if (body != null)
        {
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.gravityScale = 1;
            body.simulated = true;
            body.bodyType = RigidbodyType2D.Dynamic;
        }
        else
        {
            Debug.LogError("No Rigidbody2D component found on the player!");
        }
    }

    private void InitializeComponents()
    {
        // Get or add component references
        playerJump = GetComponent<PlayerJump>() ?? gameObject.AddComponent<PlayerJump>();
        groundDetection = GetComponent<PlayerGroundDetection>() ?? gameObject.AddComponent<PlayerGroundDetection>();
        playerCombat = GetComponent<PlayerCombat>() ?? gameObject.AddComponent<PlayerCombat>();
        playerDodge = GetComponent<PlayerDodge>() ?? gameObject.AddComponent<PlayerDodge>();
        playerAnimation = GetComponent<PlayerAnimation>() ?? gameObject.AddComponent<PlayerAnimation>();

        // Initialize each component
        playerJump.Initialize(body, anim, groundDetection);
        groundDetection.Initialize(body, anim, transform);
        playerCombat.Initialize(anim);
        playerDodge.Initialize(body, anim, groundDetection);
        playerAnimation.Initialize(anim, playerDodge, groundDetection);
    }

    void Update()
    {
        // Get horizontal input
        horizontalInputRaw = Input.GetAxisRaw("Horizontal");

        // Smooth horizontal input
        horizontalVelocity = Mathf.SmoothDamp(horizontalVelocity, horizontalInputRaw, ref velocityXSmoothing, accelerationTime);

        // Handle character flipping
        HandleCharacterFlipping();

        // Handle jump input
        playerJump.HandleJumpInput();

        // Handle throwing objects
        HandleThrowInput();

        // Handle attack input (now using C key)
        playerCombat.HandleAttackInput();

        // Handle dodge input
        playerDodge.HandleDodgeInput();

        // Update animations
        playerAnimation.UpdateAnimations(horizontalVelocity);

        // Update animator parameter "run" hanya jika grounded
        if (anim != null && groundDetection != null)
        {
            bool isGrounded = groundDetection.IsGrounded();
            anim.SetBool("run", Mathf.Abs(horizontalVelocity) > 0.1f && isGrounded);
            anim.SetBool("grounded", isGrounded);
        }

        // Toggle movement method with T key (for testing)
        if (Input.GetKeyDown(KeyCode.T))
        {
            useRigidbody = !useRigidbody;
        }
    }

    void FixedUpdate()
    {
        // Check ground state first
        groundDetection.CheckGrounded();

        // Handle jump physics
        playerJump.HandleJumpPhysics();

        // Handle horizontal movement
        HandleMovement(horizontalVelocity);

        // Handle dodge physics if currently dodging
        playerDodge.HandleDodgePhysics();

        // Update cooldown timers
        UpdateCooldowns();
    }

    private void HandleCharacterFlipping()
    {
        if (horizontalInputRaw > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
        else if (horizontalInputRaw < -0.01f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
    }

    private void HandleThrowInput()
    {
        if (Input.GetKeyDown(KeyCode.X) && lempar != null && lempar.IsAvailable)
        {
            lempar.Throw();
            Debug.Log("Player threw an object");
        }
    }

    private void HandleMovement(float smoothedInput)
    {
        if (useRigidbody && body != null)
        {
            float targetVelocityX = smoothedInput * speed;
            body.velocity = new Vector2(targetVelocityX, body.velocity.y);
        }
        else
        {
            transform.position += new Vector3(smoothedInput * speed * Time.fixedDeltaTime, 0, 0);
        }
    }

    private void UpdateCooldowns()
    {
       
        playerDodge.UpdateCooldowns();
    }

    // Public getters for other components
    public Rigidbody2D GetRigidbody2D() => body;
    public Animator GetAnimator() => anim;
    public Vector3 GetOriginalScale() => originalScale;
    public float GetSpeed() => speed;
    public bool GetUseRigidbody() => useRigidbody;
    
    // Method untuk cek immunity dari dodge system
    public bool IsImmuneToDamage()
    {
        if (playerDodge != null)
        {
            return playerDodge.IsInvincible();
        }
        return false;
    }
    
    // Method alternatif untuk compatibility
    public bool ShouldTakeDamage()
    {
        if (playerDodge != null)
        {
            return playerDodge.ShouldTakeDamage();
        }
        return true; // Default: bisa menerima damage
    }
    
    // Method untuk akses komponen dodge
    public PlayerDodge GetPlayerDodge() => playerDodge;
}
