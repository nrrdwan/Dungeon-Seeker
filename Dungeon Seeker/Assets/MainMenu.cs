using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Exit Confirmation Panel")]
    public GameObject exitConfirmationPanel; // Panel konfirmasi keluar
    
    void Start()
    {
        // Pastikan panel konfirmasi tersembunyi saat game dimulai
        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(false);
        }
    }
    
    public void TombolKeluar()
    {
        // Tampilkan panel konfirmasi instead of langsung quit
        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(true);
        }
        else
        {
            // Fallback jika panel tidak di-assign
            Application.Quit();
            Debug.Log("Game Close");
        }
    }
    
    // Method untuk tombol "Ya" di panel konfirmasi
    public void ConfirmExit()
    {
        Application.Quit();
        Debug.Log("Game Close");
    }
    
    // Method untuk tombol "Tidak" di panel konfirmasi
    public void CancelExit()
    {
        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(false);
        }
    }

    public void Mainkan()
    {
        SceneManager.LoadScene("pilihlevel"); // Ganti dengan nama scene kamu
    }

    public void upgrade()
    {
        SceneManager.LoadScene("upgrade"); // Ganti dengan nama scene kamu
    }

    public void kembali_atau_back()
    {
        SceneManager.LoadScene("mainmenu"); // Ganti dengan nama scene kamu
    }

    public void level1()
    {
        SceneManager.LoadScene("level1"); // Ganti dengan nama scene kamu
    }

    public void level2()
    {
        SceneManager.LoadScene("level2"); // Ganti dengan nama scene kamu
    }

    public void level3()
    {
        SceneManager.LoadScene("level3"); // Ganti dengan nama scene kamu
    }

    public void Credit()
    {
        SceneManager.LoadScene("credit"); // Ganti dengan nama scene credit kamu
    }
}
