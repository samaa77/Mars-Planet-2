using UnityEngine;

public class FloatingOrigin : MonoBehaviour
{
    public float threshold = 500f; // Distance threshold to reset the origin

    void LateUpdate()
    {
        Vector3 position = transform.position;

        // Check if the position exceeds the threshold
        if (position.magnitude > threshold)
        {
            // Offset to shift all objects back towards the origin
            Vector3 offset = position;

            // Iterate through all root objects in the scene
            foreach (GameObject obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (obj != gameObject) // Avoid resetting the main spacecraft itself
                {
                    obj.transform.position -= offset;
                }
            }

            // Reset the main object's position
            transform.position = Vector3.zero;

            // If the camera is a child of the spacecraft, you might need to reset its local position
            Camera.main.transform.localPosition = Vector3.zero;
        }
    }
}
