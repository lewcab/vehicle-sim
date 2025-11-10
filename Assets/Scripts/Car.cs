using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Car : MonoBehaviour
{
    // Objects to create
    public Transform car;
    public Transform wheelObj;

    // Customizable Parameters
    public enum DriveType { RWD, FWD, AWD }
    public DriveType driveType;

    // Private
    private Wheel[] wheels;


    // Start is called before the first frame update
    void Start()
    {
        SpawnWheels();
    }


    void SpawnWheels()
    {
        wheels = new Wheel[4];
        float wheelBaseW = 2f;
        float wheelBaseL = 2.5f;

        for (int i = 0; i < wheels.Length; i++)
        {
            Transform wheelTransform = Instantiate(wheelObj, car);
            wheelTransform.name = $"W-{(IsFrontWheel(i) ? "F" : "R")}{(IsLeftWheel(i) ? "L" : "R")}";

            Wheel wheel = wheelTransform.gameObject.AddComponent<Wheel>();
            wheel.Initialize(wheelBaseW, wheelBaseL, IsFrontWheel(i), IsLeftWheel(i));
            wheels[i] = wheel;
        }
    }


    void FixedUpdate()
    {
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

        // Debug.Log($"Steering input = {steering}");
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

        // Debug.Log($"Throttle input: {throttle}");
    }


    private void Brake()
    {
        // TODO: implement
        float brake = Input.GetAxis("R-Trigger");

        // Debug.Log($"Brake input: {brake}");
    }


    private bool IsFrontWheel(int wheel_i)
    {
        return wheel_i < 2;
    }

    
    private bool IsLeftWheel(int wheel_i)
    {
        return wheel_i % 2 == 0; 
    }
}
