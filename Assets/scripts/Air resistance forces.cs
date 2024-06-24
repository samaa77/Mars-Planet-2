using UnityEngine;

public class ScaledAirResistence : MonoBehaviour
{
    [Header("Aerodynamic Data")]
    [SerializeField] private float[] dragCoefficients = { 0.80859406f, 0.415849512f, 0.407191385f, 0.01619733f, 0.002796626f, -0.10439445f, -0.12627868f, -0.424713034f, 0.06799861f };
    [SerializeField] private float[] liftCoefficients = { 0.737856941f, 0.61829372f, 0.407191385f, 0.002516561f, 0.027966258f, 0.02270282f, -0.36785271f, -0.603465643f, -0.6172964f };
    [SerializeField] private float[] aoa = { 45f, 30f, 15f, -5f, -5f, -5f, -15f, -30f, -45f };

    [Header("Simulation Variables")]
    [SerializeField] private float airDensity;
    [SerializeField] private float angleOfAttack;
    [SerializeField] private float dragCoefficient;
    [SerializeField] private float liftCoefficient;
    [SerializeField] private float dragForce;
    [SerializeField] private float liftForce;
    [SerializeField] private float minRotationSpeed = 0.1f; // Minimum rotation speed
    [SerializeField] private float maxRotationSpeed = 5f;   // Maximum rotation speed
    [SerializeField] private float altitude; // Altitude of the Lander

    [Header("Ray Casting")]
    [SerializeField] private float referenceArea;
    [SerializeField] private float rayDensity = 20f; // Number of rays per unit
    [SerializeField] private bool debug = false; // Are we in debug mode?

    [Header("Scaling")]
    [SerializeField] private float ScaleFactor = 0.01f; // Scale factor
    public Transform mars; // Reference to the Mars object

    private Rigidbody landerRigidbody;
    private const float MarsRadius = 3389500f ; // Radius of Mars in meters

    void Start()
    {
        landerRigidbody = GetComponent<Rigidbody>();
        // referenceArea = CalculateReferenceArea();
    }

    void FixedUpdate()
    {
        // Calculate the projected position of the object based on its current velocity and the time step
        Vector3 projectedPosition = transform.position + landerRigidbody.velocity * Time.fixedDeltaTime;

        referenceArea = CalculateReferenceAreaWithRaycasting(projectedPosition);

        // Update the altitude each frame
        altitude = CalculateAltitude();

        // Apply forces based on the newly calculated reference area
        ApplyForces();
    }

    void ApplyForces()
    {
        Vector3 velocityVector = landerRigidbody.velocity;
        float velocityMagnitude = velocityVector.magnitude;

        // Calculate dynamic air density based on altitude
        airDensity = CalculateMarsAtmosphericDensity(altitude);

        // Calculate angle of attack and aerodynamic coefficients
        angleOfAttack = CalculateAngleOfAttack(velocityVector);
        dragCoefficient = InterpolateCoefficients(aoa, dragCoefficients, angleOfAttack);
        liftCoefficient = InterpolateCoefficients(aoa, liftCoefficients, angleOfAttack);

        // Calculate dynamic drag force
        dragForce = 0.5f * airDensity * velocityMagnitude * velocityMagnitude * dragCoefficient * referenceArea;
        // Calculate dynamic lift force
        liftForce = 0.5f * airDensity * velocityMagnitude * velocityMagnitude * liftCoefficient * referenceArea;

        // Apply dynamic drag force
        Vector3 dragDirection = -velocityVector.normalized;
        landerRigidbody.AddForce(dragDirection * dragForce, ForceMode.Force);

        // Apply dynamic lift force
        Vector3 liftDirection = Vector3.Cross(velocityVector, transform.right).normalized;
        landerRigidbody.AddForce(liftDirection * liftForce, ForceMode.Force);

        // Calculate dynamic rotation speed based on environmental factors and Rigidbody's angular drag
        float dynamicRotationSpeed = CalculateDynamicRotationSpeed(airDensity, velocityMagnitude, dragCoefficient, landerRigidbody.angularDrag);

        // Apply torque for rotation to align with the velocity vector
        Vector3 torqueDirection = Vector3.Cross(transform.forward, velocityVector).normalized;
        landerRigidbody.AddTorque(torqueDirection * dynamicRotationSpeed, ForceMode.Force);

        // Apply additional torque for pitch and yaw adjustments
        Vector3 pitchTorqueDirection = Vector3.Cross(transform.right, velocityVector).normalized;
        Vector3 yawTorqueDirection = Vector3.Cross(transform.up, velocityVector).normalized;
        landerRigidbody.AddTorque(pitchTorqueDirection * dynamicRotationSpeed, ForceMode.Force);
        landerRigidbody.AddTorque(yawTorqueDirection * dynamicRotationSpeed, ForceMode.Force);

        // Debugging lines
        Debug.DrawLine(transform.position, transform.position + dragDirection * dragForce, Color.red);
    }

    float CalculateDynamicRotationSpeed(float airDensity, float velocityMagnitude, float dragCoefficient, float angularDrag)
    {
        // Example calculation for dynamic rotation speed
        // Adjust the formula based on your simulation needs
        return Mathf.Clamp(airDensity * velocityMagnitude * dragCoefficient / (1 + angularDrag), minRotationSpeed, maxRotationSpeed);
    }

    float CalculateAngleOfAttack(Vector3 velocityVector)
    {
        Vector3 forwardVector = transform.forward;
        if (velocityVector.magnitude < 0.01f) return 0f; // Handle near-zero velocities
        float angle = Vector3.Angle(forwardVector, velocityVector);

        // Additional logic to determine the sign of the angle
        float sign = Mathf.Sign(Vector3.Dot(transform.up, Vector3.Cross(forwardVector, velocityVector)));
        float signedAngle = angle * sign;
        return signedAngle;
    }


    float InterpolateCoefficients(float[] angles, float[] coefficients, float currentAngle)
    {
        // Linear interpolation for in-between values
        for (int i = 0; i < angles.Length - 1; i++)
        {
            if (currentAngle >= angles[i] && currentAngle <= angles[i + 1])
            {
                float t = (currentAngle - angles[i]) / (angles[i + 1] - angles[i]);
                return coefficients[i] * (1 - t) + coefficients[i + 1] * t;
            }
        }
        // If the angle is outside the range, use the closest boundary value
        return currentAngle <= angles[0] ? coefficients[0] : coefficients[angles.Length - 1];
    }

    // Function to calculate the altitude above Mars' surface
    float CalculateAltitude()
    {
        // Calculate the distance from the lander to the center of Mars
        float distanceToCenter = Vector3.Distance(transform.position, mars.position);
        // Subtract Mars' radius to get the altitude above the surface
        float altitude = distanceToCenter - (MarsRadius * ScaleFactor);
        return altitude;
    }
    
    float CalculateMarsAtmosphericDensity(float altitude)
    {
        // Calculation of atmospheric density on Mars
        // Constants for Mars' atmosphere
        const float T0Lower = -31f; // Base temperature at 0m altitude
        const float T0Upper = -23.4f; // Base temperature at 7000m altitude
        const float lapseRateLower = -0.000998f; // Temperature lapse rate below 7000m
        const float lapseRateUpper = -0.00222f; // Temperature lapse rate above 7000m
        const float P0 = 0.699f; // Base pressure
        const float pressureRate = -0.00009f; // Pressure rate
        const float R = 0.1921f; // Specific gas constant for Mars' atmosphere

        // Calculation of the atmospheric density based on the altitude
        float temperature, pressure;
        if (altitude <= (7000 * ScaleFactor))
        {
            temperature = T0Lower + lapseRateLower * altitude;
            pressure = P0 * Mathf.Exp(pressureRate * altitude);
        }
        else
        {
            temperature = T0Upper + lapseRateUpper * altitude;
            pressure = P0 * Mathf.Exp(pressureRate * altitude);
        }

        float density = pressure / (R * (temperature + 273.15f));
        return density;
    }


    // Function to calculate the reference area using raycasting
    float CalculateReferenceAreaWithRaycasting(Vector3 projectedPosition)
    {
        float width = 30f * ScaleFactor; // Width of the cube
        float height = 30f * ScaleFactor; // Height of the cube
        float depth = 30f * ScaleFactor; // Depth of the cube
        float totalArea = 0f; // Initialize total area
        Vector3 offset = Vector3.zero; // Offset for the cube

        // Normalize the velocity vector to ensure correct ray direction
        Vector3 landerVelocity = landerRigidbody.velocity;
        if (landerVelocity.magnitude < 0.01f)
        {
            landerVelocity = transform.forward; // Use a default forward direction if velocity is near zero
        }

        Vector3 landerVelocityNormalized = landerVelocity.normalized;

        // Ensure the velocity vector is not zero
        if (landerVelocityNormalized == Vector3.zero)
        {
            Debug.LogError("Velocity is zero. Cannot calculate projected area.");
            return 0f;
        }

        // Cast rays within the defined cube volume
        Vector3 rayDirection = -landerVelocityNormalized;
        Vector3 cubeCenter = transform.position + offset; // Apply the offset here
        Vector3 cubeSize = new Vector3(width, height, depth);

        // Calculate spacing based on ray density
        float widthSpacing = width / rayDensity;
        float heightSpacing = height / rayDensity;
        float depthSpacing = depth / rayDensity;

        // Iterate over the volume of the cube
        for (float d = -depth / 2; d < depth / 2; d += depthSpacing)
        {
            for (float w = -width / 2; w < width / 2; w += widthSpacing)
            {
                for (float h = -height / 2; h < height / 2; h += heightSpacing)
                {
                    // Cast position is now based on a fixed point relative to the object's current position
                    Vector3 castPosition = cubeCenter + new Vector3(w, h, d);
                    Ray ray = new Ray(castPosition, rayDirection);
                    RaycastHit hit;

                    // Cast the ray and calculate the area contribution if it hits
                    if (Physics.Raycast(ray, out hit, depthSpacing))
                    {
                        float areaContribution = widthSpacing * heightSpacing;
                        totalArea += areaContribution;

                        // Conditional debug visualization
                        if (debug)
                        {
                            if (hit.transform == transform)
                            {
                                Debug.DrawLine(castPosition, hit.point, Color.blue);
                            }
                            else
                            {
                                Debug.DrawLine(castPosition, castPosition + rayDirection * depthSpacing, Color.red);
                            }
                        }
                    }
                    else if (debug)
                    {
                        // Draw the ray for the size of the cube if no hit occurs
                        Debug.DrawRay(castPosition, rayDirection * depthSpacing, Color.white);
                    }
                }
            }
        }

        // The total area is the sum of the contributions from the ray hits
        return totalArea;
    }

}
