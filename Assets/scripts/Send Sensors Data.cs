using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System.Net.Sockets;
using System.Text;
public class Sensors_Data : MonoBehaviour
{
    public string serverHost = "127.0.0.1";
    public int serverPort = 25002;

    // Counter to keep track of data sent
    private int dataSentCount = 0; 


    // Start is called before the first frame update
    public GameObject Lander;
    public GameObject Mars;
    public GameObject North_Pole;
    public GameObject Landing_Site;

    //"distance" is for distance between mars and the lander Which is measured by the altimeter sensor
    public double distance;

    public float Landing_Site_Distance;

    //"i" is for iterations which is for calculating acceleration from calculating the average of the change rate of velocity vector in each frame  
    public float i = 0;

    //"T_int" is for temperature initial which is the temperature of atmosphere in altitude of the lander 
    public double T_int;

    //"T_final" is for Temperature final which is the temperature of the lander resulted from friction between lander and air molecules Which is measured by the temperature sensor
    public double T_final;

    //"Pressure" is for the pressure of the atmosphere in altitude of the lander 
    public double Pressure;

    //"Pressure_Final" is for Pressure final which is the Pressure of the lander resulted from friction between lander and air molecules Which is measured by the pressure sensor
    public double Pressure_Final;

    //Density is for mars atmosphere density
    public double Density;
    public double Drag_Force;
    public double Drag_Coefficient;
    public double Diameter;
    public double Frontal_Area;
    public double Weight_Force;


    //Q is the amount of heat gained or lost by the lander
    public double Q;

    //S is a constant claculated from  Mach number and specific heats of air 
    public double S;

    //M = vehicle flight Mach number
    public double M;

    //Y = ratio of the specific heats of air.
    public double Y;

    //a = accommodation coefficient (taken as 1.0)
    public double a;

    // c is the specific heat
    public double c;

    //m is mass of the lander
    public double m;

    //𝛾 is the adiabatic constant.
    public double y;

    //g is gravitational acceleration on Mars
    public double g;

    //"Time1" is a timer clculate the time from the moment we play the scene   
    public double Time1;

    //Gyro is for angularVelocity magnitude
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
      
        rb = GetComponent<Rigidbody>();
        lastVelocity = rb.velocity;
        lasrEulerAngle = Lander.transform.eulerAngles;
        //Input.gyro.enabled = true;
    }

    void FixedUpdate()
    {

        //Accelometer Sensor code & Gyroscope Sensor Code claculated from change rate of velocity vector & change rate of Euler Angles
        Vector3 currentVelocity = rb.velocity;
        Vector3 curentEulerAngle = Lander.transform.eulerAngles;
        currentAcceleration = (currentVelocity - lastVelocity) / Time.fixedDeltaTime;
        currentChangeRateOfEulerAngel = (curentEulerAngle - lasrEulerAngle) / Time.fixedDeltaTime;
        lastVelocity = currentVelocity;
        lasrEulerAngle = curentEulerAngle;
        Vector3 lander = Lander.transform.position;
        Debug.DrawLine(lander, Mars.transform.position);

    }
    // Update is called once per frame
    void Update()
    {
        //Magnetometer Sensor
        Magnetometer_Vector = North_Pole.transform.position - Lander.transform.position;

        //Altimeter Sensor
        distance = Vector3.Distance(Lander.transform.position, Mars.transform.position);
        Landing_Site_Distance = Vector3.Distance(Lander.transform.position, Landing_Site.transform.position);

        //State Vector of the Lander relative to mars
        Position_Vector = Lander.transform.position - Mars.transform.position;

        //Timer Function 
        Time1 = Time.time;

        //Nasa Function to calculate the temperture , pressure and density of atmosphere when the altitude is known 
        Velocity_Vector = rb.velocity; ;
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

        //CALCULATIONS OF REENTRY-VEHICLE TEMPERATURE  Which is measured by the temperature sensor
        //From Paper from INSTITUTE FOR DEFENSE ANALYSES 1 - 111 N.Bcauregutrd Strcct. Alexandria., Virginia 223! 1772
        a = 1;
        Y = 1.29;
        M = Velocity_Vector.magnitude / 343;
        S = M * Math.Sqrt(Y / 2);
        m = 440;
        c = 900;
        T_final = (((a / 2) * Density * Math.Pow(Velocity_Vector.magnitude, 3) * (1 + (1 / (2 * Math.Sqrt(Math.PI) * S)))) / (m * c)) + T_int;

        // //CALCULATIONS OF REENTRY-VEHICLE Pressure  Which is measured by the pressure sensor
        y = 4.3;
        Pressure_Final = Math.Pow(Velocity_Vector.magnitude, 2) * Density / y;

        //Calculations of draf force on the lander
        g = 3.7;
        Drag_Coefficient = 0.24;
        Diameter = 4.5;
        Frontal_Area = Math.PI * Math.Pow(Diameter / 2, 2);
        Weight_Force = m * g;
        Drag_Force = 0.5 * Density * Drag_Coefficient * Math.Pow(Velocity_Vector.magnitude, 2) * Frontal_Area;



        // Create a TCP client
        TcpClient client = new TcpClient();

        try
        {
            // Connect to the server
            client.Connect(serverHost, serverPort);

            // Get the Scale of the object


            // Convert the Scale to a string
            string dataToSend = '\n'+"currentAcceleration [" + currentAcceleration.x + "," + currentAcceleration.y + "," + currentAcceleration.z+ "]" +'\n' + 
                "Magnetometer_Vector [" + Magnetometer_Vector.x + "," + Magnetometer_Vector.y + "," + Magnetometer_Vector.z+ "]" +'\n'+
                "currentChangeRateOfEulerAngel ["+ currentChangeRateOfEulerAngel.x + ","+ currentChangeRateOfEulerAngel.y+","+ currentChangeRateOfEulerAngel.z+ "]";

            // Send the data
            NetworkStream stream = client.GetStream();
            byte[] data = Encoding.UTF8.GetBytes(dataToSend);
            stream.Write(data, 0, data.Length);
            Debug.Log("Sent data to Python: " + dataToSend);

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

}