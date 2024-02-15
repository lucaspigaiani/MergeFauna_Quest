using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections;

public class Animal : MonoBehaviour
{
    protected Animator animator;
    protected NavMeshAgent navMeshAgent;
    protected ParabolaEffect parabolaEffect;
    private PlayerController playerController;

    protected float speed;
    protected float percentageCaptured;

    protected bool isOnCaptureArea = false;
    protected bool isRunning = false;
    protected float minimumWalkDistance = 10;

    public float walkingRadius = 10f; // Radius for random walking
    public float runAwayDistance = 20f; // Distance at which the animal starts running away from the player
    public Image captureBarFillImage; // Reference to the UI image representing the fill bar

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
    private const float WalkingVariationChance = 0.50f; // 50% chance of variation
    private const float IdleVariationChance = 0.1f; // 10% chance of variation
    private const float MinAnimationTime = 3f; // Minimum time to stay on a animation state
    private const float MaxAnimationTime = 6f; // Max time to stay on a animation state

    

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        parabolaEffect = GetComponent<ParabolaEffect>();

        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<PlayerController>();

        currentState = AnimalState.Idle;
        percentageCaptured = 100f; // Initialize to 100% captured

        // Initiate the coroutine ChangeAnimationState at start
        StartCoroutine(ChangeAnimationState(0f));

        captureBarFillImage.gameObject.SetActive(false);
    }

    bool isAnimalCaptured;
   
    protected virtual void Update()
    {
        
        if (percentageCaptured <= 0f && isAnimalCaptured == false)
        {
            Debug.Log("CaptureAnimal!");
            CaptureAnimal();
            isAnimalCaptured = true;
        }


        // Check conditions for setting the current state based on other factors
        if (percentageCaptured < 100f)
        {
            currentState = AnimalState.Running;
            
            // Update the capture bar UI
            UpdateCaptureBarUI();

            if (isRunning == false)
            {
                PlayAnimation();
                RunAwayFromPlayer();
                Debug.Log("RunAwayFromPlayer!");
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
        if (currentState == AnimalState.Walking)
        {
            if (navMeshAgent.remainingDistance < 0.1f)
            {
                currentState = AnimalState.Idle;
                PlayAnimation();
            }
        }

        if (currentState == AnimalState.Running)
        {
            if (navMeshAgent.remainingDistance < 0.25f)
            {
                isRunning = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            // Toggle between pausing and resuming time
            if (Time.timeScale == 0f)
            {
                // If time is already paused, resume it
                Time.timeScale = 1f;
            }
            else
            {
                // If time is running, pause it
                Time.timeScale = 0f;
            }
        }
    }

    private void CaptureAnimal()
    {
        navMeshAgent.isStopped = true;

        currentState = AnimalState.Idle;
        PlayAnimation();

        captureBarFillImage.gameObject.SetActive(false);
        parabolaEffect.enabled = true;

        playerController.captureAnimalEvent.Invoke();

    }

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
        Debug.Log($"Play animation: {currentState}");
    }

    protected virtual void Idle()
    {
        // Stop the walking and running animation
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsRunning", false);

        // Play the idle animation
        animator.SetBool("IsIdle", true);
        animator.SetBool("IsIdleVariation", false);
        
        // Stop navmeshAgent
        navMeshAgent.isStopped = true;
        // Add a possibility of idle variation
        if (Random.Range(0f, 1f) < IdleVariationChance)
        {
            animator.SetBool("IsIdle", false);
            animator.SetBool("IsIdleVariation", true);
        }
    }

    protected virtual void Walk()
    {
        // Play the walk animation
        animator.SetBool("IsIdle", false);
        animator.SetBool("IsIdleVariation", false);
        animator.SetBool("IsWalking", true);
        animator.SetBool("IsRunning", false);

        // Play navmeshAgent
        navMeshAgent.isStopped = false;

        navMeshAgent.speed = 3f;
    }

    private void SetRandomDestination()
    {
        // Attempt to find a valid position on the NavMesh within the walking radius
        Vector3 randomDestination = FindValidRandomDestination();

        // Set the destination for the NavMeshAgent
        navMeshAgent.SetDestination(randomDestination);
    }

    private Vector3 FindValidRandomDestination()
    {
        for (int i = 0; i < maxSampleAttempts; i++)
        {
            // Generate a random point within the walking radius
            Vector2 randomPoint = Random.insideUnitCircle * walkingRadius;
            Vector3 randomDestination = new Vector3(randomPoint.x, 0f, randomPoint.y) + transform.position;

            // Check if the distance to the random destination is greater than the minimum walk distance
            if (Vector3.Distance(transform.position, randomDestination) > minimumWalkDistance)
            {
                // Check if the position is on the NavMesh
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDestination, out hit, 1.0f, NavMesh.AllAreas))
                {
                    return hit.position;
                }
            }
        }

        Debug.Log("no valid position is found");
        // If no valid position is found, return the current position
        return transform.position;
    }

    protected virtual void Run()
    {
        // Play the run animation
        animator.SetBool("IsIdle", false);
        animator.SetBool("IsIdleVariation", false);
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsRunning", true);

        // Play navmeshAgent
        navMeshAgent.isStopped = false;

        navMeshAgent.speed = 4f;

        Debug.Log("Animal is running away from the player.");
    }
    GameObject player;
    public float avoidanceRadius = 3f;
    public int maxSampleAttempts = 10;

    private void RunAwayFromPlayer()
    {
        // Attempt to find a valid position away from the player
        Vector3 runAwayPosition = FindValidRunAwayPosition();

        // Set the destination for the NavMeshAgent to run away from the player
        navMeshAgent.SetDestination(runAwayPosition);
    }

    private Vector3 FindValidRunAwayPosition()
    {
        for (int i = 0; i < maxSampleAttempts; i++)
        {
            // Calculate a random direction away from the player
            Vector3 randomDirection = Random.onUnitSphere * runAwayDistance;

            // Ensure the position is away from the player
            Vector3 potentialPosition = transform.position + randomDirection;

            // Check if the position is outside the player's avoidance radius
            if (Vector3.Distance(potentialPosition, player.transform.position) > avoidanceRadius)
            {
                // Check if the position is on the NavMesh
                NavMeshHit hit;
                if (NavMesh.SamplePosition(potentialPosition, out hit, 1.0f, NavMesh.AllAreas))
                {
                    // Check if there's a clear path between the animal and the player
                    if (!NavMesh.Raycast(transform.position, player.transform.position, out NavMeshHit raycastHit, NavMesh.AllAreas))
                    {
                        return hit.position;
                    }
                }
            }
        }

        // If no valid position is found, return the current position
        return transform.position;
    }

    protected virtual void UpdateCaptureBarUI()
    {
        
        if (Vector3.Distance(transform.position, player.transform.position) > runAwayDistance)
        {
            // Decrease percentage captured over time 
            percentageCaptured += 20f * Time.deltaTime; // Increase by 20% per second 
        }

        // Update the UI fill bar based on the percentage captured
        captureBarFillImage.fillAmount = percentageCaptured / 100f; // Assuming the bar ranges from 0 to 1
    }

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


    protected void OnTriggerEnter(Collider other)
    {
        // Check if the triggering object has the "Player" tag
        if (other.CompareTag("Player"))
        {
            // Play the corresponding animation based on the current state
            PlayAnimation();
            if (playerController.canCapture == true)
            {
                captureBarFillImage.gameObject.SetActive(true);
            }
        }
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        // Check if the triggering object has the "Player" tag
        if (other.CompareTag("Player"))
        {
            if (playerController.canCapture == true && playerController.captureTarget == this)
            {
                // Increase percentage captured when staying in the trigger zone
                percentageCaptured -= 20f * Time.deltaTime; // Decrease by 20% per second 
                
            }
            if (isOnCaptureArea == false)
            {
                isOnCaptureArea = true;
            }
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        // Check if the triggering object has the "Player" tag
        if (other.CompareTag("Player"))
        {
            if (isOnCaptureArea == true)
            {
                isOnCaptureArea = false;
                captureBarFillImage.gameObject.SetActive(false);
            }
        }
    }
}