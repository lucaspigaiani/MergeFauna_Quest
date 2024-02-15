using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    private Animator gateAnimator; // Reference to the Animator component controlling the gate animation
    private Transform player; // Reference to the player's transform
    private bool isGateOpen = false; // Flag to track if the gate is currently open
    public float openDistance = 5f; // Distance at which the gate should open

    private void Start()
    {
        // Get references to the player and Animator component
        player = GameObject.FindGameObjectWithTag("Player").transform;
        gateAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        // Check the distance between the player and the gate
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // If the player is within the open distance and the gate is not already open
        if (distanceToPlayer <= openDistance && !isGateOpen)
        {
            // Open the gate
            OpenGate();
        }
        // If the player is outside the open distance and the gate is open
        else if (distanceToPlayer > openDistance && isGateOpen)
        {
            // Close the gate
            CloseGate();
        }
    }

    // Method to open the gate
    private void OpenGate()
    {
        gateAnimator.SetTrigger("Open");
        isGateOpen = true;
    }

    // Method to close the gate
    private void CloseGate()
    {
        gateAnimator.SetTrigger("Close");
        isGateOpen = false;
    }
}