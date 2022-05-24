using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroMenu : MonoBehaviour
{
    [SerializeField] GameObject startMenu;
    [SerializeField] GameObject creditsWindow;

    public void OnStartGameClick()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentScene + 1);
    }

    public void OnCreditsClick()
    {
        this.startMenu.SetActive(false);
        this.creditsWindow.SetActive(true);
    }

    public void OnQuitClick()
    {
        Application.Quit();
    }

    public void OnReturnClick()
    {
        this.startMenu.SetActive(true);
        this.creditsWindow.SetActive(false);
    }
}
