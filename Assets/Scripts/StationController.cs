using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationController : MonoBehaviour
{
    // References to other controllers and objects
    private PlayerController playerController;
    private XPController xPController;

    // Delivery points for spawned animals
    public Transform[] deliveryPoint;

    // Timer and flags for rotation and movement
    private float timer = 0f;
    private bool spawn = true;
    private bool startToRotate;
    private bool moveObjectsToCenter;

    // Rotation and scaling parameters
    private float rotationSpeed = 5.0f;
    private float scaleReductionSpeed = 1.0f;
    private float scaleIncreaseRate = 0.1f;
    private float maxScale = 0.5f;
    private float currentScale = 0f;

    // Objects in the station
    public GameObject leftStationAnimal;
    public GameObject rightStationAnimal;

    // Prefabs for station animals
    public GameObject stationBeetlePrefab;
    public GameObject stationElephantPrefab;

    // Particle system for merging
    public ParticleSystem mergeParticle;

    // Beetlephant object and flags
    public GameObject beetlephantObject;

    private void Start()
    {
        // Get references to other controllers
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        xPController = GameObject.FindGameObjectWithTag("XPController").GetComponent<XPController>();
    }

    private void Update()
    {
        // Check and perform rotation and movement when necessary
        if (startToRotate == true)
        {
            TurnObjectsToFaceEachOther();
        }
        if (moveObjectsToCenter == true)
        {
            MoveObjectsToCenter();
        }
    }

    // Coroutine to set the startToRotate flag
    IEnumerator SetStartToRotate()
    {
        yield return new WaitForSeconds(1f);
        startToRotate = true;
        // Start the next coroutine to move objects to the center
        StartCoroutine(SetMoveObjectsToCenter());
    }

    // Coroutine to set the moveObjectsToCenter flag
    IEnumerator SetMoveObjectsToCenter()
    {
        yield return new WaitForSeconds(1f);
        moveObjectsToCenter = true;
        // Play merge particle effect
        mergeParticle.Play();
        // Start the next coroutine to end the merge process
        StartCoroutine(EndMerge());
    }

    // Coroutine to end the merge process
    IEnumerator EndMerge()
    {
        yield return new WaitForSeconds(1f);

        // Add XP based on the animals in the left and right stations
        AddXPFromStationAnimal(leftStationAnimal);
        AddXPFromStationAnimal(rightStationAnimal);

        // Destroy the animals in the stations
        Destroy(leftStationAnimal);
        Destroy(rightStationAnimal);

        // Reset flags
        startToRotate = false;
        moveObjectsToCenter = false;

        // If the beetlephantObject is not active, turn it on and scale it
        if (beetlephantObject.activeInHierarchy == false)
        {
            StartCoroutine(TurnOnAndScaleBeetlephant());
        }
    }

    // Coroutine to turn on and scale the beetlephantObject
    IEnumerator TurnOnAndScaleBeetlephant()
    {
        // Instantiate the Beetlephant object if not assigned
        if (beetlephantObject == null)
        {
            beetlephantObject = new GameObject("Beetlephant");
        }

        // Set the initial scale to zero
        beetlephantObject.transform.localScale = Vector3.zero;

        // Activate the Beetlephant object
        beetlephantObject.SetActive(true);

        // Gradually increase the scale until it reaches the specified maxScale
        while (currentScale < maxScale)
        {
            currentScale += scaleIncreaseRate * Time.deltaTime;
            beetlephantObject.transform.localScale = new Vector3(currentScale, currentScale, currentScale);

            yield return null;
        }

        // Ensure the final scale is exactly the maxScale
        beetlephantObject.transform.localScale = new Vector3(maxScale, maxScale, maxScale);
    }

    // Rotate the left and right station animals to face each other
    private void TurnObjectsToFaceEachOther()
    {
        if (leftStationAnimal != null && rightStationAnimal != null)
        {
            // Calculate the direction from leftStationAnimal to rightStationAnimal
            Vector3 direction = rightStationAnimal.transform.position - leftStationAnimal.transform.position;

            // Check if the direction vector is not exactly zero
            if (direction != Vector3.zero)
            {
                // Calculate the rotation to face rightStationAnimal
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // Gradually rotate leftStationAnimal to face rightStationAnimal
                leftStationAnimal.transform.rotation = Quaternion.Slerp(leftStationAnimal.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                // Gradually rotate rightStationAnimal to face leftStationAnimal (optional)
                rightStationAnimal.transform.rotation = Quaternion.Slerp(rightStationAnimal.transform.rotation, Quaternion.LookRotation(-direction), rotationSpeed * Time.deltaTime);
            }
        }
    }

    // Move the left and right station animals to the center and scale them down
    private void MoveObjectsToCenter()
    {
        if (leftStationAnimal != null && rightStationAnimal != null)
        {
            // Calculate the center position between the two objects
            Vector3 centerPosition = (leftStationAnimal.transform.position + rightStationAnimal.transform.position) / 2f;

            // Move both objects to the center
            leftStationAnimal.transform.position = centerPosition;
            rightStationAnimal.transform.position = centerPosition;

            // Gradually scale down both objects
            ScaleDownObject(leftStationAnimal);
            ScaleDownObject(rightStationAnimal);
        }
    }

    // Gradually scale down the given object
    private void ScaleDownObject(GameObject obj)
    {
        if (obj != null)
        {
            // Gradually scale down the object
            Vector3 currentScale = obj.transform.localScale;
            Vector3 targetScale = Vector3.zero;

            obj.transform.localScale = Vector3.Lerp(currentScale, targetScale, scaleReductionSpeed * Time.deltaTime);
        }
    }

    // Triggered when the player stays in the collider
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timer += Time.deltaTime;
            if (timer > 5 && spawn == true)
            {
                // Check if the AnimalList in PlayerController has animals
                if (playerController.CheckAnimalList())
                {
                    // Iterate through the player's animal list and spawn corresponding animals in the stations
                    for (int i = 0; i < playerController.animalList.Count; i++)
                    {
                        GameObject animal;
                        if (playerController.animalList[i].animal == Animal.AnimalEspecies.Beetle)
                        {
                            animal = Instantiate(stationBeetlePrefab, playerController.transform.position, Quaternion.identity);
                        }
                        else
                        {
                            animal = Instantiate(stationElephantPrefab, playerController.transform.position, Quaternion.identity);
                        }

                        // Set the spawned animal as a child of the delivery point
                        animal.transform.parent = deliveryPoint[i].transform;

                        // Assign the spawned animals to left and right stations
                        if (i == 0)
                        {
                            leftStationAnimal = animal;
                        }
                        else if (i == 1)
                        {
                            rightStationAnimal = animal;
                        }

                        // Set up the parabola effect for the spawned animal
                        ParabolaEffect animalParabola = animal.GetComponent<ParabolaEffect>();
                        animalParabola.startPosition = playerController.transform.position;
                        animalParabola.target = deliveryPoint[i];
                        animalParabola.enabled = true;
                    }

                    // Start the merge process
                    StartCoroutine(SetStartToRotate());

                    // Clear the player's animal list
                    playerController.ClearAnimalList();
                    spawn = false;
                }
            }
        }
    }

    // Triggered when the player exits the collider
    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            timer = 0;
            spawn = true;
        }
    }

    // Add XP based on the animal's species
    private void AddXPFromStationAnimal(GameObject stationAnimal)
    {
        if (stationAnimal != null)
        {
            if (stationAnimal.name.Equals("Station Beetle"))
            {
                xPController.AddXP(Animal.AnimalEspecies.Beetle);
            }
            else
            {
                xPController.AddXP(Animal.AnimalEspecies.Elephant);
            }
        }
    }
}