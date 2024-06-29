using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public float delayTime = 5f; // Time in seconds before scene change

    void Start()
    {
        Invoke(nameof(ChangeScene), delayTime);
    }

    void ChangeScene()
    {
        SceneManager.LoadScene("Scene2");
    }
}