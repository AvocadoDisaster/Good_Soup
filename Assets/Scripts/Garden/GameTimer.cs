using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float totalTime = 300f; // 5 minutes default
    [SerializeField] private bool startOnAwake = true;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerFillBar; // progress bar
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color dangerColor = Color.red;
    [SerializeField] private float warningThreshold = 60f; // Yellow at 1 minute
    [SerializeField] private float dangerThreshold = 30f; // Red at 30 seconds

    [Header("Penalty Visual")]
    [SerializeField] private GameObject penaltyFlashPanel; // Red flash overlay
    [SerializeField] private float flashDuration = 0.5f;

    [Header("Events")]
    public UnityEvent OnTimerExpired;
    public UnityEvent<float> OnTimePenalty; // Passes penalty amount

    private float currentTime;
    private bool isRunning = false;
    private bool hasExpired = false;

    private void Start()
    {
        currentTime = totalTime;
        UpdateTimerDisplay();

        if (startOnAwake)
        {
            StartTimer();
        }

        if (penaltyFlashPanel != null)
        {
            penaltyFlashPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (isRunning && !hasExpired)
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                currentTime = 0;
                TimerExpired();
            }

            UpdateTimerDisplay();
        }
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void PauseTimer()
    {
        isRunning = false;
    }

    public void ResumeTimer()
    {
        if (!hasExpired)
        {
            isRunning = true;
        }
    }

    public void AddTime(float seconds)
    {
        currentTime += seconds;
        currentTime = Mathf.Min(currentTime, totalTime); // Cap at max time
        UpdateTimerDisplay();
        Debug.Log($"⏰ Added {seconds} seconds!");
    }

    public void SubtractTime(float seconds)
    {
        currentTime -= seconds;
        currentTime = Mathf.Max(currentTime, 0); // Don't go negative

        // Flash penalty visual
        if (penaltyFlashPanel != null)
        {
            ShowPenaltyFlash();
        }

        OnTimePenalty?.Invoke(seconds);
        UpdateTimerDisplay();

        Debug.Log($"⏰ PENALTY: -{seconds} seconds! Time remaining: {currentTime:F1}s");
    }

    private void ShowPenaltyFlash()
    {
        penaltyFlashPanel.SetActive(true);
        //LeanTween.alpha(penaltyFlashPanel.GetComponent<RectTransform>(), 0.7f, 0.1f);
        //LeanTween.alpha(penaltyFlashPanel.GetComponent<RectTransform>(), 0f, flashDuration)
          //  .setDelay(0.1f)
           // .setOnComplete(() => penaltyFlashPanel.SetActive(false));
    }

    private void UpdateTimerDisplay()
    {
        // Update text
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";

            // Change color based on time remaining
            if (currentTime <= dangerThreshold)
            {
                timerText.color = dangerColor;
            }
            else if (currentTime <= warningThreshold)
            {
                timerText.color = warningColor;
            }
            else
            {
                timerText.color = normalColor;
            }
        }

        // Update fill bar
        if (timerFillBar != null)
        {
            timerFillBar.fillAmount = currentTime / totalTime;

            // Change bar color
            if (currentTime <= dangerThreshold)
            {
                timerFillBar.color = dangerColor;
            }
            else if (currentTime <= warningThreshold)
            {
                timerFillBar.color = warningColor;
            }
            else
            {
                timerFillBar.color = normalColor;
            }
        }
    }

    private void TimerExpired()
    {
        hasExpired = true;
        isRunning = false;
        Debug.Log(" TIME'S UP! SOUP BURNED!");
        OnTimerExpired?.Invoke();
    }

    // Public getters
    public float GetTimeRemaining() => currentTime;
    public float GetTimeRemainingPercent() => currentTime / totalTime;
    public bool IsRunning() => isRunning;
    public bool HasExpired() => hasExpired;
}