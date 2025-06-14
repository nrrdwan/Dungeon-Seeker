using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D body;
    private Animator anim;

    private bool grounded;
    [SerializeField] private float speed = 7f; 
    [SerializeField] private bool useRigidbody = true;
    private Vector3 originalScale;
    private Vector3 startPosition;
    
    private float jumpCooldown = 0f;
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float fallThreshold = -2f;
    private bool wasRising = false;

    // Add new attack-related variables
    private float attackCooldown = 0f;
    [SerializeField] private float attackDuration = 0.5f;  // Duration of attack animation
    [SerializeField] private float attackCooldownTime = 0.6f;  // Time before player can attack again

    // Add new combo-related variables
    [SerializeField] private int maxComboCount = 3; // Maximum hits in combo
    private int currentComboCount = 0;
    private float comboResetTime = 0.8f; // Time window to input next combo hit
    private float lastAttackTime = 0f;

    // Add these variables to your PlayerMovement class
    private bool isTouchingWall;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallSlideSpeed = 2f;

    [SerializeField] private LayerMask groundLayer; // Add this to detect ground properly
    [SerializeField] private float groundCheckDistance = 0.2f; // Distance to check for ground
    [SerializeField] private Transform groundCheck; // Reference to a ground check position

    private void Awake()
    {
        // Get reference for rigidbody from object
        body = GetComponent<Rigidbody2D>();

        // Get reference for animator from object
        anim = GetComponent<Animator>();
        
        // Store the original scale of the character
        originalScale = transform.localScale;
        startPosition = transform.position;
        
        // Ensure the rigidbody is configured correctly
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
        
        // If groundCheck is not assigned, create one
        if (groundCheck == null)
        {
            // Create a child object for ground checking at the character's feet
            GameObject groundCheckObject = new GameObject("GroundCheck");
            groundCheckObject.transform.parent = transform;
            groundCheckObject.transform.localPosition = new Vector3(0, -0.5f, 0); // Adjust position to character's feet
            groundCheck = groundCheckObject.transform;
        }
    }

    // Use FixedUpdate for physics calculations
    void FixedUpdate()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        
        // Process player movement
        HandleMovement(horizontalInput);
        
        // Process cooldowns
        UpdateCooldowns();
        
        // Check for wall sliding
        CheckWallSliding();
        
        // Check if player is grounded using a more reliable method
        CheckGrounded();
    }
    
    // Handle horizontal movement based on movement mode
    private void HandleMovement(float horizontalInput)
    {
        if (useRigidbody && body != null)
        {
            // Apply velocity directly to the rigidbody
            body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);
        }
        else
        {
            // Alternative: Use transform-based movement
            transform.position += new Vector3(horizontalInput * speed * Time.fixedDeltaTime, 0, 0);
        }
    }
    
    // Update all cooldown timers
    private void UpdateCooldowns()
    {
        // Decrease jump cooldown
        if (jumpCooldown > 0)
        {
            jumpCooldown -= Time.fixedDeltaTime;
        }
        
        // Additional cooldowns can be added here as needed
    }
    
    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        // Flip player when moving left-right
        if (horizontalInput > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }

        // Jump when space key is pressed - add debug logging
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space pressed! Grounded: " + grounded + ", Jump Cooldown: " + jumpCooldown);
            if (grounded && jumpCooldown <= 0)
            {
                Jump();
                jumpCooldown = 0.1f;
            }
            // Add wall jump capability
            else if (isTouchingWall)
            {
                WallJump();
                jumpCooldown = 0.1f;
            }
        }
        
        // Handle attack input with combo system
        if (Input.GetKeyDown(KeyCode.X) && attackCooldown <= 0)
        {
            // Check if we're starting a new combo or continuing one
            if (Time.time > lastAttackTime + comboResetTime)
            {
                // Start new combo
                currentComboCount = 0;
            }
            
            // Perform the attack at current combo count
            PerformAttack();
            
            // Update combo and timing
            currentComboCount = (currentComboCount + 1) % maxComboCount;
            lastAttackTime = Time.time;
        }
        
        // Reset combo if time expired
        if (currentComboCount > 0 && Time.time > lastAttackTime + comboResetTime)
        {
            currentComboCount = 0;
        }
        
        // Decrease attack cooldown
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
            
            // When current attack animation should end
            if (attackCooldown <= attackCooldownTime - attackDuration)
            {
                // Reset attack animation if we're not continuing a combo
                if (Time.time > lastAttackTime + 0.5f)
                {
                    anim.SetInteger("attackCombo", 0);
                    anim.SetBool("attack", false);
                }
            }
        }

        // DEBUG KEYS - Add these to test animations manually
        if (Input.GetKeyDown(KeyCode.Alpha1)) // Press 1 to force jump
        {
            anim.SetBool("jump", true);
            anim.SetBool("fall", false);
            anim.SetBool("grounded", false);
            Debug.Log("FORCED: Jump animation!");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2)) // Press 2 to force fall
        {
            anim.SetBool("jump", false);
            anim.SetBool("fall", true);
            anim.SetBool("grounded", false);
            Debug.Log("FORCED: Fall animation!");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3)) // Press 3 to force idle
        {
            anim.SetBool("jump", false);
            anim.SetBool("fall", false);
            anim.SetBool("grounded", true);
            Debug.Log("FORCED: Idle animation!");
        }
        
        // NEW: Add a debug key for attack
        if (Input.GetKeyDown(KeyCode.Alpha4)) // Press 4 to force attack
        {
            anim.SetBool("attack", true);
            Debug.Log("FORCED: Attack animation!");
            
            // Auto-reset attack animation after a delay
            Invoke("ResetAttackAnimation", 0.5f);
        }

        // Add test keys for combo moves
        if (Input.GetKeyDown(KeyCode.Alpha5)) // Press 5 to force attack combo 1
        {
            anim.SetInteger("attackCombo", 1);
            anim.SetBool("attack", true);
            Debug.Log("FORCED: Attack 1 animation!");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha6)) // Press 6 to force attack combo 2
        {
            anim.SetInteger("attackCombo", 2);
            anim.SetBool("attack", true);
            Debug.Log("FORCED: Attack 2 animation!");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha7)) // Press 7 to force attack combo 3
        {
            anim.SetInteger("attackCombo", 3);
            anim.SetBool("attack", true);
            Debug.Log("FORCED: Attack 3 animation!");
        }

        UpdateAnimations(horizontalInput);

        // Toggle movement method with T key (for testing)
        if (Input.GetKeyDown(KeyCode.T))
        {
            useRigidbody = !useRigidbody;
        }
    }

    // Separated animation logic for cleaner code
    private void UpdateAnimations(float horizontalInput)
    {
        if (anim == null) return;
        
        // Wall slide has priority over other animations
        if (isTouchingWall)
        {
            anim.SetBool("wallSlide", true);
            anim.SetBool("jump", false);
            anim.SetBool("fall", false);
            return; // Skip other animation updates when wall sliding
        }
        else
        {
            anim.SetBool("wallSlide", false);
        }
        
        // Only set run animation if we're not attacking
        if (!anim.GetBool("attack"))
        {
            // Use velocity directly for run animation instead of grounded state
            anim.SetBool("run", Mathf.Abs(horizontalInput) > 0.1f && Mathf.Abs(body.velocity.y) < 0.1f);
        }
        else if (Mathf.Abs(horizontalInput) <= 0.1f)
        {
            // Ensure run animation stops when not moving
            anim.SetBool("run", false);
        }
        
        // Remove grounded parameter update
        // anim.SetBool("grounded", grounded);
        
        // Handle animation states based purely on velocity
        if (!anim.GetBool("attack"))
        {
            float fallingThreshold = 0f; // Aktifkan fall di kecepatan turun berapapun
            float risingThreshold = 0.5f;
            
            if (body.velocity.y < fallingThreshold)
            {
                anim.SetBool("jump", false);
                anim.SetBool("fall", true);
                // Tambahkan log untuk debugging
                if (!anim.GetBool("fall"))
                    Debug.Log("Animasi Fall AKTIF dengan kecepatan: " + body.velocity.y);
            }
            else if (body.velocity.y > risingThreshold) 
            {
                anim.SetBool("jump", true);
                anim.SetBool("fall", false);
            }
            else if (Mathf.Abs(body.velocity.y) < 0.1f)
            {
                // We're neither jumping nor falling - reset both animations
                anim.SetBool("jump", false);
                anim.SetBool("fall", false);
            }
        }
    }

    // Add a reliable ground check method
    private void CheckGrounded()
    {
        // Use a circle cast to check if player is touching the ground
        Vector2 position = groundCheck != null ? groundCheck.position : new Vector2(transform.position.x, transform.position.y - 0.5f);
        grounded = Physics2D.CircleCast(position, 0.2f, Vector2.down, groundCheckDistance, groundLayer);
        
        // Visualization for debugging
        Debug.DrawRay(position, Vector2.down * groundCheckDistance, grounded ? Color.green : Color.red);
        
        // Update animator grounded state if needed
        if (anim != null)
        {
            // We can use grounded parameter in the animator if it helps with transitions
            anim.SetBool("grounded", grounded);
        }
    }

    // Jump function from the provided code
    private void Jump()
    {
        if (useRigidbody && body != null)
        {
            // Use a higher jump force than movement speed
            body.velocity = new Vector2(body.velocity.x, jumpForce);
            
            // Immediately set animation states for responsiveness
            anim.SetBool("jump", true);
            anim.SetBool("fall", false);
            
            grounded = false;
            
            Debug.Log("Jump executed with force: " + jumpForce);
        }
    }
    
    // Add wall jump capability
    private void WallJump()
    {
        if (useRigidbody && body != null)
        {
            // Jump away from wall (horizontally and vertically)
            float horizontalDirection = transform.localScale.x > 0 ? -1 : 1;
            body.velocity = new Vector2(horizontalDirection * speed * 0.75f, jumpForce * 0.8f);
            
            // Flip character direction
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            
            // Set jump animation
            anim.SetBool("jump", true);
            anim.SetBool("fall", false);
            
            Debug.Log("Wall jump executed!");
        }
    }

    // Ground detection with a small delay
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Instead of using OnCollisionEnter2D for ground detection,
        // we'll rely on our CheckGrounded method which is more reliable
        // But we'll keep this for any additional collision logic
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        // We'll rely on CheckGrounded instead
    }

    // Replace your existing Attack method with this one
    private void PerformAttack()
    {
        // Cancel any previous ResetAttackAnimation calls
        CancelInvoke("ResetAttackAnimation");
        
        // Set the specific combo animation
        anim.SetInteger("attackCombo", currentComboCount + 1); // 1, 2, 3 for different attacks
        anim.SetBool("attack", true);
        
        // Set a shorter cooldown to allow for combo inputs
        attackCooldown = attackCooldownTime;
        
        Debug.Log("Player attacked! Combo hit: " + (currentComboCount + 1));
        
        // You can also add logic for different damage or effects per combo hit
        switch(currentComboCount)
        {
            case 0: // First hit
                // Light attack logic
                break;
            case 1: // Second hit
                // Medium attack logic
                break; 
            case 2: // Third hit
                // Heavy attack logic
                break;
        }
        
        // Schedule animation reset after attack duration
        Invoke("ResetAttackAnimation", attackDuration);
    }
    
    // Method to reset attack animation (used by Invoke)
    private void ResetAttackAnimation()
    {
        anim.SetBool("attack", false);
        anim.SetInteger("attackCombo", 0);
    }
    
    // Add this method to ensure all animations are properly reset
    private void ResetAllAnimations()
    {
        anim.SetBool("attack", false);
        anim.SetInteger("attackCombo", 0);
        anim.SetBool("run", false);
        anim.SetBool("jump", false);
        anim.SetBool("fall", false);
        // Remove grounded parameter reset
        // anim.SetBool("grounded", grounded);
    }

    // Add this new method
    private void CheckWallSliding()
    {
        // Cast a ray in the direction the player is facing
        float raycastDistance = 0.5f;
        Vector2 direction = new Vector2(transform.localScale.x > 0 ? 1 : -1, 0);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, raycastDistance, wallLayer);
        
        // Debug visualization
        Debug.DrawRay(transform.position, direction * raycastDistance, hit ? Color.red : Color.green);
        
        // Check if touching wall and not on ground and falling
        isTouchingWall = hit && !grounded && body.velocity.y < 0;
        
        if (isTouchingWall)
        {
            // Limit falling speed for wall slide
            body.velocity = new Vector2(body.velocity.x, Mathf.Max(body.velocity.y, -wallSlideSpeed));
            
            // Set wall slide animation
            anim.SetBool("wallSlide", true);
        }
        else
        {
            // Reset wall slide animation when not sliding
            anim.SetBool("wallSlide", false);
        }
    }

    // Add this method to visualize debug elements in the scene view
    private void OnDrawGizmosSelected()
    {
        // Draw ground check area
        Vector3 groundCheckPosition = groundCheck != null ? groundCheck.position : new Vector3(transform.position.x, transform.position.y - 0.5f, 0);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheckPosition, 0.2f);
        
        // Draw wall check area
        Vector3 wallCheckDirection = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + wallCheckDirection * 0.5f);
    }
    
    // Add this new method to play specific attack animations
    public void PlayAttackAnimation(int attackNumber)
    {
        // Ensure attack number is valid (1-3)
        attackNumber = Mathf.Clamp(attackNumber, 1, 3);
        
        // Set the attack combo parameter
        anim.SetInteger("attackCombo", attackNumber);
        anim.SetBool("attack", true);
        
        Debug.Log($"Playing attack animation {attackNumber}");
        
        // Reset after duration
        Invoke("ResetAttackAnimation", attackDuration);
    }
    
    // Add a helper method to check if an attack animation is currently playing
    public bool IsAttacking()
    {
        return anim.GetBool("attack");
    }
    
    // Menambahkan penjelasan tentang fungsi animasi falling
    /* 
     * Penjelasan kenapa animasi fall kadang tidak aktif:
     * 
     * 1. Threshold Kecepatan: Animasi fall hanya aktif jika kecepatan vertikal < -1f (fallingThreshold).
     *    Jika karakter turun dengan kecepatan lambat, nilai tersebut mungkin tidak tercapai.
     *    
     * 2. Prioritas Animasi: Ada beberapa animasi yang memiliki prioritas lebih tinggi:
     *    - Jika wallSlide = true, animasi fall tidak akan dijalankan
     *    - Jika attack = true, animasi fall tidak akan diubah
     *    
     * 3. Ketika karakter tepat mendarat di tanah, CheckGrounded() langsung mengubah
     *    status grounded menjadi true, yang dapat membatalkan animasi falling
     *    sebelum terlihat jelas.
     *    
     * 4. Jika Mathf.Abs(body.velocity.y) < 0.1f, semua animasi jump dan fall 
     *    akan direset karena dianggap tidak sedang jatuh atau melompat.
     */
}
