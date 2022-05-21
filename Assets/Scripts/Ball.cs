using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
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

    private Rigidbody2D ballRB;
    private int screenHeight;
    private float maxUpVertDrag;
    private float maxDownVertDrag;

    private bool hasGameStarted = false;
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
        if (!this.hasGameStarted)
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
            //this.downwardsVerticalDrag = 0.0f;
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
            //this.upwardsVerticalDrag = 0.0f;
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

    void OnMouseDown()
    {
        // If we click the ball low on the screen it applies full force upward. The higher we click, the less force is applied.
        // This is to have the upper deadpoint be always at roughly the same height to avoid ball flying off the screen.
        float forceFactor = (1 - (Input.mousePosition.y / this.screenHeight)) * this.impulseForceAmount;

        // We eliminate all velocity to manipulate the ball better.
        this.ballRB.velocity = Vector2.zero;

        // We calculate the vector towards the reference point, so the ball goes up slightly to one side depending on where exactly on the ball the player clicked.

        Vector2 clickPos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        Vector2 ballReferencePos = new Vector2(this.referencePoint.position.x, this.referencePoint.position.y);

        Vector2 upwardsDir = (ballReferencePos - clickPos).normalized;

        this.ballRB.AddForce(upwardsDir * forceFactor, ForceMode2D.Impulse);

        if (!this.hasGameStarted)
            this.hasGameStarted = true;
    }
}
