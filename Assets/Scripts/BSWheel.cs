using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class BSWheel : MonoBehaviour
{
    // Game Object References
    private Transform csCar;        // The transform of the car space
    private Transform csWheel;      // The transform for the wheel space
    private Transform csRolling;    // The transform for the wheel position along suspension
    private Transform wheelObj;     // The visual wheel object
    private Rigidbody carRB;        // The RB of the car
    private Transform wheelPrefab;  // The prefab for the wheel

    // Wheel Properties
    private bool isFront;   // true if front wheel, false if rear wheel
    private bool isLeft;    // true if left wheel, false if right wheel
    private float tireFC;   // friction coefficient of tire
    private float tireW;    // width of tire
    private float tireD;    // diameter of tire

    // Suspension Properties
    private Vector3 suspDirection;  // direction of suspension towards ground
    private float suspAngle;        // suspension offset
    private float suspRL;           // suspension resting length
    private float suspK;            // suspension spring coefficient
    private float suspD;            // suspension damping coefficient

    // Physical Properties
    private bool isGrounded;        // true if wheel is in contact with ground
    private float currSuspLength;   // current length of suspension
    private float prevSuspLength;   // previous length of suspension
    private Vector3 suspForce;      // current force exerted by suspension onto the car
    private Vector3 latForce;       // current lateral force exerted by tire onto the car
    private Vector3 throttleForce;  // current throttle force exerted by tire onto the car
    private Vector3 brakeForce;     // current brake force exerted by tire onto the car

    private Vector3 contactPoint;   // point of contact with ground
    private Vector3 contactNormal;  // normal at contact point
    private Vector3 wheelVelocity;  // velocity of the wheel at contact point


    void Update()
    {
        RenderSuspension();
        RenderForces();
        UpdateWheelPosition();
        UpdateWheelRotation();
    }

    /// <summary>
    /// Render the suspension raycast for debugging
    /// </summary>
    private void RenderSuspension()
    {
        Vector3 rayOrigin = csWheel.position;
        Vector3 rayDirection = csCar.TransformDirection(suspDirection);

        if (PerformSuspensionRaycast(rayOrigin, rayDirection, out RaycastHit hit))
        {
            float springLen = hit.distance - (tireD / 2f);
            Color springColor = Color.Lerp(Color.red, Color.yellow, springLen / suspRL);
            Debug.DrawLine(rayOrigin, hit.point, springColor);
        }
        else
        {
            Debug.DrawLine(rayOrigin, rayOrigin + rayDirection * (suspRL + (tireD / 2f)), Color.red);
        }
    }


    /// <summary>
    /// Render the suspension and tire forces for debugging
    /// </summary>
    private void RenderForces()
    {
        if (!isGrounded) return;

        Debug.DrawRay(csWheel.position, suspForce / carRB.mass, Color.green);
        Debug.DrawRay(contactPoint, latForce / carRB.mass, Color.red);
        Debug.DrawRay(csRolling.position, throttleForce / carRB.mass, Color.blue);
        Debug.DrawRay(csRolling.position, brakeForce / carRB.mass, Color.yellow);
    }


    /// <summary>
    /// Update visual wheel position by setting CS-Rolling local position
    /// </summary>
    private void UpdateWheelPosition()
    {
        csRolling.localPosition = suspDirection * currSuspLength;
    }


    /// <summary>
    /// Update visual wheel rotation based on wheel linear velocity
    /// </summary>
    private void UpdateWheelRotation()
    {
        if (!isGrounded) return;

        float wheelRadius = tireD / 2f;
        float linearVelocity = Vector3.Dot(wheelVelocity, csRolling.right);
        float angularVelocity = linearVelocity / wheelRadius;
        float angularDisplacement = angularVelocity * Time.deltaTime;

        wheelObj.localRotation *= Quaternion.Euler(0f, 0f, -angularDisplacement * Mathf.Rad2Deg);
    }


    /// <summary>
    /// Initialize the raycast wheel
    /// </summary>
    public void Initialize(
        Transform csCar, Rigidbody carRB, 
        Transform wheelPrefab,
        bool front, bool left,
        float carWidth, float carLength,
        float suspensionAngle, float suspensionRestLength,
        float suspensionSpringCoefficient, float suspensionDampingCoefficient,
        float tireFrictionCoefficient, float tireWidth, float tireDiameter
    )
    {
        // Set parameters
        this.csCar = csCar;
        this.carRB = carRB;
        this.wheelPrefab = wheelPrefab;
        csWheel = GetComponent<Transform>();

        isFront = front;
        isLeft = left;

        float xOffset = carLength / 2f * (isFront ? 1f : -1f);
        float zOffset = carWidth / 2f * (isLeft ? 1f : -1f);

        suspAngle = suspensionAngle * (isLeft ? -1f : 1f);
        suspRL = suspensionRestLength;
        currSuspLength = suspRL;
        prevSuspLength = suspRL;
        suspK = suspensionSpringCoefficient;
        suspD = suspensionDampingCoefficient;

        tireFC = tireFrictionCoefficient;
        tireW = tireWidth;
        tireD = tireDiameter;

        suspDirection = Quaternion.Euler(suspAngle, 0, 0) * Vector3.down;

        // Initialize CS-Wheel, given by the xOffset and yOffset
        csWheel = GetComponent<Transform>();
        csWheel.SetLocalPositionAndRotation(
            new Vector3(xOffset, 0, zOffset),
            Quaternion.identity
        );

        // Initialize CS-Rolling as a child of CS-Wheel
        csRolling = new GameObject("CS-Rolling").transform;
        csRolling.SetParent(csWheel);
        csRolling.SetLocalPositionAndRotation(
            suspDirection * currSuspLength,
            Quaternion.identity
        );

        // Initialize the wheel visuals
        InitializeWheelObj();
    }


    /// <summary>
    /// Instantiate and position the wheel visuals
    /// </summary>
    private void InitializeWheelObj()
    {
        wheelObj = Instantiate(wheelPrefab, csRolling);
        wheelObj.name = "Wheel";
        wheelObj.SetLocalPositionAndRotation(
            new Vector3(0, 0, tireW / 2 * (isLeft ? 1 : -1)),
            Quaternion.identity
        );
        wheelObj.localScale = new Vector3(tireD, tireD, tireW);
    }


    /// <summary>
    /// Perform a raycast representing the suspension
    /// </summary>
    /// <param name="origin">Base of the suspension strut</param>
    /// <param name="direction">Direction of the suspension strut from the base</param>
    /// <param name="hit">Output</param>
    /// <returns>true if ray makes contact, false otherwise</returns>
    private bool PerformSuspensionRaycast(Vector3 origin, Vector3 direction, out RaycastHit hit)
    {
        return Physics.Raycast(origin, direction, out hit, suspRL + (tireD/2f));
    }


    /// <summary>
    /// Update and apply suspension forces to carRB at the suspension base
    /// </summary>
    public void UpdateSuspensionForces()
    {
        Vector3 rayOrigin = csWheel.position;
        Vector3 rayDirection = csCar.TransformDirection(suspDirection);

        if (PerformSuspensionRaycast(rayOrigin, rayDirection, out RaycastHit hit))
        {
            isGrounded = true;

            currSuspLength = hit.distance - (tireD / 2f);
            float compression = Mathf.Clamp(suspRL - currSuspLength, 0, suspRL);
            float suspSpeed = (currSuspLength - prevSuspLength) / Time.fixedDeltaTime;
            prevSuspLength = currSuspLength;
            contactPoint = hit.point;
            contactNormal = hit.normal;

            Vector3 springForce = suspK * compression * contactNormal;
            Vector3 dampingForce = -suspD * suspSpeed * contactNormal;

            suspForce = springForce + dampingForce;
            carRB.AddForceAtPosition(suspForce, rayOrigin, ForceMode.Force);
        }
        else isGrounded = false;
    }


    /// <summary>
    /// Update and apply tire forces to carRB at the tire contact point with the ground
    /// </summary>
    public void UpdateTireForces()
    {
        if (!isGrounded) return;

        Vector3 lateralDir = csRolling.forward;
        wheelVelocity = carRB.GetPointVelocity(csRolling.position);
        float lateralVelocity = Vector3.Dot(wheelVelocity, lateralDir);
        float load = suspForce.magnitude;

        latForce = -lateralDir * lateralVelocity * tireFC * load;
        latForce = Vector3.ProjectOnPlane(latForce, contactNormal);

        // Artificially limit lateral force to prevent flipping
        // TODO: Replace with physical model
        float maxLateralForce = load * tireFC;
        if (latForce.magnitude > maxLateralForce)
            latForce = latForce.normalized * maxLateralForce;

        carRB.AddForceAtPosition(latForce, contactPoint);
    }


    /// <summary>
    /// Apply steering to the wheel
    /// </summary>
    /// <param name="input">Stick X axis input in range [-1, 1]</param>
    public void Steer(float input, float maxAngle)
    {
        if (!isFront) return;

        float steerAngle = input * maxAngle;
        csRolling.localRotation = Quaternion.Euler(0, steerAngle, 0);
    }


    /// <summary>
    /// Apply throttle to the wheel
    /// </summary>
    /// <param name="input">Trigger input in range [0, 1]</param>
    /// <param name="driveType">The drive type of the car (FWD, RWD, AWD)</param>
    /// <param name="maxForce">The maximum force applied by the wheel</param>
    /// <param name="topSpeed">The maximum speed of the car in km/h</param>
    public void Throttle(float input, BSCar.DriveType driveType, float maxForce, float topSpeed)
    {
        if (
            driveType == BSCar.DriveType.FWD && !isFront ||
            driveType == BSCar.DriveType.RWD && isFront ||
            !isGrounded
        ) return;

        float currentSpeed = carRB.velocity.magnitude;

        if (currentSpeed >= topSpeed / 3.6f) return;

        float magnitude = input * maxForce;
        throttleForce = csRolling.right * magnitude;
        throttleForce = Vector3.ProjectOnPlane(throttleForce, contactNormal);
        carRB.AddForceAtPosition(throttleForce, csRolling.position);
    }


    /// <summary>
    /// Apply brake to the wheel
    /// </summary>
    /// <param name="input">Trigger input in range [0, 1]</param>
    /// <param name="maxBrakeForce">The maximum braking force applied by the wheel</param>
    public void Brake(float input, float maxBrakeForce)
    {
        if (
            !isGrounded ||
            Vector3.Dot(wheelVelocity, csWheel.right) <= 0
        ) return;

        if (isFront) maxBrakeForce *= 1.2f;
        else maxBrakeForce *= 0.8f;

        float magnitude = -input * maxBrakeForce;
        brakeForce = csWheel.right * magnitude;
        brakeForce = Vector3.ProjectOnPlane(brakeForce, contactNormal);
        carRB.AddForceAtPosition(brakeForce, csRolling.position);
    }


    public Vector3 GetSuspensionForce() {
        return suspForce;
    }


    public bool IsGrounded() {
        return isGrounded;
    }
}
