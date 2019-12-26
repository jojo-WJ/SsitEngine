using System;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class SerColor : ISerializable
{
    public float a;
    public float b;
    public float g;
    public float r;

    //serialization constructor
    protected SerColor( SerializationInfo info, StreamingContext context )
    {
        r = (float) info.GetValue("r", typeof(float));
        g = (float) info.GetValue("g", typeof(float));
        b = (float) info.GetValue("b", typeof(float));
        a = (float) info.GetValue("a", typeof(float));
    }

    public SerColor( Color color )
    {
        r = color.r;
        g = color.g;
        b = color.b;
        a = color.a;
    }

    public SerColor( float r, float g, float b, float a )
    {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }

    /*[SecurityPermissionAttribute(
	            SecurityAction.Demand,
	            SerializationFormatter = true)]		*/
    public void GetObjectData( SerializationInfo info, StreamingContext ctxt )
    {
        info.AddValue("r", r);
        info.AddValue("g", g);
        info.AddValue("b", b);
        info.AddValue("a", a);
    }


    public bool isDifferentTo( Color other )
    {
        var changed = false;

        if (!changed && r != other.r)
            changed = true;

        if (!changed && g != other.g)
            changed = true;

        if (!changed && b != other.b)
            changed = true;

        if (!changed && a != other.a)
            changed = true;


        return changed;
    }

    public Color getColor()
    {
        return new Color(r, g, b, a);
    }

    public static implicit operator Color( SerColor i )
    {
        return i.getColor();
    }

    public static implicit operator SerColor( Color i )
    {
        return new SerColor(i);
    }

    public override string ToString()
    {
        Color cc = this;
        return cc.ToString();
    }
}