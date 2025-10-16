using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    // An object to instantiate
    public Transform car;
    public Transform wheelObj;

    private Transform wheelFL;
    private Transform wheelFR;
    private Transform wheelRL;
    private Transform wheelRR;

    // Start is called before the first frame update
    void Start()
    {
        float wheelWidthOffset = 1f;
        float wheelLengthOffset = 1.75f;
        float wheelHeightOffset = 0f;

        wheelFL = Instantiate(wheelObj);
        wheelFR = Instantiate(wheelObj);
        wheelRL = Instantiate(wheelObj);
        wheelRR = Instantiate(wheelObj);

        wheelFL.SetParent(car, false);
        wheelFR.SetParent(car, false);
        wheelRL.SetParent(car, false);
        wheelRR.SetParent(car, false);

        wheelFL.Translate(new Vector3(wheelLengthOffset, wheelHeightOffset, wheelWidthOffset));
        wheelFR.Translate(new Vector3(wheelLengthOffset, wheelHeightOffset, -wheelWidthOffset));
        wheelRL.Translate(new Vector3(-wheelLengthOffset, wheelHeightOffset, wheelWidthOffset));
        wheelRR.Translate(new Vector3(-wheelLengthOffset, wheelHeightOffset, -wheelWidthOffset));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
