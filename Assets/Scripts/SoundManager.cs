using UnityEngine;


public enum Soundtype
{
    GoodSoupTheme,
    item_Pickup,
    grannydemo3,
    grannydemo6,
    grannydemo7,
    grannydemo8,
    grannydemo15
}
public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    private AudioSource AudioSource;
    [SerializeField] private AudioClip[] SoundList;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        AudioSource = GetComponent<AudioSource>();
    }

    public static void Playsound(Soundtype sound,float volume = 1)
    {
        instance.AudioSource.PlayOneShot(instance.SoundList[(int)sound], volume);
    }
}
