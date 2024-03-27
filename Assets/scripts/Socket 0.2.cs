using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;

public class CSharpForGIT : MonoBehaviour
{
    Thread mThread;
    public string connectionIP = "127.0.0.1";
    public int connectionPort = 25001;
    IPAddress localAdd;
    TcpListener listener;
    TcpClient client;
    Vector3 receivedAttitude = Vector3.zero; // Variable to store the received attitude

    bool running;

    private void Update()
    {
        transform.eulerAngles = receivedAttitude; // Apply the received attitude as Euler angles
    }

    private void Start()
    {
        ThreadStart ts = new ThreadStart(GetInfo);
        mThread = new Thread(ts);
        mThread.Start();
    }

    void GetInfo()
    {
        localAdd = IPAddress.Parse(connectionIP);
        listener = new TcpListener(IPAddress.Any, connectionPort);
        listener.Start();

        client = listener.AcceptTcpClient();

        running = true;
        while (running)
        {
            SendAndReceiveData();
        }
        listener.Stop();
    }

    void SendAndReceiveData()
    {
        NetworkStream nwStream = client.GetStream();
        byte[] buffer = new byte[client.ReceiveBufferSize];

        // Receiving Data from the Host
        int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize); // Getting data in Bytes from Python
        string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead); // Converting byte data to string

        if (!string.IsNullOrEmpty(dataReceived) && dataReceived.StartsWith("set_attitude"))
        {
            // Parse the attitude values from the received data
            string[] splitData = dataReceived.Split(',');
            if (splitData.Length == 4) // Ensure there are enough parts
            {
                receivedAttitude = new Vector3(
                    float.Parse(splitData[1]), // Pitch
                    float.Parse(splitData[2]), // Yaw
                    float.Parse(splitData[3])); // Roll
                print("Received attitude data: " + receivedAttitude);
            }

            // Sending Data to Host
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes("Attitude set"); // Converting string to byte data
            nwStream.Write(myWriteBuffer, 0, myWriteBuffer.Length); // Sending the data in Bytes to Python
        }
    }

    // Helper method to convert string data to Vector3
    public static Vector3 StringToVector3(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // Split the items
        string[] sArray = sVector.Split(',');

        // Store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }
}
