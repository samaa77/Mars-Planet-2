using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    public TMP_Text fpsText;  // Reference to the TextMeshProUGUI component for displaying FPS
    private float deltaTime = 0.0f;

    void Update()
    {
        // Calculate the frame time
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        // Calculate FPS
        float fps = 1.0f / deltaTime;

        // Update the TextMeshProUGUI text with the FPS value
        if (fpsText != null)
        {
            fpsText.text = string.Format("{0:0.} FPS", fps);
        }
    }
}
