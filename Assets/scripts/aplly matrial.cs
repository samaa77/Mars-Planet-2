using UnityEngine;

public class ApplyMaterial : MonoBehaviour
{
    public Material parachuteMaterial;

    void Start()
    {
        MeshRenderer[] parts = GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer part in parts)
        {
            part.material = parachuteMaterial;
        }
    }
}