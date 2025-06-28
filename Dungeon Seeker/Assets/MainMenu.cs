using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void TombolKeluar()
    {
        Application.Quit();
        Debug.Log("Game Close");
    }

    public void Mainkan()
    {
        SceneManager.LoadScene("play"); // Ganti dengan nama scene kamu
    }

    public void Credit()
    {
        SceneManager.LoadScene("credit"); // Ganti dengan nama scene credit kamu
    }


}
