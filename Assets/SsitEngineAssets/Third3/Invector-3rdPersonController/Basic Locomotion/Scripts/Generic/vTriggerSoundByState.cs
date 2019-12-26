using System.Collections.Generic;
using UnityEngine;

public class vTriggerSoundByState : StateMachineBehaviour
{
    private vFisherYatesRandom _random;
    public GameObject audioSource;
    private bool isTrigger;
    public List<AudioClip> sounds;

    public float triggerTime;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter( Animator animator, AnimatorStateInfo stateInfo, int layerIndex )
    {
        isTrigger = false;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    public override void OnStateUpdate( Animator animator, AnimatorStateInfo stateInfo, int layerIndex )
    {
        if (stateInfo.normalizedTime % 1 >= triggerTime && !isTrigger) TriggerSound(animator, stateInfo, layerIndex);
    }

    private void TriggerSound( Animator animator, AnimatorStateInfo stateInfo, int layerIndex )
    {
        if (_random == null)
            _random = new vFisherYatesRandom();
        isTrigger = true;
        GameObject audioObject = null;
        if (audioSource != null)
        {
            audioObject = Instantiate(audioSource.gameObject, animator.transform.position, Quaternion.identity);
        }
        else
        {
            audioObject = new GameObject("audioObject");
            audioObject.transform.position = animator.transform.position;
        }
        if (audioObject != null)
        {
            var source = audioObject.gameObject.GetComponent<AudioSource>();
            var clip = sounds[_random.Next(sounds.Count)];
            source.PlayOneShot(clip);
        }
    }
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}
}