using UnityEngine;

public class OrbitalMechanics : MonoBehaviour
{
    public Transform mars;
    public float marsMass = 6.4171e23f; // Mass of Mars in kg
    public float landerMass = 3152.5f; // Mass of the lander in kg
    // Scaled initial velocity in Unity units per second
    public Vector3 initialVelocity = new Vector3(2.808785f, 0.408759f, 3.678469f); 
    public Vector3 currentVelocity;
    public float TorqueStrength = 15f; // Adjust this value as needed

    private Rigidbody _rigidbody;
    private const float G = 6.67430e-11f; // Universal gravitational constant

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();

        // Apply the scaled initial velocity
        _rigidbody.velocity = initialVelocity;

        // Scale down the masses and G by the simulation scale factor (1:1000)
        float scaledG = G / Mathf.Pow(1000, 2);
        marsMass /= 1000f;
        landerMass /= 1000f;

        // Calculate initial force and velocity
        Vector3 distance = transform.position - mars.position;
        float forceMagnitude = scaledG * (marsMass * landerMass) / distance.sqrMagnitude;
        Vector3 force = forceMagnitude * distance.normalized;
        currentVelocity = _rigidbody.velocity - force / landerMass * Time.fixedDeltaTime;
    }

    void FixedUpdate()
    {
        ApplyGravity();
        ApplyTorque();
    }

    void ApplyGravity()
    {
        Vector3 distance = transform.position - mars.position;
        float scaledG = G / Mathf.Pow(1000, 2);
        float forceMagnitude = scaledG * (marsMass * landerMass) / distance.sqrMagnitude;
        Vector3 force = forceMagnitude * distance.normalized;

        // Apply the gravity as an acceleration
        _rigidbody.AddForce(force, ForceMode.Acceleration);

        // Update velocity based on force
        currentVelocity -= force / landerMass * Time.fixedDeltaTime;

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
}
