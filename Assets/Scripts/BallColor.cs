using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallColor : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        this.spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        this.spriteRenderer.color = Color.magenta;
    }
}
