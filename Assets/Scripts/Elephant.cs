using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elephant : Animal
{
    private float tuskLength;

    protected override void Start()
    {
        base.Start();
        // Additional setup for Elephant's Animator, if needed
        tuskLength = 2.5f; // Example: Set the initial tusk length
    }

    protected override void Idle()
    {
        base.Idle();
        // Additional behaviors specific to elephant's idle
        Debug.Log("Elephant is idling gracefully.");
    }

    protected override void Walk()
    {
        base.Walk();
        // Additional behaviors specific to elephant's walking
        Debug.Log("Elephant is walking majestically.");
    }

    protected override void Run()
    {
        base.Run();
        // Additional behaviors specific to elephant's running
        Debug.Log("Elephant is running away from the player with its powerful strides.");
    }
}