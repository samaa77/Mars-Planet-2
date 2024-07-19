using UnityEngine;

public class FloatingOrigin : MonoBehaviour
{
    public float threshold = 1000f; // Distance threshold to reset the origin
    private Vector3 accumulatedOffset = Vector3.zero;

    void LateUpdate()
    {
        Vector3 position = transform.position;

        if (position.magnitude > threshold)
        {
            Vector3 offset = position;
            accumulatedOffset += offset;

            foreach (GameObject obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (obj != gameObject)
                {
                    obj.transform.position -= offset;
                }
            }

            transform.position = Vector3.zero;

            //Debug.Log("Accumulated Offset: " + accumulatedOffset);
        }
    }

    public Vector3 GetGlobalPosition(Vector3 localPosition)
    {
        return localPosition + accumulatedOffset;
    }

    public Vector3 GetLocalPosition(Vector3 globalPosition)
    {
        return globalPosition - accumulatedOffset;
    }

    public Vector3 GetAccumulatedOffset()
    {
        return accumulatedOffset;
    }
}
