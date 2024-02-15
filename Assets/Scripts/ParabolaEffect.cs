using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParabolaEffect : MonoBehaviour
{
    public Transform target; // Assign the target player object in the Unity Editor
    public float duration = 1.0f; // Duration of the parabola movement
    public float height = 2.0f; // Height of the parabola
    public float startScale;
    public float finalScale;

    private float startTime;
    public Vector3 startPosition;
    public Vector3 targetPosition;

    void Start()
    {
        // Initialize start time and positions
        InitializeParabola();
    }

    void Update()
    {
        // Calculate the progress of the parabola movement
        float progress = (Time.time - startTime) / duration;

        // If the parabola is still in progress
        if (progress < 1.0f)
        {
            // Calculate parabola height using Mathf.Lerp for smooth transition
            float y = Mathf.Lerp(startPosition.y, targetPosition.y + height, progress);

            // Calculate parabola position using Mathf.Lerp for smooth transition
            float x = Mathf.Lerp(startPosition.x, targetPosition.x, progress);
            float z = Mathf.Lerp(startPosition.z, targetPosition.z, progress);

            // Set the object's position
            transform.position = new Vector3(x, y, z);

            // Scale down the object over time
            float scale = Mathf.Lerp(startScale, finalScale, progress);
            transform.localScale = new Vector3(scale, scale, scale);
        }
        else
        {
            // Ensure the object reaches the target position and scale
            CompleteParabola();
        }
    }

    // Method to initialize start time and positions
    private void InitializeParabola()
    {
        startTime = Time.time;
        startPosition = transform.position;
        targetPosition = target.position;
    }

    // Method to complete the parabola movement
    private void CompleteParabola()
    {
        // Set the final position and scale
        transform.position = targetPosition;
        transform.localScale = new Vector3(finalScale, finalScale, finalScale);

        this.enabled = false; // Disable the script to stop further updates
    }
}
