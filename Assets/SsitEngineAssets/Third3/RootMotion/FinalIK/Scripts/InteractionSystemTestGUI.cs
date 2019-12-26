using RootMotion.FinalIK;
using UnityEngine;

namespace RootMotion.Demos
{
    /// <summary>
    /// Simple GUI for quickly testing out interactions.
    /// </summary>
    [RequireComponent(typeof(InteractionSystem))]
    public class InteractionSystemTestGUI : MonoBehaviour
    {
        [Tooltip("The effectors to interact with")] [SerializeField]
        private FullBodyBipedEffector[] effectors;

        [Tooltip("The object to interact to")] [SerializeField]
        private InteractionObject interactionObject;

        private InteractionSystem interactionSystem;

        private void Awake()
        {
            interactionSystem = GetComponent<InteractionSystem>();
        }

        private void OnGUI()
        {
            if (interactionSystem == null) return;

            if (GUILayout.Button("Start Interaction With " + interactionObject.name))
            {
                if (effectors.Length == 0) Debug.Log("Please select the effectors to interact with.");

                foreach (var e in effectors) interactionSystem.StartInteraction(e, interactionObject, true);
            }

            if (effectors.Length == 0) return;

            if (interactionSystem.IsPaused(effectors[0]))
                if (GUILayout.Button("Resume Interaction With " + interactionObject.name))
                    interactionSystem.ResumeAll();
        }
    }
}