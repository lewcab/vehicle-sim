using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class Wheel : MonoBehaviour
{
    public Transform wheelSpace;
    public Transform wheelObj;

    public Vector3 suspensionBase;
    public Vector3 suspensionEnd;

    private bool isFront;
    private bool isLeft;

    private float suspH;
    private float suspW;
    private float tireW;
    private float tireD;


    public void Initialize(
        Transform wheelPrefab,
        bool front, bool left,
        float carWidth, float carLength,
        float suspensionHeight, float suspensionOffset,
        float tireWidth, float tireDiameter
    )
    {
        wheelSpace = GetComponent<Transform>();

        isFront = front;
        isLeft = left;

        float xOffset = carLength / 2 * (isFront ? 1 : -1);
        float zOffset = carWidth / 2 * (isLeft ? 1 : -1);

        suspH = suspensionHeight;
        suspW = suspensionOffset;
        tireW = tireWidth;
        tireD = tireDiameter;

        // Initialize wheelSpace, given by the xOffset and yOffset
        wheelSpace = GetComponent<Transform>();
        wheelSpace.SetLocalPositionAndRotation(
            new Vector3(xOffset, 0, zOffset),
            Quaternion.identity
        );

        // Initialize wheelObj, the pysical wheel
        InitializeWheelObj(wheelPrefab);

        // Initialize suspension points
        suspensionBase = new Vector3(0, suspH, suspW * (isLeft ? -1 : 1));
        suspensionEnd = Vector3.zero;
    }

    
    void InitializeWheelObj(Transform wheelPrefab)
    {
        wheelObj = Instantiate(wheelPrefab, wheelSpace);
        wheelObj.name = "Wheel";
        wheelObj.SetLocalPositionAndRotation(
            new Vector3(0, 0, tireW / 2 * (isLeft ? 1 : -1)),
            Quaternion.identity
        );
        wheelObj.localScale = new Vector3(tireD, tireD, tireW);

    }


    /// <summary>
    /// Apply steering to the wheel
    /// </summary>
    /// <param name="input">Stick X axis input in range [-1, 1]</param>
    public void Steer(float input, float maxAngle)
    {
        float angle = input * maxAngle; // Max steer angle of 30 degrees
        if (isFront) wheelSpace.localRotation = Quaternion.Euler(0, angle, 0);
    }


    /// <summary>
    /// Apply throttle to the wheel
    /// </summary>
    /// <param name="input">Trigger input in range [0, 1]</param>
    public void Throttle(float input)
    {
        // TODO: implement
    }


    /// <summary>
    /// Apply brake to the wheel
    /// </summary>
    /// <param name="input">Trigger input in range [0, 1]</param>
    public void Brake(float input)
    {
        // TODO: implement
    }
}
