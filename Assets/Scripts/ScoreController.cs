using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreController : MonoBehaviour
{
    [Header("UI Characteristics")]
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] float scoreCountTick = default;

    [Header("Score Management Characteristics")]
    [SerializeField] int pointsThresholdForExtraLife;
    private GameController gameController;
    private int pointsForNewLifeEarned;

    private int currentScore = 0;
    public int CurrentScore => this.currentScore;
    private List<Ball> balls;
    public List<Ball> Balls
    {
        get { return this.balls; }
        set { this.balls = value; }
    }
    private float gameTimeCounter = 0.0f;


    private void Awake()
    {
        this.gameController = GameObject.FindObjectOfType<GameController>();
        this.balls = new List<Ball>();
    }


    private void Start()
    {
        this.scoreText.text = this.currentScore.ToString();

        this.pointsForNewLifeEarned = this.pointsThresholdForExtraLife;
    }

    private void Update()
    {
        if (!Ball.HasGameStarted) { return; }

        this.gameTimeCounter += Time.deltaTime;

        if((this.gameTimeCounter >= this.scoreCountTick) &&
            (this.balls.Count > 1))
        {
            AddScore();
            this.gameTimeCounter = 0.0f;
        }
    }

    public bool CheckIfBallExists(Ball ball)
    {
        return balls.Contains(ball);
    }

    public void ManageBallList(bool shouldBallBeErased, Ball ball)
    {
        if (!shouldBallBeErased)
            this.balls.Add(ball);
        else
            this.balls.Remove(ball);
    }

    private void AddScore()
    {
        int scoreToAdd = 0;

        for(int i = 0; i < this.balls.Count; i++)
        {
            scoreToAdd += this.balls[i].CurrentBaseScore;
        }

        scoreToAdd *= this.balls.Count;

        this.currentScore += scoreToAdd;

        if (this.currentScore >= this.pointsForNewLifeEarned)
        {
            this.gameController.AddOneLife();

            this.pointsForNewLifeEarned += this.pointsThresholdForExtraLife;
        }
            

        this.scoreText.text = this.currentScore.ToString();
    }
}
