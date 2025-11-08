using UnityEngine;
using TMPro;

public class ScorePopup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private string defaultText = "+1";
    [SerializeField] private Color textColor = Color.yellow;

    private float timer = 0f;
    private Vector3 startPosition;
    private CanvasGroup canvasGroup;

    private void Start()
    {
        startPosition = transform.position;

        // Setup canvas group for fading
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Setup text
        if (scoreText != null)
        {
            scoreText.text = defaultText;
            scoreText.color = textColor;
        }

        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float progress = timer / lifetime;

        // Move upward
        transform.position = startPosition + Vector3.up * (moveSpeed * timer);

        // Fade out
        if (canvasGroup != null)
        {
            canvasGroup.alpha = fadeOutCurve.Evaluate(progress);
        }
    }

    /// <summary>
    /// Set custom score text
    /// </summary>
    public void SetScoreText(string text)
    {
        if (scoreText != null)
        {
            scoreText.text = text;
        }
    }

    /// <summary>
    /// Set custom color
    /// </summary>
    public void SetColor(Color color)
    {
        if (scoreText != null)
        {
            scoreText.color = color;
        }
    }
}