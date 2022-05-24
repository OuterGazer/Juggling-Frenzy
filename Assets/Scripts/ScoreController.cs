using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] float scoreCountTick = default;


    private int currentScore = 0;
    private List<Ball> balls;
    public List<Ball> Balls
    {
        get { return this.balls; }
        set { this.balls = value; }
    }
    private float gameTimeCounter = 0.0f;


    private void Awake()
    {
        this.balls = new List<Ball>();
    }


    private void Start()
    {
        this.scoreText.text = this.currentScore.ToString();
    }

    private void Update()
    {
        if (!Ball.HasGameStarted) { return; }

        this.gameTimeCounter += Time.deltaTime;

        if(this.gameTimeCounter >= this.scoreCountTick)
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

        this.scoreText.text = this.currentScore.ToString();
    }
}
