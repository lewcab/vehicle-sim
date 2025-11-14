using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class Car : MonoBehaviour
{
    // Debugging
    public bool isLogInputs;
    public bool isRenderSuspension;

    // Prefabs
    public Transform wheelPrefab;

    // Customizable Parameters
    public float length;
    public float width;
    public enum DriveType { RWD, FWD, AWD }
    public DriveType driveType;

    // Wheel/Suspension Parameters
    public float steeringAngle;
    public float suspensionHeight;
    public float suspensionOffset;
    public float tireWidth;
    public float tireDiameter;

    // Private
    private Transform car;
    private GameObject body;
    private Wheel[] wheels;


    void Start()
    {
        InitCar();
        SpawnWheels();
        InitPhysics();
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].InitJoints(car.GetComponent<Rigidbody>());
        }
    }


    void InitCar()
    {
        car = GetComponent<Transform>();
        body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";

        body.transform.SetParent(car, false);
        body.transform.localScale = new(length, 0.1f, width);
    }


    void SpawnWheels()
    {
        wheels = new Wheel[4];

        for (int i = 0; i < wheels.Length; i++)
        {
            Transform wheelTransform = new GameObject().transform;
            wheelTransform.SetParent(car.transform, false);
            wheelTransform.name = $"CS-{(IsFrontWheel(i) ? "F" : "B")}{(IsLeftWheel(i) ? "L" : "R")}";

            Wheel wheel = wheelTransform.gameObject.AddComponent<Wheel>();
            wheel.Initialize(
                wheelPrefab,
                IsFrontWheel(i), IsLeftWheel(i),
                width, length,
                suspensionHeight, suspensionOffset,
                tireWidth, tireDiameter
            );
            wheels[i] = wheel;
        }
    }


    void InitPhysics()
    {
        // Add Rigidbody to the car body
        Rigidbody carRb = car.gameObject.AddComponent<Rigidbody>();
        carRb.mass = 10f; // TODO: make configurable
        carRb.drag = 0.05f;
        carRb.angularDrag = 0.1f;
        Destroy(body.GetComponent<BoxCollider>());

        // Initialize wheel physics
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].InitPhysics();
        }
    }


    void FixedUpdate()
    {
        if (isLogInputs) LogInputs();
        if (isRenderSuspension) foreach (var wheel in wheels) wheel.RenderSuspension();
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].Steer(Input.GetAxis("L-Stick-X"), steeringAngle);
            wheels[i].Throttle(Input.GetAxis("R-Trigger"), driveType);
            wheels[i].Brake(Input.GetAxis("L-Trigger"));
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
