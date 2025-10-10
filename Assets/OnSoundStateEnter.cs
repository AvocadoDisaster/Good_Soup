using UnityEngine;

public class OnSoundStateEnter : StateMachineBehaviour
{
    [SerializeField] private Soundtype sound;
    [SerializeField, Range(0, 1)] private float volume = 1;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SoundManager.Playsound(sound,volume);
    }
}
