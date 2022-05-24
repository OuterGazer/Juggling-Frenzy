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

        if (this.playerLives <= 0)
        {
            this.playerLives = 0;
        }

        this.livesText.text = $"Lives: {this.playerLives}";
    }

    [Header("UI Characteristics")]
    [SerializeField] TextMeshProUGUI livesText;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject gameOverWindow;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI gameOverScoreText;
    [SerializeField] InputAction pauseButton;
    [SerializeField] InputAction restartButton;
    [SerializeField] InputAction quitButton;


    //Cached references
    private ScoreController scoreController;
    private LeftHand leftHand;


    //Boolean variables
    private bool isGamePaused = false;
    private bool isBallLost = false;
    private bool isGameOver = false;

    private void Awake()
    {
        this.pauseButton.Enable();
        this.restartButton.Enable();
        this.quitButton.Enable();

        this.scoreController = GameObject.FindObjectOfType<ScoreController>();
        this.leftHand = GameObject.FindObjectOfType<LeftHand>();
    }
    private void OnDestroy()
    {
        this.pauseButton.Disable();
        this.restartButton.Disable();
        this.quitButton.Disable();
    }

    private void Start()
    {
        this.livesText.text = $"Lives: {this.playerLives}";
    }

    private void Update()
    {
        if (this.pauseButton.triggered && !this.isGameOver)
            this.isGamePaused = !this.isGamePaused;

        ManagePauseState();

        ManagePauseControls();
    }

    private void ManagePauseState()
    {
        if(this.isGamePaused && !Mathf.Approximately(Time.timeScale, 0))
        {
            Time.timeScale = 0;
            AudioListener.pause = true;

            MakeGameElementsDisappearOrAppear(true);

            this.pauseMenu.SetActive(true);
        }
        else if(!this.isGamePaused && Mathf.Approximately(Time.timeScale, 0))
        {
            // This if block statement triggers when loosing a ball and it instantly sets timeScale again to 1, ruining the minipause effect.
            // It also triggers on game over and activates all game elements again
            if (this.isBallLost || this.isGameOver) { return; }

            Time.timeScale = 1;
            AudioListener.pause = false;

            MakeGameElementsDisappearOrAppear(false);

            this.pauseMenu.SetActive(false);
        }        
    }

    private void ManagePauseControls()
    {
        if (this.isGamePaused || this.isGameOver)
        {
            if (this.restartButton.triggered)
                UnityEngine.SceneManagement.SceneManager.LoadScene(1);

            if (this.quitButton.triggered)
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    private void MakeGameElementsDisappearOrAppear(bool shouldDisappear)
    {
        this.leftHand.gameObject.SetActive(!shouldDisappear);
        foreach (Ball item in this.scoreController.Balls)
            item.GetComponentInChildren<SpriteRenderer>().enabled = !shouldDisappear;
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
        this.isBallLost = true;
        Time.timeScale = 0.0f;
        // Play SFX        

        yield return new WaitForSecondsRealtime(1.5f); // Really add the time till the sfx is done playing

        this.isBallLost = false;
        Time.timeScale = 1.0f;
        
        if(this.playerLives <= 0)
            FinishGame();
    }

    private void FinishGame()
    {
        this.isGameOver = true;

        Time.timeScale = 0;
        AudioListener.pause = true;

        MakeGameElementsDisappearOrAppear(true);
        this.livesText.enabled = false;
        this.scoreText.enabled = false;

        this.gameOverWindow.SetActive(true);

        this.gameOverScoreText.text = this.scoreController.CurrentScore.ToString();
    }
}
