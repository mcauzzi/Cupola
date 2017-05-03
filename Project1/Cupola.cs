using System.IO.Ports;

public class Cupola{
    public static void Main() {
        /*
        SerialPort _serialPort = new SerialPort();
        _serialPort.PortName = "COM3";
        _serialPort.BaudRate = 9600;
        _serialPort.DataBits = 8;
        _serialPort.Parity = Parity.None;
        _serialPort.StopBits = StopBits.One;
        _serialPort.Handshake = Handshake.None;

        _serialPort.Open();
        _serialPort.Write("<f><v23>");
        //_serialPort.Write("<t100>");
        _serialPort.Write("<v23>");

        _serialPort.Close();

        System.Console.ReadLine();
        */

        USBConnection.init();
        USBConnection.open();
        CommandList cl = new CommandList();
        cl.add(new Command(Command.cmdtype.TIME, 100));
        cl.add(new Command(Command.cmdtype.ULTRAVIOLET, 24));
        cl.add(new Command(Command.cmdtype.ULTRAVIOLET, 25));
        cl.add(new Command(Command.cmdtype.ULTRAVIOLET, 26));
        cl.sendwait();
        USBConnection.close();
        System.Console.ReadLine();
    }
}