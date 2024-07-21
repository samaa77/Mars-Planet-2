using UnityEngine;

public class ShowHideComponent : MonoBehaviour
{
    public GameObject partToToggle;

    public float showHeight = -270;

    void Update()
    {
        float currentHeight = transform.position.y;

        if (currentHeight <= showHeight)
        {
            partToToggle.SetActive(true);
        }
        else
        {
            partToToggle.SetActive(false);
        }
    }
}