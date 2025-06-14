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

    // Attack-related variables
    private float attackCooldown = 0f;
    [SerializeField] private float attackDuration = 0.5f;  // Duration of attack animation
    [SerializeField] private float attackCooldownTime = 0.6f;  // Time before player can attack again

    // Combo-related variables
    [SerializeField] private int maxComboCount = 3; // Maximum hits in combo
    private int currentComboCount = 0;
    private float comboResetTime = 0.8f; // Time window to input next combo hit
    private float lastAttackTime = 0f;

    // Wall sliding variables
    private bool isTouchingWall;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallSlideSpeed = 2f;

    // Ground checking variables
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private Transform groundCheck;

    // Movement variables
    private float horizontalInputRaw;
    private float horizontalVelocity;
    private float velocityXSmoothing;
    [SerializeField] private float accelerationTime = 0.08f;    // Jump control variables - with strict air control
    private bool wasGroundedLastFrame = false;
    private bool jumpPressed = false;
    private float lastGroundedTime = 0f;
    private float jumpBufferTime = 0f;
    private bool isJumping = false; // Track if player is in the middle of a jump
    private float airTime = 0f;     // Track time spent in air
    private bool hasJumpedThisPress = false;  // Track if current jump button press has been used

    // Throwable object reference
    [SerializeField] private Lempar lempar;
    [SerializeField] private float throwCooldown = 0.5f;
    private float throwTimer = 0f;

    // Dodge-related variables
    [SerializeField] private float dodgeSpeed = 12f;     // Speed boost during dodge
    [SerializeField] private float dodgeDuration = 0.5f; // How long the dodge lasts
    [SerializeField] private float dodgeCooldown = 0.8f; // Time before player can dodge again
    [SerializeField] private float airDodgeSpeed = 10f;  // Speed during air dodge (slightly lower than ground)
    [SerializeField] private float airDodgeUpwardForce = 5f; // Optional upward boost for air dodge
    [SerializeField] private bool maintainYVelocityInAirDodge = true; // Whether to maintain Y velocity in air dodge
    [SerializeField] private bool allowMultipleAirDodges = false; // Whether to allow multiple air dodges before landing
    private float currentDodgeCooldown = 0f;            // Current cooldown timer
    private bool isDodging = false;                     // Is the player currently dodging
    private float dodgeTimer = 0f;                      // Track how long player has been dodging
    private bool isDamageImmune = false;                // Is player currently immune to damage
    private bool hasAirDodged = false;                 // Track if player has already air dodged since last ground contact

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
    }    // Use FixedUpdate for physics calculations
    void FixedUpdate()
    {
        // Check ground state first
        CheckGrounded();
        
        // Update last grounded time when on ground
        if (grounded)
        {
            lastGroundedTime = Time.time;
        }
        
        // Handle jump physics
        HandleJump();
        
        // Check for wall sliding
        CheckWallSliding();
        
        // Handle horizontal movement
        HandleMovement(horizontalVelocity);
        
        // Handle dodge physics if currently dodging
        if (isDodging)
        {
            HandleDodge();
        }
        
        // Update cooldown timers - Modified to include dodgeCooldown
        UpdateCooldowns();
        
        // Store current grounded state for next frame
        wasGroundedLastFrame = grounded;
    }    // COMPLETELY REDESIGNED jump handling with absolute air control
    private void HandleJump()
    {
        if (jumpPressed)
        {
            // TRIPLE-CHECK ground state before allowing jump
            if (grounded && body.velocity.y <= 0.1f && !isJumping)
            {
                // Additional raycast verification that we're truly on ground
                RaycastHit2D groundVerify = Physics2D.Raycast(
                    transform.position,
                    Vector2.down,
                    0.6f,
                    groundLayer
                );
                
                Debug.DrawRay(transform.position, Vector2.down * 0.6f, Color.magenta, 0.5f);
                
                if (groundVerify.collider != null)
                {
                    // We have multiple confirmations we're on ground - safe to jump
                    Jump();
                    hasJumpedThisPress = true;
                    jumpCooldown = 0.3f;
                    Debug.Log("JUMP EXECUTED - Multiple ground confirmations!");
                }
                else
                {
                    Debug.Log("Jump DENIED - Failed final ground verification");
                }
            }
            // Only allow wall jumps if we're in a valid wall slide
            else if (isTouchingWall && !grounded && jumpCooldown <= 0)
            {
                WallJump();
                hasJumpedThisPress = true;
                jumpCooldown = 0.3f;
                Debug.Log("WALL JUMP EXECUTED");
            }
            else
            {
                Debug.Log("Jump DENIED - Not on ground. Grounded: " + grounded + 
                          ", isJumping: " + isJumping +
                          ", airTime: " + airTime + 
                          ", Y Velocity: " + body.velocity.y);
            }
            
            // Always clear jump flag after handling
            jumpPressed = false;
        }
    }    // Debug function to log current jump state - replaced CheckCanJump with direct checks
    private void LogJumpState()
    {
        string state = "JUMP STATE: " +
            "Grounded=" + grounded +
            ", IsJumping=" + isJumping +
            ", AirTime=" + airTime.ToString("F2") +
            ", Y-Vel=" + body.velocity.y.ToString("F2") +
            ", HasJumpedThisPress=" + hasJumpedThisPress;
            
        Debug.Log(state);
    }
      // Helper method to draw box casts for debugging
    private void DrawBoxCast2D(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance, Color color)
    {
        // Draw the box at the origin
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        
        // Create rotated corner vectors
        Vector2 topLeftOffset = rotation * new Vector2(-size.x / 2, size.y / 2);
        Vector2 topRightOffset = rotation * new Vector2(size.x / 2, size.y / 2);
        Vector2 bottomLeftOffset = rotation * new Vector2(-size.x / 2, -size.y / 2);
        Vector2 bottomRightOffset = rotation * new Vector2(size.x / 2, -size.y / 2);
        
        // Add these to origin to get box corners
        Vector2 topLeft = new Vector2(origin.x + topLeftOffset.x, origin.y + topLeftOffset.y);
        Vector2 topRight = new Vector2(origin.x + topRightOffset.x, origin.y + topRightOffset.y);
        Vector2 bottomLeft = new Vector2(origin.x + bottomLeftOffset.x, origin.y + bottomLeftOffset.y);
        Vector2 bottomRight = new Vector2(origin.x + bottomRightOffset.x, origin.y + bottomRightOffset.y);
        
        // Draw the box outline
        Debug.DrawLine(topLeft, topRight, color);
        Debug.DrawLine(topRight, bottomRight, color);
        Debug.DrawLine(bottomRight, bottomLeft, color);
        Debug.DrawLine(bottomLeft, topLeft, color);
        
        // Calculate end position
        Vector2 endOrigin = new Vector2(
            origin.x + direction.x * distance, 
            origin.y + direction.y * distance
        );
        
        // Add rotated offsets to end position
        Vector2 endTopLeft = new Vector2(endOrigin.x + topLeftOffset.x, endOrigin.y + topLeftOffset.y);
        Vector2 endTopRight = new Vector2(endOrigin.x + topRightOffset.x, endOrigin.y + topRightOffset.y);
        Vector2 endBottomLeft = new Vector2(endOrigin.x + bottomLeftOffset.x, endOrigin.y + bottomLeftOffset.y);
        Vector2 endBottomRight = new Vector2(endOrigin.x + bottomRightOffset.x, endOrigin.y + bottomRightOffset.y);
        
        // Draw the destination box
        Debug.DrawLine(endTopLeft, endTopRight, color);
        Debug.DrawLine(endTopRight, endBottomRight, color);
        Debug.DrawLine(endBottomRight, endBottomLeft, color);
        Debug.DrawLine(endBottomLeft, endTopLeft, color);
    }
    
    // Helper method to draw a circle using Debug.DrawLine
    private void DrawDebugCircle(Vector2 center, float radius, Color color)
    {
        // Number of segments to use for the circle approximation
        int segments = 16;
        float angleDelta = 360f / segments;
        
        // Draw each segment of the circle
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleDelta * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleDelta * Mathf.Deg2Rad;
            
            Vector2 point1 = new Vector2(
                center.x + radius * Mathf.Cos(angle1),
                center.y + radius * Mathf.Sin(angle1)
            );
            
            Vector2 point2 = new Vector2(
                center.x + radius * Mathf.Cos(angle2),
                center.y + radius * Mathf.Sin(angle2)
            );
            
            Debug.DrawLine(point1, point2, color);
        }
    }
    
    // Handle horizontal movement based on movement mode
    private void HandleMovement(float smoothedInput)
    {
        if (useRigidbody && body != null)
        {
            // Gunakan smoothing pada velocity X
            float targetVelocityX = smoothedInput * speed;
            body.velocity = new Vector2(targetVelocityX, body.velocity.y);
        }
        else
        {
            transform.position += new Vector3(smoothedInput * speed * Time.fixedDeltaTime, 0, 0);
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
        
        // Decrease dodge cooldown
        if (currentDodgeCooldown > 0)
        {
            currentDodgeCooldown -= Time.fixedDeltaTime;
        }
        
        // Removed throwTimer as we're now using lempar.IsAvailable
        
        // Reset air dodge ability when landing
        if (grounded && hasAirDodged)
        {
            hasAirDodged = false;
        }
        
        // Additional cooldowns can be added here as needed
    }
      // Update is called once per frame
    void Update()
    {
        // Get horizontal input
        horizontalInputRaw = Input.GetAxisRaw("Horizontal");

        // Smooth horizontal input
        horizontalVelocity = Mathf.SmoothDamp(horizontalVelocity, horizontalInputRaw, ref velocityXSmoothing, accelerationTime);

        // Flip player when moving left-right
        if (horizontalInputRaw > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
        else if (horizontalInputRaw < -0.01f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
        
        // Modified: Check for both Space key and Up Arrow key for jumping
        bool jumpKeyDown = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow);
        bool jumpKeyUp = Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.UpArrow);
        
        // Reset tracking when key is released
        if (jumpKeyUp)
        {
            hasJumpedThisPress = false;
            Debug.Log("Jump key released - reset jump tracking");
        }
        
        // Only register jump input if definitely on ground or touching wall
        if (jumpKeyDown && ((grounded && jumpCooldown <= 0) || (isTouchingWall && !grounded)))
        {
            jumpPressed = true;
            Debug.Log("JUMP REGISTERED - " + (grounded ? "Character is on ground" : "Character is wall sliding"));
        }
        
        // Track time in the air to prevent mid-air jumps
        if (!grounded)
        {
            airTime += Time.deltaTime;
        }
        else
        {
            airTime = 0f;
            isJumping = false; // Reset jumping state when on ground
        }
        
        // Modified: Handle throwing objects with X key - Now matching PlayerLempar logic
        if (Input.GetKeyDown(KeyCode.X) && lempar != null && lempar.IsAvailable)
        {
            lempar.Throw();
            // No need for throwTimer now as we rely on lempar.IsAvailable
            
            // Optional: Play throw animation
            // anim.SetTrigger("throw");
            
            Debug.Log("Player threw an object");
        }
        
        // Modified: Handle attack input with C key instead of X
        if (Input.GetKeyDown(KeyCode.C) && attackCooldown <= 0)
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

        // Handle dodge input with Z key - Modified to allow air dodges
        if (Input.GetKeyDown(KeyCode.Z) && currentDodgeCooldown <= 0 && 
           (grounded || !hasAirDodged || allowMultipleAirDodges) && !isDodging)
        {
            StartDodge();
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

        // Add test key for dodge
        if (Input.GetKeyDown(KeyCode.Alpha8)) // Press 8 to force dodge animation
        {
            anim.SetBool("dodge", true);
            Debug.Log("FORCED: Dodge animation!");
            
            // Auto-reset dodge animation after a delay
            Invoke("ResetDodgeAnimation", 0.5f);
        }

        UpdateAnimations(horizontalVelocity);

        // Toggle movement method with T key (for testing)
        if (Input.GetKeyDown(KeyCode.T))
        {
            useRigidbody = !useRigidbody;
        }
    }
    
    // Separated animation logic for cleaner code
    private void UpdateAnimations(float smoothedInput)
    {
        if (anim == null) return;
        
        // Dodge animation has priority over other animations
        if (isDodging)
        {
            anim.SetBool("dodge", true);
            return; // Skip other animation updates when dodging
        }
        else
        {
            anim.SetBool("dodge", false);
        }
        
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
            anim.SetBool("run", Mathf.Abs(smoothedInput) > 0.1f && Mathf.Abs(body.velocity.y) < 0.1f);
        }
        else if (Mathf.Abs(smoothedInput) <= 0.1f)
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
    }    // Enhanced ground check method with better detection and debugging
    private void CheckGrounded()
    {
        // Get the check position (use a lower position to ensure detection)
        Vector2 position = groundCheck != null ? groundCheck.position : new Vector2(transform.position.x, transform.position.y - 0.5f);
        
        // Store previous state to detect changes
        bool wasGrounded = grounded;
        
        // Increase detection parameters for more reliable ground checks
        float width = 0.5f; // Wider check area
        float checkDistance = groundCheckDistance * 1.5f; // Increase the check distance
        float circleRadius = 0.15f; // Use larger radius for better detection
        
        // Cast rays from multiple positions under the player
        bool centerGrounded = Physics2D.CircleCast(position, circleRadius, Vector2.down, checkDistance, groundLayer);
        bool leftGrounded = Physics2D.CircleCast(new Vector2(position.x - width/2, position.y), circleRadius, Vector2.down, checkDistance, groundLayer);
        bool rightGrounded = Physics2D.CircleCast(new Vector2(position.x + width/2, position.y), circleRadius, Vector2.down, checkDistance, groundLayer);
        
        // Also check if we're directly on ground (for thin platforms)
        bool directGrounded = Physics2D.OverlapCircle(new Vector2(position.x, position.y - 0.1f), circleRadius, groundLayer);        // Enhanced ground detection with multiple safety checks
        bool groundContactDetected = centerGrounded || leftGrounded || rightGrounded || directGrounded;
        
        // Only consider grounded if ALL of these conditions are met:
        // 1. Ground contact detected
        // 2. Not moving upward significantly
        // 3. Not flagged as currently jumping
        // 4. Not in the early phase of a jump (airTime check)
        grounded = groundContactDetected && 
                  body.velocity.y <= 0.1f && 
                  (!isJumping || airTime > 0.5f); // Only unflag "isJumping" after sufficient air time
                  
        // Debug visuals to show ground state
        if (grounded)
        {
            Debug.DrawRay(transform.position, Vector2.right * 0.5f, Color.green, 0.1f);
        }
        else
        {
            Debug.DrawRay(transform.position, Vector2.right * 0.5f, Color.red, 0.1f);
        }
        
        // Additional check: if vertical velocity is very low, consider as grounded
        if (Mathf.Abs(body.velocity.y) < 0.1f && body.velocity.y >= 0)
        {
            // If we're standing still or moving upward very slightly, check for ground more aggressively
            RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, checkDistance * 1.2f, groundLayer);
            if (hit.collider != null)
            {
                grounded = true;
                Debug.Log("Zero velocity ground contact detected");
            }
        }
        
        // Visualize ground checks with more info
        Debug.DrawRay(position, Vector2.down * checkDistance, centerGrounded ? Color.green : Color.red);        Debug.DrawRay(new Vector2(position.x - width/2, position.y), Vector2.down * checkDistance, leftGrounded ? Color.green : Color.red);
        Debug.DrawRay(new Vector2(position.x + width/2, position.y), Vector2.down * checkDistance, rightGrounded ? Color.green : Color.red);
        
        // Draw approximate circle using Debug.DrawLine for overlap check
        Color circleColor = directGrounded ? Color.cyan : Color.magenta;
        Vector2 circleCenter = new Vector2(position.x, position.y - 0.1f);
        DrawDebugCircle(circleCenter, circleRadius, circleColor);
        
        // Enhanced logging for ground detection issues
        if (wasGrounded != grounded)
        {
            string detectionInfo = "Center: " + centerGrounded + 
                                  ", Left: " + leftGrounded + 
                                  ", Right: " + rightGrounded + 
                                  ", Direct: " + directGrounded;
            Debug.Log("Grounded state changed to: " + grounded + " (" + detectionInfo + ")");
            Debug.Log("Player Y velocity: " + body.velocity.y);
        }
        
        // Update animator state
        if (anim != null)
        {
            anim.SetBool("grounded", grounded);
        }
    }    // Completely overhauled jump method with absolute air control
    private void Jump()
    {
        if (useRigidbody && body != null)
        {
            // FINAL SAFETY CHECK: Make sure we're not already jumping or in the air
            if (isJumping || body.velocity.y > 0.5f || !grounded)
            {
                Debug.LogWarning("JUMP BLOCKED - Already in air! This should never happen!");
                return;
            }
            
            // Apply vertical force for jumping
            body.velocity = new Vector2(body.velocity.x, jumpForce);
            
            // Update animations
            anim.SetBool("jump", true);
            anim.SetBool("fall", false);
            anim.SetBool("grounded", false);
            
            // Set all air state variables
            grounded = false;
            wasGroundedLastFrame = false;
            isJumping = true;
            airTime = 0.01f; // Initialize air time
            
            // Force disable ground detection for a short time to prevent false positives
            StartCoroutine(DisableGroundDetection());
            
            // Log debug info
            Debug.Log("JUMP EXECUTED! Force: " + jumpForce + " - Air control enabled");
        }
    }
    
    // Coroutine to temporarily disable ground detection after jumping
    private System.Collections.IEnumerator DisableGroundDetection()
    {
        // Temporarily disable ground check to prevent immediate re-landing
        float originalGroundCheckDistance = groundCheckDistance;
        groundCheckDistance = 0;
        
        // Wait a short time
        yield return new WaitForSeconds(0.1f);
        
        // Restore ground check
        groundCheckDistance = originalGroundCheckDistance;
    }
    
    // Wall jump with improved physics handling
    private void WallJump()
    {
        if (useRigidbody && body != null)
        {
            // Jump away from wall (horizontally and vertically)
            float horizontalDirection = transform.localScale.x > 0 ? -1 : 1;
            body.velocity = new Vector2(horizontalDirection * speed * 0.75f, jumpForce * 0.8f);
            
            // Flip character direction
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            
            // Update animations
            anim.SetBool("jump", true);
            anim.SetBool("fall", false);
            anim.SetBool("wallSlide", false);
            
            // Force update states
            grounded = false;
            isTouchingWall = false;
            
            // Log debug info
            Debug.Log("Wall jump executed!");
        }
    }    // Enhanced collision detection to complement raycast ground detection
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check for ground collisions - only consider contacts on the bottom of the character
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // If contact normal is pointing upward, we're on top of something
            if (contact.normal.y > 0.7f)
            {
                // Check if the collision object is on the ground layer
                if (((1 << collision.gameObject.layer) & groundLayer) != 0)
                {
                    Debug.Log("Ground collision detected with: " + collision.gameObject.name);
                    grounded = true;
                    lastGroundedTime = Time.time;
                    
                    // Update animator
                    if (anim != null)
                    {
                        anim.SetBool("grounded", true);
                    }
                    
                    break;
                }
            }
        }
    }
    
    private void OnCollisionStay2D(Collision2D collision)
    {
        // Similar logic to OnCollisionEnter2D to ensure continued ground detection
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f) // Less strict check for staying on ground
            {
                if (((1 << collision.gameObject.layer) & groundLayer) != 0)
                {
                    grounded = true;
                    lastGroundedTime = Time.time;
                    break;
                }
            }
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        // We still primarily rely on CheckGrounded for determining when we leave the ground
        // But we can detect specific ground colliders being exited
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            // Don't immediately set grounded to false
            // Let the raycast method determine this to prevent false negatives
            Debug.Log("Exited collision with ground object: " + collision.gameObject.name);
        }
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
        anim.SetBool("dodge", false);
        // Remove grounded parameter reset
        // anim.SetBool("grounded", grounded);
    }    // Improved wall sliding detection
    private void CheckWallSliding()
    {
        // Only check for wall sliding if we're not grounded
        if (grounded)
        {
            // Not touching wall if grounded
            if (isTouchingWall)
            {
                isTouchingWall = false;
                anim.SetBool("wallSlide", false);
            }
            return;
        }
        
        // Cast rays at multiple heights to detect walls more reliably
        float raycastDistance = 0.5f;
        Vector2 direction = new Vector2(transform.localScale.x > 0 ? 1 : -1, 0);
        
        // Cast from center of character
        RaycastHit2D hitCenter = Physics2D.Raycast(transform.position, direction, raycastDistance, wallLayer);
        
        // Cast from a bit higher
        RaycastHit2D hitUpper = Physics2D.Raycast(
            new Vector2(transform.position.x, transform.position.y + 0.5f), 
            direction, raycastDistance, wallLayer
        );
        
        // Cast from a bit lower
        RaycastHit2D hitLower = Physics2D.Raycast(
            new Vector2(transform.position.x, transform.position.y - 0.3f), 
            direction, raycastDistance, wallLayer
        );
        
        // Debug visualization
        Debug.DrawRay(transform.position, direction * raycastDistance, hitCenter ? Color.red : Color.green);
        Debug.DrawRay(new Vector2(transform.position.x, transform.position.y + 0.5f), direction * raycastDistance, hitUpper ? Color.red : Color.green);
        Debug.DrawRay(new Vector2(transform.position.x, transform.position.y - 0.3f), direction * raycastDistance, hitLower ? Color.red : Color.green);
        
        // Player is touching wall if any raycast hits and is falling
        bool touchingWallNow = (hitCenter || hitUpper || hitLower) && body.velocity.y < 0;
        
        // Log state changes
        if (touchingWallNow != isTouchingWall)
        {
            Debug.Log("Wall touching state changed: " + touchingWallNow);
            isTouchingWall = touchingWallNow;
        }
        
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
    }    // Enhanced visualization for debugging collisions and physics
    private void OnDrawGizmosSelected()
    {
        // Determine positions for visualization
        Vector3 position = transform.position;
        Vector3 groundCheckPosition = groundCheck != null ? groundCheck.position : new Vector3(position.x, position.y - 0.5f, 0);
        
        // Calculate sizes based on character
        float characterWidth = 0.8f;
        float checkDistance = groundCheckDistance;
        if (Application.isPlaying && body != null)
        {
            // Use actual collider size when available
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                characterWidth = collider.bounds.size.x;
            }
        }
        
        // Draw standard ground check areas
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheckPosition, 0.2f);
        
        // Draw expanded ground checks
        Gizmos.color = new Color(1f, 0.7f, 0, 0.7f); // Orange semi-transparent
        Gizmos.DrawWireSphere(new Vector3(groundCheckPosition.x - characterWidth/2, groundCheckPosition.y, 0), 0.15f);
        Gizmos.DrawWireSphere(new Vector3(groundCheckPosition.x + characterWidth/2, groundCheckPosition.y, 0), 0.15f);
        
        // Draw box cast area
        Gizmos.color = new Color(0, 1f, 1f, 0.5f); // Cyan semi-transparent
        Vector3 boxCenter = new Vector3(position.x, position.y - 0.4f, 0);
        Vector3 boxSize = new Vector3(characterWidth, 0.2f, 0.1f);
        Gizmos.DrawWireCube(boxCenter, boxSize);
        Gizmos.DrawWireCube(boxCenter + new Vector3(0, -0.1f, 0), boxSize);
        
        // Draw wall check areas
        Vector3 wallCheckDirection = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        Gizmos.color = Color.blue;
        
        // Multiple wall check points
        float wallRayDistance = 0.5f;
        Gizmos.DrawLine(position, position + wallCheckDirection * wallRayDistance);
        Gizmos.DrawLine(position + new Vector3(0, 0.5f, 0), 
                        position + new Vector3(0, 0.5f, 0) + wallCheckDirection * wallRayDistance);
        Gizmos.DrawLine(position + new Vector3(0, -0.3f, 0), 
                        position + new Vector3(0, -0.3f, 0) + wallCheckDirection * wallRayDistance);
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
    
    // Public method to check if player is currently immune to damage
    public bool IsImmuneToDamage()
    {
        return isDamageImmune;
    }
    
    // Add a helper method to check if an attack animation is currently playing
    public bool IsAttacking()
    {
        return anim.GetBool("attack");
    }
    
    // Add a helper method to check if a dodge is currently active
    public bool IsDodging()
    {
        return isDodging;
    }

    // Start a dodge
    private void StartDodge()
    {
        isDodging = true;
        isDamageImmune = true;
        dodgeTimer = 0f;
        
        // Set animation
        anim.SetBool("dodge", true);
        
        // Apply dodge speed in the facing direction
        float direction = transform.localScale.x > 0 ? 1f : -1f;
        
        // Different handling for ground vs air dodge
        if (grounded)
        {
            // Ground dodge - standard implementation
            float yVelocity = body.velocity.y;
            body.velocity = new Vector2(direction * dodgeSpeed, yVelocity);
            
            Debug.Log("Player performed ground dodge!");
        }
        else
        {
            // Air dodge - special implementation
            float yVelocity = maintainYVelocityInAirDodge ? body.velocity.y : 0;
            
            // Apply air dodge force - option to add upward boost
            if (airDodgeUpwardForce > 0 && body.velocity.y < 0) 
            {
                // Only add upward force if falling (stops falling momentum)
                yVelocity = airDodgeUpwardForce;
            }
            
            body.velocity = new Vector2(direction * airDodgeSpeed, yVelocity);
            
            // Mark that we've used our air dodge
            hasAirDodged = true;
            
            Debug.Log("Player performed air dodge!");
        }
        
        // Start cooldown
        currentDodgeCooldown = dodgeCooldown;
    }
    
    // Handle dodge physics and timing
    private void HandleDodge()
    {
        // Update dodge timer
        dodgeTimer += Time.fixedDeltaTime;
        
        if (dodgeTimer >= dodgeDuration)
        {
            EndDodge();
        }
        else
        {
            // Maintain dodge speed
            float direction = transform.localScale.x > 0 ? 1f : -1f;
            
            // Different handling for ground vs air dodge
            if (grounded)
            {
                // Only override horizontal velocity on ground
                body.velocity = new Vector2(direction * dodgeSpeed, body.velocity.y);
            }
            else
            {
                // For air dodge, we've already set velocity in StartDodge
                // Optional: maintain horizontal velocity only
                body.velocity = new Vector2(direction * airDodgeSpeed, body.velocity.y);
            }
        }
    }
    
    // End the dodge state
    private void EndDodge()
    {
        isDodging = false;
        anim.SetBool("dodge", false);
        
        // Keep damage immunity a bit longer than the dodge itself
        Invoke("EndDamageImmunity", 0.2f);
        
        Debug.Log("Dodge ended, immunity remains briefly.");
    }
    
    // End damage immunity
    private void EndDamageImmunity()
    {
        isDamageImmune = false;
        Debug.Log("Damage immunity ended.");
    }
    
    // Reset dodge animation (used by Invoke)
    private void ResetDodgeAnimation()
    {
        anim.SetBool("dodge", false);
    }
}
