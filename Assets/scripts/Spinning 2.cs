using UnityEngine;

public class PulsingSpinning : MonoBehaviour
{
    public float pulseMagnitude = 500.0f; // The magnitude of the pulse force
    public float pulseInterval = 1.0f; // The interval in seconds between each pulse
    public int totalPulses = 5; // Total number of pulses to apply
    public float spinningSpeed; // The current spinning speed of the object

    private Rigidbody rb;
    private float nextPulseTime = 0.0f;
    private int pulseCount = 0; // Counter for the number of pulses applied

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Check if it's time for the next pulse
        if (ShouldPulse())
        {
            ApplyPulse();
        }

        // Calculate and update the spinning speed
        UpdateSpinningSpeed();
    }

    bool ShouldPulse()
    {
        // Check if we haven't reached the total number of pulses
        return Time.time >= nextPulseTime && pulseCount < totalPulses;
    }

    void ApplyPulse()
    {
        // Apply the pulse force as an impulse
        rb.AddTorque(Vector3.forward * pulseMagnitude, ForceMode.Impulse);

        // Update the time for the next pulse
        nextPulseTime = Time.time + pulseInterval;

        // Increment the pulse counter
        pulseCount++;

        // Optional: Log for debugging
        Debug.Log("Pulse applied: " + pulseCount);
    }

    void UpdateSpinningSpeed()
    {
        spinningSpeed = rb.angularVelocity.magnitude;
    }
}
