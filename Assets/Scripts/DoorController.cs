using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    private Animator gateAnimator; // Reference to the Animator component controlling the gate animation
    private Transform player; // Reference to the player's transform
    public float openDistance = 5f; // Distance at which the gate should open
    private bool isGateOpen = false; // Flag to track if the gate is currently open

    private void Start()
    {
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
            gateAnimator.SetTrigger("Open");
            isGateOpen = true;
        }
        // If the player is outside the open distance and the gate is open
        else if (distanceToPlayer > openDistance && isGateOpen)
        {
            // Close the gate
            gateAnimator.SetTrigger("Close");
            isGateOpen = false;
        }
    }
}