using UnityEngine;
using UnityEngine.SceneManagement;

public class sound : MonoBehaviour
{
    private static sound instance;
    private AudioSource audioSource;

    // Daftar scene yang diperbolehkan untuk BGM tetap hidup
    private string[] allowedScenes = { "mainmenu", "credit", "pilihlevel", "upgrade" };

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
            SceneManager.sceneLoaded += OnSceneLoaded; // Daftar event saat ganti scene
        }
        else
        {
            Destroy(gameObject); // Hapus duplikat
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Jika scene baru tidak termasuk yang diizinkan, maka hapus BGM
        if (!IsSceneAllowed(scene.name))
        {
            Destroy(gameObject);
        }
    }

    private bool IsSceneAllowed(string sceneName)
    {
        foreach (string allowed in allowedScenes)
        {
            if (sceneName == allowed)
                return true;
        }
        return false;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Hapus event listener saat dihancurkan
    }
}
