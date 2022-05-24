using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroMenu : MonoBehaviour
{
    [Header("Menu Windows")]
    [SerializeField] GameObject startMenu;
    [SerializeField] GameObject creditsWindow;


    [Header("Sound Characteristics")]
    private AudioSource audioSource;
    [SerializeField] AudioClip hoverOverSFX;
    [SerializeField] AudioClip clickSFX;


    private void Awake()
    {
        this.audioSource = this.gameObject.GetComponent<AudioSource>();
    }

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

    public void OnButtonEntered()
    {
        this.audioSource.PlayOneShot(this.hoverOverSFX);
    }

    public void OnButtonClicked()
    {
        this.audioSource.PlayOneShot(this.clickSFX);
    }
}
