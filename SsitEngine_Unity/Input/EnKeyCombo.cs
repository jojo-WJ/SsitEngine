using System;

namespace SsitEngine.Unity.SsitInput
{
	/// <summary>
	///     组和键
	/// </summary>
	[Flags]
    public enum EnKeyCombo
    {
        FOCUS = 1,
        DUPLICATE = 2,
        UNDO = 4,
        REDO = 8
    }
}