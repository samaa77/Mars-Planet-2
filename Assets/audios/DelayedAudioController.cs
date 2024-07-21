using UnityEngine;

public class DelayedAudioController : MonoBehaviour
{
    public AudioClip audioClip;
    private AudioSource audioSource;

    public float delay = 2f; // Delay in seconds before playing
    public float duration = 5f; // Duration of audio clip playback

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        Invoke("PlayDelayedAudio", delay);
    }

    void PlayDelayedAudio()
    {
        audioSource.clip = audioClip;
        audioSource.Play();
        Invoke("StopAudio", duration);
    }

    void StopAudio()
    {
        audioSource.Stop();
    }
}
