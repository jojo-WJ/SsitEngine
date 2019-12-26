using UnityEngine;

public class vGetTransform : MonoBehaviour
{
    [Tooltip("Select the transform you want to use as reference to the Match Target")]
    public AvatarTarget avatarTarget;

    [Tooltip("Check the Exit Time of your animation and insert here")]
    public float endExitTimeAnimation = 0.8f;

    [Tooltip("End the match target of the animation")]
    public float endMatchTarget;

    [Tooltip(
        "Use a transform to help the character climb any height, take a look at the Example Scene ClimbUp, StepUp, JumpOver objects.")]
    public Transform matchTarget;

    [Tooltip("Check what position XYZ you want the matchTarget to work")]
    public Vector3 matchTargetMask;

    [Tooltip("Start the match target of the animation")]
    public float startMatchTarget;

    [Tooltip("Rotate Character for this rotation when active")]
    public bool useTriggerRotation;
}