using Microsoft.Extensions.Options;

namespace JudicialEvidence.Api.Services;

public class FileStorageOptions
{
    public string Root { get; set; } = "evidence-store";
}

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream stream, string fileName);
    Stream OpenRead(string relativePath);
    string GetFullPath(string relativePath);
    Task<string> CopyToAsync(string sourceRelativePath, string suffix);
}

public class FileStorageService : IFileStorageService
{
    private readonly FileStorageOptions _options;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IOptions<FileStorageOptions> options, ILogger<FileStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;
        Directory.CreateDirectory(_options.Root);
    }

    public async Task<string> SaveAsync(Stream stream, string fileName)
    {
        var safeName = Path.GetFileName(fileName);
        var stamped = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}_{safeName}";
        var bucket = DateTime.UtcNow.ToString("yyyyMM");
        var dir = Path.Combine(_options.Root, bucket);
        Directory.CreateDirectory(dir);
        var fullPath = Path.Combine(dir, stamped);
        await using var fs = File.Create(fullPath);
        stream.Position = 0;
        await stream.CopyToAsync(fs);
        await fs.FlushAsync();
        var relative = Path.Combine(bucket, stamped);
        _logger.LogInformation("Evidence file stored at {Path}", relative);
        return relative;
    }

    public Stream OpenRead(string relativePath)
    {
        var fullPath = GetFullPath(relativePath);
        return File.OpenRead(fullPath);
    }

    public string GetFullPath(string relativePath)
    {
        var root = Path.GetFullPath(_options.Root);
        var full = Path.GetFullPath(Path.Combine(root, relativePath));
        if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Path traversal detected.");
        }
        return full;
    }

    public async Task<string> CopyToAsync(string sourceRelativePath, string suffix)
    {
        var sourceFull = GetFullPath(sourceRelativePath);
        var dir = Path.Combine(_options.Root, "copies");
        Directory.CreateDirectory(dir);
        var name = Path.GetFileName(sourceRelativePath);
        var copyName = $"{suffix}_{name}";
        var dest = Path.Combine(dir, copyName);
        await using var src = File.OpenRead(sourceFull);
        await using var dst = File.Create(dest);
        await src.CopyToAsync(dst);
        await dst.FlushAsync();
        return Path.Combine("copies", copyName);
    }
}
