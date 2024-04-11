// 2024-04-11 AI-Tag 
// This was created with assistance from Muse, a Unity Artificial Intelligence product

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class MarsGravityNew : MonoBehaviour 
{
    // Target object for this gravity field
    public Transform gravityTarget;

    // Strength of the gravitational pull
    public double intensity = 15000f;

    // Rotational force applied (optional)
    public double torque = 500f;

    // Constant for gravitational acceleration (Mars gravity)
    public double gravity = 3.71f; 

    // Grid size for quantization
    public float gridSize = 0.1f;

    private Rigidbody rb; 

    private double3 position; // Added for double precision position

    void Start()
    {
        // Get the Rigidbody component attached to this GameObject
        rb = GetComponent<Rigidbody>();

        // Initialize position with the object's starting position
        position = new double3(transform.position.x, transform.position.y, transform.position.z);
    }

    void FixedUpdate()
    {
        // Update the gravity application 	
        processGravity();
    }

    void processGravity()
    {
        // Calculate the direction vector from this object to the target in double precision
        double3 direction = position - new double3(gravityTarget.position.x, gravityTarget.position.y, gravityTarget.position.z);

        // Normalize the direction vector to get a unit vector
        double3 normalizedDirection = math.normalize(direction);

        // Apply a force towards the target based on direction, gravity constant, and Rigidbody mass
        rb.AddForce(-new Vector3((float)normalizedDirection.x, (float)normalizedDirection.y, (float)normalizedDirection.z) * (float)gravity * rb.mass, ForceMode.Force);

        // Draw a red ray to visualize the direction of gravity force (for debugging)
        Debug.DrawRay(transform.position, new Vector3((float)normalizedDirection.x, (float)normalizedDirection.y, (float)normalizedDirection.z), Color.red);

        // Apply quantization to the position
        Vector3 currentPosition = transform.position;
        float x = Mathf.Round(currentPosition.x / gridSize) * gridSize;
        float y = Mathf.Round(currentPosition.y / gridSize) * gridSize;
        float z = Mathf.Round(currentPosition.z / gridSize) * gridSize;

        transform.position = new Vector3(x, y, z);

        // Update position in double precision
        position = new double3(transform.position.x, transform.position.y, transform.position.z);
    }
}
