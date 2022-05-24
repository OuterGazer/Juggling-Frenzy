using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LeftHand : MonoBehaviour
{
    [Header("Movement Characteristics")]
    [Tooltip("Sets the lateral speed of the hand")]
    [Range(0.0f, 10.0f)][SerializeField] float moveSpeed;

    [Header("Score Characteristics")]
    [SerializeField] int bounceLimitToNegativeBaseScore = default;

    [Header("SFX Characteristics")]
    [SerializeField] AudioClip ballBounceSFX;


    private Rigidbody2D handRB;
    private Vector2 handMovement;
    private AudioSource audioSource;


    private void Awake()
    {
        this.handRB = this.gameObject.GetComponent<Rigidbody2D>();
        this.audioSource = this.gameObject.GetComponent<AudioSource>();
    }


    private void OnMove(InputValue handMovement)
    {
        this.handMovement = new Vector2(handMovement.Get<Vector2>().x, 0);
    }

    private void FixedUpdate()
    {
        // Move left hand clamping movement within screen limits
        
        this.handRB.MovePosition(this.handRB.position += (this.handMovement * this.moveSpeed * Time.fixedDeltaTime));

        this.handRB.position = new Vector2(Mathf.Clamp(this.handRB.position.x, 1.20f, 8.40f), this.handRB.position.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            Ball bouncingBall = collision.gameObject.GetComponent<Ball>();

            // We allow bounced balls to be able to be copied
            if (!bouncingBall.CanBallBeCopied)
                bouncingBall.CanBallBeCopied = true;

            // A bounced ball will have its base score reset, after too many bounces it will get an increasingly negative penalty score to avoid abusing the platform with gazillion balls 
            if (bouncingBall.LeftHandBounces < this.bounceLimitToNegativeBaseScore)
                bouncingBall.CurrentBaseScore = Ball.BaseScore;
            else
                bouncingBall.CurrentBaseScore = bouncingBall.PenaltyScore * (bouncingBall.LeftHandBounces - this.bounceLimitToNegativeBaseScore);


                bouncingBall.AddLeftHandBounce();

            this.audioSource.pitch = Random.Range(0.90f, 1.1f);
            this.audioSource.PlayOneShot(this.ballBounceSFX);
        }
    }
}
