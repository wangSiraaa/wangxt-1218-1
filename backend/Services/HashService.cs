using System.Security.Cryptography;

namespace JudicialEvidence.Api.Services;

public interface IHashService
{
    Task<string> ComputeSha256Async(Stream stream);
    bool Verify(string computed, string expected);
}

public class HashService : IHashService
{
    public async Task<string> ComputeSha256Async(Stream stream)
    {
        stream.Position = 0;
        using var sha = SHA256.Create();
        var bytes = await sha.ComputeHashAsync(stream);
        stream.Position = 0;
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public bool Verify(string computed, string expected)
    {
        if (string.IsNullOrWhiteSpace(computed) || string.IsNullOrWhiteSpace(expected))
        {
            return false;
        }
        return string.Equals(
            computed.Trim().ToLowerInvariant(),
            expected.Trim().ToLowerInvariant(),
            StringComparison.Ordinal);
    }
}
