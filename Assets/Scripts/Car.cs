using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class Car : MonoBehaviour
{
    // Debugging
    public bool isLogInputs;
    public bool isRenderSuspension;

    // References
    public Transform car;

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
    private GameObject body;
    private Wheel[] wheels;


    void Start()
    {
        InitCar();
        SpawnWheels();
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
            wheelTransform.SetParent(car, false);
            wheelTransform.name = $"W-{(IsFrontWheel(i) ? "F" : "B")}{(IsLeftWheel(i) ? "L" : "R")}";

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


    void FixedUpdate()
    {
        if (isLogInputs) LogInputs();
        if (isRenderSuspension) RenderSuspension();
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].Steer(Input.GetAxis("L-Stick-X"), steeringAngle);
        }
    }
    

    private void RenderSuspension()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            Debug.DrawLine(
                wheels[i].suspensionBase + wheels[i].wheelSpace.position,
                wheels[i].suspensionEnd + wheels[i].wheelSpace.position,
                Color.red
            );
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
