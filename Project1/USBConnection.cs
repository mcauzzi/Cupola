using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

static class USBConnection
{
    private static SerialPort _serialPort;

    public static void init()
    {
        _serialPort = new SerialPort();
        _serialPort.PortName = "COM3";
        _serialPort.BaudRate = 9600;
        _serialPort.DataBits = 8;
        _serialPort.Parity = Parity.None;
        _serialPort.StopBits = StopBits.One;
        _serialPort.Handshake = Handshake.RequestToSend;
    }

    public static void open()
    {
        _serialPort.Open();
    }

    public static void close()
    {
        _serialPort.Close();
    }

    public static void send(String str)
    {
        _serialPort.Write(str);
    }
}
