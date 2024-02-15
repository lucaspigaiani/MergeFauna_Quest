using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // Find the main camera in the scene
        FindMainCamera();
    }

    void Update()
    {
        // Rotate the canvas to face the camera
        FaceTheCamera();
    }

    // Method to find the main camera
    private void FindMainCamera()
    {
        mainCamera = Camera.main;

        // Ensure that the main camera is not null
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found in the scene.");
        }
    }

    // Method to rotate the canvas to face the camera
    private void FaceTheCamera()
    {
        if (mainCamera != null)
        {
            // Use LookAt to make the canvas face the main camera
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                                mainCamera.transform.rotation * Vector3.up);
        }
    }
}