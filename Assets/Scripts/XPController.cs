using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class XPController : MonoBehaviour
{
    public Image xpFillImage; // Reference to the UI Image
    public TextMeshProUGUI xpText; // Reference to the UI Text

    private float currentXP = 0f;
    private float currentLevel = 1f;
    private float xpToNextLevel = 100f; // Adjust this value as needed for the XP required to level up
    private float xpMultiplier = 1f;

    public float beetleXP;
    public float elephantXP;

    public ParticleSystem xpIncrease;


    void Start()
    {
        UpdateUI();
    }

    public void AddXP( Animal.AnimalEspecies animalType)
    {
        float xpToAdd = 0;

        // Check the animal type and add XP accordingly
        switch (animalType)
        {
            case Animal.AnimalEspecies.Beetle:
                xpToAdd = beetleXP * xpMultiplier; // Adjust the XP multiplier as needed
                break;

            case Animal.AnimalEspecies.Elephant:
                xpToAdd = elephantXP * xpMultiplier;
                break;

            default:
                break;
        }

        // Add XP to the current XP
        currentXP += xpToAdd;

        // Check for level up
        while (currentXP >= xpToNextLevel && currentLevel < 5)
        {
            LevelUp();
        }

        // Update the UI after adding XP
        UpdateUI();
        xpIncrease.Play();
    }

    void LevelUp()
    {
        // Increase the level
        currentLevel++;

        // Reset XP to 0 for the next level
        currentXP = 0;

        // Update the XP required for the next level (you can adjust this formula)
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.5f);
    }

    void UpdateUI()
    {
        // Calculate the fill amount for the XP Image
        float fillAmount = currentXP / xpToNextLevel;

        // Set the fill amount of the Image
        xpFillImage.fillAmount = fillAmount;

        // Update the Text with XP information
        xpText.text = "Lv. " + currentLevel;
    }
}
