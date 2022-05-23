using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LeftHand : MonoBehaviour
{
    [Header("Movement Characteristics")]
    [Tooltip("Sets the lateral speed of the hand")]
    [Range(0.0f, 10.0f)][SerializeField] float moveSpeed;


    private Rigidbody2D handRB;
    private Vector2 handMovement;


    private void Awake()
    {
        this.handRB = this.gameObject.GetComponent<Rigidbody2D>();
    }


    private void OnMove(InputValue handMovement)
    {
        this.handMovement = new Vector2(handMovement.Get<Vector2>().x, 0);
    }

    private void FixedUpdate()
    {
        this.handRB.MovePosition(this.handRB.position += (this.handMovement * this.moveSpeed * Time.fixedDeltaTime));
    }
}
