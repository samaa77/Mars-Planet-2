using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;

public class ScaledThrustersPulses : MonoBehaviour
{
    // Network and threading variables
    private Thread thrusterThread;
    public string thrusterConnectionIP = "127.0.0.1";
    public int thrusterConnectionPort = 25001;
    private TcpListener thrusterListener;
    private TcpClient thrusterClient;

    // Thruster properties
    private Vector3[] thrusterDirections = { new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1) };
    public float GroupedMagnitude = 1.0f; // Global multiplier for all thrusters
    public float[] thrusterMagnitudes = new float[4]; // Individual thruster magnitudes
    private const float maxForce = 302.5f; // Max force per thruster in Newtons
    public const float scaleFactor = 100f; // Scale factor for simulation
    public bool debug = false; // Toggle debug mode for visualizations
    public bool effectsEnabled = true; // Toggle for thruster visual effects

    // Thruster positions and effects
    public GameObject[] thrusterLocations;
    public ParticleSystem[] thrusterEffects;
    private Vector3[] rotationAngles = { new Vector3(30, -28, 4), new Vector3(-30, 28, -4), new Vector3(30, 28, -4), new Vector3(-30, -28, 4) };

    // Rigidbody and rotation data
    private Rigidbody Rb;
    private Quaternion[] previousThrusterRotations;

    // Pulse duration control
    public float pulseDuration = 0.1f;
    private float pulseStartTime;
    private bool isRunning = false; // Control loop for network threads

    void Start()
    {
        // Validate thruster locations
        if (thrusterLocations == null || thrusterLocations.Length == 0)
        {
            Debug.LogError("Thruster locations array not assigned or empty.");
            return;
        }

        // Validate thruster effects
        if (thrusterEffects == null || thrusterEffects.Length != thrusterLocations.Length)
        {
            Debug.LogError("Thruster effects array not assigned or does not match the number of thrusters.");
            return;
        }

        // Initialize rigidbody and rotations
        Rb = GetComponent<Rigidbody>();
        previousThrusterRotations = new Quaternion[thrusterLocations.Length];
        for (int i = 0; i < thrusterLocations.Length; i++)
        {
            previousThrusterRotations[i] = Quaternion.Euler(rotationAngles[i]);
        }

        // Start the TCP listener for incoming connections
        thrusterListener = new TcpListener(IPAddress.Parse(thrusterConnectionIP), thrusterConnectionPort);
        thrusterListener.Start();

        // Start a separate thread for network communication
        isRunning = true;
        thrusterThread = new Thread(ListenForThrusters);
        thrusterThread.Start();
    }

    void FixedUpdate()
    {
        // Ensure Rigidbody component is available
        if (Rb == null)
        {
            Debug.LogError("Rigidbody component not found.");
            return;
        }

        // Apply forces based on thruster data
        for (int i = 0; i < thrusterLocations.Length; i++)
        {
            // Calculate scaled force magnitude
            float scaledMagnitude = thrusterMagnitudes[i] * GroupedMagnitude / scaleFactor;
            Vector3 worldSpaceThrusterDirection = transform.TransformDirection(thrusterDirections[i]);
            Quaternion currentThrusterRotation = transform.rotation * previousThrusterRotations[i];

            // Apply force at each thruster location
            Rb.AddForceAtPosition(currentThrusterRotation * worldSpaceThrusterDirection * scaledMagnitude, thrusterLocations[i].transform.position, ForceMode.Force);

            // Debug visualization of thruster direction
            if (debug)
            {
                Debug.DrawRay(thrusterLocations[i].transform.position, currentThrusterRotation * worldSpaceThrusterDirection * -scaledMagnitude * 5, Color.red, 0.2f);
            }

            // Activate or deactivate thruster effects based on force
            if (effectsEnabled && thrusterMagnitudes[i] > 0.1f * maxForce)
            {
                if (!thrusterEffects[i].isPlaying)
                {
                    thrusterEffects[i].Play();
                }
            }
            else
            {
                if (thrusterEffects[i].isPlaying)
                {
                    thrusterEffects[i].Stop();
                }
            }
        }

        // Check pulse duration and reset magnitudes if time has elapsed
        if (Time.time - pulseStartTime > pulseDuration)
        {
            for (int i = 0; i < thrusterLocations.Length; i++)
            {
                thrusterMagnitudes[i] = 0f;
            }
        }
    }

    void Update()
    {
        // Update thruster rotations at the end of each frame
        for (int i = 0; i < thrusterLocations.Length; i++)
        {
            previousThrusterRotations[i] = Quaternion.Euler(rotationAngles[i]);
        }
    }

    void ListenForThrusters()
    {
        // Listen for incoming connections
        while (isRunning)
        {
            try
            {
                thrusterClient = thrusterListener.AcceptTcpClient();
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
        // Read data from the client and update thruster magnitudes
        while (isRunning && thrusterClient != null)
        {
            try
            {
                NetworkStream stream = thrusterClient.GetStream();
                byte[] data = new byte[1024];
                int bytesRead = stream.Read(data, 0, data.Length);

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
        // Cleanup on application quit
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
