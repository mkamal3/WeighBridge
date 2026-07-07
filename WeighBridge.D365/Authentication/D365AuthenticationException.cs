namespace WeighBridge.D365.Authentication;

public sealed class D365AuthenticationException : Exception
{
    public D365AuthenticationException(string message) : base(message)
    {
    }

    public D365AuthenticationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
