using WeightBridgeApp.Models;

namespace WeightBridgeApp.Services;

public interface IWeightReader
{
    event EventHandler<WeightReadingEventArgs>? WeightReceived;
    bool IsConnected { get; }
    Task ConnectAsync(DeviceSettings settings);
    Task DisconnectAsync();
}
