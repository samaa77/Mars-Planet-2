using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirResistance : MonoBehaviour

{
    public float dragCoefficient = 0.24f; // Drag coefficient for a sphere
    public float Frontal_Area ; // Cross-sectional area of the object (m^2)
    public float lowerAtmosphereHeight = 7000.0f; // Height at which the lower atmosphere ends and the upper atmosphere begins (m)
    public float m = 440;
    public float Diameter;
    public Vector3 velocity;
    public float speed ;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        velocity = rb.velocity;
        speed = velocity.magnitude;

        // Calculate the air density as a function of altitude
        float height = transform.position.y;
        float temperature, pressure;

        if (height <= lowerAtmosphereHeight)
        {
            // Lower atmosphere
            temperature = -31 - 0.000998f * height;
            pressure = 0.699f * Mathf.Exp(-0.00009f * height);
        }
        else
        {
            // Upper atmosphere
            temperature = -23.4f - 0.00222f * height;
            pressure = 0.699f * Mathf.Exp(-0.00009f * height);
        }

        float airDensity = pressure / (0.1921f * (temperature + 273.1f));

        // Calculate the drag force
        float g = 3.7f;
        Diameter = 4.5f;
        Frontal_Area = Mathf.PI * Mathf.Pow(Diameter / 2, 2);
        float Weight_Force = m * g;
        float dragForceMagnitude = 0.5f * dragCoefficient * airDensity * speed * speed * Frontal_Area;

        // Apply the drag force in the opposite direction of the velocity
        Vector3 dragForce = -velocity.normalized * dragForceMagnitude;

        rb.AddForce(dragForce);
    }
}

