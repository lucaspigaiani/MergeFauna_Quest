using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{ 
    public float movementSpeed = 5f;
    public float rotationSpeed = 500f;
    public Animator animator;
    private CharacterController characterController;
    private bool isCapturing = false;
    public GameObject animalRope;
    public GameObject coneArea;
    public ParticleSystem levelUpParticle;

    public Animal captureTarget;

    public bool canCapture = true;
    public GameObject canvas;

    // List to store animals
    public List<Animal> animalList = new List<Animal>();

    public UnityEvent captureAnimalEvent;

    private void Captured() 
    {
        Debug.Log("Captured event!");

        startCapture = false;
        resetCapture = true;

        levelUpParticle.Play();

        animalList.Add(captureTarget);
        if (animalList.Count == 2)
        {
            canCapture = false;
        }
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

    bool startCapture;
    bool resetCapture;
    private void CaptureAction()
    {
        // Check for capturing input
        if (startCapture == true)
        {
            // Call the capturing method
            Capturing();
        }

        if (resetCapture == true)
        {
            // Reset the capture timer when the space key is released
            ResetCapture();
            resetCapture = false;
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
            // Set the capturing flag to true
            isCapturing = true;
        }

        var lookDir = (transform.position - captureTarget.transform.position) * -1;
        lookDir.y = 0; // keep only the horizontal direction
        coneArea.transform.rotation = Quaternion.LookRotation(lookDir);

    }

    void ResetCapture()
    {
        animator.SetBool("ResetCapturing", true);
        animator.SetBool("IsCapturing", false);
        animalRope.gameObject.SetActive(false);
        coneArea.gameObject.SetActive(false);
        isCapturing = false;
    }

    public bool CheckAnimalList() 
    {
        if (animalList.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Animal"))
        {
            if (canCapture == false)
            {
                canvas.SetActive(true);
            }
            else
            {
                if (captureTarget == null)
                {
                    captureTarget = other.GetComponent<Animal>();
                    startCapture = true;
                    resetCapture = false;
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag.Equals("Animal"))
        {
            if (canCapture == false)
            {
                canvas.SetActive(true);
            }
            else
            {
                if (captureTarget == null)
                {
                    captureTarget = other.GetComponent<Animal>();
                    startCapture = true;
                    resetCapture = false;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Animal"))
        {
            if (canCapture == false)
            {
                canvas.SetActive(false);
            }
            else
            {
                if (captureTarget == other.GetComponent<Animal>())
                {
                    Debug.Log("OnTriggerExit");
                    captureTarget = null;
                    startCapture = false;
                    resetCapture = true;
                }
            }
        }
    }
}
