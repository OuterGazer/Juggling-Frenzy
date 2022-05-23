using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour, IPointerDownHandler
{
    [Header("Physics characteristics")]
    [Tooltip("This is the force applied upwards upon ball click.")]
    [SerializeField] float impulseForceAmount = default;
    [Tooltip("This is the childed reference point used to calculate the sideways moving. The closer it is to the ball, the more the ball will move towards the sides upon click.")]
    [SerializeField] Transform referencePoint;
    [Tooltip("Normal drag applies in X and Y, this drag is used to exclusively dampen movement in +Y so the ball moves naturally to the sides.")]
    [SerializeField] float upwardsVerticalDrag = default;
    [Tooltip("Normal drag applies in X and Y, this drag is used to exclusively dampen movement in -Y so the ball moves naturally to the sides.")]
    [SerializeField] float downwardsVerticalDrag = default;
    [Tooltip("This is the speed in which upwards drag is increased as the ball falls to dampen it's downwards velocity")]
    [SerializeField] float upwardsDragApplicationSpeedFactor = default;
    [Tooltip("This is the speed in which downwards drag is increased as the ball goes up to dampen it's upwards velocity")]
    [SerializeField] float downwardsDragApplicationSpeedFactor = default;

    [Header("Miscellaneous")]
    [Tooltip("Link to ball prefab to spawn copies of it on right click")]
    [SerializeField] Ball ballPrefab;
    [SerializeField] float copyUpwardsImpulseReductionFactor = default;


    private Rigidbody2D ballRB;
    private int screenHeight;
    private float maxUpVertDrag;
    private float maxDownVertDrag;

    private static bool hasGameStarted = false;
    private bool isBallFalling = false;

    private void Awake()
    {
        this.ballRB = this.gameObject.GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        this.screenHeight = Screen.height;

        this.maxUpVertDrag = this.upwardsVerticalDrag;
        this.maxDownVertDrag = this.downwardsVerticalDrag;

        // We initialize it at 0.0f, so he ball doesn't weirdly stop on top deadpoint after very first click.
        this.downwardsVerticalDrag = 0.0f;
    }

    private void Update()
    {
        // Upon beginning, the first ball should be static in the center of the screen until the player clicks it.
        if (!hasGameStarted)
        {
            KeepBallInScreenCenter();
            return;
        }

        ChangeDragOnBallVelocityDirection();
    }
    

    private void FixedUpdate()
    {
        // Regular drag applies in all directions and prevents lateral moving of the ball
        // Here I will apply a constant vertical drag contrary to the ball's current velocity direction
        if(Mathf.Sign(this.ballRB.velocity.y) == +1)
        {
            this.ballRB.AddForce(-Vector2.up * this.upwardsVerticalDrag, ForceMode2D.Force);
        }
        else
        {
            this.ballRB.AddForce(Vector2.up * this.downwardsVerticalDrag, ForceMode2D.Force);
        }
    }

    private void KeepBallInScreenCenter()
    {
        this.gameObject.transform.position = new Vector3(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, 0, 0)).x,
                                                         Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height / 2, 0)).y);
    }

    private void ChangeDragOnBallVelocityDirection()
    {
        if (this.ballRB.velocity.y <= 0 && !this.isBallFalling)
        {
            this.upwardsVerticalDrag = 0.0f;
            this.isBallFalling = true;
        }
        else if (this.ballRB.velocity.y < 0)
        {
            if (this.upwardsVerticalDrag < this.maxUpVertDrag)
            {
                this.upwardsVerticalDrag += Time.deltaTime * this.upwardsDragApplicationSpeedFactor;
            }
        }
        else if (this.ballRB.velocity.y >= 0 && this.isBallFalling)
        {
            this.downwardsVerticalDrag = 0.0f;
            this.isBallFalling = false;
        }
        else if (this.ballRB.velocity.y > 0)
        {
            if (this.downwardsVerticalDrag < this.maxDownVertDrag)
            {
                this.downwardsVerticalDrag += Time.deltaTime * this.downwardsDragApplicationSpeedFactor;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            AddUpwardsImpulse(false, this.ballRB);

            if (!hasGameStarted)
                hasGameStarted = true;
        }       
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Prevent copies spawning if game hasn't started
            if (!hasGameStarted) { return; }

            // Spawn a copy and get it's Rigidbody2D to pass it later onto the method that will apply the upwards impulse
            Ball ball = GameObject.Instantiate<Ball>(this.ballPrefab, this.gameObject.transform.position, Quaternion.identity);
            Rigidbody2D copyBallRB = ball.GetComponent<Rigidbody2D>();

            AddUpwardsImpulse(true, copyBallRB);
        }
    }

    private void AddUpwardsImpulse(bool isCopy, Rigidbody2D inBallRB)
    {
        // If we click the ball low on the screen it applies full force upward. The higher we click, the less force is applied.
        // This is to have the upper deadpoint be always at roughly the same height to avoid ball flying off the screen.
        float forceFactor = (1 - (Mouse.current.position.ReadValue().y / this.screenHeight)) * this.impulseForceAmount;

        // We eliminate all velocity to manipulate the ball better.
        inBallRB.velocity = Vector2.zero;

        // We calculate the vector towards the reference point, so the ball goes up slightly to one side depending on where exactly on the ball the player clicked.

        Vector2 clickPos = new Vector2(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()).x, Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()).y);
        Vector2 ballReferencePos = new Vector2(this.referencePoint.position.x, this.referencePoint.position.y);

        Vector2 upwardsDir = (ballReferencePos - clickPos).normalized;

        // If we are not creating a new ball, send current ball upwards
        // Else, send current ball upwards with full impulse and copy ball in same direction with reduced impulse
        if (!isCopy)
        {
            inBallRB.AddForce(upwardsDir * forceFactor, ForceMode2D.Impulse);
        }            
        else
        {
            this.ballRB.AddForce(upwardsDir * forceFactor, ForceMode2D.Impulse);
            inBallRB.AddForce(upwardsDir * forceFactor * this.copyUpwardsImpulseReductionFactor, ForceMode2D.Impulse);
        }            
    }
}
