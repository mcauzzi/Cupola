﻿using System.IO.Ports;

public class USBConnection
{
    private SerialPort serialPort;

    public USBConnection(string portName)
    {
        serialPort = new SerialPort
        {
            PortName = portName,
            BaudRate = 9600,
            DataBits = 8,
            Parity = Parity.None,
            StopBits = StopBits.One,
            Handshake = Handshake.RequestToSend
        };
    }

    public void Open()
    {
        serialPort.Open();
    }

    public void Close()
    {
        serialPort.Close();
    }

    public void Send(string str)
    {
        serialPort.Write(str);
    }
}