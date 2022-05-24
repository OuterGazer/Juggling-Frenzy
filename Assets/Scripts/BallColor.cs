using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallColor : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private static int lastColor;

    private float redTone;
    private float greenTone;
    private float blueTone;

    private void Awake()
    {
        this.spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        ChooseBallColor();

        this.spriteRenderer.color = new Color(this.redTone, this.greenTone, this.blueTone);
    }

    private void ChooseBallColor()
    {
        // 0 corresponds to orange F85E1E, 1 corresponds to blue 037EFA and 2 corresponds to violet F84D5A
        // Afterwards a tone will be chosen among analogue colors

        int mainColor;
        do
        {
            mainColor = Random.Range(0, 3);

        } while (mainColor == lastColor);

        lastColor = mainColor;

        switch (mainColor)
        {
            case 0:
                ChooseTone(224.0f, 252.0f, 20.0f, 155.0f, 16.0f, 30.0f);
                break;
            case 1:
                ChooseTone(3.0f, 17.0f, 10.0f, 253.0f, 227.0f, 253.0f);
                break;
            case 2:
                ChooseTone(224.0f, 252.0f, 58.0f, 115.0f, 65.0f, 252.0f);
                break;
        }
    }

    private void ChooseTone(float minRed, float maxRed, float minGreen, float maxGreen, float minBlue, float maxBlue)
    {
        this.redTone = Random.Range(minRed, maxRed);
        this.redTone /= 255.0f;

        this.greenTone = Random.Range(minGreen, maxGreen);
        this.greenTone /= 255.0f;

        this.blueTone = Random.Range(minBlue, maxBlue);
        this.blueTone /= 255.0f;
    }
}
