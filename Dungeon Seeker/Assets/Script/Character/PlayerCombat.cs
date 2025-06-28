using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int maxComboCount = 3;
    [SerializeField] private float comboResetTime = 1.0f; // Reset combo jika idle
    
    [Header("Attack Properties")]
    [SerializeField] private float attackDamage = 25f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private LayerMask enemyLayer;
    
    private float lastAttackTime = 0f;
    private int currentCombo = 0;
    private bool canAttack = true;

    // Component references
    private Animator anim;

    public void Initialize(Animator animator)
    {
        anim = animator;
    }

    public void HandleAttackInput()
    {
        // Handle attack input (C key only) - TANPA JEDA APAPUN
        if (Input.GetKeyDown(KeyCode.C))
        {
            // LANGSUNG attack tanpa pengecekan interval
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        lastAttackTime = Time.time;

        // Increment combo sesuai urutan press
        currentCombo++;
        if (currentCombo > maxComboCount)
        {
            currentCombo = 1; // Loop kembali
        }

        // FORCE IMMEDIATE ANIMATION
        ForceAttackAnimation();

        // Execute attack immediately
        ExecuteAttack();

        Debug.Log($"FORCE Attack {currentCombo} - Press count!");
    }

    private void ForceAttackAnimation()
    {
        if (anim != null)
        {
            StopAllCoroutines();
            anim.ResetTrigger("attack"); // Reset dulu
            anim.SetTrigger("attack");   // Langsung trigger
            Debug.Log($"FORCE ANIMATION - Attack Trigger (Combo {currentCombo})");
        }
    }

    private IEnumerator ForceReset()
    {
        // Wait minimal untuk animasi
        yield return new WaitForSeconds(0.2f);
        
        // Force reset
        if (anim != null)
        {
            anim.SetBool("attack", false);
            Debug.Log("FORCE RESET - Attack Bool = FALSE");
        }
    }

    private void ExecuteAttack()
    {
        Vector2 attackPosition = GetAttackPosition();
        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(attackPosition, attackRange, enemyLayer);
        
        foreach (Collider2D enemy in enemiesHit)
        {
            // Gunakan SistemNyawaMob sebagai pengganti Health
            var enemyHealth = enemy.GetComponent<SistemNyawaMob>();
            if (enemyHealth != null)
            {
                // Kurangi nyawa berdasarkan combo
                for (int i = 0; i < currentCombo; i++)
                {
                    enemyHealth.KurangiNyawa();
                }
                
                Debug.Log($"Hit {enemy.name} with combo {currentCombo}!");
            }

            var enemyRb = enemy.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                float knockbackForce = 2f + (currentCombo * 0.8f);
                enemyRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            }
        }

        CreateAttackEffect(attackPosition);
    }

    private Vector2 GetAttackPosition()
    {
        float direction = transform.localScale.x > 0 ? 1f : -1f;
        Vector2 attackOffset = new Vector2(direction * attackRange * 0.8f, 0.1f);
        return (Vector2)transform.position + attackOffset;
    }

    private void CreateAttackEffect(Vector2 position)
    {
        Color effectColor = GetComboColor();
        float effectSize = 0.3f + (currentCombo * 0.15f);
        
        Debug.DrawRay(position, Vector2.up * effectSize, effectColor, 0.3f);
        Debug.DrawRay(position, Vector2.down * effectSize, effectColor, 0.3f);
        Debug.DrawRay(position, Vector2.left * effectSize, effectColor, 0.3f);
        Debug.DrawRay(position, Vector2.right * effectSize, effectColor, 0.3f);
    }

    private Color GetComboColor()
    {
        switch (currentCombo)
        {
            case 1: return Color.white;
            case 2: return Color.yellow;
            case 3: return Color.red;
            default: return Color.white;
        }
    }

    private void Update()
    {
        // Reset combo jika idle terlalu lama (tapi tidak menghalangi input)
        if (Time.time - lastAttackTime > comboResetTime && currentCombo > 0)
        {
            currentCombo = 0;
            Debug.Log("Combo reset - ready for new sequence");
        }
    }

    // Method untuk test LANGSUNG di inspector atau debug
    [ContextMenu("Test Attack Animation")]
    public void TestAttackAnimation()
    {
        if (anim != null)
        {
            anim.SetBool("attack", false);
            anim.Update(0f);
            anim.SetBool("attack", true);
            Debug.Log("TEST ATTACK ANIMATION - Should work immediately!");
        }
    }

    // Method untuk forced combo reset
    public void ResetCombo()
    {
        currentCombo = 0;
        StopAllCoroutines();
        if (anim != null)
        {
            anim.SetBool("attack", false);
        }
        Debug.Log("Combo manually reset");
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            // Attack range visualization
            Gizmos.color = GetComboColor();
            Vector2 attackPos = GetAttackPosition();
            Gizmos.DrawWireSphere(attackPos, attackRange);
            
            // Hand speed indicator
            if (currentCombo > 0)
            {
                Gizmos.color = Color.green; // Green = ready for hand speed
                float size = 0.1f + (currentCombo * 0.1f);
                Gizmos.DrawWireCube(transform.position + Vector3.up * 2.5f, Vector3.one * size);
            }
        }
    }

    // Public getters
    public bool IsAttacking() => false; // Always ready for instant attack
    public int GetCurrentCombo() => currentCombo;
    public bool CanAttack() => canAttack;
    public float GetHandSpeedInterval() => Time.time - lastAttackTime;
}
