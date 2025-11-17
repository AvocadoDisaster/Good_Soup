using System.Collections;
using UnityEngine;
using TMPro;

public class EndScreenController : MonoBehaviour
{
    [Header("Final Values (Loaded From PlayerPrefs)")]
    public int finalIngredients;
   
    public int finalScore;

    [Header("UI References")]
    public TextMeshProUGUI ingredientsText;
   
    public TextMeshProUGUI scoreText;

    [Header("Stars (Assign 5 objects)")]
    public GameObject[] stars;

    [Header("Animation Settings")]
    public float countSpeed = 0.05f;

    void Start()
    {
        // Load values saved from GameManager
        finalIngredients = PlayerPrefs.GetInt("IngredientsScored", 0);
        
        finalScore = PlayerPrefs.GetInt("FinalScore", 0);

        // Reset UI to 0
        ingredientsText.text = "0";
        
        scoreText.text = "0";

        foreach (var s in stars)
            s.SetActive(false);

        StartCoroutine(PlayEndScreen());
    }

    IEnumerator PlayEndScreen()
    {
        // Count up each part in order
        yield return CountUp(ingredientsText, finalIngredients);
      
        yield return CountUp(scoreText, finalScore);

        // Stars
        int starCount = GetStarCount(finalScore);
        for (int i = 0; i < starCount; i++)
        {
            stars[i].SetActive(true);
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator CountUp(TextMeshProUGUI text, int target)
    {
        int value = 0;
        while (value < target)
        {
            value++;
            text.text = value.ToString();
            yield return new WaitForSeconds(countSpeed);
        }
    }

    int GetStarCount(int score)
    {
        if (score >= 40) return 5;
        if (score >= 30) return 4;
        if (score >= 20) return 3;
        if (score >= 10) return 2;
        return 1;
    }
}