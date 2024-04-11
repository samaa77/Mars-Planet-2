using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;

public class ThrustersPulses : MonoBehaviour
{
    // Thruster control variables
    Thread thrusterThread;
    public string thrusterConnectionIP = "127.0.0.1";
    public int thrusterConnectionPort = 25001;
    TcpListener thrusterListener;
    TcpClient thrusterClient;

    public Vector3[] thrusterDirections = new Vector3[4]; // Array to store the thruster directions
    public float[] thrusterMagnitudesPrecentages = new float[4]; // Array to store the thruster magnitudes
    public Vector3[] rotationAngles; // Array to store the Euler angles for each thruster

    public GameObject[] thrusterLocations; // Array to store the thruster locations

    private Rigidbody Rb;
    private float[] previousThrusterMagnitudes;
    private Vector3[] previousThrusterEulerAngles;
    private bool hasFixedUpdateBeenCalledThisFrame;

    // Public variable to control the pulse duration
    public float pulseDuration = 0.1f; // Default pulse duration is 0.1 seconds

    private float pulseStartTime; // Time when the pulse started

    void Awake()
    {
        // Check if the thrusterLocations array is assigned
        if (thrusterLocations == null || thrusterLocations.Length == 0)
        {
            Debug.LogError("Thruster locations array not assigned or empty. Please assign the empty objects representing the thruster locations to the thrusterLocations array in the inspector.");
            return;
        }

        // Check if the thrusterDirections array has the same length as the thrusterLocations array
        if (thrusterDirections.Length != thrusterLocations.Length)
        {
            Debug.LogError("Thruster directions array and thruster locations array have different lengths. Please make sure that both arrays have the same number of elements.");
            return;
        }

        // Check if the thrusterMagnitudes array has the same length as the thrusterLocations array
        if (thrusterMagnitudesPrecentages.Length != thrusterLocations.Length)
        {
            Debug.LogError("Thruster magnitudes array and thruster locations array have different lengths. Please make sure that both arrays have the same number of elements.");
            return;
        }

        // Check if the thrusterEulerAngles array has the same length as the thrusterLocations array
        if (rotationAngles.Length != thrusterLocations.Length)
        {
            Debug.LogError("Thruster Euler angles array and thruster locations array have different lengths. Please make sure that both arrays have the same number of elements.");
            return;
        }

        Rb = GetComponent<Rigidbody>();
        previousThrusterMagnitudes = new float[thrusterMagnitudesPrecentages.Length];
        previousThrusterEulerAngles = new Vector3[rotationAngles.Length];
        hasFixedUpdateBeenCalledThisFrame = false;
    }

    void Start()
    {
        // Create a TcpListener to listen for incoming connections on the specified port
        thrusterListener = new TcpListener(IPAddress.Parse(thrusterConnectionIP), thrusterConnectionPort);
        thrusterListener.Start();

        // Start a thread to listen for incoming connections and read data from the client
        thrusterThread = new Thread(ListenForThrusters);
        thrusterThread.Start();
    }

    void FixedUpdate()
    {
        // Check if the Rigidbody component is assigned
        if (Rb == null)
        {
            Debug.LogError("Rigidbody component not found. Please make sure that the object has a Rigidbody component.");
            return;
        }

        // Check if FixedUpdate has already been called this frame
        if (hasFixedUpdateBeenCalledThisFrame)
        {
            return;
        }

        // Apply rotated forces from each thruster
        for (int i = 0; i < thrusterLocations.Length; i++)
        {
            if (thrusterMagnitudesPrecentages[i] != previousThrusterMagnitudes[i])
            {
                Debug.Log("Thruster " + i + " magnitude changed to " + thrusterMagnitudesPrecentages[i]);
                previousThrusterMagnitudes[i] = thrusterMagnitudesPrecentages[i];

                // If the thruster magnitude has changed, start the pulse timer
                pulseStartTime = Time.time;
            }

            // Convert the thruster magnitude to a percentage
            float percentage = thrusterMagnitudesPrecentages[i] / 100f;

            // Calculate the force based on the percentage and the maximum thrust force
            float force = percentage * 302.5f;

            // Create rotation quaternion from Euler angles
            Quaternion rotation = Quaternion.Euler(rotationAngles[i]);

            // Transform the thruster direction from local space to world space
            Vector3 worldSpaceThrusterDirection = transform.TransformDirection(thrusterDirections[i]);

            // Rotate the force vector by the thruster rotation
            Vector3 rotatedForce = rotation * (worldSpaceThrusterDirection * force);

            // Apply the rotated force at the thruster location as an impulse
            Rb.AddForceAtPosition(rotatedForce, thrusterLocations[i].transform.position, ForceMode.Impulse);

            // Draw a ray to visualize the thruster direction
            Debug.DrawRay(thrusterLocations[i].transform.position, -rotatedForce, Color.red, 1f);
        }


        // Check if the rotation angles have changed for any of the thrusters
        for (int i = 0; i < rotationAngles.Length; i++)
        {
            if (rotationAngles[i] != previousThrusterEulerAngles[i])
            {
                // Print the rotation angles to the console
                //Debug.Log("Rotation angles for thruster " + i + " changed to: " + rotationAngles[i]);

                // Update the previous rotation angles
                previousThrusterEulerAngles[i] = rotationAngles[i];
            }
        }

        // Check if the pulse duration has elapsed
        if (Time.time - pulseStartTime > pulseDuration)
        {
            // Stop applying the force
            for (int i = 0; i < thrusterLocations.Length; i++)
            {
                thrusterMagnitudesPrecentages[i] = 0f;
            }
        }

        // Set the flag to indicate that FixedUpdate has been called this frame
        hasFixedUpdateBeenCalledThisFrame = true;
    }

    void Update()
    {
        // Reset the flag at the beginning of each frame
        hasFixedUpdateBeenCalledThisFrame = false;
    }

    void ListenForThrusters()
    {
        while (true)
        {
            // Wait for a client to connect
            thrusterClient = thrusterListener.AcceptTcpClient();

            // Create a new thread to continuously read data from the client
            Thread readDataThread = new Thread(ReadDataFromClient);
            readDataThread.Start();
        }
    }

    void ReadDataFromClient()
    {
        while (true)
        {
            // Read the data from the client
            NetworkStream stream = thrusterClient.GetStream();
            byte[] data = new byte[1024];
            int bytesRead = stream.Read(data, 0, data.Length);

            // Parse the data into the thrusterMagnitudes array
            string dataString = Encoding.UTF8.GetString(data, 0, bytesRead);
            string[] thrusterData = dataString.Split(';');
            for (int i = 0; i < thrusterData.Length; i++)
            {
                if (!float.TryParse(thrusterData[i], out thrusterMagnitudesPrecentages[i]))
                {
                    Debug.LogError("Invalid data received from client: " + thrusterData[i]);
                    continue;
                }
            }

            // Print a message to the console indicating that data has been received
            Debug.Log("Data received from client: " + dataString);
        }
    }
}
