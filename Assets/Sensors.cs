using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
public class Sensors : MonoBehaviour
{
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


    public Vector3 Magnetometer_Vector;
    public Vector3 Position_Vector;
    Rigidbody rb;
    public Vector3 Velocity_Vector;
    public Vector3 Last_Velocity_Vector;
    public Vector3 Acceleration_Vector;
    public Vector3 Euler_Angles;
    public Vector3 Angular_Velocity;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Input.gyro.enabled = true;
    }
    // Update is called once per frame
    void Update()
    {
        //Magnetometer Sensor
        Magnetometer_Vector = North_Pole.transform.position - Lander.transform.position;

        //Altimeter Sensor
        distance = Vector3.Distance(Lander.transform.position, Mars.transform.position);
        Landing_Site_Distance = Vector3.Distance(Lander.transform.position, Landing_Site.transform.position);

        //State Vector of the lander relative to mars
        Position_Vector = Lander.transform.position - Mars.transform.position;

        //Timer Function 
        Time1 = Time.time;

        //Accelometer Sensor code claculated from average change rate of velocity vector
        Velocity_Vector = rb.velocity;
        i++;
        if (i == 10)
        {
            Acceleration_Vector = (Velocity_Vector - Last_Velocity_Vector) / (10 * Time.deltaTime);
            i = 0;
            Last_Velocity_Vector = rb.velocity;
        }

        //Gyrscope Code
        Euler_Angles = Lander.transform.eulerAngles;
        Angular_Velocity = rb.angularVelocity;
        Gyro = rb.angularVelocity.magnitude;

        //Nasa Function to calculate the temperture , pressure and density of atmosphere when the altitude is known 
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
  

        

    }
    private void FixedUpdate()
    {
        Vector3 lander = Lander.transform.position;
        Debug.DrawLine(lander, Mars.transform.position);
    }
}
