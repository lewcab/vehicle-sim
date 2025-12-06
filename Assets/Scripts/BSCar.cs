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

    // Prefabs and Visual Components
    public Transform carShellPrefab;
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


    void Start()
    {
        InitCar();
        InitWheels();
    }


    /// <summary>
    /// Initializes the car body, Rigidbody, and visual shell.
    /// </summary>
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

        // Add car shell visual
        if (carShellPrefab != null)
        {
            Transform carShell = Instantiate(carShellPrefab, car);
            carShell.localPosition = new Vector3(0f, (bodyThickness/2) - suspensionDepth, 0f);
        }
    }


    /// <summary>
    /// Initializes the four wheels of the car.
    /// </summary>
    void InitWheels()
    {
        wheels = new BSWheel[4];

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
        if (isLogInputs) 
            LogInputs();

        HandleInput();
        foreach (BSWheel w in wheels) w.UpdateSuspensionForces();
        foreach (BSWheel w in wheels) w.UpdateTireForces();

        if (isRenderSuspension)
            for (int i = 0; i < wheels.Length; i++) 
                wheels[i].RenderSuspension();
    }


    /// <summary>
    /// Handles user input for steering, throttle, and braking.
    /// </summary>
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
    

    /// <summary>
    /// Determines if the wheel is a front wheel based on its index.
    /// </summary>
    /// <param name="wheel_i">Index of the wheel</param>
    /// <returns>true if wheel is front wheel, false otherwise</returns>
    private bool IsFrontWheel(int wheel_i)
    {
        return wheel_i < 2;
    }

    
    /// <summary>
    /// Determines if the wheel is a left wheel based on its index.
    /// </summary>
    /// <param name="wheel_i">Index of the wheel</param>
    /// <returns>true if wheel is left wheel, false otherwise</returns>
    private bool IsLeftWheel(int wheel_i)
    {
        return wheel_i % 2 == 0;
    }


    /// <summary>
    /// Logs the current input values for debugging purposes.
    /// </summary>
    private void LogInputs()
    {
        Debug.Log($"Inputs @ {Time.fixedTime}");
        Debug.Log($"\tL-Stick-X: {Input.GetAxis("L-Stick-X")}");
        Debug.Log($"\tR-Stick-X: {Input.GetAxis("R-Stick-X")}");
        Debug.Log($"\tL-Trigger: {Input.GetAxis("L-Trigger")}");
        Debug.Log($"\tR-Trigger: {Input.GetAxis("R-Trigger")}");
    }
}
