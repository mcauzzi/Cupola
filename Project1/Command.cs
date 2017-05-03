using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Command
{
    public enum cmdtype
    {
        TIME,
        VISIBLE,
        INFRARED,
        ULTRAVIOLET,
        PHOTO
    }

    private cmdtype type;
    private int value;

    public Command(cmdtype type)
    {
        if(type != cmdtype.PHOTO)
            throw new System.ArgumentException("You must instance PHOTO without parameters","type");

        this.type = type;
    }

    public Command(cmdtype type, int value)
    {
        if (type == cmdtype.PHOTO)
            throw new System.ArgumentException("You can't instance a PHOTO with parameters", "type");
        if (type == cmdtype.TIME && (value<0 || value>999))
            throw new System.ArgumentException("Wrong value of TIME", "value");
        if ((type == cmdtype.VISIBLE || type == cmdtype.INFRARED || type == cmdtype.ULTRAVIOLET) && (value < 1 || value>50))
            throw new System.ArgumentException("Wrong number of led", "value");

        this.type = type;
        this.value = value;
    }

    public void send()
    {
        USBConnection.send(toString());
    }

    public string toString()
    {
        switch (type) {
            case cmdtype.TIME: 
                return "<t" + value + ">";
            case cmdtype.VISIBLE:
                return "<v" + (value-1) + ">";
            case cmdtype.INFRARED:
                return "<i" + (value-1) + ">";
            case cmdtype.ULTRAVIOLET:
                return "<u" + (value-1) + ">";
            default:
                return "<f>";
        }
    }
}
