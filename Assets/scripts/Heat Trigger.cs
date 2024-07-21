using UnityEngine;

public class HeatTrigger : MonoBehaviour
{
    public GameObject lander;               // Reference to the lander GameObject
    public GameObject mars;                 // Reference to the Mars GameObject
    public float triggerDistance = 10f;     // Distance at which to trigger the particle system
    public float stopDistance = 20f;        // Distance at which to stop the particle system

    public GameObject particleSystemObject; // Reference to the Particle System GameObject
    private ParticleSystem particles;       // Reference to the Particle System component
    private bool particlesStarted;          // Flag to track if particles have started

    private FloatingOrigin floatingOrigin;  // Reference to the Floating Origin script

    [Header("Debugging")]
    [SerializeField] private float currentDistance; // Variable to store the current distance (for display in Inspector)

    void Start()
    {
        particles = particleSystemObject.GetComponent<ParticleSystem>();
        particles.Stop(); // Ensure particles start off
        particlesStarted = false;

        floatingOrigin = FindObjectOfType<FloatingOrigin>(); // Get the Floating Origin script

        // Initialize currentDistance
        currentDistance = CalculateDistance();
    }

    void Update()
    {
        // Calculate the distance between lander and Mars
        currentDistance = CalculateDistance();
        //Debug.Log("Current Distance: " + currentDistance);

        // Activate particle system when within trigger distance
        if (currentDistance >= stopDistance && currentDistance <= triggerDistance)
        {
            if (!particlesStarted)
            {
                particles.Play();
                particlesStarted = true;
                Debug.Log("Particle System started at distance: " + currentDistance);
            }
        }
        // Stop particle system when it exceeds the stop distance
        else 
        {
            if (particlesStarted)
            {
                particles.Stop();
                particlesStarted = false;
                Debug.Log("Particle System stopped at distance: " + currentDistance);
            }
        }

    }

    float CalculateDistance()
    {
        // Get the global positions of the lander and Mars
        Vector3 landerGlobalPosition = floatingOrigin.GetGlobalPosition(lander.transform.position);
        Vector3 marsGlobalPosition = floatingOrigin.GetGlobalPosition(mars.transform.position);

        // Calculate the distance between lander and Mars
        float distance = Vector3.Distance(landerGlobalPosition, marsGlobalPosition);
        //Debug.Log("Calculated Distance: " + distance);
        return distance;
    }
}
