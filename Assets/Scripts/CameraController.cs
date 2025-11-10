using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform camSpace;
    public Transform camTransform;


    // Update is called once per frame
    void FixedUpdate()
    {
        float rotation = Input.GetAxis("R-Stick-X");
        camSpace.Rotate(
            0f,
            rotation * Time.deltaTime * 100f,
            0f
        );

        // Debug.Log($"Camera input: {rotation}");
    }
}
