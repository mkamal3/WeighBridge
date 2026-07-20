using System.IO.Ports;
using System.Text;
using WeightBridgeApp.Models;

namespace WeightBridgeApp.Services;

public class SerialWeightReader : IWeightReader
{
    private SerialPort? _serialPort;
    private readonly StringBuilder _buffer = new();

    public event EventHandler<WeightReadingEventArgs>? WeightReceived;
    public bool IsConnected => _serialPort?.IsOpen == true;

    public Task ConnectAsync(DeviceSettings settings)
    {
        if (IsConnected)
            return Task.CompletedTask;

        var parity = Enum.TryParse(settings.Parity, true, out Parity parsedParity)
            ? parsedParity
            : Parity.None;

        var stopBits = Enum.TryParse(settings.StopBits, true, out StopBits parsedStopBits)
            ? parsedStopBits
            : StopBits.One;

        _serialPort = new SerialPort(settings.ComPort, settings.BaudRate, parity, settings.DataBits, stopBits)
        {
            Encoding = Encoding.ASCII,
            ReadTimeout = 1000,
            NewLine = "\r\n"
        };

        _serialPort.DataReceived += SerialPort_DataReceived;
        _serialPort.Open();
        return Task.CompletedTask;
    }

    public Task DisconnectAsync()
    {
        if (_serialPort != null)
        {
            _serialPort.DataReceived -= SerialPort_DataReceived;
            if (_serialPort.IsOpen)
                _serialPort.Close();
            _serialPort.Dispose();
            _serialPort = null;
        }

        _buffer.Clear();
        return Task.CompletedTask;
    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            if (_serialPort == null)
                return;

            var incoming = _serialPort.ReadExisting();
            if (string.IsNullOrEmpty(incoming))
                return;

            _buffer.Append(incoming);
            ProcessBuffer();
        }
        catch
        {
            // In production, write this to a log table/file.
        }
    }

    private void ProcessBuffer()
    {
        var data = _buffer.ToString();
        var lines = data.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

        for (var i = 0; i < lines.Length - 1; i++)
            RaiseReading(lines[i]);

        _buffer.Clear();
        _buffer.Append(lines.LastOrDefault() ?? string.Empty);

        // Some indicators stream without newline. Keep the latest readable data small.
        if (_buffer.Length > 80)
        {
            RaiseReading(_buffer.ToString());
            _buffer.Clear();
        }
    }

    private void RaiseReading(string raw)
    {
        if (WeightParser.TryParse(raw, out var weight, out var stable))
            WeightReceived?.Invoke(this, new WeightReadingEventArgs(weight, stable, raw));
    }
}
