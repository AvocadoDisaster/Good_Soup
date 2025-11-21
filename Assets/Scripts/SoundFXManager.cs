using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager instance;

    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Grandma Voice Lines")]
    public AudioClip[] grandmaVoiceLines;
    [Range(0f, 1f)]
    public float grandmaChance = 0.33f;  // 1 in 3 chance

    [Header("Ember Voice Lines")]
    public AudioClip[] emberVoiceLines;

    private void Awake()
    {

        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void PlayGrandmaVoiceLine()
    {

        if (Random.value > grandmaChance)
            return;

        if (grandmaVoiceLines.Length == 0)
            return;

        int index = Random.Range(0, grandmaVoiceLines.Length);
        audioSource.PlayOneShot(grandmaVoiceLines[index]);
    }

    public void PlayEmberVoiceLine()
    {
        if (emberVoiceLines.Length == 0)
            return;

        int index = Random.Range(0, emberVoiceLines.Length);
        audioSource.PlayOneShot(emberVoiceLines[index]);
    }
}