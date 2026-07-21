namespace WeightBridgeApp.Services;

public class WeightReadingEventArgs : EventArgs
{
    public WeightReadingEventArgs(decimal weight, bool isStable, string rawData)
    {
        Weight = weight;
        IsStable = isStable;
        RawData = rawData;
        ReadingTime = DateTime.Now;
    }

    public decimal Weight { get; }
    public bool IsStable { get; }
    public string RawData { get; }
    public DateTime ReadingTime { get; }
}
