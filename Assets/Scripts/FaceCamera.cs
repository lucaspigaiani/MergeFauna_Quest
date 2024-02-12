using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // Find the main camera in the scene
        mainCamera = Camera.main;

        // Ensure that the main camera is not null
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found in the scene.");
        }
    }

    void Update()
    {
        // Rotate the canvas to face the camera
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                                mainCamera.transform.rotation * Vector3.up);
        }
    }
}