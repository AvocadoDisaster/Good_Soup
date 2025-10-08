using UnityEditor;
using UnityEngine;

public enum SoundType
{
    Ember,
    Grandma,
    Pot,
    Throwing,
    Rally,
    Carrying,
    FootSteps,
    Ingredients,

}
[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] soundList;
    private static SoundManager Instance;
    private AudioSource _audioSource;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(SoundType sound, float Volume = 1)
    {
        Instance._audioSource.PlayOneShot(Instance.soundList[(int)sound], Volume);

    }
    
}
