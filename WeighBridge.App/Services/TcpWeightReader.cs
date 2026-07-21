using System.Net.Sockets;
using System.Text;
using WeightBridgeApp.Models;

namespace WeightBridgeApp.Services;

public class TcpWeightReader : IWeightReader
{
    private TcpClient? _client;
    private CancellationTokenSource? _cts;
    private readonly StringBuilder _buffer = new();

    public event EventHandler<WeightReadingEventArgs>? WeightReceived;
    public bool IsConnected => _client?.Connected == true;

    public async Task ConnectAsync(DeviceSettings settings)
    {
        if (IsConnected)
            return;

        _client = new TcpClient();
        await _client.ConnectAsync(settings.IpAddress, settings.TcpPort);
        _cts = new CancellationTokenSource();
        _ = Task.Run(() => ReadLoopAsync(_cts.Token));
    }

    public Task DisconnectAsync()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        _client?.Close();
        _client?.Dispose();
        _client = null;
        _buffer.Clear();
        return Task.CompletedTask;
    }

    private async Task ReadLoopAsync(CancellationToken token)
    {
        if (_client == null)
            return;

        var stream = _client.GetStream();
        var bytes = new byte[1024];

        try
        {
            while (!token.IsCancellationRequested)
            {
                var count = await stream.ReadAsync(bytes, token);
                if (count == 0)
                    break;

                var text = Encoding.ASCII.GetString(bytes, 0, count);
                _buffer.Append(text);
                ProcessBuffer();
            }
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
