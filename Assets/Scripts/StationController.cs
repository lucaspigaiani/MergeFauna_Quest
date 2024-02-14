using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StationController : MonoBehaviour
{
    private PlayerController playerController;
    public Transform[] deliveryPoint;
    public GameObject stationBeetle;
    public GameObject stationElephant;
    void Start()
    {
        // Get the PlayerController component from the player GameObject
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    float timer = 0f;
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timer += Time.deltaTime;
            if (timer > 1f)
            {
                // Check if the AnimalList in PlayerController has animals
                if (playerController.CheckAnimalList())
                {
                    for (int i = 0; i < playerController.animalList.Count; i++)
                    {
                        GameObject animal;
                        if (playerController.animalList[i].animal == Animal.AnimalEspecies.Beetle)
                        {
                            animal = Instantiate(stationBeetle, deliveryPoint[i].transform);
                        }
                        else
                        {
                            animal = Instantiate(stationElephant, deliveryPoint[i].transform);
                        }

                        ParabolaEffect animalParabola = animal.GetComponent<ParabolaEffect>();
                        animalParabola.startPosition = playerController.transform.position;
                        animalParabola.target = deliveryPoint[i];
                        animalParabola.enabled = true;
                    }
                    playerController.animalList.Clear();
                }
            }
        }
          
    }
}
