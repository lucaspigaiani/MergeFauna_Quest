using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections;

public class Animal : MonoBehaviour
{
    protected float speed;
    protected Animator animator;
    protected float percentageCaptured;

    protected CanvasGroup canvasGroup;
    protected bool isOnCaptureArea = false;
    protected NavMeshAgent navMeshAgent;
    protected Vector3 targetPosition;
    protected bool isRunning = false;
    protected float minimumWalkDistance = 3;

    public float walkingRadius = 10f; // Radius for random walking
    public float runAwayDistance = 20f; // Distance at which the animal starts running away from the player
    public float alphaAnimationDuration = 0.5f; // Duration of the alpha animation in seconds
    public Image captureBarFillImage; // Reference to the UI image representing the fill bar

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
    private const float MinAnimationTime = 3f; // Minimum time to stay on a animation state
    private const float MaxAnimationTime = 6f; // Max time to stay on a animation state

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        player = GameObject.FindGameObjectWithTag("Player");

        currentState = AnimalState.Idle;
        percentageCaptured = 0f; // Initialize to 0% captured

        // Add CanvasGroup component
        canvasGroup = captureBarFillImage.gameObject.AddComponent<CanvasGroup>();

        // Initialize the alpha to zero (hidden)
        canvasGroup.alpha = 0f;

        // Initiate the coroutine ChangeAnimationState at start
        StartCoroutine(ChangeAnimationState(0f));
    }

    protected virtual void Update()
    {
        if (percentageCaptured >= 1f)
        {
            player.GetComponent<PlayerController>().captureAnimalEvent.Invoke();
        }

        Debug.Log($"RunAwayFromPlayer!");

        // Check conditions for setting the current state based on other factors
        if (percentageCaptured > 0f)
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

                // Hide the fill bar with alpha animation when percentage captured becomes zero
            if (canvasGroup.alpha > 0f)
            {
                StartCoroutine(AlphaAnimation(canvasGroup, 0f, alphaAnimationDuration));
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
            if (navMeshAgent.remainingDistance < 0.1f)
            {
                isRunning = false;
            }
        }
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
        animator.SetTrigger("Walking");
        
        // Play navmeshAgent
        navMeshAgent.isStopped = false;

        navMeshAgent.speed = 3.5f;
    }

    private void SetRandomDestination()
    {
        // Generate a random point within the walking radius
        Vector2 randomPoint = Random.insideUnitCircle * walkingRadius;
        Vector3 randomDestination = new Vector3(randomPoint.x, 0f, randomPoint.y) + transform.position;

        // Check if the distance to the random destination is greater than the minimum walk distance
        if (Vector3.Distance(transform.position, randomDestination) > minimumWalkDistance)
        {
            // Set the destination for the NavMeshAgent
            navMeshAgent.SetDestination(randomDestination);
        }
        else
        {
            // If the distance is less than the minimum, recalculate the random destination
            SetRandomDestination();
        }
    }

    protected virtual void Run()
    {
        // Play the run animation
        animator.SetBool("IsIdle", false);
        animator.SetBool("IsIdleVariation", false);
        animator.SetTrigger("Running");

        // Play navmeshAgent
        navMeshAgent.isStopped = false;

        navMeshAgent.speed = 4f;

        Debug.Log("Animal is running away from the player.");
    }
    GameObject player;
    private void RunAwayFromPlayer()
    {
        // Calculate a position away from the player
        Vector3 runAwayPosition = transform.position + (transform.position - player.transform.position).normalized * runAwayDistance;

        // Check if the calculated position is outside the navigation map
        if (!NavMesh.SamplePosition(runAwayPosition, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            // If the position is outside the navigation map, choose a random side and run
            float randomAngle = Random.Range(0f, 360f);
            Vector3 randomDirection = Quaternion.Euler(0f, randomAngle, 0f) * transform.forward;
            runAwayPosition = transform.position + randomDirection * runAwayDistance;
        }

        // Set the destination for the NavMeshAgent to run away from the player
        navMeshAgent.SetDestination(runAwayPosition);
    }

    protected virtual void UpdateCaptureBarUI()
    {
        if (Vector3.Distance(transform.position, player.transform.position) > runAwayDistance)
        {
            // Decrease percentage captured over time 
            percentageCaptured -= Time.deltaTime;
        }
       
        // Update the UI fill bar based on the percentage captured
        captureBarFillImage.fillAmount = percentageCaptured / 100f; // Assuming the bar ranges from 0 to 1
    }

    // Coroutine for alpha animation
    protected IEnumerator AlphaAnimation(CanvasGroup targetCanvasGroup, float targetAlpha, float duration)
    {
        float startTime = Time.time;
        float startAlpha = targetCanvasGroup.alpha;

        while (Time.time < startTime + duration)
        {
            float progress = (Time.time - startTime) / duration;
            targetCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            yield return null;
        }

        // Ensure the final alpha value is set correctly
        targetCanvasGroup.alpha = targetAlpha;
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
            // Show the fill bar with alpha animation
            StartCoroutine(AlphaAnimation(canvasGroup, 1f, alphaAnimationDuration));

            // Play the corresponding animation based on the current state
            PlayAnimation();
        }
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        // Check if the triggering object has the "Player" tag
        if (other.CompareTag("Player"))
        {
            // Increase percentage captured when staying in the trigger zone
            percentageCaptured += 5f * Time.deltaTime; // Increase by 5% per second 
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
            }
        }
    }
}