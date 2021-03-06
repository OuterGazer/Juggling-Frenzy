using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour, IPointerDownHandler
{
    [Header("Physics Characteristics")]
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
    [Tooltip("The maximum amount of balls that will be boosted up after left clicking when there are manu stacked up on top of each other")]
    [SerializeField] int maxStackedBallsToBump = default;

    [Header("New Ball Creation Characteristics")]
    [Tooltip("Link to ball prefab to spawn copies of it on right click")]
    [SerializeField] Ball ballPrefab;
    [Tooltip("The factor to apply to upwards impulse of the copy")]
    [SerializeField] float copyUpwardsImpulseReductionFactor = default;


    [Header("Ball Score Characteristics")]
    [SerializeField] int currentBaseScore = default;    
    public int CurrentBaseScore
    {
        get { return this.currentBaseScore; }
        set { this.currentBaseScore = value; }
    }
    [SerializeField] int penaltyScore = default;
    public int PenaltyScore => this.penaltyScore;

    private static int baseScore;
    public static int BaseScore => baseScore;

    private int leftHandBounces;
    public int LeftHandBounces => this.leftHandBounces;
    public void SetLeftHandBouncesToZero()
    {
        this.leftHandBounces = 0;
    }
    public void AddLeftHandBounce()
    {
        this.leftHandBounces += 1;
    }


    [Header("UI Tutorials")]
    [SerializeField] GameObject basicTutorial;
    [SerializeField] GameObject advancedTutorial;


    [Header("SFX Characteristics")]
    [SerializeField] AudioClip clickBallSFX;
    [SerializeField] AudioClip copyBallSFX;
    [SerializeField] AudioClip ballBounceSFX;


    // Cached references
    private Rigidbody2D ballRB;
    private LayerMask ballMask;
    private int screenHeight;
    private static float maxUpVertDrag;
    private static float maxDownVertDrag;
    private static float currentZValue;
    private ScoreController scoreController;
    private GameController gameController;
    private AudioSource audioSource;


    // Boolean variables
    private static bool hasGameStarted = false;
    public static bool HasGameStarted => hasGameStarted;
    private bool isBallFalling = false;
    private bool canBallBeCopied = false;
    public bool CanBallBeCopied
    {
        get { return this.canBallBeCopied; }
        set { this.canBallBeCopied = value; }
    }
    private static bool hasGameProgressed = false;
    private static bool hasBumpedStackedBalls = false;


    private void Awake()
    {
        this.ballRB = this.gameObject.GetComponent<Rigidbody2D>();
        this.ballMask = LayerMask.GetMask("Ball");
        this.scoreController = GameObject.FindObjectOfType<ScoreController>();
        this.gameController = GameObject.FindObjectOfType<GameController>();
        this.audioSource = this.gameObject.GetComponent<AudioSource>();
    }

    private void Start()
    {
        this.screenHeight = Screen.height;

        // First ball sets max up and down drag for all balls to come next, next balls will ignore this piece of code
        // This is needed because copied balls would inherit the current drag values in the moment of making click, causing unwanted behaviour as the game progressed
        if(Mathf.Approximately(maxUpVertDrag, 0) ||
           Mathf.Approximately(maxDownVertDrag, 0))
        {
            maxUpVertDrag = this.upwardsVerticalDrag;
            maxDownVertDrag = this.downwardsVerticalDrag;
        }        

        // We initialize it at 0.0f, so he ball doesn't weirdly stop on top deadpoint after very first click.
        this.downwardsVerticalDrag = 0.0f;

        // We add on to the Z value each ball so when we create new, we set them one Z level behind to prevent Z fighting
        currentZValue = this.gameObject.transform.position.z;

        // We add the ball to the list for managing the score and set the base score, as current score will be changing
        this.scoreController.ManageBallList(false, this);

        // First ball sets the base score for all balls, ther est of the balls get the base score on spawn (if not they copy the current base score from the ball they spawn from)
        if (baseScore == 0)
            baseScore = this.currentBaseScore;
        else
            this.currentBaseScore = baseScore;

        SetLeftHandBouncesToZero();
    }

    private void Update()
    {
        ChangeDragOnBallVelocityDirection();

        // If ball falls lower than screen botton begin life subtraction/ball destruction process
        if (this.gameObject.transform.position.y <= -0.40f)
            ProcessBallDestruction();
    }
    

    private void FixedUpdate()
    {
        // Upon beginning, the first ball should be static in the center of the screen until the player clicks it.
        if (!hasGameStarted)
        {
            ShowTutorialTips();
            KeepBallInScreenCenter();
            return;
        }

        // Regular drag applies in all directions and prevents lateral moving of the ball
        // Here I will apply a constant vertical drag contrary to the ball's current velocity direction
        if (Mathf.Sign(this.ballRB.velocity.y) == +1)
        {
            this.ballRB.AddForce(-Vector2.up * this.upwardsVerticalDrag, ForceMode2D.Force);
        }
        else
        {
            this.ballRB.AddForce(Vector2.up * this.downwardsVerticalDrag, ForceMode2D.Force);
        }
    }

    private void OnDestroy()
    {
        // We erase the ball from the score list on destroy
        this.scoreController.ManageBallList(true, this);
    }

    private void ShowTutorialTips()
    {
        if (this.gameController.IsGamePaused) { return; }

        if (!hasGameProgressed)
        {
            if (!this.basicTutorial.activeInHierarchy)
                this.basicTutorial.SetActive(true);
        }else
        {
            if (!this.advancedTutorial.activeInHierarchy)
                this.advancedTutorial.SetActive(true);
        }
        
    }

    private void KeepBallInScreenCenter()
    {
        this.ballRB.position = new Vector2(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, 0, 0)).x,
                                           Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height / 2, 0)).y);

        this.ballRB.constraints = RigidbodyConstraints2D.FreezePositionY;
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
            if (this.upwardsVerticalDrag < maxUpVertDrag)
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
            if (this.downwardsVerticalDrag < maxDownVertDrag)
            {
                this.downwardsVerticalDrag += Time.deltaTime * this.downwardsDragApplicationSpeedFactor;
            }
        }
    }

    private void ProcessBallDestruction()
    {
        // If there was only one ball, set it again in the middle, reset its base score and don't subtract a life, else subtract life and destroy ball
        Ball[] totalBalls = GameObject.FindObjectsOfType<Ball>();

        if (totalBalls.Length < 2)
        {
            hasGameStarted = false;
            this.currentBaseScore = baseScore;
        }            
        else
        {
            GameObject.FindObjectOfType<GameController>().LooseOneLife();
            GameObject.Destroy(this.gameObject);
        }            
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // First of all we calculate the exact position of the pointer as we are goig to use it many times.
        Vector2 clickPos = new Vector2(Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()).x, Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()).y);

        // We raycast from pointer position and camera Z in case more than one ball lays on top of others so we bump upwards all of them
        // In case of right click to copy, only first ball copies and rest from behind get bumped upwards
        RaycastHit2D[] ballsClicked = Physics2D.RaycastAll(new Vector3(clickPos.x, clickPos.y, Camera.main.transform.position.z),
                                                           clickPos, Mathf.Infinity, this.ballMask, -1.0f, currentZValue + 1.0f);

        // Safeguard against just clicking the background
        if (ballsClicked.Length < 1) { return; }        

        if(eventData.button == PointerEventData.InputButton.Left)
        {
            PlaySFX(this.clickBallSFX);

            LoopThroughClickedBalls(clickPos, ballsClicked, 0);

            // Prevents ball from going back to the middle of the screen after clicking, else put necesary code in place to be able to play the game
            if (!hasGameStarted)
            {
                hasGameStarted = true;

                hasGameProgressed = true;

                this.basicTutorial.SetActive(false);
                this.advancedTutorial.SetActive(false);

                this.ballRB.constraints = RigidbodyConstraints2D.None;
                this.ballRB.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
                
        }       
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Prevent copies spawning if game hasn't started
            if (!hasGameStarted) { return; }

            // We can copy balls from an existing ball only after the existing ball have at least bounced once or been clicked on once
            if (!this.canBallBeCopied) { return; }

            if (this.canBallBeCopied)
                this.canBallBeCopied = false;

            PlaySFX(this.copyBallSFX);

            // Spawn a copy and get it's Rigidbody2D to pass it later onto the method that will apply the upwards impulse
            // Set its Z value higher than the last ball with highest value to prevent Z fighting
            Ball copyBall = GameObject.Instantiate<Ball>(this.ballPrefab,
                                                     new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y, currentZValue + 1.0f),
                                                     Quaternion.identity);
            Rigidbody2D copyBallRB = copyBall.GetComponent<Rigidbody2D>();

            AddUpwardsImpulse(true, copyBallRB, clickPos);

            SetBallScore(this);

            // We finally boost upwards the rest of the balls after giving impulse to the first one and the copy
            LoopThroughClickedBalls(clickPos, ballsClicked, 1);

        }
    }

    private void PlaySFX(AudioClip SFX)
    {
        this.audioSource.pitch = UnityEngine.Random.Range(0.90f, 1.1f);
        this.audioSource.PlayOneShot(SFX);
    }
    

    private void AddUpwardsImpulse(bool isCopy, Rigidbody2D inBallRB, Vector2 clickPos)
    {
        // If we click the ball low on the screen it applies full force upward. The higher we click, the less force is applied.
        // This is to have the upper deadpoint be always at roughly the same height to avoid ball flying off the screen.
        float forceFactor = (1 - (Mouse.current.position.ReadValue().y / this.screenHeight)) * this.impulseForceAmount;

        // We eliminate all velocity to manipulate the ball better.
        inBallRB.velocity = Vector2.zero;

        // We calculate the vector towards the reference point, so the ball goes up slightly to one side depending on where exactly on the ball the player clicked.        
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
            this.ballRB.velocity = Vector2.zero;
            this.ballRB.AddForce(upwardsDir * forceFactor, ForceMode2D.Impulse);

            inBallRB.AddForce(upwardsDir * forceFactor * this.copyUpwardsImpulseReductionFactor, ForceMode2D.Impulse);
        }            
    }

    private void LoopThroughClickedBalls(Vector2 clickPos, RaycastHit2D[] ballsClicked, int startBall)
    {
        for (int i = startBall; i < ballsClicked.Length; i++)
        {
            if (ballsClicked.Length > 1 && hasBumpedStackedBalls) { return; }
            
            Rigidbody2D ballRB = ballsClicked[i].rigidbody;
            AddUpwardsImpulse(false, ballRB, clickPos);

            Ball ball = ballsClicked[i].transform.GetComponent<Ball>();
            if (!ball.canBallBeCopied)
                ball.canBallBeCopied = true;

            ball.SetLeftHandBouncesToZero();
            SetBallScore(ball);

            // If we have bumped upwards many balls stacked on top of each other, prevent players from doing it again until they click a single ball.
            // This is to avoid degenerate strategies that quickly click left and right at a ball's deadpoint to spawn near infinite balls.
            if (i >= this.maxStackedBallsToBump)
            {
                hasBumpedStackedBalls = true;
                break;
            }else
                hasBumpedStackedBalls = false;
        }
    }

    private static void SetBallScore(Ball ball)
    {
        if (ball.CurrentBaseScore >= 0)
            ball.CurrentBaseScore += baseScore;
        else
            ball.CurrentBaseScore = baseScore;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Side Limit"))
            PlaySFX(this.ballBounceSFX);
    }
}
