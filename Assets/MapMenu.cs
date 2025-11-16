using UnityEngine;
using UnityEngine.SceneManagement;

public class MapMenu : MonoBehaviour
{
    [Header("Canvas Reference")]
    public GameObject mainMenuCanvas;

    public void PlayNormal()
    {
       SceneManager.LoadScene("Milestone1Scene");
    }

    public void PlayHard()
    {
        SceneManager.LoadScene("HardScene");
    }

    public void BackToMainMenu()
    {
        mainMenuCanvas.SetActive(true);
        gameObject.SetActive(false);
    }
}
