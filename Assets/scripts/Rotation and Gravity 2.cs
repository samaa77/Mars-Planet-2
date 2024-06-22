using UnityEngine;

public class OrbitalMechanics2 : MonoBehaviour
{
    // Public variables for easy editing in Unity Editor
    public Transform mars;
    public float realMarsMass = 6.4171e23f; // Actual mass of Mars in kg
    public float realLanderMass = 3152.5f; // Actual mass of the lander in kg
    public Vector3 realInitialVelocity = new Vector3(2808.78515445f, 408.75889788f, 3678.46907861f); // Actual initial velocity in m/s
    private float TorqueStrength = 15f; // Adjust this value as needed 
    public float scalingFactor = 1000f; // Adjust this value as needed

    public Vector3 currentVelocity; // For monitoring velocity changes

    // Private variables for internal use
    private float scaledMarsMass; // Scaled mass of Mars in Unity units
    private float scaledLanderMass; // Scaled mass of the lander in Unity units
    private Vector3 scaledInitialVelocity; // Scaled initial velocity in Unity units per second
    private Rigidbody _rigidbody;
    private float scaledG; // Scaled universal gravitational constant

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        ScalePhysicsQuantities();

        // Apply the scaled initial velocity
        _rigidbody.velocity = scaledInitialVelocity;

        // Initial velocity setup
        currentVelocity = _rigidbody.velocity;
    }

    void FixedUpdate()
    {
        ApplyGravity();
        ApplyTorque();

        // Update current velocity for monitoring
        currentVelocity = _rigidbody.velocity;
    }

    void ApplyGravity()
    {
        Vector3 distance = mars.position - transform.position;
        float distanceMagnitude = distance.magnitude;
        float forceMagnitude = scaledG * (scaledMarsMass * scaledLanderMass) / Mathf.Pow(distanceMagnitude, 2);
        Vector3 force = forceMagnitude * distance.normalized;

        // Apply the gravity as an acceleration
        _rigidbody.AddForce(force, ForceMode.Acceleration);
    }

    void ApplyTorque()
    {
        // Calculate the torque based on the object's position relative to the center of mass
        Vector3 leverArm = transform.position - _rigidbody.worldCenterOfMass;
        Vector3 force = -leverArm.normalized * TorqueStrength;
        Vector3 torque = Vector3.Cross(leverArm, force);

        // Apply the torque to the Rigidbody
        _rigidbody.AddTorque(torque, ForceMode.Force);
    }

    // Function to scale physical quantities for the simulation
    private void ScalePhysicsQuantities()
    {
        // Scale factor for the simulation
        const float scaleFactor = 1000f;

        // Scale the masses by the scale factor (1:1000)
        scaledMarsMass = realMarsMass / scaleFactor;
        scaledLanderMass = realLanderMass / scaleFactor;

        // Scale the gravitational constant by the cube of the scale factor
        scaledG = 6.67430e-11f / Mathf.Pow(scaleFactor, 2);

        // Scale the initial velocity by the scale factor (1:1000)
        scaledInitialVelocity = realInitialVelocity / scaleFactor;
    }
}
