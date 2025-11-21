using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseCanvas;      // Entire pause UI canvas
    public GameObject tileBackground;
    public GameObject pausePanel;
    public GameObject controlsPanel;

    public CountdownTimer timer;

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        pauseCanvas.SetActive(true);
        tileBackground.SetActive(true);

        pausePanel.SetActive(true);
        controlsPanel.SetActive(false);

        timer.PauseTimer();
    }

    
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        pauseCanvas.SetActive(false);
        tileBackground.SetActive(false);

        pausePanel.SetActive(false);
        controlsPanel.SetActive(false);

        timer.ResumeTimer();
    }

  
    public void OpenControls()
    {
        pausePanel.SetActive(false);
        controlsPanel.SetActive(true);
    }

    public void BackToPauseMenu()
    {
        controlsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartMenuScene");
    }
}
