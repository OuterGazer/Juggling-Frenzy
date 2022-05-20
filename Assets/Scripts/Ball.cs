using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    [SerializeField] float impulseForceAmount = default;
    [SerializeField] Transform referencePoint;

    private Rigidbody2D ballRB;
    private int screenHeight;

    private void Awake()
    {
        this.ballRB = this.gameObject.GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        this.screenHeight = Screen.height;

        this.ballRB.AddForce(Vector2.up * this.impulseForceAmount, ForceMode2D.Impulse);
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
    }
}
