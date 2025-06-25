using UnityEngine;

public class PortalTeleporter : MonoBehaviour
{
    [Header("Titik Tujuan Teleport")]
    public Transform targetSpawnPoint; // BUKAN portalnya, tapi titik yang tepat

    [Header("Cooldown Teleport (anti bolak-balik)")]
    public float teleportCooldown = 1f;

    private bool canTeleport = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canTeleport) return;

        if (other.CompareTag("Player"))
        {
            // ✅ Pindahkan ke titik yang lebih tepat, bukan ke tengah portal
            other.transform.position = targetSpawnPoint.position;

            StartCoroutine(TeleportCooldown());
        }
    }

    private System.Collections.IEnumerator TeleportCooldown()
    {
        canTeleport = false;
        yield return new WaitForSeconds(teleportCooldown);
        canTeleport = true;
    }
}
