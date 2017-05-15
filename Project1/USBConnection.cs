using System;
using System.IO.Ports;

public static class USBConnection
{
    private static SerialPort serialPort;

    public static void Init()
    {
        serialPort = new SerialPort()
        {
            PortName = "COM3",
            BaudRate = 9600,
            DataBits = 8,
            Parity = Parity.None,
            StopBits = StopBits.One,
            Handshake = Handshake.RequestToSend
        };
    }

    public static void Open()
    {
        serialPort.Open();
    }

    public static void Close()
    {
        serialPort.Close();
    }

    public static void Send(String str)
    {
        serialPort.Write(str);
    }
}
