using UnityEngine;
using UnityEngine.SceneManagement;

public class KembaliKeMainmenu : MonoBehaviour
{
    [Header("Nama scene MainMenu")]
    [SerializeField] private string namaMainMenu = "mainmenu";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC ditekan → Kembali ke MainMenu");
            SceneManager.LoadScene(namaMainMenu);
        }
    }
}
