using UnityEngine;

public class vEditorToolbarAttribute : PropertyAttribute
{
    public readonly string title;

    public vEditorToolbarAttribute( string title )
    {
        this.title = title;
    }
}