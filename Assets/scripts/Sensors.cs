using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System.Text;

public class Sensors : MonoBehaviour
{
    // Start is called before the first frame update

    //"Distance_Between_2_Centers_Of_Mars_And_Lander(Km)" is for distance between mars and the lander Which is measured by the altimeter sensor
    private double Distance_Between_2_Centers_Of_Mars_And_Lander_Km;

    public double Altitude_Km;

    public float Landing_Site_Distance_Km;

    //"i" is for iterations which is for calculating acceleration from calculating the average of the change rate of velocity vector in each frame  
    private float i = 0;

    //"Temperature_Of_Atmosphere(C)" is the temperature of atmosphere in altitude of the lander 
    public double Temperature_Of_Atmosphere_C;

    //"Temperature_Of_Lander(C)" is the temperature of the lander resulted from friction between lander and air molecules Which is measured by the temperature sensor
    public double Temperature_Of_Lander_C;

    //"Pressure_Of_Atmosphere(Pa)" is for the pressure of the atmosphere in altitude of the lander 
    public double Pressure_Of_Atmosphere_Pa;

    //"Pressure_On_Lander(Pa)" is for Pressure final which is the Pressure of the lander resulted from friction between lander and air molecules Which is measured by the pressure sensor
    public double Pressure_On_Lander_Pa;

    //Density(Kg/m^3) is for mars atmosphere density
    public double Density_Kg_m3;
    public double Drag_Force_N;
    public double Drag_Coefficient;
    public double Diameter_m;
    public double Frontal_Area_m2;
    public double Weight_Force_N;

    //Heat_Gained(J) is the amount of heat gained or lost by the lander
    public double Heat_Gained_J;

    //S_Constant is a constant calculated from Mach number and specific heats of air 
    public double S_Constant;

    //M(Mach_Number) = vehicle flight Mach number
    public double M_Mach_Number;

    //Ratio_Of_The_Specific_Heats = ratio of the specific heats of air.
    public double Ratio_Of_The_Specific_Heats;

    //Accommodation_Coefficient = accommodation coefficient (taken as 1.0)
    public double Accommodation_Coefficient;

    // Specific_Heat is the specific heat
    public double Specific_Heat;

    //Lander_Mass(Kg) is mass of the lander
    public double Lander_Mass_Kg;

    //Adiabatic_Constant is the adiabatic constant.
    public double Adiabatic_Constant;

    //Gravitational_Acceleration(m/s^2) is gravitational acceleration on Mars
    public double Gravitational_Acceleration_m_s2;

    //"Timer(s)" is a timer to calculate the time from the moment we play the scene   
    public double Timer_s;

    //Angular_Velocity_Magnitude is for angularVelocity magnitude
    public double Angular_Velocity_Magnitude;

    public Vector3 currentAcceleration;
    public Vector3 Magnetometer_Vector;
    public Vector3 Position_Vector;
    private Rigidbody rb;
    private Vector3 lastVelocity;
    private Vector3 Velocity_Vector;
    public Vector3 lasrEulerAngle;
    public Vector3 currentChangeRateOfEulerAngel;

    public GameObject Lander;
    public GameObject Mars;
    public GameObject North_Pole;
    public GameObject Landing_Site;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lastVelocity = rb.velocity;
        lasrEulerAngle = Lander.transform.eulerAngles;
        //Input.gyro.enabled = true;
    }

    void FixedUpdate()
    {
        //Accelometer Sensor code & Gyroscope Sensor Code calculated from change rate of velocity vector & change rate of Euler Angles
        Vector3 currentVelocity = rb.velocity / 10; // Scale down by 10
        Vector3 curentEulerAngle = Lander.transform.eulerAngles;
        currentAcceleration = (currentVelocity - lastVelocity) / Time.fixedDeltaTime / 10; // Scale down by 10
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
        Distance_Between_2_Centers_Of_Mars_And_Lander_Km = Vector3.Distance(Lander.transform.position, Mars.transform.position) / 10; // Scale down by 10
        Landing_Site_Distance_Km = Vector3.Distance(Lander.transform.position, Landing_Site.transform.position) / 10; // Scale down by 10

        // Calculate Altitude_Km
        Altitude_Km = Distance_Between_2_Centers_Of_Mars_And_Lander_Km - 3389.5;

        //State Vector of the lander relative to mars
        Position_Vector = (Lander.transform.position - Mars.transform.position) / 10; // Scale down by 10

        //Timer Function 
        Timer_s = Time.time;

        //Nasa Function to calculate the temperature, pressure, and density of atmosphere when the altitude is known 
        Velocity_Vector = rb.velocity / 10; // Scale down by 10
        if (Distance_Between_2_Centers_Of_Mars_And_Lander_Km >= 700)
        {
            Temperature_Of_Atmosphere_C = -23.4 - 0.00222 * Distance_Between_2_Centers_Of_Mars_And_Lander_Km;
            Pressure_Of_Atmosphere_Pa = 0.699 * Math.Exp(-0.00009 * Distance_Between_2_Centers_Of_Mars_And_Lander_Km);
            Density_Kg_m3 = Pressure_Of_Atmosphere_Pa / (0.1921 * (Temperature_Of_Atmosphere_C + 273.1));
        }
        if (Distance_Between_2_Centers_Of_Mars_And_Lander_Km < 700)
        {
            Temperature_Of_Atmosphere_C = -31 - 0.000998 * Distance_Between_2_Centers_Of_Mars_And_Lander_Km;
            Pressure_Of_Atmosphere_Pa = 0.699 * Math.Exp(-0.00009 * Distance_Between_2_Centers_Of_Mars_And_Lander_Km);
            Density_Kg_m3 = Pressure_Of_Atmosphere_Pa / (0.1921 * (Temperature_Of_Atmosphere_C + 273.1));
        }

        //CALCULATIONS OF REENTRY-VEHICLE TEMPERATURE Which is measured by the temperature sensor
        //From Paper from INSTITUTE FOR DEFENSE ANALYSES 1 - 111 N.Bcauregutrd Strcct. Alexandria., Virginia 223! 1772
        Accommodation_Coefficient = 1;
        Ratio_Of_The_Specific_Heats = 1.29;
        M_Mach_Number = Velocity_Vector.magnitude / 343;
        S_Constant = M_Mach_Number * Math.Sqrt(Ratio_Of_The_Specific_Heats / 2);
        Lander_Mass_Kg = 440;
        Specific_Heat = 900;
        Temperature_Of_Lander_C = (((Accommodation_Coefficient / 2) * Density_Kg_m3 * Math.Pow(Velocity_Vector.magnitude, 3) * (1 + (1 / (2 * Math.Sqrt(Math.PI) * S_Constant)))) / (Lander_Mass_Kg * Specific_Heat)) + Temperature_Of_Atmosphere_C;

        // //CALCULATIONS OF REENTRY-VEHICLE Pressure Which is measured by the pressure sensor
        Adiabatic_Constant = 4.3;
        Pressure_On_Lander_Pa = Math.Pow(Velocity_Vector.magnitude, 2) * Density_Kg_m3 / Adiabatic_Constant;

        //Calculations of draft force on the lander
        Gravitational_Acceleration_m_s2 = 3.7;
        Drag_Coefficient = 0.24;
        Diameter_m = 4.5;
        Frontal_Area_m2 = Math.PI * Math.Pow(Diameter_m / 2, 2);
        Weight_Force_N = Lander_Mass_Kg * Gravitational_Acceleration_m_s2;
        Drag_Force_N = 0.5 * Density_Kg_m3 * Drag_Coefficient * Math.Pow(Velocity_Vector.magnitude, 2) * Frontal_Area_m2;

    }
}
