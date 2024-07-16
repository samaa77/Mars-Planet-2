using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TrajectoryPoint
{
    public float TimeToLanding { get; set; }
    public Vector3 Position { get; set; }

    public TrajectoryPoint(float timeToLanding, Vector3 position)
    {
        TimeToLanding = timeToLanding;
        Position = position;
    }
}

public class TrajectoryGenerator : MonoBehaviour
{
    public string filePath = "Assets/lander_positions.csv";  // Path to your CSV file
    public Transform mars;  // Transform of Mars center

    [Header("Debug Info")]
    public float timeToLanding;  // Time to landing in seconds
    public float altitude;  // Altitude from Mars surface to lander in meters

    [Header("Position Relative to Mars")]
    public Vector3 positionRelativeToMars;  // Position of the lander relative to Mars center

    private float scaleFactor = 100f;  // Scale factor: 1 unit in simulation = scaleFactor meters in real life

    private List<TrajectoryPoint> trajectoryPoints;
    private float startTime;
    private int currentSegment;

    private float marsRadius = 3389500f;  // Mars's radius in meters (3389.5 km * scaleFactor)

    void Start()
    {
        trajectoryPoints = LoadTrajectoryPointsFromCSV(filePath);
        if (trajectoryPoints != null && trajectoryPoints.Count > 0)
        {
            startTime = Time.time;
        }
        else
        {
            Debug.LogError("Failed to load trajectory points.");
        }
    }

    void Update()
    {
        if (trajectoryPoints != null && trajectoryPoints.Count > 0)
        {
            AnimateLander();
            UpdateLanderInfo();
        }
    }

    List<TrajectoryPoint> LoadTrajectoryPointsFromCSV(string filePath)
    {
        var points = new List<TrajectoryPoint>();
        try
        {
            using (var reader = new StreamReader(filePath))
            {
                // Skip header line
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    if (values.Length < 4) continue;

                    if (float.TryParse(values[0], out float timeToLanding) &&
                        float.TryParse(values[1], out float x) &&
                        float.TryParse(values[2], out float y) &&
                        float.TryParse(values[3], out float z))
                    {
                        var point = new TrajectoryPoint(timeToLanding, new Vector3(x, y, z));
                        points.Add(point);
                    }
                }
            }
        }
        catch (IOException e)
        {
            Debug.LogError("Error reading CSV file: " + e.Message);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error parsing CSV file: " + e.Message);
        }

        return points;
    }

    void AnimateLander()
    {
        float elapsedTime = Time.time - startTime;
        float currentTimeToLanding = trajectoryPoints[0].TimeToLanding - elapsedTime;

        if (currentSegment < trajectoryPoints.Count - 1)
        {
            while (currentSegment < trajectoryPoints.Count - 1 && trajectoryPoints[currentSegment + 1].TimeToLanding <= currentTimeToLanding)
            {
                currentSegment++;
            }

            if (currentSegment < trajectoryPoints.Count - 1)
            {
                TrajectoryPoint previousPoint = trajectoryPoints[currentSegment];
                TrajectoryPoint nextPoint = trajectoryPoints[currentSegment + 1];

                float t = (previousPoint.TimeToLanding - currentTimeToLanding) /
                          (previousPoint.TimeToLanding - nextPoint.TimeToLanding);
                Vector3 interpolatedPosition = Vector3.Lerp(previousPoint.Position, nextPoint.Position, t) * scaleFactor;
                transform.position = mars.position + interpolatedPosition;

                // Update position relative to Mars
                positionRelativeToMars = interpolatedPosition;

                // Calculate time to landing and altitude
                timeToLanding = currentTimeToLanding;
                altitude = Vector3.Distance(transform.position, mars.position) - marsRadius * scaleFactor;
            }
            else if (currentTimeToLanding <= 0)
            {
                // Final position at landing
                transform.position = mars.position + trajectoryPoints[trajectoryPoints.Count - 1].Position * scaleFactor;

                // Update position relative to Mars
                positionRelativeToMars = trajectoryPoints[trajectoryPoints.Count - 1].Position * scaleFactor;

                // Calculate time to landing and altitude
                timeToLanding = 0;
                altitude = Vector3.Distance(transform.position, mars.position) - marsRadius * scaleFactor;
            }
        }
    }

    void UpdateLanderInfo()
    {
        // Update debug info fields
        timeToLanding = Mathf.Max(timeToLanding, 0);  // Ensure time to landing is non-negative
        altitude = Mathf.Max(altitude, 0);  // Ensure altitude is non-negative
    }
}
