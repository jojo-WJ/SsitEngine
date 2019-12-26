using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class vClassHeaderAttribute : Attribute
{
    public string header;
    public string helpBoxText;
    public string iconName;
    public bool openClose;
    public bool useHelpBox;

    public vClassHeaderAttribute( string header, bool openClose = true, string iconName = "icon_v2",
        bool useHelpBox = false, string helpBoxText = "" )
    {
        this.header = header;
        this.openClose = openClose;
        this.iconName = iconName;
        this.useHelpBox = useHelpBox;
        this.helpBoxText = helpBoxText;
    }

    public vClassHeaderAttribute( string header, string helpBoxText )
    {
        this.header = header;
        openClose = true;
        iconName = "icon_v2";
        useHelpBox = true;
        this.helpBoxText = helpBoxText;
    }
}