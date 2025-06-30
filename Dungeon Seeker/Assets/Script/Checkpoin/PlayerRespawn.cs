using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private SistemNyawa sistemNyawa;

    private void Start()
    {
        sistemNyawa = GetComponent<SistemNyawa>();
    }

    public void Respawn()
    {
        Debug.Log("♻️ Respawn ke checkpoint...");
        transform.position = CheckpointManager.GetCheckpoint();

        if (sistemNyawa != null)
        {
            sistemNyawa.ResetNyawa(); // ✅ aman & sesuai standar OOP
        }
    }
}
