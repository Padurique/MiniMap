using UnityEngine;

public class PlayerAudioListener : MonoBehaviour
{
    private AudioListener audioListener;

    void Start()
    {
        audioListener = GetComponent<AudioListener>();

        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        foreach (AudioListener listener in listeners)
        {
            if (listener != audioListener)
                Destroy(listener);
        }
    }

    void Update()
    {
        audioListener.transform.rotation = transform.rotation;
    }
}