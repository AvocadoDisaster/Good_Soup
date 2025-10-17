using UnityEngine;

public class CreditMenu : MonoBehaviour
{
    public  GameObject mainMenuCanvas;

    public void BackToMainMenu()
    {
        mainMenuCanvas.SetActive(true);
        gameObject.SetActive(false);
    }
}
