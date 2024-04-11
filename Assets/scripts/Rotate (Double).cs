// 2024-04-11 AI-Tag 
// This was created with assistance from Muse, a Unity Artificial Intelligence product

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class RotateDouble : MonoBehaviour
{
    public Transform target;
    public double speed; // Changed to double

    private double3 position; // Added for double precision position

    // Start is called before the first frame update
    void Start()
    {
        position = new double3(transform.position.x, transform.position.y, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        double angle = speed * Time.deltaTime;
        double3 relativePos = position - new double3(target.position.x, target.position.y, target.position.z);

        // Compute the rotation
        double3 rotatedPos = new double3(
            relativePos.x * math.cos(angle) - relativePos.z * math.sin(angle), 
            relativePos.y, 
            relativePos.x * math.sin(angle) + relativePos.z * math.cos(angle)
            );
        
        // Add the target's position back
        position = rotatedPos + new double3(target.position.x, target.position.y, target.position.z);

        // Convert the double3 back to Vector3 and apply it
        transform.position = new Vector3((float)position.x, (float)position.y, (float)position.z);
    }
}
