using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    private void Start()
    {
        this.livesText.text = $"Lives: {this.playerLives}";
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
