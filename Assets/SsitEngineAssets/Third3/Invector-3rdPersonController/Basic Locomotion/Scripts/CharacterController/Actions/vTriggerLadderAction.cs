using Invector;
using UnityEngine;
using UnityEngine.Events;

[vClassHeader("Trigger Ladder Action", false)]
public class vTriggerLadderAction : vMonoBehaviour
{
    [Tooltip("Use this to limit the trigger to active if forward of character is close to this forward")]
    public bool activeFromForward;

    [Header("Trigger Action Options")] [Tooltip("Automatically execute the action without the need to press a Button")]
    public bool autoAction;

    [Tooltip("End the match target of the animation")]
    public float endMatchTarget;

    [Tooltip("Trigger an Animation - Use the exactly same name of the AnimationState you want to trigger")]
    public string exitAnimation;

    [Tooltip(
        "Use a transform to help the character climb any height, take a look at the Example Scene ClimbUp, StepUp, JumpOver objects.")]
    public Transform matchTarget;

    public UnityEvent OnDoAction;
    public UnityEvent OnPlayerEnter;
    public UnityEvent OnPlayerExit;
    public UnityEvent OnPlayerStay;

    [Tooltip("Trigger an Animation - Use the exactly same name of the AnimationState you want to trigger")]
    public string playAnimation;

    [Tooltip("Start the match target of the animation")]
    public float startMatchTarget;

    [Tooltip("Rotate Character for this rotation when active")]
    public bool useTriggerRotation;
}