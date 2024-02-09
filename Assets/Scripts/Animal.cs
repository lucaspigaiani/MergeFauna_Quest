using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Animal : MonoBehaviour
{
    protected float speed;
    protected Animator animator;
    protected float percentageCaptured;
    public Image captureBarFillImage; // Reference to the UI image representing the fill bar
    public float alphaAnimationDuration = 0.5f; // Duration of the alpha animation in seconds

    protected CanvasGroup canvasGroup;
    protected bool isOnCaptureArea = false;
    protected enum AnimalState
    {
        Idle,
        Walking,
        Running
    }

    protected AnimalState currentState;

    // Constants for chance of variation
    private const float WalkingVariationChance = 0.2f; // 20% chance of variation
    private const float IdleVariationChance = 0.1f; // 10% chance of variation

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        currentState = AnimalState.Idle;
        percentageCaptured = 0f; // Initialize to 0% captured

        // Add CanvasGroup component
        canvasGroup = captureBarFillImage.gameObject.AddComponent<CanvasGroup>();

        // Initialize the alpha to zero (hidden)
        canvasGroup.alpha = 0f;
    }

    protected virtual void Update()
    {
        if (isOnCaptureArea == false)
        {
            // Decrease percentage captured over time 
            percentageCaptured -= Time.deltaTime;
        }

        // Check conditions for setting the current state based on other factors
        if (percentageCaptured > 0f)
        {
            currentState = AnimalState.Running;

            // Update the capture bar UI
            UpdateCaptureBarUI();
        }
        else
        {
            if (Random.Range(0f, 1f) < WalkingVariationChance)
            {
                currentState = AnimalState.Walking;
            }
            else
            {
                currentState = AnimalState.Idle;
            }

            // Hide the fill bar with alpha animation when percentage captured becomes zero
            if (canvasGroup.alpha > 0f)
            {
                StartCoroutine(AlphaAnimation(canvasGroup, 0f, alphaAnimationDuration));
            }
        }

        // Play the corresponding animation based on the current state
        PlayAnimation();//############ need to fix this part
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
            percentageCaptured += 5f * Time.deltaTime; // Example: Increase by 5% per second (you can adjust this value)
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

    protected virtual void Idle()
    {
        // Play the idle animation
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsRunning", false);
        animator.SetTrigger("IdleTrigger");

        // Add a possibility of idle variation
        if (Random.Range(0f, 1f) < IdleVariationChance)
        {
            animator.SetTrigger("IdleVariationTrigger");
        }

        Debug.Log("Animal is idle.");
    }

    protected virtual void Walk()
    {
        animator.SetBool("IsWalking", true);
        animator.SetBool("IsRunning", false);
        Debug.Log("Animal is walking.");
    }

    protected virtual void Run()
    {
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsRunning", true);
        Debug.Log("Animal is running away from the player.");
    }

    protected virtual void UpdateCaptureBarUI()
    {
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
}