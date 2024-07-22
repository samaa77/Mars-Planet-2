using System;
using System.Collections;
using UnityEngine;

public class ThrustersPulses2 : MonoBehaviour
{
    // Thruster control variables
    // Array to store the thruster directions
    private Vector3[] thrusterDirections = { new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1) };    
    public float GroupedMagnitude = 1.0f; // Multiplier for all thrusters' magnitudes    
    public float[] thrusterMagnitudes = new float[4]; // Array to store the thruster magnitudes

    public GameObject[] thrusterLocations; // Array to store the thruster locations
    public ParticleSystem[] thrusterEffects;

    // Array to store the rotation angles for each thruster
    private Vector3[] rotationAngles = { new Vector3(30, -28, 4), new Vector3(-30, 28, -4), new Vector3(30, 28, -4), new Vector3(-30, -28, 4) };    

    private Rigidbody Rb;
    private float[] previousThrusterMagnitudes;
    private Quaternion[] previousThrusterRotations;

    // Public variable to control the pulse duration
    public float pulseDuration = 0.1f; // Default pulse duration is 0.1 seconds

    private float pulseStartTime; // Time when the pulse started

    private float[] possibleMagnitudes = { 0f, 151.25f, 302.5f }; // Possible magnitudes for thrusters

    // Additional variables for debugging and effects control
    public bool debug = true;
    public bool effectsEnabled = true;
    private float maxForce = 302.5f;

    void Start()
    {
        // Check if the thrusterLocations array is assigned
        if (thrusterLocations == null || thrusterLocations.Length == 0)
        {
            Debug.LogError("Thruster locations array not assigned or empty." + '\n' + 
            "Please assign the empty objects representing the thruster locations to the thrusterLocations array in the inspector.");
            return;
        }

        // Validate thruster effects
        if (thrusterEffects == null || thrusterEffects.Length != thrusterLocations.Length)
        {
            Debug.LogError("Thruster effects array not assigned or does not match the number of thrusters.");
            return;
        }

        Rb = GetComponent<Rigidbody>();
        previousThrusterMagnitudes = new float[thrusterMagnitudes.Length];
        previousThrusterRotations = new Quaternion[thrusterLocations.Length];

        // Initialize the previous thruster rotations based on the rotationAngles array
        for (int i = 0; i < thrusterLocations.Length; i++)
        {
            previousThrusterRotations[i] = Quaternion.Euler(rotationAngles[i]);
        }

        // Start a coroutine to set random thruster magnitudes
        StartCoroutine(SetRandomThrusterMagnitudes());
    }

    void FixedUpdate()
    {
        // Check if the Rigidbody component is assigned
        if (Rb == null)
        {
            Debug.LogError("Rigidbody component not found. Please make sure that the object has a Rigidbody component.");
            return;
        }

        // Apply rotated forces from each thruster
        for (int i = 0; i < thrusterLocations.Length; i++)
        {
            // Multiply each thruster's magnitude by the GroupedThrustersMagnitude factor
            float scaledMagnitude = thrusterMagnitudes[i] * GroupedMagnitude;

            // Transform the thruster direction from local space to world space
            Vector3 worldSpaceThrusterDirection = transform.TransformDirection(thrusterDirections[i]);

            // Calculate the current thruster rotation based on the spacecraft's rotation
            Quaternion currentThrusterRotation = transform.rotation * previousThrusterRotations[i];

            // Apply the rotated force at the thruster location as an impulse
            Rb.AddForceAtPosition(currentThrusterRotation * worldSpaceThrusterDirection * scaledMagnitude, 
            thrusterLocations[i].transform.position, ForceMode.Force);

            // Debug visualization of thruster direction
            if (debug)
            {
                Debug.DrawRay(thrusterLocations[i].transform.position, currentThrusterRotation * worldSpaceThrusterDirection * -scaledMagnitude * 5, Color.red, 0.2f);
            }

            // Activate or deactivate thruster effects based on force
            if (effectsEnabled && thrusterMagnitudes[i] > 0.1f * maxForce)
            {
                if (!thrusterEffects[i].isPlaying)
                {
                    thrusterEffects[i].Play();
                }
            }
            else
            {
                if (thrusterEffects[i].isPlaying)
                {
                    thrusterEffects[i].Stop();
                }
            }
        }

        // Check if the pulse duration has elapsed
        if (Time.time - pulseStartTime > pulseDuration)
        {
            // Stop applying the force
            for (int i = 0; i < thrusterLocations.Length; i++)
            {
                thrusterMagnitudes[i] = 0f;
            }
        }
    }

    void Update()
    {
        // Update the previous thruster rotations at the end of each frame
        for (int i = 0; i < thrusterLocations.Length; i++)
        {
            previousThrusterRotations[i] = Quaternion.Euler(rotationAngles[i]);
        }
    }

    private IEnumerator SetRandomThrusterMagnitudes()
    {
        while (true)
        {
            // Set random magnitudes for each thruster
            for (int i = 0; i < thrusterMagnitudes.Length; i++)
            {
                thrusterMagnitudes[i] = possibleMagnitudes[UnityEngine.Random.Range(0, possibleMagnitudes.Length)];
            }

            // Print a message to the console indicating that new magnitudes have been set
            Debug.Log("New thruster magnitudes set: " + string.Join(", ", thrusterMagnitudes));

            // Update the pulse start time
            pulseStartTime = Time.time;

            // Wait for the next pulse duration
            yield return new WaitForSeconds(pulseDuration);
        }
    }
}
