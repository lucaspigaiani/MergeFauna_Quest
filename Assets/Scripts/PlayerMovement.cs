using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float movementSpeed = 5f;

    private void Update()
    {
        // Get input from the player
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate the movement direction
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        // Move the player
        MovePlayer(movement);
    }

    private void MovePlayer(Vector3 movement)
    {
        // Calculate the movement amount
        Vector3 moveAmount = movement * movementSpeed * Time.deltaTime;

        // Apply the movement
        transform.Translate(moveAmount);
    }
}
