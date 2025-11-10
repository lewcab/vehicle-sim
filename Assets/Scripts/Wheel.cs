using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class Wheel : MonoBehaviour
{
    public Transform t;

    private Transform wheelObject;
    private bool isFront;
    private bool isLeft;

    private float wheelWidth = 0.1f;


    public void Initialize(
        float wheelBaseW, float wheelBaseL, bool front, bool left
    )
    {
        t = GetComponent<Transform>();

        isFront = front;
        isLeft = left;

        float xOffset = wheelBaseL / 2 * (isFront ? 1: -1);
        float zOffset = wheelBaseW / 2 * (isLeft ? 1 : -1);

        t.localPosition = new(xOffset, 0f, zOffset);
    }
}
