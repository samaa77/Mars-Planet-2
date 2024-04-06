using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;

public class ThrustersWithRotation : MonoBehaviour
{
    // Thruster control variables
    Thread thrusterThread;
    public string thrusterConnectionIP = "127.0.0.1";
    public int thrusterConnectionPort = 25001;
    TcpListener thrusterListener;
    TcpClient thrusterClient;

    public Vector3[] thrusterDirections = new Vector3[4]; // Array to store the thruster directions
    public float[] thrusterMagnitudes = new float[4]; // Array to store the thruster magnitudes
    public Vector3[] rotationAngles; // Array to store the Euler angles for each thruster

    public GameObject[] thrusterLocations; // Array to store the thruster locations

    private Rigidbody Rb;
    private float[] previousThrusterMagnitudes;
    private Vector3[] previousThrusterEulerAngles;
    private bool hasFixedUpdateBeenCalledThisFrame;

    void Start()
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
        if (thrusterMagnitudes.Length != thrusterLocations.Length)
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
        previousThrusterMagnitudes = new float[thrusterMagnitudes.Length];
        previousThrusterEulerAngles = new Vector3[rotationAngles.Length];
        hasFixedUpdateBeenCalledThisFrame = false;

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
            if (thrusterMagnitudes[i] != previousThrusterMagnitudes[i])
            {
                Debug.Log("Thruster " + i + " magnitude changed to " + thrusterMagnitudes[i]);
                previousThrusterMagnitudes[i] = thrusterMagnitudes[i];
            }

            // Create rotation quaternion from Euler angles
            Quaternion rotation = Quaternion.Euler(rotationAngles[i]);

            // Transform the thruster direction from local space to world space
            Vector3 worldSpaceThrusterDirection = transform.TransformDirection(thrusterDirections[i]);

            // Rotate the force vector by the thruster rotation
            Vector3 rotatedForce = rotation * (worldSpaceThrusterDirection * thrusterMagnitudes[i]);

            // Apply the rotated force at the thruster location
            Rb.AddForceAtPosition(rotatedForce, thrusterLocations[i].transform.position);

            // Draw a ray to visualize the thruster direction
            Debug.DrawRay(thrusterLocations[i].transform.position, -rotatedForce, Color.red, 0.2f);
        }

        // Check if the rotation angles have changed for any of the thrusters
        for (int i = 0; i < rotationAngles.Length; i++)
        {
            if (rotationAngles[i] != previousThrusterEulerAngles[i])
            {
                // Print the rotation angles to the console
                Debug.Log("Rotation angles for thruster " + i + " changed to: " + rotationAngles[i]);

                // Update the previous rotation angles
                previousThrusterEulerAngles[i] = rotationAngles[i];
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

            // Read the data from the client
            NetworkStream stream = thrusterClient.GetStream();
            byte[] data = new byte[1024];
            int bytesRead = stream.Read(data, 0, data.Length);

            // Parse the data into the thrusterMagnitudes array
            string dataString = Encoding.UTF8.GetString(data, 0, bytesRead);
            string[] thrusterData = dataString.Split(';');
            for (int i = 0; i < thrusterData.Length; i++)
            {
                if (!float.TryParse(thrusterData[i], out thrusterMagnitudes[i]))
                {
                    Debug.LogError("Invalid data received from client: " + thrusterData[i]);
                    continue;
                }
            }


            // Close the client connection
            thrusterClient.Close();
        }
    }

}
