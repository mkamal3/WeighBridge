namespace WeighBridge.D365.Health;

public interface ID365ConnectionVerifier
{
    /// <summary>
    /// Verifies that Azure AD authentication against the configured D365 environment succeeds.
    /// Does not require the custom weighbridge service to exist yet.
    /// </summary>
    Task<D365ConnectionVerificationResult> VerifyAsync(CancellationToken cancellationToken = default);
}

public sealed record D365ConnectionVerificationResult
{
    public required bool Succeeded { get; init; }

    public string? ErrorMessage { get; init; }

    public static D365ConnectionVerificationResult Success() =>
        new() { Succeeded = true };

    public static D365ConnectionVerificationResult Failure(string errorMessage) =>
        new() { Succeeded = false, ErrorMessage = errorMessage };
}
