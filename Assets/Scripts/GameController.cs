using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    [Header("Gameplay characteristics")]
    [SerializeField] int playerLives = default;
    public int PlayerLives => this.playerLives;
    public void SetPlayersLives(int inLives)
    {
        this.playerLives += inLives;

        this.livesText.text = $"Lives: {this.playerLives}";
    }

    [Header("UI Characteristics")]
    [SerializeField] TextMeshProUGUI livesText;
    [SerializeField] InputAction pauseButton;
    [SerializeField] GameObject pauseMenu;


    //Cached references
    private ScoreController scoreController;
    private LeftHand leftHand;


    //Boolean variables
    private bool isGamePaused = false;

    private void Awake()
    {
        this.pauseButton.Enable();
        this.scoreController = GameObject.FindObjectOfType<ScoreController>();
        this.leftHand = GameObject.FindObjectOfType<LeftHand>();
    }
    private void OnDestroy()
    {
        this.pauseButton.Disable();
    }

    private void Start()
    {
        this.livesText.text = $"Lives: {this.playerLives}";
    }

    private void Update()
    {
        if (this.pauseButton.triggered)
            this.isGamePaused = !this.isGamePaused;

        ManagePauseState();

    }

    private void ManagePauseState()
    {
        if(this.isGamePaused && !Mathf.Approximately(Time.timeScale, 0))
        {
            Time.timeScale = 0;
            AudioListener.pause = true;

            this.leftHand.gameObject.SetActive(false);
            foreach (Ball item in this.scoreController.Balls)
                item.GetComponentInChildren<SpriteRenderer>().enabled = false;

            this.pauseMenu.SetActive(true);
        }
        else if(!this.isGamePaused && Mathf.Approximately(Time.timeScale, 0))
        {
            Time.timeScale = 1;
            AudioListener.pause = false;

            this.leftHand.gameObject.SetActive(true);
            foreach (Ball item in this.scoreController.Balls)
                item.GetComponentInChildren<SpriteRenderer>().enabled = true;

            this.pauseMenu.SetActive(false);
        }
    }

    public void LooseOneLife()
    {
        SetPlayersLives(-1);

        this.StartCoroutine(BallLostEffect(this.playerLives));
    }

    public void AddOneLife()
    {
        SetPlayersLives(1);
    }

    private IEnumerator BallLostEffect(int lives)
    {
        Time.timeScale = 0.0f;
        // Play SFX

        yield return new WaitForSecondsRealtime(1.5f); // Really add the time till the sfx is done playing

        Time.timeScale = 1.0f;

        if(this.playerLives <= 0)
        {
            this.playerLives = 0;
            this.livesText.text = $"Lives: {this.playerLives}";

            // TODO: Set game ending logic
        }
    }
}
