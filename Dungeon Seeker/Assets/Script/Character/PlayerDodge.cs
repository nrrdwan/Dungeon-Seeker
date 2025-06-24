using UnityEngine;
using System.Collections;

public class PlayerDodge : MonoBehaviour
{
    [Header("Dodge Settings")]
    [SerializeField] private float dodgeForce = 10f;
    [SerializeField] private float dodgeDuration = 0.3f;
    [SerializeField] private float dodgeCooldown = 1f;
    
    [Header("Invincibility")]
    [SerializeField] private float invincibilityDuration = 0.5f;
    [SerializeField] private LayerMask originalLayer = 8; // Player layer
    [SerializeField] private LayerMask dodgeLayer = 11;   // Player dodge layer (no collision with enemies)

    private bool isDodging = false;
    private bool canDodge = true;
    private bool isInvincible = false;
    private float lastDodgeTime = 0f;
    private Vector2 dodgeDirection;
    private bool dodgeImpulseApplied = false; // Track apakah impulse sudah di-apply

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

    public void HandleDodgeInput()
    {
        // Handle dodge input (Left Shift or Z key)
        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Z)) && canDodge && !isDodging)
        {
            if (Time.time - lastDodgeTime >= dodgeCooldown)
            {
                StartDodge();
            }
        }
    }

    public void HandleDodgePhysics()
    {
        // HAPUS CONTINUOUS VELOCITY APPLICATION
        // Sekarang dodge hanya apply impulse sekali di StartDodge()
        // Tidak ada physics handling yang continuous
    }

    private void StartDodge()
    {
        isDodging = true;
        canDodge = false;
        lastDodgeTime = Time.time;
        dodgeImpulseApplied = false;

        // Determine dodge direction based on input
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            // Dodge in input direction
            dodgeDirection = new Vector2(horizontalInput, 0).normalized;
        }
        else
        {
            // Dodge in facing direction if no input
            float facingDirection = transform.localScale.x > 0 ? 1f : -1f;
            dodgeDirection = new Vector2(facingDirection, 0);
        }

        // APPLY IMPULSE SEKALI SAJA - bukan continuous velocity
        if (body != null)
        {
            // Apply dodge impulse (one-time burst)
            body.AddForce(dodgeDirection * dodgeForce, ForceMode2D.Impulse);
            dodgeImpulseApplied = true;
            
            Debug.Log($"Dodge impulse applied: {dodgeDirection * dodgeForce}");
        }

        // Start dodge coroutines
        StartCoroutine(DodgeMovement());
        StartCoroutine(DodgeInvincibility());

        // Update animations
        if (anim != null)
        {
            anim.SetBool("dodge", true);
            anim.SetTrigger("dodgeStart");
        }

        Debug.Log($"Dodge started in direction: {dodgeDirection}");
    }

    private IEnumerator DodgeMovement()
    {
        float elapsedTime = 0f;

        while (elapsedTime < dodgeDuration)
        {
            elapsedTime += Time.deltaTime;
            
            // OPTIONAL: Apply drag untuk stop dodge lebih cepat
            if (elapsedTime > dodgeDuration * 0.7f) // Setelah 70% duration
            {
                body.velocity = new Vector2(body.velocity.x * 0.9f, body.velocity.y);
            }
            
            yield return null;
        }

        // End dodge movement
        EndDodge();
    }

    private IEnumerator DodgeInvincibility()
    {
        // Start invincibility
        isInvincible = true;
        SetPlayerLayer(dodgeLayer);
        
        // Visual feedback for invincibility
        StartCoroutine(InvincibilityFlicker());

        yield return new WaitForSeconds(invincibilityDuration);

        // End invincibility
        isInvincible = false;
        SetPlayerLayer(originalLayer);
        
        Debug.Log("Invincibility ended");
    }

    private IEnumerator InvincibilityFlicker()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) yield break;

        Color originalColor = spriteRenderer.color;
        float flickerRate = 0.1f;

        while (isInvincible)
        {
            // Make player semi-transparent
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
            yield return new WaitForSeconds(flickerRate);
            
            // Restore normal color
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flickerRate);
        }

        // Ensure normal color is restored
        spriteRenderer.color = originalColor;
    }

    private void EndDodge()
    {
        isDodging = false;

        // STOP SLIDING - apply brake
        if (body != null)
        {
            body.velocity = new Vector2(body.velocity.x * 0.5f, body.velocity.y); // Reduce horizontal velocity
        }

        // Reset animations
        if (anim != null)
        {
            anim.SetBool("dodge", false);
        }

        Debug.Log("Dodge ended");
    }

    private void SetPlayerLayer(LayerMask layer)
    {
        // Convert LayerMask to layer number
        int layerNumber = Mathf.RoundToInt(Mathf.Log(layer.value, 2));
        gameObject.layer = layerNumber;
        
        // Also set layer for child objects if needed
        foreach (Transform child in transform)
        {
            child.gameObject.layer = layerNumber;
        }
    }

    public void UpdateCooldowns()
    {
        // Update dodge availability based on cooldown
        if (!canDodge && Time.time - lastDodgeTime >= dodgeCooldown)
        {
            canDodge = true;
            Debug.Log("Dodge available again");
        }
    }

    // Method to check if player should take damage (called by damage dealers)
    public bool ShouldTakeDamage()
    {
        return !isInvincible;
    }

    // Visual feedback for dodge direction
    private void OnDrawGizmosSelected()
    {
        if (isDodging)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, dodgeDirection * 2f);
        }

        // Show dodge cooldown status
        Gizmos.color = canDodge ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.2f);
    }

    // Public getters
    public bool IsDodging() => isDodging;
    public bool IsInvincible() => isInvincible;
    public bool CanDodge() => canDodge;
    public float GetDodgeCooldownProgress() => (Time.time - lastDodgeTime) / dodgeCooldown;
}
