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
    public double distance;
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
    public double Acceleration;
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


    public Vector3 Position_Vector;
    Rigidbody rb;
    public Vector3 Velocity_Vector;
    public Vector3 Last_Velocity_Vector;
    public Vector3 Acceleration_Vector;
    public Vector3 Euler_Angles;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Input.gyro.enabled = true;
    }
    // Update is called once per frame
    void Update()
    {
       
        distance = Vector3.Distance(Lander.transform.position, Mars.transform.position);
        Position_Vector = Lander.transform.position - Mars.transform.position;
        Time1 = Time.time;
        Velocity_Vector = rb.velocity;
        i++;
        if (i == 10)
        {
            Acceleration_Vector = (Velocity_Vector - Last_Velocity_Vector) / (10 * Time.deltaTime);
            i = 0;
            Last_Velocity_Vector = rb.velocity;
        }
        Euler_Angles = Lander.transform.eulerAngles;

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
        Acceleration = (Weight_Force - Drag_Force) / m;

        

    }
    private void FixedUpdate()
    {
        Vector3 lander = Lander.transform.position;
        lander.x += -100f;
        lander.y += 100f;
        lander.z += -100f;
        Debug.DrawLine(lander, Mars.transform.position);
    }
}
