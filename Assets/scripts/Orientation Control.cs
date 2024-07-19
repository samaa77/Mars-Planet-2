using UnityEngine;

public class LanderOrientationController : MonoBehaviour
{
    public float rotationSpeed = 100.0f; // Speed at which the lander rotates

    void Update()
    {
        // Initialize rotation amounts for pitch, yaw, and roll
        float pitch = 0;
        float yaw = 0;
        float roll = 0;

        // Check input and adjust the rotation amounts accordingly
        if (Input.GetKey(KeyCode.W))
        {
            pitch = rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            pitch = -rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            yaw = -rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            yaw = rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            roll = rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.E))
        {
            roll = -rotationSpeed * Time.deltaTime;
        }

        // Apply rotations to the lander object
        transform.Rotate(pitch, yaw, roll);

        // Log the angles moved
        if (pitch != 0 || yaw != 0 || roll != 0)
        {
            Debug.Log($"Pitch: {pitch}, Yaw: {yaw}, Roll: {roll}");
        }
    }
}
