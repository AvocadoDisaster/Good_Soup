using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Canvas References")]
    public GameObject mapMenuCanvas;
    public GameObject optionsMenuCanvas;
    public GameObject mainMenuCanvas;

    public void PlayGame()
    {
        mapMenuCanvas.SetActive(true);
        mainMenuCanvas.SetActive(false);
    }

    public void OpenOptions()
    {
        optionsMenuCanvas.SetActive(true);
        mainMenuCanvas.SetActive(false);
    }

    public void OpenCredits()
    {
               SceneManager.LoadScene("Credits");
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
