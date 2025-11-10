using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CountdownTimer : MonoBehaviour
{
    public float maxTime = 120f;
    private float currentTime;
    public Slider timeSlider;
    public TMP_Text timeText;
    public bool isPaused = false;

    void Start()
    {
        currentTime = maxTime;
        if (timeSlider != null)
        {
            timeSlider.maxValue = maxTime;
            timeSlider.value = currentTime;
        }
        UpdateTimerUI();
    }

    void Update()
    {
        if (isPaused) return;
        currentTime -= Time.deltaTime;
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            SceneManager.LoadScene("YouWin");
        }
    

        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        if (timeSlider != null)
            timeSlider.value = currentTime;

        if (timeText != null)
            timeText.text = $"{Mathf.CeilToInt(currentTime)}s";
    }

    public void PauseTimer() => isPaused = true;
    public void ResumeTimer() => isPaused = false;
    public void ResetTimer()
    {
        currentTime = maxTime;
        UpdateTimerUI();
    }
}


