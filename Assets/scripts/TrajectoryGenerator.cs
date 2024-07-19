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
    public Vector3 positionRelativeToMars;  // Position of the lander relative to Mars center

    private float scaleFactor = 0.01f;  // Scale factor: 1 unit in simulation = scaleFactor meters in real life

    private List<TrajectoryPoint> trajectoryPoints;
    private float startTime;
    private int currentSegment;

    private float marsRadius = 33895f;  // Mars's radius in meters

    void Start()
    {
        Debug.Log("Start method called");
        trajectoryPoints = LoadTrajectoryPointsFromCSV(filePath);
        trajectoryPoints = FilterTrajectoryPoints(trajectoryPoints); // Filter points to ensure altitude decreases

        if (trajectoryPoints != null && trajectoryPoints.Count > 0)
        {
            startTime = Time.time;
            currentSegment = 0;

            // Set initial position based on the first trajectory point
            transform.position = mars.position + trajectoryPoints[0].Position * scaleFactor;

            // Debug: Print initial Lander position
            Debug.Log($"Initial Transform Position: {transform.position}");

            // Debug: Print loaded trajectory points
            foreach (var point in trajectoryPoints)
            {
                Debug.Log($"TimeToLanding: {point.TimeToLanding}, Position: {point.Position}");
            }
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
            float elapsedTime = Time.time - startTime;
            float currentTimeToLanding = trajectoryPoints[0].TimeToLanding - elapsedTime;

            if (currentSegment < trajectoryPoints.Count - 1 && currentTimeToLanding <= trajectoryPoints[currentSegment + 1].TimeToLanding)
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
                altitude = Vector3.Distance(transform.position, mars.position) - marsRadius;

                Debug.Log($"Interpolated Position: {interpolatedPosition}");
                Debug.Log($"Transform Position: {transform.position}");
                Debug.Log($"Position Relative to Mars: {positionRelativeToMars}");
                Debug.Log($"Time to Landing: {timeToLanding}");
                Debug.Log($"Altitude: {altitude}");
            }
            else if (currentTimeToLanding <= 0)
            {
                // Final position at landing
                transform.position = mars.position + trajectoryPoints[trajectoryPoints.Count - 1].Position * scaleFactor;

                // Update position relative to Mars
                positionRelativeToMars = trajectoryPoints[trajectoryPoints.Count - 1].Position * scaleFactor;

                // Calculate time to landing and altitude
                timeToLanding = 0;
                altitude = Vector3.Distance(transform.position, mars.position) - marsRadius;

                Debug.Log($"Final Position: {trajectoryPoints[trajectoryPoints.Count - 1].Position * scaleFactor}");
                Debug.Log($"Transform Position: {transform.position}");
                Debug.Log($"Position Relative to Mars: {positionRelativeToMars}");
                Debug.Log($"Time to Landing: {timeToLanding}");
                Debug.Log($"Altitude: {altitude}");
            }

            // Ensure time to landing is non-negative
            timeToLanding = Mathf.Max(timeToLanding, 0);

            // Ensure altitude is non-negative
            altitude = Mathf.Max(altitude, 0);
        }
    }

    List<TrajectoryPoint> LoadTrajectoryPointsFromCSV(string filePath)
    {
        var points = new List<TrajectoryPoint>();
        try
        {
            using (var reader = new StreamReader(filePath))
            {
                reader.ReadLine(); // Skip header line
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

    List<TrajectoryPoint> FilterTrajectoryPoints(List<TrajectoryPoint> points)
    {
        var filteredPoints = new List<TrajectoryPoint>();
        if (points == null || points.Count == 0)
            return filteredPoints;

        filteredPoints.Add(points[0]);
        for (int i = 1; i < points.Count; i++)
        {
            if (points[i].Position.y <= filteredPoints[filteredPoints.Count - 1].Position.y)
            {
                filteredPoints.Add(points[i]);
            }
        }

        return filteredPoints;
    }
}
