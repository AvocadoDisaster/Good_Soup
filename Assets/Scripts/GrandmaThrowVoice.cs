using UnityEngine;

public class GrandmaThrowVoice : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.J))
        {
            SoundFXManager.instance.PlayGrandmaVoiceLine();
        }
    }
}