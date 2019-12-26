using Invector;
using UnityEngine;

[vClassHeader("vComment", false)]
public class vComment : vMonoBehaviour
{
#if UNITY_EDITOR
    [TextArea(5, 3000)] public string comment;
#endif
}