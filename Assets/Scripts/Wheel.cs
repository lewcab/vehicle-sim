using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public abstract class Wheel : MonoBehaviour
{
    // Game Object References
    public Transform csCar;         // The transform of the car space
    public Transform csWheel;       // The transform for the wheel space
    public Rigidbody carRB;         // The RB of the car
    public Transform wheelPrefab;   // The prefab for the wheel

    // Wheel Properties
    public bool isFront;            // true if front wheel, false if rear wheel
    public bool isLeft;             // true if left wheel, false if right wheel
    public float tireW;             // width of tire
    public float tireD;             // diameter of tire

    public float xOffset;
    public float zOffset;

    // Suspension Properties
    public float suspDepth;         // suspension height
    public float suspAngle;         // suspension offset
    public float suspRL;            // suspension resting length
    public float suspK;             // suspension spring coefficient
    public float suspD;             // suspension damping coefficient

    // Physical Properties
    public bool isGrounded;
    public float currSuspLength;
    public float prevSuspLength;
    public Vector3 suspForce;


    public abstract void Initialize(
        Transform csCar, Rigidbody carRB,
        Transform wheelPrefab,
        bool front, bool left,
        float track, float wheelbase,
        float suspensionHeight, float suspensionAngle, float suspensionRestLength,
        float suspensionSpringCoefficient, float suspensionDampingCoefficient,
        float tireWidth, float tireDiameter
    );


    public abstract void UpdateSuspensionForces();


    public abstract void RenderSuspension();


    public abstract void UpdateTireForces(float load);


    public abstract void Steer(
        float input, float maxAngle
    );


    public abstract void Throttle(
        float input, Car.DriveType driveType
    );

    public abstract void Brake(
        float input
    );
}
