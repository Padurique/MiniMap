using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Tilemaps;

public class WaveManager : MonoBehaviour
{
    // Replace single prefab with an array
    public GameObject[] enemyPrefabs;
    public int enemiesPerWave = 5;
    public int totalWaves = 3;
    public Tilemap spawnTilemap;
    public PlayerHealth playerHealth;

    public Text waveText;
    public Text enemyCountText;

    public Canvas endGameCanvas;
    public Text endGameMessageText;
    public Text countdownText;
    public float restartDelay = 5f;

    private int currentWave = 0;
    private int enemiesAlive = 0;
    private bool isWaveInProgress = false;
    private bool gameStarted = false;

    void Start()
    {
        UpdateWaveText();
        UpdateEnemyCountText();
        // Remove or comment out: StartCoroutine(StartNextWave());
    }

    IEnumerator StartNextWave()
    {
        if (isWaveInProgress) yield break;

        yield return new WaitForSeconds(2f);
        if (currentWave < totalWaves)
        {
            currentWave++;
            isWaveInProgress = true;
            UpdateWaveText(); 
            SpawnEnemies();
        }
        else
        {
            StartCoroutine(ShowEndGameOverlay("Winner!", false));
        }
    }

    void SpawnEnemies()
    {
        enemiesAlive = enemiesPerWave;
        UpdateEnemyCountText();

        for (int i = 0; i < enemiesPerWave; i++)
        {
            Vector3 spawnPos = GetRandomTilePosition();

            // Randomly select an enemy type
            int prefabIndex = Random.Range(0, enemyPrefabs.Length);
            GameObject enemy = Instantiate(enemyPrefabs[prefabIndex], spawnPos, Quaternion.identity);

            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.waveManager = this;
            }
            else
            {
                Debug.LogError("Spawned enemy does not have an Enemy script!");
            }
        }
    }


    Vector3 GetRandomTilePosition()
    {
        BoundsInt bounds = spawnTilemap.cellBounds;
        int attempts = 0;
        while (attempts < 100)
        {
            int x = Random.Range(bounds.xMin, bounds.xMax);
            int y = Random.Range(bounds.yMin, bounds.yMax);
            Vector3Int cellPosition = new Vector3Int(x, y, 0);
            if (spawnTilemap.HasTile(cellPosition))
            {
                return spawnTilemap.GetCellCenterWorld(cellPosition);
            }
            attempts++;
        }
        throw new System.Exception("Could not find valid spawn position");
    }

    public void EnemyDefeated()
    {
        enemiesAlive--;
        UpdateEnemyCountText(); 

        if (enemiesAlive <= 0)
        {
            isWaveInProgress = false;
            StartCoroutine(StartNextWave());
        }
    }

    void UpdateWaveText()
    {
        if (waveText != null)
        {
            waveText.text = $"Wave: {currentWave}/{totalWaves}";
            Debug.Log($"Wave Updated: {waveText.text}");
        }
        else
        {
            Debug.LogError("Wave Text UI is not assigned in the Inspector!");
        }
    }

    void UpdateEnemyCountText()
    {
        if (enemyCountText != null)
        {
            enemyCountText.text = $"Enemies: {enemiesAlive}";
            Debug.Log($"Enemies Left: {enemyCountText.text}");
        }
        else
        {
            Debug.LogError("Enemy Count UI is not assigned in the Inspector!");
        }
    }

    void Update()
    {
        if (playerHealth.CurrentHealth <= 0) 
        {
            StartCoroutine(ShowEndGameOverlay("You Died", true));
        }
    }

    void EndGame(string message)
    {
        StartCoroutine(ShowEndGameOverlay(message, true));
    }

    IEnumerator ShowEndGameOverlay(string message, bool restart)
    {
        endGameCanvas.gameObject.SetActive(true);
        endGameMessageText.text = message;
        float timer = restartDelay;
        Time.timeScale = 0f; // Pause game

        while (timer > 0)
        {
            countdownText.text = restart ? $"Restarting in {Mathf.CeilToInt(timer)}..." : "";
            yield return new WaitForSecondsRealtime(1f);
            timer -= 1f;
        }

        Time.timeScale = 1f; // Resume game
        if (restart)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void StartWaves()
    {
        if (!gameStarted)
        {
            gameStarted = true;
            StartCoroutine(StartNextWave());
        }
    }

    public void StopWaves()
    {
        if (isWaveInProgress || gameStarted)
        {
            // Reset wave state
            isWaveInProgress = false;
            gameStarted = false;
            currentWave = 0;
            enemiesAlive = 0;

            // Destroy all current enemies
            foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                Destroy(enemy);
            }

            // Reset UI
            UpdateWaveText();
            UpdateEnemyCountText();
        }
    }

    public bool IsWaveInProgress()
    {
        return isWaveInProgress;
    }
}
