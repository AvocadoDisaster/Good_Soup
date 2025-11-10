using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [Header("Canvas References")]
    public GameObject mapMenuCanvas;
    public GameObject controlsMenuCanvas;
    public GameObject mainMenuCanvas;
    public GameObject creditMenuCanvas;

    public void PlayGame()
    {
        mapMenuCanvas.SetActive(true);
        mainMenuCanvas.SetActive(false);
    }

    public void OpenControls()
    {
        controlsMenuCanvas.SetActive(true);
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
