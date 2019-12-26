using UnityEngine;

public class vFootStepHandler : MonoBehaviour
{
    public enum StepHandleType
    {
        materialName,
        textureName
    }

    public StepHandleType stepHandleType;

    [field:
        Tooltip(
            "Use this to select a specific material or texture if your mesh has multiple materials, the footstep will play only the selected index.")]
    [field: SerializeField]
    public int material_ID { get; }
}