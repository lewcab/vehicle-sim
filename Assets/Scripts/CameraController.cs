using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public Transform camSpace;
    public Transform camTransform;

    public InputActionReference inCamRotation;
    public InputActionReference inCamHeight;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float rotation = inCamRotation.action.ReadValue<float>();
        camSpace.Rotate(
            0f,
            rotation * Time.deltaTime * 100f,
            0f
        );
    }
}
