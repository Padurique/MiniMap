using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public Canvas menuCanvas; // Assign your MenuCanvas here in the Inspector
    private bool isPaused = false;

    public Sprite[] arrowSkins; // Assign your skin sprites in the Inspector
    public GameObject playerArrow; // Assign the playerarrow GameObject in the Inspector

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        menuCanvas.gameObject.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        menuCanvas.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetArrowSkin(int skinIndex)
    {
        if (playerArrow != null && arrowSkins != null && skinIndex >= 0 && skinIndex < arrowSkins.Length)
        {
            var sr = playerArrow.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sprite = arrowSkins[skinIndex];
        }
    }
}
