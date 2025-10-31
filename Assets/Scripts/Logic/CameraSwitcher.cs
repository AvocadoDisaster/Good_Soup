using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Cameras")]
    public Camera mainCamera;
    public Camera altCamera;

    [Header("Objects to Toggle")]
    public GameObject spoon;
    public GameObject spoon1;
    public GameObject rallyCaller;
    public GameObject rallyCaller1;

    [Header("Key Settings")]
    public KeyCode switchKey = KeyCode.C;

    private bool isMainActive = true;

    void Start()
    {
        // Start with main camera and first set active
        mainCamera.enabled = true;
        altCamera.enabled = false;

        SetActiveSet(isMainActive);
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

            SetActiveSet(isMainActive);
            FixAudioListeners();
        }
    }

    void SetActiveSet(bool mainActive)
    {
        // Enable one set, disable the other
        spoon.SetActive(mainActive);
        rallyCaller.SetActive(mainActive);
        spoon1.SetActive(!mainActive);
        rallyCaller1.SetActive(!mainActive);
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
