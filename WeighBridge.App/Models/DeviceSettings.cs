namespace WeightBridgeApp.Models;

public class DeviceSettings
{
    public int SettingId { get; set; } = 1;
    public string ConnectionType { get; set; } = "Mock"; // Mock, Serial, TCP
    public string ComPort { get; set; } = "COM1";
    public int BaudRate { get; set; } = 9600;
    public string Parity { get; set; } = "None";
    public int DataBits { get; set; } = 8;
    public string StopBits { get; set; } = "One";
    public string IpAddress { get; set; } = "192.168.1.100";
    public int TcpPort { get; set; } = 4001;
}
