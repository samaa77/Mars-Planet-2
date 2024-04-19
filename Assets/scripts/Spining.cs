// This script causes the spacecraft to spin around its own axis in a pulsing motion.

// Import the necessary libraries.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spining : MonoBehaviour
{
    public float speed = 100.0f;
    public float pulseDuration = 1.0f;
    public float pulseInterval = 1.0f;

    private float timeSinceLastPulse = 0.0f;
    private bool isSpinning = false;

    void Update()
    {
        // Check if it is time to start a new pulse.
        if (timeSinceLastPulse >= pulseInterval)
        {
            isSpinning = true;
            timeSinceLastPulse = 0.0f;
        }

        // If the spacecraft is spinning, rotate it around its own axis.
        if (isSpinning)
        {
            transform.Rotate(Vector3.up * speed * Time.deltaTime);
        }

        // Increment the time since the last pulse.
        timeSinceLastPulse += Time.deltaTime;

        // Check if the pulse has ended.
        if (timeSinceLastPulse >= pulseDuration)
        {
            isSpinning = false;
        }
    }
}
