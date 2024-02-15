using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections;

public class Animal : MonoBehaviour
{
    // Components
    protected Animator animator;
    protected NavMeshAgent navMeshAgent;
    protected ParabolaEffect parabolaEffect;
    protected PlayerController playerController;

    // Attributes
    public Image captureBarFillImage; // Reference to the UI image representing the fill bar
    protected float speed;
    protected float percentageCaptured;
    protected bool isOnCaptureArea = false;
    protected bool isRunning = false;
    protected float minimumWalkDistance = 10;
    protected float avoidanceRadius = 3f;
    protected int maxSampleAttempts = 10;
    protected bool isAnimalCaptured;
    protected float avoidDistance = 5f;

    // Enums
    public enum AnimalEspecies
    {
        Beetle,
        Elephant
    }
    public AnimalEspecies animal;

    public enum AnimalState
    {
        Idle,
        Walking,
        Running
    }
    public AnimalState currentState;

    // Constants for chance of variation
    private const float WalkingVariationChance = 0.5f; // 50% chance of variation
    private const float IdleVariationChance = 0.1f; // 10% chance of variation
    private const float MinAnimationTime = 3f; // Minimum time to stay on an animation state
    private const float MaxAnimationTime = 6f; // Max time to stay on an animation state
    private const float CaptureRate = 20f; // 20% per second

    protected virtual void Start()
    {
        // Initialization
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        parabolaEffect = GetComponent<ParabolaEffect>();
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        currentState = AnimalState.Idle;
        percentageCaptured = 100f; // Initialize to 100% captured

        // Start animation state change coroutine
        StartCoroutine(ChangeAnimationState(0f));

        // Deactivate capture bar initially
        captureBarFillImage.gameObject.SetActive(false);
    }

    protected virtual void Update()
    {
        // Check if the animal is fully captured
        if (percentageCaptured <= 0f && !isAnimalCaptured)
        {
            CaptureAnimal();
            isAnimalCaptured = true;
        }

        // Check conditions for setting the current state based on other factors
        if (percentageCaptured < 100f)
        {
            currentState = AnimalState.Running;

            // Update the capture bar UI
            UpdateCaptureBarUI();

            if (!isRunning)
            {
                PlayAnimation();
                RunAwayFromPlayer();
                isRunning = true;
            }
        }
        else
        {
            if (currentState == AnimalState.Running)
            {
                currentState = AnimalState.Idle;
                PlayAnimation();
                isRunning = false;
            }
        }

        // Check if the animal is walking and reached its destination
        if (currentState == AnimalState.Walking)
        {
            if (navMeshAgent.remainingDistance < 0.1f)
            {
                currentState = AnimalState.Idle;
                PlayAnimation();
            }
        }

        // Check if the animal is running and reached its destination
        if (currentState == AnimalState.Running)
        {
            if (navMeshAgent.remainingDistance < 0.1f)
            {
                if (isOnCaptureArea)
                {
                    RunAwayFromPlayer();
                }
                else
                {
                    SetRandomDestination();
                }
            }
        }
    }

    // Method to capture the animal
    private void CaptureAnimal()
    {
        navMeshAgent.isStopped = true;
        currentState = AnimalState.Idle;
        PlayAnimation();
        captureBarFillImage.gameObject.SetActive(false);
        parabolaEffect.enabled = true;
        playerController.captureAnimalEvent.Invoke();
    }

    // Method to play the appropriate animation based on the current state
    protected virtual void PlayAnimation()
    {
        switch (currentState)
        {
            case AnimalState.Idle:
                Idle();
                break;
            case AnimalState.Walking:
                Walk();
                break;
            case AnimalState.Running:
                Run();
                break;
        }
    }

    // Method to handle idle animation
    protected virtual void Idle()
    {
        // Stop the idleVariation, walking and running animation
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsRunning", false);
        animator.SetBool("IsIdleVariation", false);

        // Play the idle animation
        animator.SetBool("IsIdle", true);

        // Stop navmeshAgent
        navMeshAgent.isStopped = true;

        // Add a possibility of idle variation
        if (Random.Range(0f, 1f) < IdleVariationChance)
        {
            animator.SetBool("IsIdleVariation", true);
        }
    }

    // Method to handle walk animation
    protected virtual void Walk()
    {
        // Stop the idles and running animation
        animator.SetBool("IsIdle", false);
        animator.SetBool("IsIdleVariation", false);
        animator.SetBool("IsRunning", false);

        // Play the walk animation
        animator.SetBool("IsWalking", true);

        // Play navmeshAgent
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = 3f;
    }

    // Method to set a random destination for the animal
    protected void SetRandomDestination()
    {
        // Generate a random position within the NavMesh area
        Vector3 randomDestination = RandomNavMeshPosition(10f);

        // Set the NavMeshAgent's destination to the random position
        navMeshAgent.SetDestination(randomDestination);
    }

    // Method to get a random position within the NavMesh area
    protected Vector3 RandomNavMeshPosition(float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;

        NavMeshHit navMeshHit;
        NavMesh.SamplePosition(randomDirection, out navMeshHit, radius, NavMesh.AllAreas);

        return navMeshHit.position;
    }

    // Method to handle run animation
    protected virtual void Run()
    {
        // Stop the idles and walk animation
        animator.SetBool("IsIdle", false);
        animator.SetBool("IsIdleVariation", false);
        animator.SetBool("IsWalking", false);

        // Play the run animation
        animator.SetBool("IsRunning", true);

        // Play navmeshAgent
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = 4f;
    }

    // Method to make the animal run away from the player
    void RunAwayFromPlayer()
    {
        // Calculate the opposite direction from the player
        Vector3 oppositeDirection = transform.position - playerController.transform.position;

        // Set the NavMeshAgent's destination to the opposite direction
        Vector3 destination = transform.position + oppositeDirection;
        if (Vector3.Distance(transform.position, destination) < avoidDistance)
        {
            // If the destination is too close, choose a position with some units ahead
            destination = transform.position + oppositeDirection.normalized * avoidDistance;
        }
        navMeshAgent.SetDestination(destination);

        // Check if the NavMeshAgent is at the edge of the NavMesh area
        if (AtEdgeOfNavMesh())
        {
            // Choose a random side opposite to the player
            Vector3 randomDirection = Quaternion.Euler(0, Random.Range(0, 360), 0) * oppositeDirection;
            destination = transform.position + randomDirection;
            if (Vector3.Distance(transform.position, destination) < avoidDistance)
            {
                // If the destination is too close, choose a position with some units ahead
                destination = transform.position + randomDirection.normalized * avoidDistance;
            }
            navMeshAgent.SetDestination(destination);
        }
    }

    // Method to check if the NavMeshAgent is at the edge of the NavMesh area
    bool AtEdgeOfNavMesh()
    {
        NavMeshHit navMeshHit;

        // Check if the NavMeshAgent's position is close to the edge
        if (NavMesh.SamplePosition(transform.position, out navMeshHit, 0.1f, NavMesh.AllAreas))
        {
            return false;
        }

        return true;
    }

    // Method to update the capture bar UI
    protected virtual void UpdateCaptureBarUI()
    {
        if (!isOnCaptureArea)
        {
            percentageCaptured += CaptureRate * Time.deltaTime; // Increase by capture rate (0f-100f) per second 
        }
        else
        {
            percentageCaptured -= CaptureRate * Time.deltaTime; // Decrease by capture rate (0f-100f) per second 
        }

        // Update the UI fill bar based on the percentage captured
        captureBarFillImage.fillAmount = percentageCaptured / 100f; // Assuming the bar ranges from 0 to 1
    }

    // Coroutine to change the animal state over time
    protected IEnumerator ChangeAnimationState(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        if (currentState == AnimalState.Idle)
        {
            if (Random.Range(0f, 1f) < WalkingVariationChance)
            {
                currentState = AnimalState.Walking;
                SetRandomDestination();
            }
            else
            {
                currentState = AnimalState.Idle;
            }

            PlayAnimation();
        }

        float newWaitTime = Random.Range(MinAnimationTime, MaxAnimationTime);
        StartCoroutine(ChangeAnimationState(newWaitTime));
    }

    // OnTriggerEnter event handler
    protected void OnTriggerEnter(Collider other)
    {
        // Check if the triggering object has the "Player" tag
        if (other.CompareTag("Player"))
        {
            // Play the corresponding animation based on the current state
            PlayAnimation();
            if (playerController.canCapture)
            {
                captureBarFillImage.gameObject.SetActive(true);
            }

            if (!isOnCaptureArea)
            {
                isOnCaptureArea = true;
            }
        }
    }

    // OnTriggerStay event handler
    protected virtual void OnTriggerStay(Collider other)
    {
        // Check if the triggering object has the "Player" tag
        if (other.CompareTag("Player"))
        {
            if (playerController.canCapture && playerController.captureTarget == this)
            {
                UpdateCaptureBarUI();
                
            }
            if (!isOnCaptureArea)
            {
                isOnCaptureArea = true;
            }
        }
    }

    // OnTriggerExit event handler
    protected virtual void OnTriggerExit(Collider other)
    {
        // Check if the triggering object has the "Player" tag
        if (other.CompareTag("Player"))
        {
            if (isOnCaptureArea)
            {
                isOnCaptureArea = false;
                captureBarFillImage.gameObject.SetActive(false);
            }
        }
    }
}