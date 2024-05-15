using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarsScaledGravity : MonoBehaviour
{
    public Transform GravityTarget;
    public float ScaledGravitationalConstant = 6.674f * Mathf.Pow(10, -11) / Mathf.Pow(1000, 2); // Adjusted for scale
    public float MassOfMars = 6.4171f * Mathf.Pow(10, 23) / Mathf.Pow(1000, 3); // Mars' mass scaled down
    public float RadiusOfMars = 3389.5f; // Mars' radius in Unity units (scaled)
    public Vector3 TorqueDirection = Vector3.up; // Default rotational axis
    public float TorqueStrength = 10f; // Adjust this value as needed

    private Rigidbody _rigidbody;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        ProcessGravity();
        ApplyTorque();
    }

    void ProcessGravity()
    {
        Vector3 direction = GravityTarget.position - transform.position;
        float distance = direction.magnitude;
        float gravityIntensity = ScaledGravitationalConstant * MassOfMars / Mathf.Pow(distance, 2);
        
        // Apply the gravity as an acceleration
        Vector3 gravityForce = direction.normalized * gravityIntensity * _rigidbody.mass;
        _rigidbody.AddForce(gravityForce, ForceMode.Acceleration);

        Debug.DrawRay(transform.position, direction.normalized * 10, Color.red);
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
