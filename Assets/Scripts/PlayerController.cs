using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    // Private variables
    private Animator animator;

    // Public variables
    public float movementSpeed = 5f;
    public float rotationSpeed = 500f;
    public Animal captureTarget;
    public GameObject animalRope;
    public GameObject coneArea;
    public ParticleSystem levelUpParticle;
    public GameObject canvas;

    // List to store captured animals
    public List<Animal> animalList = new List<Animal>();

    // UnityEvent for capturing animals
    [HideInInspector] public UnityEvent captureAnimalEvent;

    // Capture state flags
     public bool canCapture = true;
    [SerializeField] private bool isCapturing = false;
    [SerializeField] private bool startCapture;
    [SerializeField] private bool resetCapture;

    // CharacterController reference
    private CharacterController characterController;

    // Method called when an animal is captured
    private void Captured()
    {
        startCapture = false;
        resetCapture = true;

        levelUpParticle.Play();

        // Add captured animal to the list
        animalList.Add(captureTarget);
        
        // Disable capturing after capturing two animals
        if (animalList.Count == 2)
        {
            canCapture = false;
        }
    }

    // Initialization
    void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        captureAnimalEvent.AddListener(Captured);
    }

    // Update is called once per frame
    void Update()
    {
        // Handle player movement
        Movement();

        // Handle capturing action
        CaptureAction();

        // Pause/Resume time with Left Control key
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Time.timeScale = (Time.timeScale == 0f) ? 1f : 0f;
        }

        // Set the y-position to 0 to keep the player on the same level
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }

    // Handle player movement
    private void Movement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        

        if (movement.magnitude > 0f)
        {
            // Rotate the player to face the movement direction
            Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);

            // Play the movement animation
            animator.SetBool("IsIdle", false);
            animator.SetBool("IsRunning", true);
        }
        else
        {
            // If not moving, stop the movement animation
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsIdle", true);
        }

        // Move the player based on the input using CharacterController
        characterController.Move(movement * movementSpeed * Time.deltaTime);
    }

    // Handle capturing action
    private void CaptureAction()
    {
        if (startCapture == true)
        {
            Capturing();
        }

        if (resetCapture == true)
        {
            ResetCapture();
            resetCapture = false;
        }
    }

    // Handle capturing
    private void Capturing()
    {
        if (!isCapturing)
        {
            // Play the capturing animation
            animator.SetBool("ResetCapturing", false);
            animator.SetBool("IsCapturing", true);
            animalRope.gameObject.SetActive(true);
            coneArea.gameObject.SetActive(true);
            isCapturing = true;
        }

        // Rotate coneArea to face the captured animal
        var lookDir = (transform.position - captureTarget.transform.position) * -1;
        lookDir.y = 0;
        coneArea.transform.rotation = Quaternion.LookRotation(lookDir);
    }

    // Reset capturing
    private void ResetCapture()
    {
        animator.SetBool("IsCapturing", false);
        animator.SetBool("ResetCapturing", true);
        animalRope.gameObject.SetActive(false);
        coneArea.gameObject.SetActive(false);
        isCapturing = false;
        captureTarget = null;
    }

    // Check if there are captured animals
    public bool CheckAnimalList()
    {
        return animalList.Count > 0;
    }

    // Reset the possibilitie to capture and clear the list of animals
    public void ClearAnimalList() 
    {
        animalList.Clear();
        canCapture = true;
    }

    // Handle trigger events
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Animal"))
        {
            HandleAnimalTrigger(other);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Animal"))
        {
            HandleAnimalTrigger(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Animal"))
        {
            if (canCapture == false)
            {
                canvas.SetActive(false);
            }
            else
            {
                if (captureTarget == other.GetComponent<Animal>())
                {
                    captureTarget = null;
                    startCapture = false;
                    resetCapture = true;
                }
            }
        }
    }

    // Handle trigger events for animals
    private void HandleAnimalTrigger(Collider other)
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