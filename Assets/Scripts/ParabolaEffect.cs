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
        startTime = Time.time;
        startPosition = transform.position;
        targetPosition = target.position;
    }

    void Update()
    {
        float progress = (Time.time - startTime) / duration;

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
            // Ensure the object reaches the target position
            transform.position = targetPosition;
            transform.localScale = new Vector3(finalScale, finalScale, finalScale);

            // Optionally, you can destroy the object or deactivate it after reaching the target
            // Destroy(gameObject);
            // gameObject.SetActive(false);
            this.enabled = false;
        }

    }
}
