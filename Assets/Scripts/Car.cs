using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Car : MonoBehaviour
{
    // Debugging
    public bool isLogInputs;

    // Objects to create
    public Transform car;
    public Transform wheelObj;

    // Customizable Parameters
    public float length;
    public float width;
    public enum DriveType { RWD, FWD, AWD }
    public DriveType driveType;

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
            Transform wheelTransform = Instantiate(wheelObj, car);
            wheelTransform.name = $"W-{(IsFrontWheel(i) ? "F" : "R")}{(IsLeftWheel(i) ? "L" : "R")}";

            Wheel wheel = wheelTransform.gameObject.AddComponent<Wheel>();
            wheel.Initialize(width, length, IsFrontWheel(i), IsLeftWheel(i));
            wheels[i] = wheel;
        }
    }


    void FixedUpdate()
    {
        if (isLogInputs) LogInputs();
        Steer();
        Throttle();
        Brake();
    }


    private void Steer()
    {
        float steering = Input.GetAxis("L-Stick-X");
        steering *= 30f;

        for (int i = 0; i < wheels.Length; i++)
        {
            if (IsFrontWheel(i)) wheels[i].t.localRotation = Quaternion.Euler(0f, steering, 0f);
        }
    }


    private void Throttle()
    {
        float throttle = Input.GetAxis("R-Trigger");
        throttle *= -40;

        for (int i = 0; i < wheels.Length; i++)
        {
            if (driveType == DriveType.AWD) wheels[i].t.Rotate(0f, 0f, throttle);
            if (driveType == DriveType.RWD && !IsFrontWheel(i)) wheels[i].t.Rotate(0f, 0f, throttle);
            if (driveType == DriveType.FWD && IsFrontWheel(i)) wheels[i].t.Rotate(0f, 0f, throttle);

        }
    }


    private void Brake()
    {
        // TODO: implement
        float brake = Input.GetAxis("L-Trigger");
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
