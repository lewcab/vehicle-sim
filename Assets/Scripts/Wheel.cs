using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class Wheel : MonoBehaviour
{
    private Transform csWheel;      // The root transform for the wheel assembly
    private Transform csSuspension; // The transform that handles suspension movement
    private Transform csSteering;   // The transform that handles steering rotation
    private Transform csRolling;    // The transform that handles tire rolling rotation
    private Transform wheelObj;     // The visual/physical representation of the wheel
    private Rigidbody carRB;        // The RB of the car's body

    private Vector3 suspensionBase;
    private Vector3 suspensionEnd;

    private bool isFront;
    private bool isLeft;

    private float suspDepth;    // suspension height
    private float suspAngle;    // suspension offset
    private float suspRL;       // suspension resting length
    private float suspK;        // suspension spring coefficient
    private float suspD;        // suspension damping coefficient

    private float tireW;
    private float tireD;


    public void Initialize(
        Transform wheelPrefab,
        bool front, bool left,
        float carWidth, float carLength,
        float suspensionHeight, float suspensionAngle, float suspensionRestLength,
        float suspensionSpringCoefficient, float suspensionDampingCoefficient,
        float tireWidth, float tireDiameter
    )
    {
        csWheel = GetComponent<Transform>();

        isFront = front;
        isLeft = left;

        float xOffset = carLength / 2f * (isFront ? 1f : -1f);
        float zOffset = carWidth / 2f * (isLeft ? 1f : -1f);

        suspDepth = suspensionHeight;
        suspAngle = suspensionAngle * (isLeft ? -1f : 1f);
        suspRL = suspensionRestLength;
        suspK = suspensionSpringCoefficient;
        suspD = suspensionDampingCoefficient;

        tireW = tireWidth;
        tireD = tireDiameter;

        // Initialize suspension points
        suspensionBase = new Vector3(0, suspDepth, Mathf.Tan(suspAngle * Mathf.Deg2Rad) * suspDepth);
        suspensionEnd = Quaternion.Euler(suspAngle, 0, 0) * Vector3.down * suspRL;

        // Initialize CS-Wheel, given by the xOffset and yOffset
        csWheel = GetComponent<Transform>();
        csWheel.SetLocalPositionAndRotation(
            new Vector3(xOffset, 0, zOffset),
            Quaternion.identity
        );

        // Initialize CS-Suspension as a child of csWheel
        csSuspension = new GameObject("CS-Suspension").transform;
        csSuspension.SetParent(csWheel, false);
        csSuspension.localPosition = suspensionEnd;

        // Initialize CS-Steering as a child of wheelSpace
        csSteering = new GameObject("CS-Steering").transform;
        csSteering.SetParent(csSuspension, false);

        // Initialize CS-Rolling as a child of steeringSpace
        csRolling = new GameObject("CS-Rolling").transform;
        csRolling.SetParent(csSteering, false);

        // Initialize wheelObj, the pysical wheel
        InitializeWheelObj(wheelPrefab);
    }


    void InitializeWheelObj(Transform wheelPrefab)
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
    /// Initialize physics components for the wheel
    /// </summary>
    public void InitPhysics()
    {
        // Add Rigidbody to the wheel object
        Rigidbody rb = wheelObj.gameObject.AddComponent<Rigidbody>();

        // Add Collider to the tire object
        MeshCollider tireCollider = wheelObj.Find("Tire").gameObject.AddComponent<MeshCollider>();
        tireCollider.convex = true;
        tireCollider.sharedMesh = wheelObj.Find("Tire").GetComponent<MeshFilter>().sharedMesh;

        // Add Physics Material to the tire collider
        PhysicMaterial tireMaterial = new PhysicMaterial();
        tireMaterial.dynamicFriction = 0.5f;
        tireMaterial.staticFriction = 0.6f;
        tireMaterial.bounciness = 0.01f;
        tireCollider.material = tireMaterial;
    }



    /// <summary>
    /// Setup joints connecting the wheel to the car
    /// </summary>
    /// <param name="car">Rigidbody of car</param>
    public void InitJoints(Rigidbody car)
    {
        JoinCSWheelToCar(car);
        JoinCSSuspensionToCSWheel();
        JoinCSSteeringToCSSuspension();
        JoinCSRollingToCSSteering();
        JoinWheelObjToCSRolling();
    }


    private void JoinCSWheelToCar(Rigidbody car)
    {
        // Primary joint: Connect csWheel to the car
        ConfigurableJoint wheelJoint = csWheel.gameObject.AddComponent<ConfigurableJoint>();
        wheelJoint.connectedBody = car;

        // Set the anchor point of the joint (relative to csWheel)
        wheelJoint.autoConfigureConnectedAnchor = true;
        wheelJoint.anchor = Vector3.zero;

        // Lock all linear motion
        wheelJoint.xMotion = ConfigurableJointMotion.Locked;
        wheelJoint.yMotion = ConfigurableJointMotion.Locked;
        wheelJoint.zMotion = ConfigurableJointMotion.Locked;

        // Allow no angular motion for csWheel
        wheelJoint.angularXMotion = ConfigurableJointMotion.Locked;
        wheelJoint.angularYMotion = ConfigurableJointMotion.Locked;
        wheelJoint.angularZMotion = ConfigurableJointMotion.Locked;
    }


    private void JoinCSSuspensionToCSWheel()
    {
        // Joint: Connect csSuspension to csWheel
        ConfigurableJoint suspensionJoint = csSuspension.gameObject.AddComponent<ConfigurableJoint>();
        suspensionJoint.connectedBody = csWheel.GetComponent<Rigidbody>();

        suspensionJoint.autoConfigureConnectedAnchor = true;
        suspensionJoint.anchor = Vector3.zero;

        // Lock all linear motion
        suspensionJoint.xMotion = ConfigurableJointMotion.Locked;
        suspensionJoint.yMotion = ConfigurableJointMotion.Locked;
        suspensionJoint.zMotion = ConfigurableJointMotion.Locked;

        // Allow no angular motion for csSuspension
        suspensionJoint.angularXMotion = ConfigurableJointMotion.Locked;
        suspensionJoint.angularYMotion = ConfigurableJointMotion.Locked;
        suspensionJoint.angularZMotion = ConfigurableJointMotion.Locked;
    }

    private void JoinCSSteeringToCSSuspension()
    {
        // Joint: Connect csSteering to csSuspension
        ConfigurableJoint steeringJoint = csSteering.gameObject.AddComponent<ConfigurableJoint>();
        steeringJoint.connectedBody = csSuspension.GetComponent<Rigidbody>();

        steeringJoint.autoConfigureConnectedAnchor = true;
        steeringJoint.anchor = Vector3.zero;

        // Lock all linear motion
        steeringJoint.xMotion = ConfigurableJointMotion.Locked;
        steeringJoint.yMotion = ConfigurableJointMotion.Locked;
        steeringJoint.zMotion = ConfigurableJointMotion.Locked;

        // Allow angularYMotion for steering if this is a front wheel
        steeringJoint.angularXMotion = ConfigurableJointMotion.Locked;
        steeringJoint.angularYMotion = isFront ? ConfigurableJointMotion.Free : ConfigurableJointMotion.Locked;
        steeringJoint.angularZMotion = ConfigurableJointMotion.Locked;
    }

    
    private void JoinCSRollingToCSSteering()
    {
        // Joint: Connect csRolling to csSteering
        ConfigurableJoint rollingJoint = csRolling.gameObject.AddComponent<ConfigurableJoint>();
        rollingJoint.connectedBody = csSteering.GetComponent<Rigidbody>();

        rollingJoint.autoConfigureConnectedAnchor = true;
        rollingJoint.anchor = Vector3.zero;

        // Lock all linear motion
        rollingJoint.xMotion = ConfigurableJointMotion.Locked;
        rollingJoint.yMotion = ConfigurableJointMotion.Locked;
        rollingJoint.zMotion = ConfigurableJointMotion.Locked;

        // Allow angularZMotion for rolling
        rollingJoint.angularXMotion = ConfigurableJointMotion.Locked;
        rollingJoint.angularYMotion = ConfigurableJointMotion.Locked;
        rollingJoint.angularZMotion = ConfigurableJointMotion.Free;
    }

    
    private void JoinWheelObjToCSRolling()
    {
        // Joint: Connect wheelObj to csRolling
        FixedJoint wheelObjJoint = wheelObj.gameObject.AddComponent<FixedJoint>();
        wheelObjJoint.connectedBody = csRolling.GetComponent<Rigidbody>();

        wheelObjJoint.autoConfigureConnectedAnchor = true;
        wheelObjJoint.anchor = Vector3.zero;
    }


    public void RenderSuspension()
    {
        Debug.DrawLine(csWheel.position, csSuspension.position, Color.red);
    }


    /// <summary>
    /// Apply steering to the wheel
    /// </summary>
    /// <param name="input">Stick X axis input in range [-1, 1]</param>
    public void Steer(float input, float maxAngle)
    {
        float angle = input * maxAngle; // Max steer angle of 30 degrees
        if (isFront) csSteering.localRotation = Quaternion.Euler(0, angle, 0);
    }


    /// <summary>
    /// Apply throttle to the wheel
    /// </summary>
    /// <param name="input">Trigger input in range [0, 1]</param>
    /// <param name="driveType">The drive type of the car (FWD, RWD, AWD)</param>
    public void Throttle(float input, Car.DriveType driveType)
    {
        if (
            driveType == Car.DriveType.FWD && !isFront ||
            driveType == Car.DriveType.RWD && isFront
        ) return;

        float maxTorque = 10000f;
        float torque = input * maxTorque;

        Rigidbody rb = wheelObj.GetComponent<Rigidbody>();
        rb.AddTorque(wheelObj.transform.forward * -torque);
    }


    /// <summary>
    /// Apply brake to the wheel
    /// </summary>
    /// <param name="input">Trigger input in range [0, 1]</param>
    public void Brake(float input)
    {
        float maxBrakeTorque = 1500f;
        Rigidbody rb = wheelObj.GetComponent<Rigidbody>();
        Vector3 localAngularVelocity = wheelObj.transform.InverseTransformDirection(rb.angularVelocity);
        if (localAngularVelocity.z != 0)
        {
            float brakeTorque = input * maxBrakeTorque * -Mathf.Sign(localAngularVelocity.z);
            rb.AddTorque(wheelObj.transform.forward * brakeTorque);
        }
    }
}
