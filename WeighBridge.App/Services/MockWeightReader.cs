using WeightBridgeApp.Models;

namespace WeightBridgeApp.Services;

public class MockWeightReader : IWeightReader
{
    private System.Threading.Timer? _timer;
    private readonly Random _random = new();
    private decimal _currentWeight = 12000;

    public event EventHandler<WeightReadingEventArgs>? WeightReceived;
    public bool IsConnected { get; private set; }

    public Task ConnectAsync(DeviceSettings settings)
    {
        IsConnected = true;
        _timer = new System.Threading.Timer(_ => GenerateWeight(), null, 0, 800);
        return Task.CompletedTask;
    }

    public Task DisconnectAsync()
    {
        IsConnected = false;
        _timer?.Dispose();
        _timer = null;
        return Task.CompletedTask;
    }

    private void GenerateWeight()
    {
        if (!IsConnected)
            return;

        var change = _random.Next(-50, 51);
        _currentWeight += change;
        if (_currentWeight < 0)
            _currentWeight = 0;

        var stable = _random.Next(0, 10) > 1;
        var raw = stable ? $"ST,GS,+{_currentWeight:0} kg" : $"US,GS,+{_currentWeight:0} kg";
        WeightReceived?.Invoke(this, new WeightReadingEventArgs(_currentWeight, stable, raw));
    }
}
