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
    private float tireW;    // width of tire
    private float tireD;    // diameter of tire

    private float xOffset;
    private float zOffset;

    // Suspension Properties
    private Vector3 suspensionDirection;    // direction of suspension towards ground
    private float suspAngle;                // suspension offset
    private float suspRL;                   // suspension resting length
    private float suspK;                    // suspension spring coefficient
    private float suspD;                    // suspension damping coefficient

    // Physical Properties
    private bool isGrounded;        // true if wheel is in contact with ground
    private float currSuspLength;   // current length of suspension
    private float prevSuspLength;   // previous length of suspension
    private Vector3 suspForce;      // current force exerted by suspension onto the car

    private Vector3 contactPoint;   // point of contact with ground
    private Vector3 contactNormal;  // normal at contact point
    private Vector3 wheelVelocity;  // velocity of the wheel at contact point


    void FixedUpdate()
    {
        UpdateWheelPosition();
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
        float tireWidth, float tireDiameter
    )
    {
        // Set parameters
        this.csCar = csCar;
        this.carRB = carRB;
        this.wheelPrefab = wheelPrefab;
        csWheel = GetComponent<Transform>();

        isFront = front;
        isLeft = left;

        xOffset = carLength / 2f * (isFront ? 1f : -1f);
        zOffset = carWidth / 2f * (isLeft ? 1f : -1f);

        suspAngle = suspensionAngle * (isLeft ? -1f : 1f);
        suspRL = suspensionRestLength;
        currSuspLength = suspRL;
        prevSuspLength = suspRL;
        suspK = suspensionSpringCoefficient;
        suspD = suspensionDampingCoefficient;

        tireW = tireWidth;
        tireD = tireDiameter;

        suspensionDirection = Quaternion.Euler(suspAngle, 0, 0) * Vector3.down;

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
            suspensionDirection * currSuspLength,
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
        Vector3 rayDirection = csCar.TransformDirection(suspensionDirection);

        if (PerformSuspensionRaycast(rayOrigin, rayDirection, out RaycastHit hit))
        {
            isGrounded = true;

            currSuspLength = hit.distance - (tireD / 2f);
            float compression = Mathf.Clamp(suspRL - currSuspLength, 0, suspRL);
            float suspensionVelocity = (prevSuspLength - currSuspLength) / Time.fixedDeltaTime;
            prevSuspLength = currSuspLength;
            contactPoint = hit.point;
            contactNormal = hit.normal;

            Vector3 springForce = compression * suspK * contactNormal;
            Vector3 dampingForce = suspD * suspensionVelocity * contactNormal;

            suspForce = springForce + dampingForce;
            carRB.AddForceAtPosition(suspForce, rayOrigin, ForceMode.Force);

            Debug.DrawRay(rayOrigin, suspForce / carRB.mass, Color.cyan);
        }
        else isGrounded = false;
    }


    /// <summary>
    /// Render the suspension raycast for debugging
    /// </summary>
    public void RenderSuspension()
    {
        Vector3 rayOrigin = csWheel.position;
        Vector3 rayDirection = csCar.TransformDirection(suspensionDirection);

        if (PerformSuspensionRaycast(rayOrigin, rayDirection, out RaycastHit hit))
        {
            float springLen = hit.distance - (tireD / 2f);
            Color springColor = Color.Lerp(Color.red, Color.green, springLen / suspRL);
            Debug.DrawLine(rayOrigin, hit.point, springColor);
        }
        else
        {
            Debug.DrawLine(rayOrigin, rayOrigin + rayDirection * (suspRL + (tireD / 2f)), Color.yellow);
        }
    }


    /// <summary>
    /// Update and apply tire forces to carRB at the tire contact point with the ground
    /// </summary>
    /// <param name="load">The force being supported by the wheel</param>
    public void UpdateTireForces(float load)
    {
        if (!isGrounded) return;

        Vector3 lateralDir = csRolling.forward;
        wheelVelocity = carRB.GetPointVelocity(csRolling.position);
        float lateralVelocity = Vector3.Dot(wheelVelocity, lateralDir);
        float lateralFrictionCoefficient = 0.9f;

        Vector3 lateralForce = -lateralDir * lateralVelocity * lateralFrictionCoefficient * load;
        lateralForce = Vector3.ProjectOnPlane(lateralForce, contactNormal);

        carRB.AddForceAtPosition(lateralForce, contactPoint);

        Debug.DrawRay(contactPoint, lateralForce / carRB.mass, Color.blue);
    }

    
    /// <summary>
    /// Update visual wheel position by setting CS-Rolling local position
    /// </summary>
    private void UpdateWheelPosition()
    {
        csRolling.localPosition = suspensionDirection * currSuspLength;
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
    public void Throttle(float input, BSCar.DriveType driveType)
    {
        if (
            driveType == BSCar.DriveType.FWD && !isFront ||
            driveType == BSCar.DriveType.RWD && isFront ||
            !isGrounded
        ) return;

        float maxForce = 10000f;

        float magnitude = input * maxForce;
        Vector3 force = csRolling.right * magnitude;
        force = Vector3.ProjectOnPlane(force, contactNormal);
        Vector3 position = csRolling.position;
        carRB.AddForceAtPosition(force, position);
        Debug.DrawRay(position, force/carRB.mass, Color.magenta);
    }


    /// <summary>
    /// Apply brake to the wheel
    /// </summary>
    /// <param name="input">Trigger input in range [0, 1]</param>
    public void Brake(float input)
    {
        if (
            !isGrounded ||
            Vector3.Dot(wheelVelocity, csWheel.right) <= 0
        ) return;

        float maxBrakeForce = 1500f;

        float magnitude = -input * maxBrakeForce;
        Vector3 force = csWheel.right * magnitude;
        force = Vector3.ProjectOnPlane(force, contactNormal);
        Vector3 position = csWheel.position + suspensionDirection * currSuspLength;
        carRB.AddForceAtPosition(force, position);
        Debug.DrawRay(position, force/carRB.mass, Color.yellow);
    }


    public Vector3 GetSuspensionForce() {
        return suspForce;
    }

    public bool IsGrounded() {
        return isGrounded;
    }
}
