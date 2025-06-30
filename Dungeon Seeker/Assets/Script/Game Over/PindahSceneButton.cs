using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PindahSceneButton : MonoBehaviour
{
    [Header("Nama scene tujuan (wajib sama persis dengan Build Settings)")]
    public string namaSceneTujuan;

    private Button tombol;

    private void Awake()
    {
        tombol = GetComponent<Button>();

        if (tombol != null)
        {
            tombol.onClick.AddListener(PindahScene);
        }
        else
        {
            Debug.LogWarning("⚠️ Tidak ada komponen Button di " + gameObject.name);
        }
    }

    public void PindahScene()
    {
        Time.timeScale = 1f; // ⚠️ Kembalikan waktu sebelum load scene

        // ✅ Hancurkan player jika ada
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Destroy(player);
            Debug.Log("🗑️ Player dihancurkan sebelum pindah scene.");
        }

        if (!string.IsNullOrEmpty(namaSceneTujuan))
        {
            Debug.Log("🔁 Pindah ke scene: " + namaSceneTujuan);
            SceneManager.LoadScene(namaSceneTujuan);
        }
        else
        {
            Debug.LogWarning("⚠️ Nama scene tujuan belum diisi di " + gameObject.name);
        }
    }
}
