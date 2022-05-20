using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    [SerializeField] float impulseForceAmount = default;

    private Rigidbody2D ballRB;

    private void Awake()
    {
        this.ballRB = this.gameObject.GetComponent<Rigidbody2D>();
    }

    void OnMouseDown()
    {
        this.ballRB.AddForce(Vector2.up * this.impulseForceAmount, ForceMode2D.Impulse);
    }
}
