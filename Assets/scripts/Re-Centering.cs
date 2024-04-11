using System;
using UnityEngine;

public class SceneOriginRecentering : MonoBehaviour
{
    public GameObject[] objectsToCenter; // The objects you want to keep at the origin
    public float recenteringInterval = 1f; // How often to recenter the scene, in seconds

    private float timer = 0f; // A timer for tracking when to recenter

    void Update()
    {
        timer += Time.deltaTime; // Increment the timer by the time since the last frame

        // Check if the timer has exceeded the recentering interval
        if (timer > recenteringInterval)
        {
            // Calculate the average position of the objects
            Vector3 averagePosition = Vector3.zero;
            foreach (GameObject go in objectsToCenter)
            {
                averagePosition += go.transform.position;
            }
            averagePosition /= objectsToCenter.Length;

            // Recenter the scene based on the average position
            RecenterScene(averagePosition);

            timer = 0f; // Reset the timer
        }
    }

    private void RecenterScene(Vector3 newCenter)
    {
        Vector3 shift = -newCenter; // Calculate the shift vector
        foreach (GameObject go in FindObjectsOfType<GameObject>(true)) // Loop through all active GameObjects in the scene
        {
            if (go != this.gameObject) // Skip the current game object to avoid infinite loop
            {
                go.transform.position += shift; // Shift each GameObject's position by the shift vector
            }
        }
    }
}
