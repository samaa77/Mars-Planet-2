using UnityEngine;

public class LanderOrientationController : MonoBehaviour
{
    public float rotationSpeed = 100.0f; // Speed at which the lander rotates

    private float pitchInputDuration = 0f;
    private float yawInputDuration = 0f;
    private float rollInputDuration = 0f;

    private Vector3 previousRotation;

    void Start()
    {
        previousRotation = transform.eulerAngles;
    }

    void Update()
    {
        // Initialize rotation amounts for pitch, yaw, and roll
        float pitch = 0;
        float yaw = 0;
        float roll = 0;

        // Check input and adjust the rotation amounts accordingly
        if (Input.GetKey(KeyCode.W))
        {
            pitchInputDuration += Time.deltaTime;
            pitch = rotationSpeed * pitchInputDuration;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            pitchInputDuration += Time.deltaTime;
            pitch = -rotationSpeed * pitchInputDuration;
        }
        else
        {
            pitchInputDuration = 0;
        }

        if (Input.GetKey(KeyCode.A))
        {
            yawInputDuration += Time.deltaTime;
            yaw = -rotationSpeed * yawInputDuration;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            yawInputDuration += Time.deltaTime;
            yaw = rotationSpeed * yawInputDuration;
        }
        else
        {
            yawInputDuration = 0;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            rollInputDuration += Time.deltaTime;
            roll = rotationSpeed * rollInputDuration;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            rollInputDuration += Time.deltaTime;
            roll = -rotationSpeed * rollInputDuration;
        }
        else
        {
            rollInputDuration = 0;
        }

        // Apply rotations to the lander object
        transform.Rotate(pitch * Time.deltaTime, yaw * Time.deltaTime, roll * Time.deltaTime);

        // Log the change in rotation angles if all changes are greater than 5 degrees
        Vector3 currentRotation = transform.eulerAngles;
        Vector3 rotationChange = currentRotation - previousRotation;

        if (Mathf.Abs(rotationChange.x) > 5 && Mathf.Abs(rotationChange.y) > 5 && Mathf.Abs(rotationChange.z) > 5)
        {
            int pitchChange = Mathf.RoundToInt(rotationChange.x);
            int yawChange = Mathf.RoundToInt(rotationChange.y);
            int rollChange = Mathf.RoundToInt(rotationChange.z);

            Debug.Log($"Rotation Change -> Pitch: {pitchChange}, Yaw: {yawChange}, Roll: {rollChange}");
            previousRotation = currentRotation;
        }
    }
}
