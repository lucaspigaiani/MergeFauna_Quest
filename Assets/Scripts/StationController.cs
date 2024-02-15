using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class StationController : MonoBehaviour
{
    private PlayerController playerController;
    private XPController xPController;
    public Transform[] deliveryPoint;

    private float timer = 0f;
    private bool spawn = true;
    private float rotationSpeed = 5.0f; // Adjust this speed as needed
    private float scaleReductionSpeed = 1.0f; // Adjust this speed as needed
    private bool startToRotate;
    private bool moveObjectsToCenter;

    public GameObject leftStationAnimal;
    public GameObject rightStationAnimal;

    public GameObject stationBeetlePrefab;
    public GameObject stationElephantPrefab;

    public ParticleSystem mergeParticle;

    public GameObject beetlephantObject;
    public float scaleIncreaseRate = 0.1f;
    public float maxScale = 0.5f;

    private float currentScale = 0f;

    private void Start()
    {
        // Get the PlayerController component from the player GameObject
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        xPController = GameObject.FindGameObjectWithTag("XPController").GetComponent<XPController>();

    }

    private void Update()
    {
        if (startToRotate == true)
        {
            TurnObjectsToFaceEachOther();
            
        }
        if (moveObjectsToCenter == true)
        {
            MoveObjectsToCenter();
        }
    }

    IEnumerator SetStartToRotate()
    {
        yield return new WaitForSeconds(1f);
        startToRotate = true;
        StartCoroutine(SetMoveObjectsToCenter());
    } 
    
    IEnumerator SetMoveObjectsToCenter()
    {
        yield return new WaitForSeconds(1f);
        moveObjectsToCenter = true;
        mergeParticle.Play();
        StartCoroutine(EndMerge());
    }

    IEnumerator EndMerge()
    {
        yield return new WaitForSeconds(1f);
      
        if (leftStationAnimal.name.Equals("Station Beetle"))
        {
            xPController.AddXP(Animal.AnimalEspecies.Beetle);
        }
        else
        {
            xPController.AddXP(Animal.AnimalEspecies.Elephant);
        }

        if (rightStationAnimal.name.Equals("Station Beetle"))
        {
            xPController.AddXP(Animal.AnimalEspecies.Beetle);
        }
        else
        {
            xPController.AddXP(Animal.AnimalEspecies.Elephant);
        }

        Destroy(leftStationAnimal);
        Destroy(rightStationAnimal);
        startToRotate = false;
        moveObjectsToCenter = false;

        if (beetlephantObject.activeInHierarchy == false)
        {
            StartCoroutine(TurnOnAndScaleBeetlephant());
        }
    }

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

        Debug.Log("Beetlephant activation and scaling complete!");
    }

    private void TurnObjectsToFaceEachOther()
    {
        if (leftStationAnimal != null && rightStationAnimal != null)
        {
            // Calculate the direction from leftStationAnimal to rightStationAnimal
            Vector3 direction = rightStationAnimal.transform.position - leftStationAnimal.transform.position;

            // Calculate the rotation to face rightStationAnimal
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Gradually rotate leftStationAnimal to face rightStationAnimal
            leftStationAnimal.transform.rotation = Quaternion.Slerp(leftStationAnimal.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Gradually rotate rightStationAnimal to face leftStationAnimal (optional)
            rightStationAnimal.transform.rotation = Quaternion.Slerp(rightStationAnimal.transform.rotation, Quaternion.LookRotation(-direction), rotationSpeed * Time.deltaTime);
        }
    }

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

                        animal.transform.parent = deliveryPoint[i].transform;

                        if (i == 0)
                        {
                            leftStationAnimal = animal;
                        }
                        else if (i == 1)
                        {
                            rightStationAnimal = animal;
                        }

                        ParabolaEffect animalParabola = animal.GetComponent<ParabolaEffect>();
                        animalParabola.startPosition = playerController.transform.position;
                        animalParabola.target = deliveryPoint[i];
                        animalParabola.enabled = true;
                    }
                    StartCoroutine(SetStartToRotate());
                    playerController.animalList.Clear();
                    spawn = false;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            timer = 0;
            spawn = true;
        }
    }
}
