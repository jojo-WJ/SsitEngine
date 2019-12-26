using SsitEngine.Data;
using UnityEngine;
using UnityEngine.Events;

namespace SSIT.Events
{
    public class StringEvent : UnityEvent<string>
    {
    }

    public class IntEvent : UnityEvent<int>
    {
    }

    public class IntsEvent : UnityEvent<int, int>
    {
    }

    public class FloatEvent : UnityEvent<float>
    {
    }

    public class BoolEvent : UnityEvent<bool>
    {
    }

    public class BoolAdvanceEvent : UnityEvent<int, bool>
    {
    }

    public class InfoEvent : UnityEvent<IInfoData>
    {
    }

    public class InfoEventToggle : UnityEvent<IInfoData, bool>
    {
    }

    public class ColorEvent : UnityEvent<Color>
    {
    }

    public class Vector3Event : UnityEvent<Vector3>
    {
    }

    public class StringsEvent : UnityEvent<string, string>
    {
    }
}