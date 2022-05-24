using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText;


    private int currentScore = 0;
    private List<Ball> balls;


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
        //TODO: add score per second logic here
    }

    public void ManageBallList(bool shouldBallBeErased, Ball ball)
    {
        if (!shouldBallBeErased)
            this.balls.Add(ball);
        else
            this.balls.Remove(ball);
    }
}
