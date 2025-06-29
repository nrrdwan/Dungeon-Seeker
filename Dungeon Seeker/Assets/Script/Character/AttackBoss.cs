using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBoss : MonoBehaviour
{
    private HealthBoss2 bossTerdekat;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bos"))
        {
            bossTerdekat = other.GetComponent<HealthBoss2>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Bos"))
        {
            if (bossTerdekat == other.GetComponent<HealthBoss2>())
            {
                bossTerdekat = null;
            }
        }
    }

    // Panggil fungsi ini saat serangan melee (misal dari PlayerMovement)
    public void TryHitBoss()
    {
        if (bossTerdekat != null)
        {
            bossTerdekat.KurangiNyawa();
        }
    }
}
