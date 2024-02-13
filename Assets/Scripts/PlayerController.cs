using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{ 
    public float movementSpeed = 5f;
    public float rotationSpeed = 500f;
    public float captureHoldTime = 10f; // Time to hold space for capturing
    public Animator animator;
    private CharacterController characterController;
    private bool isCapturing = false;
    private float captureTimer = 0f;
    public GameObject animalRope;
    public GameObject coneArea;
    private bool preventCapture;



    public UnityEvent captureAnimalEvent;

    private void Captured() 
    {
        Debug.Log("Captured event!");
    }
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        captureAnimalEvent.AddListener(Captured);
    }

    void Update()
    {
        Movement();
        CaptureAction();

        if (Input.anyKeyDown && captureAnimalEvent != null)
        {
            captureAnimalEvent.Invoke();
        }
    }

    private void Movement()
    {
        // Check for movement input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate the movement direction
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        // Check if there is any movement input
        if (movement.magnitude > 0f)
        {
            // Rotate the player to face the movement direction
            Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);

            // Play the movement animation
            animator.SetBool("IsRunning", true);
            animator.SetBool("IsIdle", false);
        }
        else
        {
            // If not moving, stop the movement animation
            // If not moving, stop the movement animation
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsIdle", true);
        }

        // Move the player based on the input using CharacterController
        characterController.Move(movement * movementSpeed * Time.deltaTime);
    }

    private void CaptureAction()
    {
        // Check for capturing input
        if (Input.GetKey(KeyCode.Space))
        {
            if (preventCapture == true)
            {
                // Call the capturing method
                Capturing();

                // Check if the player has held down the space key for ten seconds
                if (isCapturing)
                {
                    captureTimer += Time.deltaTime;

                    if (captureTimer >= captureHoldTime)
                    {
                        // Call the captured method
                        ResetCapture();
                        preventCapture = false;
                    }
                }
            }
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            // Reset the capture timer when the space key is released
            ResetCapture();
        }
    }

    void Capturing()
    {
        // Play the capturing animation
        if (isCapturing == false)
        {
            animator.SetBool("IsCapturing", true);
            animator.SetBool("ResetCapturing", false);
            animalRope.gameObject.SetActive(true);
            coneArea.gameObject.SetActive(true);
        }

        // Set the capturing flag to true
        isCapturing = true;
    }

    void ResetCapture()
    {
        captureTimer = 0f;
        animator.SetBool("ResetCapturing", true);
        animator.SetBool("IsCapturing", false);
        animalRope.gameObject.SetActive(false);
        coneArea.gameObject.SetActive(false);
        isCapturing = false;
        preventCapture = true;
    }
}
