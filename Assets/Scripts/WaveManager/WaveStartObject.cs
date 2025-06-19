using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveStartObject : MonoBehaviour
{
    public WaveManager waveManager;
    public float interactRange = 2f; // Adjust as needed
    public GameObject promptText; // Assign your WavePromptText object here

    private Transform player;
    private bool canStart = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        canStart = distance <= interactRange;

        // Show or hide the prompt
        if (promptText != null)
            promptText.SetActive(canStart);

        if (canStart && Input.GetKeyDown(KeyCode.E))
        {
            if (!waveManager.IsWaveInProgress())
            {
                waveManager.StartWaves();
            }
            else
            {
                waveManager.StopWaves();
            }
        }
    }

    // Optional: Draw interaction range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
