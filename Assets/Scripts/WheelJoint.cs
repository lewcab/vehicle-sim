using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class WheelJoint : Wheel
{
    private Transform csSteering;   // The transform that handles steering rotation
    private Transform csRolling;    // The transform that handles tire rolling rotation
    private Transform wheelObj;     // The visual/physical representation of the wheel

    private Rigidbody wheelRB;      // The RB of the wheel object
    private Rigidbody suspensionRB; // The RB of the suspension
    private Rigidbody steeringRB;   // The RB of the steering
    private Rigidbody rollingRB;    // The RB of the rolling


    public override void Initialize(
        Transform csCar, Rigidbody carRB, 
        Transform wheelPrefab,
        bool front, bool left,
        float carWidth, float carLength,
        float suspensionHeight, float suspensionAngle, float suspensionRestLength,
        float suspensionSpringCoefficient, float suspensionDampingCoefficient,
        float tireWidth, float tireDiameter
    )
    {
        // Set parameters
        this.csCar = csCar;
        this.carRB = carRB;
        csWheel = GetComponent<Transform>();

        isFront = front;
        isLeft = left;

        xOffset = carLength / 2f * (isFront ? 1f : -1f);
        zOffset = carWidth / 2f * (isLeft ? 1f : -1f);

        suspDepth = suspensionHeight;
        suspAngle = suspensionAngle * (isLeft ? -1f : 1f);
        suspRL = suspensionRestLength;
        suspK = suspensionSpringCoefficient;
        suspD = suspensionDampingCoefficient;

        tireW = tireWidth;
        tireD = tireDiameter;

        // Initialize CS-Wheel, given by the xOffset and yOffset
        csWheel = GetComponent<Transform>();
        csWheel.SetLocalPositionAndRotation(
            new Vector3(xOffset, 0, zOffset) + Quaternion.Euler(suspAngle, 0, 0) * Vector3.down * suspRL,
            Quaternion.identity
        );

        // Initialize CS-Steering as a child of wheelSpace
        csSteering = new GameObject("CS-Steering").transform;
        csSteering.SetParent(csWheel, false);

        // Initialize CS-Rolling as a child of steeringSpace
        csRolling = new GameObject("CS-Rolling").transform;
        csRolling.SetParent(csSteering, false);

        // Initialize wheelObj, the pysical wheel
        InitializeWheelObj(wheelPrefab);

        // Initialize physical components
        InitPhysics();

        // Initialize joints
        InitJoints();
        
        // Recursvely set layer to "Car Wheel"
        foreach (Transform child in csWheel.GetComponentsInChildren<Transform>())
        {
            child.gameObject.layer = LayerMask.NameToLayer("Car Wheel");
        }
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

        // Add Rigidbody to csWheel, csSuspension, csSteering, csRolling
        wheelRB = csWheel.gameObject.AddComponent<Rigidbody>();
        steeringRB = csSteering.gameObject.AddComponent<Rigidbody>();
        rollingRB = csRolling.gameObject.AddComponent<Rigidbody>();
    }


    /// <summary>
    /// Setup joints connecting the wheel to the car
    /// </summary>
    public void InitJoints()
    {
        JoinCSWheelToCar();
        JoinCSSteeringToCSWheel();
        JoinCSRollingToCSSteering();
        JoinWheelObjToCSRolling();
    }


    private void JoinCSWheelToCar()
    {
        // Joint: Connect csWheel to car body
        ConfigurableJoint wheelJoint = csWheel.gameObject.AddComponent<ConfigurableJoint>();
        wheelJoint.connectedBody = carRB;

        wheelJoint.autoConfigureConnectedAnchor = true;
        wheelJoint.anchor = Vector3.zero;

        // Lock all linear, except Y motion for suspension
        wheelJoint.xMotion = ConfigurableJointMotion.Locked;
        wheelJoint.yMotion = ConfigurableJointMotion.Free;
        wheelJoint.zMotion = ConfigurableJointMotion.Locked;

        // Lock all angular motion
        wheelJoint.angularXMotion = ConfigurableJointMotion.Locked;
        wheelJoint.angularYMotion = ConfigurableJointMotion.Locked;
        wheelJoint.angularZMotion = ConfigurableJointMotion.Locked;

        // Set suspension spring and damper
        JointDrive suspensionDrive = new JointDrive();
        suspensionDrive.positionSpring = suspK;
        suspensionDrive.positionDamper = suspD;
        suspensionDrive.maximumForce = Mathf.Infinity;
        wheelJoint.yDrive = suspensionDrive;
    }


    private void JoinCSSteeringToCSWheel()
    {
        // Joint: Connect csSteering to csWheel
        ConfigurableJoint steeringJoint = csSteering.gameObject.AddComponent<ConfigurableJoint>();
        steeringJoint.connectedBody = csWheel.GetComponent<Rigidbody>();

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


    public override void RenderSuspension()
    {
        // TODO: Implement
    }


    /// Apply steering to the wheel
    /// </summary>
    /// <param name="input">Stick X axis input in range [-1, 1]</param>
    public override void Steer(float input, float maxAngle)
    {
        float angle = input * maxAngle; // Max steer angle of 30 degrees
        if (isFront) csSteering.localRotation = Quaternion.Euler(0, angle, 0);
    }


    /// <summary>
    /// Apply throttle to the wheel
    /// </summary>
    /// <param name="input">Trigger input in range [0, 1]</param>
    /// <param name="driveType">The drive type of the car (FWD, RWD, AWD)</param>
    public override void Throttle(float input, Car.DriveType driveType)
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
    public override void Brake(float input)
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
