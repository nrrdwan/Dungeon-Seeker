using UnityEngine;

public class LemparMob : MonoBehaviour
{
    public GameObject peluruPrefab;
    public Transform titikLempar; // tempat peluru keluar (misal mulut mob)
    public float jarakLempar = 5f;
    public float kecepatanLempar = 5f;
    public float delayLempar = 2f;

    private float waktuTerakhirLempar;
    private Transform targetPlayer;

    private void Start()
    {
        targetPlayer = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (targetPlayer == null) return;

        float jarak = Vector2.Distance(transform.position, targetPlayer.position);
        if (jarak <= jarakLempar && Time.time > waktuTerakhirLempar + delayLempar)
        {
            LemparKePlayer();
            waktuTerakhirLempar = Time.time;
        }
    }

    void LemparKePlayer()
    {
        GameObject peluru = Instantiate(peluruPrefab, titikLempar.position, Quaternion.identity);
        Rigidbody2D rb = peluru.GetComponent<Rigidbody2D>();

        Vector2 arah = (targetPlayer.position - titikLempar.position).normalized;
        rb.velocity = arah * kecepatanLempar;
    }
}
