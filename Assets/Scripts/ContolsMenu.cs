using UnityEngine;

public class ContolsMenu : MonoBehaviour
{
    public GameObject optionsMenuCanvas;
    public void BackToOptions()
    {
        optionsMenuCanvas.SetActive(true);
        gameObject.SetActive(false);
    }
}
