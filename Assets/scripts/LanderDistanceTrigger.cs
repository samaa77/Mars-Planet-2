using UnityEngine;

public class LanderDistanceTrigger : MonoBehaviour
{
    public GameObject lander;           // Reference to the lander GameObject
    public GameObject mars;             // Reference to the Mars GameObject
    public float triggerDistance = 10f; // Distance at which to trigger the particle system

    public GameObject particleSystemObject; // Reference to the Particle System GameObject
    private ParticleSystem particles;       // Reference to the Particle System component
    private bool particlesStarted;          // Flag to track if particles have started

    [Header("Debugging")]
    [SerializeField] private float currentDistance; // Variable to store the current distance (for display in Inspector)

    void Start()
    {
        particles = particleSystemObject.GetComponent<ParticleSystem>();
        particles.Stop(); // Ensure particles start off
        particlesStarted = false;

        // Initialize currentDistance
        currentDistance = CalculateDistance();
    }

    void Update()
    {
        // Calculate the distance between lander and Mars
        currentDistance = CalculateDistance();

        // Activate particle system when distance is reached
        if (currentDistance <= triggerDistance)
        {
            if (!particlesStarted)
            {
                particles.Play();
                particlesStarted = true;
                Debug.Log("Particle System started at distance: " + currentDistance);
            }
        }
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
        // Calculate the distance between lander and Mars
        float distance = Vector3.Distance(lander.transform.position, mars.transform.position);
        return distance;
    }
}