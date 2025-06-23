using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // Pindah ke scene bernama "Play"
    public void GoToPlay()
    {
        SceneManager.LoadScene("play");
    }

    // Pindah ke scene bernama "Credit"
    public void GoToCredit()
    {
        SceneManager.LoadScene("credit");
    }

    // Keluar dari aplikasi
    public void ExitGame()
    {
        Debug.Log("Keluar dari game...");
        Application.Quit();
    }
}
