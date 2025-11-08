using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Canvas References")]
    public GameObject mapMenuCanvas;
    public GameObject optionsMenuCanvas;
    public GameObject mainMenuCanvas;
    public GameObject creditMenuCanvas;

    public void PlayGame()
    {
        SceneManager.LoadScene("Milestone1Scene");
    }

    public void OpenOptions()
    {
        optionsMenuCanvas.SetActive(true);
        mainMenuCanvas.SetActive(false);
    }

    public void OpenCredits()
    {
        creditMenuCanvas.SetActive(true);
        mainMenuCanvas.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
