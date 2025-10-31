using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera mainCamera;
    public Camera altCamera;
    public KeyCode switchKey = KeyCode.C;

    private bool isMainActive = true;

    void Start()
    {
        // Start with main camera active
        mainCamera.enabled = true;
        altCamera.enabled = false;

        FixAudioListeners();
    }

    void Update()
    {
        if (Input.GetKeyDown(switchKey))
        {
            // Toggle camera state
            isMainActive = !isMainActive;
            mainCamera.enabled = isMainActive;
            altCamera.enabled = !isMainActive;

            FixAudioListeners();
        }
    }

    // Ensure only the active camera has AudioListener enabled
    void FixAudioListeners()
    {
        AudioListener mainAL = mainCamera.GetComponent<AudioListener>();
        AudioListener altAL = altCamera.GetComponent<AudioListener>();

        if (mainAL != null) mainAL.enabled = mainCamera.enabled;
        if (altAL != null) altAL.enabled = altCamera.enabled;
    }
}
