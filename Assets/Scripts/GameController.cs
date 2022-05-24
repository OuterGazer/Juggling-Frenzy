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
}
