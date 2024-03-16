using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class marsGravity : MonoBehaviour // Use PascalCase for class names
{
    // Target object for this gravity field
    public Transform gravityTarget;

    // Strength of the gravitational pull
    public float intensity = 15000f;

    // Rotational force applied (optional)
    public float torque = 500f;

    // Constant for gravitational acceleration (Earth's gravity)
    public float gravity = 3.71f; // 3.71m/s^2

    private Rigidbody rb; // Use private for member variables

    void Start()
    {
        // Get the Rigidbody component attached to this GameObject
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Update the gravity application 	
        processGravity();
    }

    void processGravity()
    {
        // Calculate the direction vector from this object to the target
        Vector3 direction = transform.position - gravityTarget.position;

        // Normalize the direction vector to get a unit vector
        Vector3 normalizedDirection = direction.normalized;

        // Apply a force towards the target based on direction, gravity constant, and Rigidbody mass
        rb.AddForce(-normalizedDirection * gravity * rb.mass, ForceMode.Force);

        // Draw a red ray to visualize the direction of gravity force (for debugging)
        Debug.DrawRay(transform.position, normalizedDirection, Color.red);
    }
}
