using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float attackColldown;
    [SerializeField] private float range;
    [SerializeField] private float colliderDistance;
    [SerializeField] private int damage;
    [SerializeField] private BoxCollider2D BoxCollider;
    private float cooldownTimer = Mathf.Infinity;
    private Animator animator;

 


    private void Awake()
    {
        animator = GetComponent<Animator>();

    }

    private void Update()
    {
        cooldownTimer += Time.deltaTime;

        if (PlayerInSight())
        {
            if (cooldownTimer >= attackColldown)
            {
                cooldownTimer = 0;
                animator.SetTrigger("attack");
            }
        }
    }


    private bool PlayerInSight()
    {
        RaycastHit2D hit = Physics2D.BoxCast(BoxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance, new Vector3(BoxCollider.bounds.size.x * range, BoxCollider.bounds.size.y, BoxCollider.bounds.size.z), 0, Vector2.left, 0);
        // Implement logic to check if the player is in sight
        // This could involve raycasting, checking distances, etc.
        return hit.collider != null && hit.collider.CompareTag("Player"); // Placeholder for demonstration
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(BoxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,
        new Vector3(BoxCollider.bounds.size.x * range, BoxCollider.bounds.size.y, BoxCollider.bounds.size.z));
    }
    
    private void DamagePlayer()
    {
        RaycastHit2D hit = Physics2D.BoxCast(
            BoxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,
            new Vector3(BoxCollider.bounds.size.x * range, BoxCollider.bounds.size.y, BoxCollider.bounds.size.z),
            0, Vector2.left, 0);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            // Cek apakah player punya SistemNyawa
            SistemNyawa sistemNyawa = hit.collider.GetComponent<SistemNyawa>();
            if (sistemNyawa != null)
            {
                // Panggil fungsi pengurangan nyawa
                sistemNyawa.SendMessage("KurangiNyawa");
            }
        }
    }

}
