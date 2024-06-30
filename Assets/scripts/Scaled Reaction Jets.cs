using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;

public class ScaledThrustersPulses : MonoBehaviour
{
    // Thruster control variables
    private Thread thrusterThread;
    public string thrusterConnectionIP = "127.0.0.1";
    public int thrusterConnectionPort = 25001;
    private TcpListener thrusterListener;
    private TcpClient thrusterClient;

    private Vector3[] thrusterDirections = { new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1) }; // Thruster directions
    public float GroupedMagnitude = 1.0f; // Multiplier for all thrusters' magnitudes    
    public float[] thrusterMagnitudes = new float[4]; // Array to store the thruster magnitudes
    private const float maxForce = 302.5f; // Maximum force of each thruster in Newtons
    public const float scaleFactor = 100f; // Scale factor for 1:100 simulation

    public GameObject[] thrusterLocations; // Array to store the thruster locations
    private Vector3[] rotationAngles = { new Vector3(30, -28, 4), new Vector3(-30, 28, -4), new Vector3(30, 28, -4), new Vector3(-30, -28, 4) }; // Rotation angles for each thruster

    private Rigidbody Rb;
    private float[] previousThrusterMagnitudes;
    private Quaternion[] previousThrusterRotations;

    public float pulseDuration = 0.1f; // Pulse duration in seconds
    private float pulseStartTime;

    private bool isRunning = false;

    void Start()
    {
        // Check if the thrusterLocations array is assigned
        if (thrusterLocations == null || thrusterLocations.Length == 0)
        {
            Debug.LogError("Thruster locations array not assigned or empty. Please assign the empty objects representing the thruster locations to the thrusterLocations array in the inspector.");
            return;
        }

        Rb = GetComponent<Rigidbody>();
        previousThrusterMagnitudes = new float[thrusterMagnitudes.Length];
        previousThrusterRotations = new Quaternion[thrusterLocations.Length];

        // Initialize the previous thruster rotations based on the rotationAngles array
        for (int i = 0; i < thrusterLocations.Length; i++)
        {
            previousThrusterRotations[i] = Quaternion.Euler(rotationAngles[i]);
        }

        // Create a TcpListener to listen for incoming connections on the specified port
        thrusterListener = new TcpListener(IPAddress.Parse(thrusterConnectionIP), thrusterConnectionPort);
        thrusterListener.Start();

        // Start a thread to listen for incoming connections and read data from the client
        isRunning = true;
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

        // Apply rotated forces from each thruster
        for (int i = 0; i < thrusterLocations.Length; i++)
        {
            // Multiply each thruster's magnitude by the GroupedMagnitude factor and scale factor
            float scaledMagnitude = thrusterMagnitudes[i] * GroupedMagnitude / scaleFactor;

            // Transform the thruster direction from local space to world space
            Vector3 worldSpaceThrusterDirection = transform.TransformDirection(thrusterDirections[i]);

            // Calculate the current thruster rotation based on the spacecraft's rotation
            Quaternion currentThrusterRotation = transform.rotation * previousThrusterRotations[i];

            // Apply the rotated force at the thruster location
            Rb.AddForceAtPosition(currentThrusterRotation * worldSpaceThrusterDirection * scaledMagnitude, thrusterLocations[i].transform.position, ForceMode.Force);

            // Draw a ray to visualize the thruster direction
            Debug.DrawRay(thrusterLocations[i].transform.position, currentThrusterRotation * worldSpaceThrusterDirection * -scaledMagnitude * 5, Color.red, 0.2f);
        }

        // Check if the pulse duration has elapsed
        if (Time.time - pulseStartTime > pulseDuration)
        {
            // Stop applying the force
            for (int i = 0; i < thrusterLocations.Length; i++)
            {
                thrusterMagnitudes[i] = 0f;
            }
        }
    }

    void Update()
    {
        // Update the previous thruster rotations at the end of each frame
        for (int i = 0; i < thrusterLocations.Length; i++)
        {
            previousThrusterRotations[i] = Quaternion.Euler(rotationAngles[i]);
        }
    }

    void ListenForThrusters()
    {
        while (isRunning)
        {
            try
            {
                // Wait for a client to connect
                thrusterClient = thrusterListener.AcceptTcpClient();

                // Create a new thread to continuously read data from the client
                Thread readDataThread = new Thread(ReadDataFromClient);
                readDataThread.Start();
            }
            catch (Exception ex)
            {
                Debug.LogError("Error in ListenForThrusters: " + ex.Message);
            }
        }
    }

    void ReadDataFromClient()
    {
        while (isRunning && thrusterClient != null)
        {
            try
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
                    if (float.TryParse(thrusterData[i], out float percentage))
                    {
                        // Convert percentage to actual force
                        thrusterMagnitudes[i] = percentage / 100.0f * maxForce;
                    }
                    else
                    {
                        Debug.LogError("Invalid data received from client: " + thrusterData[i]);
                        thrusterMagnitudes[i] = 0f;
                    }
                }

                // Print a message to the console indicating that data has been received
                Debug.Log("Data received from client: " + dataString);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error in ReadDataFromClient: " + ex.Message);
            }
        }
    }

    private void OnApplicationQuit()
    {
        isRunning = false;

        if (thrusterClient != null)
        {
            thrusterClient.Close();
        }

        if (thrusterListener != null)
        {
            thrusterListener.Stop();
        }

        if (thrusterThread != null)
        {
            thrusterThread.Join();
        }
    }
}
