using System;

public class Command
{
    public const int MAXTIME = 999;
    public const int MAXLED = 45;

    public enum Cmdtype
    {
        TIME,
        VISIBLE,
        INFRARED,
        ULTRAVIOLET,
        PHOTO
    }

    //constructor for a PHOTO
    public Command(Cmdtype type)
    {
        if (type != Cmdtype.PHOTO)
            throw new ArgumentException("You can only instance PHOTO without parameters", nameof(type));

        Type = type;
    }

    //constructor for a led
    public Command(Cmdtype type, int value)
    {
        if (type == Cmdtype.PHOTO)
            throw new ArgumentException("You can't instance a PHOTO with parameters", nameof(type));
        if (type == Cmdtype.TIME && (value < 0 || value > MAXTIME))
            throw new ArgumentException("Wrong value of TIME", nameof(value));
        if ((type == Cmdtype.VISIBLE || type == Cmdtype.INFRARED || type == Cmdtype.ULTRAVIOLET) &&
            (value < 1 || value > MAXLED))
            throw new ArgumentException("Wrong number of led", nameof(value));

        Type = type;
        Value = value;
    }

    //constructor for a string, es. <t>, <v30>
    public Command(string str)
    {
        var type = GetType(str);

        if (type == Cmdtype.PHOTO)
        {
            Type = type;
        }
        else
        {
            var value = GetValue(str) + 1;
            if (type == Cmdtype.TIME && (value < 0 || value > MAXTIME))
                throw new ArgumentException("Wrong value of TIME", nameof(str));
            if ((type == Cmdtype.VISIBLE || type == Cmdtype.INFRARED || type == Cmdtype.ULTRAVIOLET) &&
                (value < 1 || value > MAXLED))
                throw new ArgumentException("Wrong number of led", nameof(str));

            Type = type;
            Value = value;
        }
    }

    public Cmdtype Type { get; }
    public int Value { get; }

    //returns the type of the string in input
    private Cmdtype GetType(string str)
    {
        switch (str[1])
        {
            case 't': return Cmdtype.TIME;
            case 'v': return Cmdtype.VISIBLE;
            case 'i': return Cmdtype.INFRARED;
            case 'u': return Cmdtype.ULTRAVIOLET;
            case 'p': return Cmdtype.PHOTO;
            default: throw new ArgumentException("No type recognised in the string", nameof(str));
        }
    }

    //returns the value of the string in input
    private int GetValue(string str)
    {
        return int.Parse(str.Substring(2, str.IndexOf('>') - 2));
    }

    public void Send(USBConnection usbCon)
    {
        usbCon.Send(ToString());
    }

    public override string ToString()
    {
        switch (Type)
        {
            case Cmdtype.TIME:
                return "<t" + Value + ">";
            case Cmdtype.VISIBLE:
                return "<v" + (Value - 1) + ">";
            case Cmdtype.INFRARED:
                return "<i" + (Value - 1) + ">";
            case Cmdtype.ULTRAVIOLET:
                return "<u" + (Value - 1) + ">";
            default:
                return "<f>";
        }
    }
}