using TMPro;
using UnityEngine;

public class EndScore : MonoBehaviour
{
    GameManager gameManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TextMeshProUGUI score;
    void Start()
    {
        score = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        Updatescore();
    }
    private void Updatescore()
    {
        int finalscore = gameManager.GetTotalScore();
        DontDestroyOnLoad(this);
    }
}
