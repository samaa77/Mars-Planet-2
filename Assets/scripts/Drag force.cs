using UnityEngine;
using System.Collections;

public class ProjectedDragForce : MonoBehaviour
{
    public float airDensity = 1.225f; // kg/m^3 (Density of air)
    public float dragCoefficient = 1.0f; // Base drag coefficient
    public float referenceArea = 1.0f; // m^2 (Reference area for drag calculation)

    private Rigidbody rb;
    private Collider collider;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }

    void FixedUpdate()
    {
        // Calculate drag force based on formula:
        // F_drag = 0.5 * airDensity * velocity.magnitude^2 * dragCoefficient * projectedArea
        Vector3 dragForce = Vector3.zero;
        if (rb != null && collider != null)
        {
            Vector3 velocity = rb.velocity;
            float speedSqr = velocity.sqrMagnitude; // More performant than magnitude

            // Calculate projected area based on object orientation
            float projectedArea = CalculateProjectedArea(velocity);

            dragForce = -0.5f * airDensity * speedSqr * dragCoefficient * projectedArea * transform.forward;

            // Apply drag force to the rigidbody
            rb.AddForce(dragForce);
        }
    }

    float CalculateProjectedArea(Vector3 velocity)
    {
        // Number of rays to cast
        int numRays = 100;

        // Initialize projected area
        float projectedArea = 0.0f;

        // Cast rays in various directions
        for (int i = 0; i < numRays; i++)
        {
            // Generate random direction within a hemisphere facing the velocity vector
            Vector3 direction = Random.insideUnitSphere;
            direction = Vector3.ProjectOnPlane(direction, velocity.normalized);

            // Cast ray from object's center in the generated direction
            Ray ray = new Ray(transform.position, direction);
            RaycastHit hit;

            // Check if ray intersects with the object
            if (collider.Raycast(ray, out hit, Mathf.Infinity))
            {
                // Calculate area of triangle formed by ray origin, hit point, and velocity vector
                Vector3 triangleNormal = Vector3.Cross(hit.point - transform.position, velocity);
                float triangleArea = 0.5f * triangleNormal.magnitude;

                // Update projected area
                projectedArea += triangleArea;
            }
        }

        // Adjust projected area based on reference area
        projectedArea /= referenceArea;

        return Mathf.Clamp(projectedArea, 0.0f, 1.0f); // Ensure positive value
    }

}
