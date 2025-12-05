using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class BSCar : MonoBehaviour
{
    // Debugging
    public bool isLogInputs;
    public bool isRenderSuspension;
    public bool isKeyboardControl;

    // Prefabs and Components
    public Transform wheelPrefab;

    // Customizable Parameters
    public float wheelbase;
    public float track;
    public float carWeight;
    public enum DriveType { RWD, FWD, AWD }
    public DriveType driveType;

    // Wheel & Suspension Parameters
    public float steeringAngle;
    public float suspensionDepth;
    public float suspensionAngle;
    public float suspensionRestLength;
    public float suspensionSpringCoefficient;
    public float suspensionDampingCoefficient;
    public float tireWidth;
    public float tireDiameter;

    // Private References
    private Transform car;
    private Rigidbody carRB;
    private GameObject body;
    private BSWheel[] wheels;
    private float[] wheelLoads;
    private float[] suspensionForces;


    void Start()
    {
        InitCar();
        InitWheels();
    }


    void InitCar()
    {
        car = GetComponent<Transform>();
        body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";

        float bodyThickness = 0.05f;
        body.transform.SetParent(car, false);
        body.transform.localScale = new(wheelbase, bodyThickness, track);
        body.transform.localPosition = new Vector3(0f, (bodyThickness/2) - suspensionDepth, 0f);

        // Add Rigidbody to the car body
        carRB = car.gameObject.AddComponent<Rigidbody>();
        carRB.mass = carWeight;
        carRB.centerOfMass = new Vector3(0f, -suspensionDepth / 2f, 0f);

        body.GetComponent<Collider>().excludeLayers = LayerMask.GetMask("Car Wheel");
    }


    void InitWheels()
    {
        wheels = new BSWheel[4];
        wheelLoads = new float[4];
        suspensionForces = new float[4];

        for (int i = 0; i < wheels.Length; i++)
        {
            Transform wheelTransform = new GameObject().transform;
            wheelTransform.SetParent(car.transform, false);
            wheelTransform.name = $"CS-{(IsFrontWheel(i) ? "F" : "B")}{(IsLeftWheel(i) ? "L" : "R")}";

            BSWheel wheel;
            wheel = wheelTransform.gameObject.AddComponent<BSWheel>();

            wheel.Initialize(
                car, carRB,
                wheelPrefab,
                IsFrontWheel(i), IsLeftWheel(i),
                track, wheelbase,
                suspensionAngle, suspensionRestLength,
                suspensionSpringCoefficient, suspensionDampingCoefficient,
                tireWidth, tireDiameter
            );
            wheels[i] = wheel;
        }
    }


    void FixedUpdate()
    {
        HandleInput();
        if (isLogInputs) 
            LogInputs();

        for (int i = 0; i < wheels.Length; i++) 
            wheels[i].UpdateSuspensionForces();
        
        UpdateWheelLoads();
        for (int i = 0; i < wheels.Length; i++)
            wheels[i].UpdateTireForces(wheelLoads[i]);

        if (isRenderSuspension)
            for (int i = 0; i < wheels.Length; i++) 
                wheels[i].RenderSuspension();
    }


    private void HandleInput()
    {
        float steerInput;
        float throttleInput;
        float brakeInput;

        if (isKeyboardControl)
        {
            steerInput = 0f;
            if (Input.GetKey(KeyCode.A)) steerInput = -1f;
            else if (Input.GetKey(KeyCode.D)) steerInput = 1f;

            throttleInput = Input.GetKey(KeyCode.W) ? 1f : 0f;
            brakeInput = Input.GetKey(KeyCode.S) ? 1f : 0f;
        }
        else
        {
            steerInput = Input.GetAxis("L-Stick-X");
            throttleInput = Input.GetAxis("R-Trigger");
            brakeInput = Input.GetAxis("L-Trigger");
        }

        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].Steer(steerInput, steeringAngle);
            wheels[i].Throttle(throttleInput, driveType);
            wheels[i].Brake(brakeInput);
        }
    }
    

    private void UpdateWheelLoads()
    {
        float totalLoad = carRB.mass * Physics.gravity.magnitude;
        float totalSuspensionForce = 0f;

        for (int i = 0; i < wheels.Length; i++)
        {
            suspensionForces[i] = wheels[i].GetSuspensionForce().magnitude;
            if (wheels[i].IsGrounded())
                totalSuspensionForce += suspensionForces[i];
        }

        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i].IsGrounded() && totalSuspensionForce > 0f)
                wheelLoads[i] = (suspensionForces[i] / totalSuspensionForce) * totalLoad;
            else
                wheelLoads[i] = 0f;
        }
    }


    private bool IsFrontWheel(int wheel_i)
    {
        return wheel_i < 2;
    }


    private bool IsLeftWheel(int wheel_i)
    {
        return wheel_i % 2 == 0;
    }


    private void LogInputs()
    {
        Debug.Log($"Inputs @ {Time.fixedTime}");
        Debug.Log($"\tL-Stick-X: {Input.GetAxis("L-Stick-X")}");
        Debug.Log($"\tR-Stick-X: {Input.GetAxis("R-Stick-X")}");
        Debug.Log($"\tL-Trigger: {Input.GetAxis("L-Trigger")}");
        Debug.Log($"\tR-Trigger: {Input.GetAxis("R-Trigger")}");
    }
}
