using UnityEngine;

public class PeluruMob : MonoBehaviour
{
    public float jarakMaksimum = 10f;
    private Vector3 posisiAwal;

    void Start()
    {
        posisiAwal = transform.position;
    }

    void Update()
    {
        float jarakTempuh = Vector3.Distance(posisiAwal, transform.position);
        if (jarakTempuh >= jarakMaksimum)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            SistemNyawa nyawa = other.GetComponent<SistemNyawa>();
            
            // Check if the player is immune to damage (during dodge)
            if (playerMovement != null && playerMovement.IsImmuneToDamage())
            {
                // Player is dodging, don't deal damage but still destroy the projectile
                Debug.Log("Player dodged the projectile!");
                Destroy(gameObject);
                return;
            }
            
            // Player is not immune, deal damage as normal
            if (nyawa != null)
            {
                nyawa.SendMessage("KurangiNyawa");
            }

            Destroy(gameObject); // peluru hilang setelah mengenai player
        }
    }
}
