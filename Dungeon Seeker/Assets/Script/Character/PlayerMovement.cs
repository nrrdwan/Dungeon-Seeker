using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] private float throwCooldown = 5f;
    [SerializeField] private float baseThrowCooldown = 5f; // Base cooldown value
    private float lastThrowTime = 0f;

    [Header("Combat Cooldown")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float baseAttackCooldown = 2f; // Base cooldown value
    private float lastAttackTime = 0f; // Waktu terakhir attack
    [SerializeField] private int maxComboCount = 3; // Maksimal combo sebelum cooldown
    private int currentComboCount = 0; // Combo saat ini
    [SerializeField] private float comboResetTime = 2f; // Waktu reset combo jika tidak input
    private float lastComboTime = 0f; // Waktu combo terakhir   

    // Component References
    private PlayerJump playerJump;
    private PlayerGroundDetection groundDetection;
    private PlayerCombat playerCombat;
    private PlayerDodge playerDodge;
    private PlayerAnimation playerAnimation;


    private void Awake()
    {
        // Jika kita sedang di scene MainMenu, langsung hancurkan player
        if (SceneManager.GetActiveScene().name == "mainmenu")
        {
            Destroy(gameObject);
            return;
        }

        // Cek apakah sudah ada player lain di DontDestroyOnLoad
        GameObject[] existingPlayers = GameObject.FindGameObjectsWithTag("Player");
        
        if (existingPlayers.Length > 1)
        {
            Debug.Log("🔄 Menghapus player duplikat...");
            foreach (GameObject player in existingPlayers)
            {
                if (player != this.gameObject && player.scene.name != "DontDestroyOnLoad")
                {
                    Destroy(player);
                }
            }
        }

        // Tetap hidup di antara scene
        if (transform.parent == null)
        {
            DontDestroyOnLoad(this.gameObject);
        }

        // Lanjutkan inisialisasi
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        originalScale = transform.localScale;
        startPosition = transform.position;
        ConfigureRigidbody();
        InitializeComponents();
        baseThrowCooldown = throwCooldown;
        baseAttackCooldown = attackCooldown;
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "mainmenu")
        {
            Debug.Log("🧹 MainMenu terdeteksi, player dihancurkan.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (PlayerStatTracker.Instance != null)
        {
            throwCooldown = PlayerStatTracker.Instance.throwCooldown;
            attackCooldown = PlayerStatTracker.Instance.attackCooldown;
        }

        // Tambahkan ini untuk memindahkan kembali ke posisi awal
        if (startPosition != Vector3.zero)
        {
            transform.position = startPosition;
            Debug.Log("🔁 Player dikembalikan ke posisi awal: " + startPosition);
        }
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

        // Handle attack input (now using C key) - Ubah ini
        HandleAttackInput();

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
        // Debug untuk cek status
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log($"X pressed - Time: {Time.time}, LastThrow: {lastThrowTime}, Cooldown: {throwCooldown}");
            Debug.Log($"Can throw: {Time.time >= lastThrowTime + throwCooldown}");
            Debug.Log($"Lempar available: {lempar != null && lempar.IsAvailable}");
        }

        // Cek cooldown terlebih dahulu
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (Time.time < lastThrowTime + throwCooldown)
            {
                float remainingTime = (lastThrowTime + throwCooldown) - Time.time;
                Debug.Log($"Throw on cooldown! Wait {remainingTime:F1} seconds");
                return; // Keluar dari method jika masih cooldown
            }

            if (lempar != null && lempar.IsAvailable)
            {
                lempar.Throw();
                lastThrowTime = Time.time;
                Debug.Log($"Player threw an object at time: {Time.time}");
            }
            else
            {
                Debug.Log("Cannot throw - lempar not available or null");
            }
        }
    }

    void HandleAttackInput()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            // Reset combo jika sudah terlalu lama tidak input
            if (Time.time - lastComboTime > comboResetTime)
            {
                currentComboCount = 0;
                Debug.Log("Combo reset due to timeout");
            }

            // Cek apakah sudah mencapai max combo dan masih dalam cooldown
            if (currentComboCount >= maxComboCount && Time.time < lastAttackTime + attackCooldown)
            {
                float remainingTime = (lastAttackTime + attackCooldown) - Time.time;
                Debug.Log($"Attack on cooldown! Wait {remainingTime:F1} seconds");
                return;
            }

            // Jika sudah melewati cooldown, reset combo
            if (currentComboCount >= maxComboCount && Time.time >= lastAttackTime + attackCooldown)
            {
                currentComboCount = 0;
                Debug.Log("Cooldown finished - combo reset");
            }

            // Lakukan attack
            if (playerCombat != null)
            {
                playerCombat.HandleAttackInput();
                currentComboCount++;
                lastComboTime = Time.time;
                
                Debug.Log($"Player attacked! Combo: {currentComboCount}/{maxComboCount}");
                
                // Jika sudah mencapai max combo, mulai cooldown
                if (currentComboCount >= maxComboCount)
                {
                    lastAttackTime = Time.time;
                    Debug.Log($"Max combo reached! Cooldown started for {attackCooldown} seconds");
                }
            }
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
        
        // Optional: Bisa tambahkan logic untuk UI cooldown indicator
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

    // Method to apply throw cooldown upgrade
    public void ApplyThrowCooldownUpgrade(float reduction)
    {
        throwCooldown = Mathf.Max(0.5f, baseThrowCooldown - reduction);
        Debug.Log($"Throw cooldown reduced to: {throwCooldown}");
    }

    // Method to apply attack cooldown upgrade
    public void ApplyAttackCooldownUpgrade(float reduction)
    {
        attackCooldown = Mathf.Max(0.2f, baseAttackCooldown - reduction);
        Debug.Log($"Attack cooldown reduced to: {attackCooldown}");
    }
}
