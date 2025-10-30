using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    [Header("UI References")]
    public Slider volumeSlider;
    public TextMeshProUGUI volumeLabel;
    public GameObject mainMenuCanvas;

    public void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);   
        volumeSlider.value = savedVolume;   
        AudioListener.volume = savedVolume; 
        UpdateVolumeText(savedVolume);

        volumeSlider.onValueChanged.AddListener( OnVolumeChanged );
    }

    
    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
        UpdateVolumeText(value);
    }

    public void UpdateVolumeText(float value)
    {
        int percentage = Mathf.RoundToInt(value * 100);
        volumeLabel.text = $"Volume: {percentage}%";
    }

    public void BackToMainMenu()
    {
        mainMenuCanvas.SetActive(true);
        gameObject.SetActive(false);
    }
}
