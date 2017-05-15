using System;

public class Command
{
    public enum Cmdtype
    {
        TIME,
        VISIBLE,
        INFRARED,
        ULTRAVIOLET,
        PHOTO
    }

    public Cmdtype Type {  get; private set; }
    public int Value {  get; private set; }

    //constructor for a PHOTO
    public Command(Cmdtype type)
    {
        if (type != Cmdtype.PHOTO)
            throw new ArgumentException("You can only instance PHOTO without parameters", "type");

        this.Type = type;
    }

    //constructor for a led
    public Command(Cmdtype type, int value)
    {
        if (type == Cmdtype.PHOTO)
            throw new ArgumentException("You can't instance a PHOTO with parameters", "type");
        if (type == Cmdtype.TIME && (value<0 || value>999))
            throw new ArgumentException("Wrong value of TIME", "value");
        if ((type == Cmdtype.VISIBLE || type == Cmdtype.INFRARED || type == Cmdtype.ULTRAVIOLET) && (value < 1 || value>50))
            throw new ArgumentException("Wrong number of led", "value");

        this.Type = type;
        this.Value = value;
    }

    //constructor for a string, es. <t>, <v30>
    public Command(string str)
    {
        Cmdtype type = GetType(str);

        if (type == Cmdtype.PHOTO)
        {
            this.Type = type;
        } else {
            int value = GetValue(str) + 1;
            if (type == Cmdtype.TIME && (value < 0 || value > 999))
                throw new ArgumentException("Wrong value of TIME", nameof(str));
            if ((type == Cmdtype.VISIBLE || type == Cmdtype.INFRARED || type == Cmdtype.ULTRAVIOLET) && (value < 1 || value > 50))
                throw new ArgumentException("Wrong number of led", nameof(str));

            this.Type = type;
            Value = value;
        }
    }

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
            default: throw new ArgumentException("No type recognised in the string", "str");
        }
    }

    //returns the value of the string in input
    private int GetValue(string str)
    {
        return int.Parse(str.Substring(2, str.IndexOf('>')-2));
    }

    public void Send()
    {
        USBConnection.Send(ToString());
    }

    public override string ToString()
    {
        switch (Type) {
            case Cmdtype.TIME: 
                return "<t" + Value + ">";
            case Cmdtype.VISIBLE:
                return "<v" + (Value-1) + ">";
            case Cmdtype.INFRARED:
                return "<i" + (Value-1) + ">";
            case Cmdtype.ULTRAVIOLET:
                return "<u" + (Value-1) + ">";
            default:
                return "<f>";
        }
    }
}
