using UnityEngine;

public class OrbitalMechanics2 : MonoBehaviour
{
    // Public variables for easy editing in Unity Editor
    public Transform mars;
    public float actualMarsMass = 6.4171e23f; // Actual mass of Mars in kg
    public float actualLanderMass = 3152.5f; // Actual mass of the lander in kg
    public Vector3 actualInitialVelocity = new Vector3(2808.78515445f, 408.75889788f, 3678.46907861f); // Actual initial velocity in m/s
    public Vector3 currentVelocity;
    public float TorqueStrength = 15f; // Adjust this value as needed 

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

        // Calculate initial force and velocity
        Vector3 distance = transform.position - mars.position;
        float forceMagnitude = scaledG * (scaledMarsMass * scaledLanderMass) / distance.sqrMagnitude;
        Vector3 force = forceMagnitude * distance.normalized;
        currentVelocity = _rigidbody.velocity - force / scaledLanderMass * Time.fixedDeltaTime;
    }

    void FixedUpdate()
    {
        ApplyGravity();
        ApplyTorque();
    }

    void ApplyGravity()
    {
        Vector3 distance = transform.position - mars.position;
        float forceMagnitude = scaledG * (scaledMarsMass * scaledLanderMass) / distance.sqrMagnitude;
        Vector3 force = forceMagnitude * distance.normalized;

        // Apply the gravity as an acceleration
        _rigidbody.AddForce(force, ForceMode.Acceleration);

        // Update velocity based on force
        currentVelocity -= force / scaledLanderMass * Time.fixedDeltaTime;

        // Update position based on current velocity
        transform.position += currentVelocity * Time.fixedDeltaTime;
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
        scaledMarsMass = actualMarsMass / scaleFactor;
        scaledLanderMass = actualLanderMass / scaleFactor;

        // Scale the gravitational constant by the cube of the scale factor
        scaledG = 6.67430e-11f / Mathf.Pow(scaleFactor, 3);

        // Scale the initial velocity by the scale factor (1:1000)
        scaledInitialVelocity = actualInitialVelocity / scaleFactor;
    }
}
