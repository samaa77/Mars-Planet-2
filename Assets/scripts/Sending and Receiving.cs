using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;

public class CombinedSensorsAndThrusterControl : MonoBehaviour
{
    // Thruster control variables
    Thread thrusterThread;
    public string thrusterConnectionIP = "127.0.0.1";
    public int thrusterConnectionPort = 25001;
    TcpListener thrusterListener;
    TcpClient thrusterClient;

    bool thrusterRunning;

    // Sensors data variables
    public string sensorsServerHost = "127.0.0.1";
    public int sensorsServerPort = 25002;
    private int dataSentCount = 0;

    public GameObject Lander;
    public GameObject Mars;
    public GameObject North_Pole;
    public GameObject Landing_Site;

    public double distance;
    public float Landing_Site_Distance;
    public float i = 0;
    public double T_int;
    public double T_final;
    public double Pressure;
    public double Pressure_Final;
    public double Density;
    public double Drag_Force;
    public double Drag_Coefficient;
    public double Diameter;
    public double Frontal_Area;
    public double Weight_Force;
    public double Q;
    public double S;
    public double M;
    public double Y;
    public double a;
    public double c;
    public double m;
    public double y;
    public double g;
    public double Time1;
    public double Gyro;
    public Vector3 currentAcceleration;
    public Vector3 Magnetometer_Vector;
    public Vector3 Position_Vector;
    private Rigidbody rb;
    private Vector3 lastVelocity;
    private Vector3 Velocity_Vector;
    public Vector3 lasrEulerAngle;
    public Vector3 currentChangeRateOfEulerAngel;

    void Start()
    {
        // Initialize sensors data
        rb = GetComponent<Rigidbody>();
        lastVelocity = rb.velocity;
        lasrEulerAngle = Lander.transform.eulerAngles;

        // Initialize thruster control
        ThreadStart thrusterThreadStart = new ThreadStart(InitThrusterSocket);
        thrusterThread = new Thread(thrusterThreadStart);
        thrusterThread.Start();
    }

    void FixedUpdate()
    {
        // Sensors data calculations
        Vector3 currentVelocity = rb.velocity;
        Vector3 curentEulerAngle = Lander.transform.eulerAngles;
        currentAcceleration = (currentVelocity - lastVelocity) / Time.fixedDeltaTime;
        currentChangeRateOfEulerAngel = (curentEulerAngle - lasrEulerAngle) / Time.fixedDeltaTime;
        lastVelocity = currentVelocity;
        lasrEulerAngle = curentEulerAngle;
        Vector3 lander = Lander.transform.position;
        Debug.DrawLine(lander, Mars.transform.position);
    }

    void InitThrusterSocket()
    {
        // Initialize thruster control socket
        thrusterListener = new TcpListener(System.Net.IPAddress.Parse(thrusterConnectionIP), thrusterConnectionPort);
        thrusterListener.Start();
        thrusterClient = thrusterListener.AcceptTcpClient();
        thrusterRunning = true;

        while (thrusterRunning)
        {
            ReceiveThrusterData();
        }

        thrusterListener.Stop();
    }

    void ReceiveThrusterData()
    {
        // Receive thruster data from Python
        NetworkStream nwStream = thrusterClient.GetStream();
        byte[] buffer = new byte[thrusterClient.ReceiveBufferSize];
        int bytesRead = nwStream.Read(buffer, 0, thrusterClient.ReceiveBufferSize);
        string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        if (!string.IsNullOrEmpty(dataReceived) && dataReceived.StartsWith("fire_thrusters"))
        {
            // Parse the thruster data
            string[] thrusterData = dataReceived.Substring(15).Split(';');
            Debug.Log("Received thruster data list: [" + string.Join(", ", thrusterData) + "]");

            // Send confirmation back to Python
            byte[] response = Encoding.ASCII.GetBytes("Thruster data received");
            nwStream.Write(response, 0, response.Length);
        }
    }

    void Update()
    {
        // Sensors data calculations continued...
        Magnetometer_Vector = North_Pole.transform.position - Lander.transform.position;
        distance = Vector3.Distance(Lander.transform.position, Mars.transform.position);
        Landing_Site_Distance = Vector3.Distance(Lander.transform.position, Landing_Site.transform.position);
        Position_Vector = Lander.transform.position - Mars.transform.position;

        Velocity_Vector = rb.velocity;
        if (distance >= 7000)
        {
            T_int = -23.4 - 0.00222 * distance;
            Pressure = 0.699 * Math.Exp(-0.00009 * distance);
            Density = Pressure / (0.1921 * (T_int + 273.1));
        }
        if (distance < 7000)
        {
            T_int = -31 - 0.000998 * distance;
            Pressure = 0.699 * Math.Exp(-0.00009 * distance);
            Density = Pressure / (0.1921 * (T_int + 273.1));
        }

        a = 1;
        Y = 1.29;
        M = Velocity_Vector.magnitude / 343;
        S = M * Math.Sqrt(Y / 2);
        m = 440;
        c = 900;
        T_final = (((a / 2) * Density * Math.Pow(Velocity_Vector.magnitude, 3) * (1 + (1 / (2 * Math.Sqrt(Math.PI) * S)))) / (m * c)) + T_int;

        y = 4.3;
        Pressure_Final = Math.Pow(Velocity_Vector.magnitude, 2) * Density / y;

        g = 3.7;
        Drag_Coefficient = 0.24;
        Diameter = 4.5;
        Frontal_Area = Math.PI * Math.Pow(Diameter / 2, 2);
        Weight_Force = m * g;
        Drag_Force = 0.5 * Density * Drag_Coefficient * Math.Pow(Velocity_Vector.magnitude, 2) * Frontal_Area;

        // Send sensors data to Python
        TcpClient client = new TcpClient();

        try
        {

            // Connect to the server
            client.Connect(sensorsServerHost, sensorsServerPort);

            // Prepare data to send
            string dataToSend = '\n' + "currentAcceleration [" + currentAcceleration.x + "," + currentAcceleration.y + "," + currentAcceleration.z + "]" + '\n' +
                "Magnetometer_Vector [" + Magnetometer_Vector.x + "," + Magnetometer_Vector.y + "," + Magnetometer_Vector.z + "]" + '\n' +
                "currentChangeRateOfEulerAngel [" + currentChangeRateOfEulerAngel.x + "," + currentChangeRateOfEulerAngel.y + "," + currentChangeRateOfEulerAngel.z + "]";
            
            // Send the data
            NetworkStream stream = client.GetStream();
            byte[] data = Encoding.UTF8.GetBytes(dataToSend);
            stream.Write(data, 0, data.Length);
            Debug.Log("Sent data to Python: ");

            // Increment the counter and print the count
            dataSentCount++;
            Debug.Log("Data sent count: " + dataSentCount);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error sending data to Python: " + e.Message);
        }
        finally
        {
            // Close the client
            client.Close();
        }
    }

    private void OnApplicationQuit()
    {
        thrusterRunning = false;
    }
}